using CSharpTest.Net.Collections;
using IndexEngine.Paths;
using Lucene.Net.Documents;
using SoftCircuits.CsvParser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Wintellect.PowerCollections;

namespace IndexEngine
{
    public static class IndexHelper
    {

        #region fields
        private static int viewerReadIndex = 0;
        private static int[] lookup = new int[3];



        #endregion
        #region save info
        private static void CheckDir()
        {
            if (!System.IO.Directory.Exists(ProjectInfo.InfoPath))
            {
                System.IO.Directory.CreateDirectory(ProjectInfo.InfoPath);

            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(ProjectInfo.InfoPath);

                foreach (System.IO.FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
            }
        }
        private static void SaveInfoToDisk()
        {


            using (StreamWriter file =
                new StreamWriter(ProjectInfo.KeyPath))
            {
                file.WriteLine(ProjectInfo.TextFieldKey);
                file.WriteLine(ProjectInfo.SenderFieldKey);
                file.WriteLine(ProjectInfo.DateFieldKey);
                file.WriteLine(ProjectInfo.Data.LineCount.ToString());

            }
        }

        private static void SaveUsersToDisk()
        {

            using (StreamWriter file = new StreamWriter(ProjectInfo.UsersPath))
            {
                foreach (var kvp in ProjectInfo.Data.UserColors)
                {
                    file.WriteLine(kvp.Key + "+" + kvp.Value.ToArgb().ToString());
                }
            }
        }

        private static void SaveFieldsToDisk()
        {

            using (StreamWriter file = new StreamWriter(ProjectInfo.FieldsPath))
            {
                foreach (var field in ProjectInfo.Data.SelectedFields)
                {
                    file.WriteLine(field);
                }
            }
        }
        private static void SaveStatsToDisk()
        {

            using (StreamWriter file = new StreamWriter(ProjectInfo.StatsPath))
            {
                foreach (var kvp in ProjectInfo.Data.MessagesPerDay)
                {
                    file.WriteLine(kvp.Key.ToString() + "#" + kvp.Value);
                }
            }
        }
        #endregion
        #region load info
        internal static OrderedDictionary<string, string> LoadInfoFromDisk(string keyPath)
        {

            OrderedDictionary<string, string> info = new OrderedDictionary<string, string>();
            using (StreamReader reader = new StreamReader(keyPath))
            {
                info.Add("TextFieldKey", reader.ReadLine());
                info.Add("SenderFieldKey", reader.ReadLine());
                info.Add("DateFieldKey", reader.ReadLine());
                info.Add("LineCount", reader.ReadLine());

            }
            return info;
        }

        internal static List<string> LoadFieldsFromDisk(string fieldsPath)
        {
            List<string> fields = new List<string>();
            using (StreamReader reader = new StreamReader(fieldsPath))
            {

                while (!reader.EndOfStream)
                {
                    fields.Add(reader.ReadLine());

                }

            }
            return fields;
        }

        internal static HashSet<string> LoadUsersFromDisk(string usersPath)
        {
            HashSet<string> users = new HashSet<string>();
            using (StreamReader reader = new StreamReader(usersPath))
            {
                while (!reader.EndOfStream)
                {
                    var kvp = reader.ReadLine().Split('+');

                    users.Add(kvp[0]);
                    ProjectInfo.Data.UserColors.Add(kvp[0], Color.FromArgb(int.Parse(kvp[1]))); //bad practice!
                }
            }

            return users;
        }

        internal static BTreeDictionary<DateTime, int> LoadStatsFromDisk(string statsPath)
        {
            BTreeDictionary<DateTime, int> stats = new BTreeDictionary<DateTime, int>();
            using (StreamReader reader = new StreamReader(statsPath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    string[] kvp = line.Split('#');
                    stats.Add(DateTime.Parse(kvp[0]), Int32.Parse(kvp[1]));
                }
            }
            return stats;
        }
        #endregion
        private static void InitLookup(string[] allFields)
        {
            lookup = new int[3];
            foreach (var field in ProjectInfo.Data.SelectedFields)
            {
                if (field == ProjectInfo.DateFieldKey)
                {
                    lookup[0] = Array.IndexOf(allFields, ProjectInfo.DateFieldKey);
                }
                if (field == ProjectInfo.SenderFieldKey)
                {
                    lookup[1] = Array.IndexOf(allFields, ProjectInfo.SenderFieldKey);
                }
                if (field == ProjectInfo.TextFieldKey)
                {
                    lookup[2] = Array.IndexOf(allFields, ProjectInfo.TextFieldKey);
                }
            }
        }

        public static int GetViewerReadIndex()
        {
            return viewerReadIndex;
        }

        public static void ResetViewerReadIndex(int index = 0)
        {
            viewerReadIndex = index;

            if (viewerReadIndex < 0)
                viewerReadIndex = 0;

            else if (viewerReadIndex >= LuceneService.DirReader.MaxDoc)
                viewerReadIndex = LuceneService.DirReader.MaxDoc - 1;
        }

        public static HashSet<DateTime> LoadAllActiveDates()
        {
            HashSet<DateTime> dates = new HashSet<DateTime>();

            for (int i = 0; i < LuceneService.DirReader.MaxDoc; ++i)
            {
                Document document = LuceneService.DirReader.Document(i);

                string dateField = ProjectInfo.DateFieldKey;
                string dateString = document.GetField(dateField).GetStringValue();

                DateTime fullDate = DateTools.StringToDate(dateString);
                DateTime shortDate = new DateTime(fullDate.Year, fullDate.Month, fullDate.Day);

                dates.Add(shortDate);
            }

            return dates;
        }

        public static List<DynamicMessage> LoadPreviousDocumentsFromIndex(int count)
        {
            if (viewerReadIndex == 0)
                return new List<DynamicMessage>();

            List<DynamicMessage> messages = new List<DynamicMessage>();
            int bottomEdge = Math.Max(viewerReadIndex - count, -1);

            for (int i = viewerReadIndex; i > bottomEdge; --i)
            {
                List<string> msgData = new List<string>();
                Document document = LuceneService.DirReader.Document(i);

                foreach (var field in ProjectInfo.Data.SelectedFields)
                {
                    msgData.Add(document.GetField(field).GetStringValue());
                }

                int id = document.GetField("id").GetInt32Value().Value;
                DynamicMessage msg = new DynamicMessage(msgData, ProjectInfo.Data.SelectedFields, ProjectInfo.DateFieldKey, id);

                messages.Add(msg);
            }

            viewerReadIndex = Math.Max(bottomEdge, 0);

            messages.Reverse();
            return messages;
        }

        public static List<DynamicMessage> LoadNextDocumentsFromIndex(int count)
        {
            if (viewerReadIndex == LuceneService.DirReader.MaxDoc - 1)
                return new List<DynamicMessage>();

            List<DynamicMessage> messages = new List<DynamicMessage>();
            int topEdge = Math.Min(viewerReadIndex + count, LuceneService.DirReader.MaxDoc);

            for (int i = viewerReadIndex; i < topEdge; ++i)
            {
                List<string> msgData = new List<string>();
                Document document = LuceneService.DirReader.Document(i);

                foreach (var field in ProjectInfo.Data.SelectedFields)
                {
                    msgData.Add(document.GetField(field).GetStringValue());
                }

                int id = document.GetField("id").GetInt32Value().Value;
                DynamicMessage msg = new DynamicMessage(msgData, ProjectInfo.Data.SelectedFields, ProjectInfo.DateFieldKey, id);

                messages.Add(msg);
            }

            viewerReadIndex = Math.Min(topEdge, LuceneService.DirReader.MaxDoc - 1);
            return messages;
        }

        public static List<DynamicMessage> LoadNDocumentsFromIndex(int count)
        {
            List<DynamicMessage> messages = new List<DynamicMessage>();


            for (int i = viewerReadIndex; i < count + viewerReadIndex; i++)
            {
                Document document;
                List<string> temp = new List<string>();
                if (i < LuceneService.DirReader.MaxDoc)
                {
                    document = LuceneService.DirReader.Document(i);
                }
                else
                {
                    break;
                }

                foreach (var field in ProjectInfo.Data.SelectedFields)
                {


                    temp.Add(document.GetField(field).GetStringValue());


                }

                DynamicMessage message = new DynamicMessage(temp, ProjectInfo.Data.SelectedFields, ProjectInfo.DateFieldKey, document.GetField("id").GetInt32Value().Value);
                messages.Add(message);


            }

            viewerReadIndex = count + viewerReadIndex;
            return messages;
        }

        public static int PopulateIndex(string filePath, string[] allFields, bool header)
        {
            InitLookup(allFields);
            int result = 0;
            int count = 0;

            int indexingValue = 0;
            if (lookup != null)
            {
                string[] row = null;
                DateTime date;
                using (var fileReader = new CsvReader(filePath))
                {
                    if (!header)
                    {
                        fileReader.ReadRow(ref row); //header read;
                    }

                    while (fileReader.ReadRow(ref row))

                    {
                        count++;
                        var t = row[lookup[0]];
                        date = DateTime.Parse(t);
                        ProjectInfo.Data.UserKeys.Add(row[lookup[1]]);


                        var day = date.Date;
                        if (!ProjectInfo.Data.MessagesPerDay.Keys.Contains(day))
                        {
                            ProjectInfo.Data.MessagesPerDay.Add(day, 1);
                        }
                        else
                        {
                            int temp = ProjectInfo.Data.MessagesPerDay[day];
                            temp++;
                            ProjectInfo.Data.MessagesPerDay.TryUpdate(day, temp);
                        }

                        Document document = new Document();
                        document.Add(new Int32Field("id", indexingValue, Field.Store.YES));

                        indexingValue++;

                        for (int i = 0; i < row.Length; i++)
                        {
                            if (lookup.Contains(i))
                            {
                                if (i == lookup[0])
                                {
                                    var temp = DateTools.DateToString(date, DateTools.Resolution.SECOND); //just in case
                                    document.Add(new StringField(allFields[i], temp, Field.Store.YES));
                                }
                                if (i == lookup[1])
                                {
                                    document.Add(new StringField(allFields[i], row[i], Field.Store.YES));
                                }
                                if (i == lookup[2])
                                {

                                    document.Add(new TextField(allFields[i], row[i], Field.Store.YES));
                                }
                            }
                            else
                            {
                                if (ProjectInfo.Data.SelectedFields.Contains(allFields[i]))
                                {
                                    document.Add(new StringField(allFields[i], row[i], Field.Store.YES));
                                }

                            }
                            //TODO: Still need to redesign this. Rework storing/indexing paradigm.

                        }
                        LuceneService.Writer.AddDocument(document);
                    }
                    LuceneService.Writer.Commit();
                    LuceneService.Writer.Flush(triggerMerge: false, applyAllDeletes: false);
                    ProjectInfo.Data.LineCount = count;
                    CheckDir();
                    PopulateUserColors();
                    SaveInfoToDisk();
                    SaveFieldsToDisk();
                    SaveUsersToDisk();
                    SaveStatsToDisk();


                    result = 1;
                    return result;
                }
            }
            return result;
        }


        private static void PopulateUserColors()
        {
            var colors = ColorLibrary.ColorGenerator.GenerateHSLuvColors(ProjectInfo.Data.UserKeys.Count, false);
            int i = 0;
            foreach (var user in ProjectInfo.Data.UserKeys)
            {
                ProjectInfo.Data.UserColors.Add(user, colors[i]);
                i++;
            }

        }




        internal static void UnloadData()
        {
            viewerReadIndex = 0;
            lookup = new int[3];
        }
    }
}
