using IndexEngine.Containers;
using IndexEngine.Data.Paths;
using IndexEngine.Search;
using Lucene.Net.Documents;
using SoftCircuits.CsvParser;
using System.Drawing;

namespace IndexEngine.Indexes
{
    public static class IndexHelper
    {
        private const int MAX_TERM_LENGTH = 32766;
        private const int PERIOD_OF_SAVING_INTERMEDIATE_RESULTS_OF_POPULATING_INDEX = 2 * 1000 * 1000;

        private static int _viewerReadIndex = 0;
        private static int[] _lookup = new int[3];

        #region SavingData

        private static void PrepareDirectory()
        {
            if (!Directory.Exists(ProjectInfo.InfoPath))
            {
                Directory.CreateDirectory(ProjectInfo.InfoPath);
            }
            else
            {
                var dirInfo = new DirectoryInfo(ProjectInfo.InfoPath);

                foreach (var file in dirInfo.GetFiles())
                {
                    file.Delete();
                }
            }
        }

        private static void SaveInfoToDisk()
        {
            using (var writer = new StreamWriter(ProjectInfo.KeyPath))
            {
                writer.WriteLine(ProjectInfo.TextFieldKey);
                writer.WriteLine(ProjectInfo.SenderFieldKey);
                writer.WriteLine(ProjectInfo.DateFieldKey);
                writer.WriteLine(ProjectInfo.Data.LineCount.ToString());
            }
        }

        private static void SaveUsersToDisk()
        {
            using (var writer = new StreamWriter(ProjectInfo.UsersPath))
            {
                foreach (var kvp in ProjectInfo.Data.UserColors)
                {
                    writer.WriteLine($"{kvp.Key}+{kvp.Value.ToArgb()}");
                }
            }
        }

        private static void SaveFieldsToDisk()
        {
            File.WriteAllLines(ProjectInfo.FieldsPath, ProjectInfo.Data.SelectedFields);
        }

        private static void SaveStatsToDisk()
        {
            using (var writer = new StreamWriter(ProjectInfo.StatsPath))
            {
                foreach (var kvp in ProjectInfo.Data.MessagesPerDay)
                {
                    writer.WriteLine(kvp.Key.ToString() + "#" + kvp.Value);
                }
            }
        }

        #endregion

        #region LoadingData

        internal static Dictionary<string, string> LoadInfoFromDisk(string keyPath)
        {
            var info = new Dictionary<string, string>();

            using (var reader = new StreamReader(keyPath))
            {
                info.Add("TextFieldKey", reader.ReadLine() ?? string.Empty);
                info.Add("SenderFieldKey", reader.ReadLine() ?? string.Empty);
                info.Add("DateFieldKey", reader.ReadLine() ?? string.Empty);
                info.Add("LineCount", reader.ReadLine() ?? string.Empty);
            }

            return info;
        }

        internal static List<string> LoadFieldsFromDisk(string fieldsPath)
        {
            return File.ReadAllLines(fieldsPath).ToList();
        }

        internal static HashSet<string> LoadUsersFromDisk(string usersPath)
        {
            var users = new HashSet<string>();

            using (var reader = new StreamReader(usersPath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine() ?? string.Empty;
                    string[] kvp = line.Split('+');

                    string user = kvp[0];
                    Color color = Color.FromArgb(int.Parse(kvp[1]));

                    users.Add(user);
                    ProjectInfo.Data.UserColors.Add(user, color);
                }
            }

            return users;
        }

        internal static Dictionary<DateTime, int> LoadStatsFromDisk(string statsPath)
        {
            var stats = new Dictionary<DateTime, int>();

            using (var reader = new StreamReader(statsPath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine() ?? string.Empty;
                    string[] kvp = line.Split('#');

                    DateTime date = DateTime.Parse(kvp[0]);
                    int count = int.Parse(kvp[1]);

                    stats.Add(date, count);
                }
            }

            return stats;
        }

        #endregion

        #region UnloadingData

        internal static void UnloadData()
        {
            _viewerReadIndex = 0;
            _lookup = new int[3];
        }

        #endregion

        #region ParametersManipulation

        public static int GetViewerReadIndex()
        {
            return _viewerReadIndex;
        }

        public static void ResetViewerReadIndex(int index = 0)
        {
            _viewerReadIndex = index;

            if (_viewerReadIndex < 0)
                _viewerReadIndex = 0;

            else if (_viewerReadIndex >= LuceneService.DirReader?.MaxDoc)
                _viewerReadIndex = LuceneService.DirReader.MaxDoc - 1;
        }

        private static void InitLookup(string[] allFields)
        {
            _lookup = new int[3];

            foreach (var field in ProjectInfo.Data.SelectedFields)
            {
                if (field == ProjectInfo.DateFieldKey)
                {
                    _lookup[0] = Array.IndexOf(allFields, ProjectInfo.DateFieldKey);
                }
                if (field == ProjectInfo.SenderFieldKey)
                {
                    _lookup[1] = Array.IndexOf(allFields, ProjectInfo.SenderFieldKey);
                }
                if (field == ProjectInfo.TextFieldKey)
                {
                    _lookup[2] = Array.IndexOf(allFields, ProjectInfo.TextFieldKey);
                }
            }
        }

        #endregion

        #region Extracting

        public static HashSet<ActiveDate> LoadAllActiveDates()
        {
            var dates = new HashSet<ActiveDate>();

            for (int i = 0; i < LuceneService.DirReader?.MaxDoc; ++i)
            {
                Document document = LuceneService.DirReader.Document(i);

                string dateField = ProjectInfo.DateFieldKey;
                string dateString = document.GetField(dateField).GetStringValue();

                DateTime fullDate = DateTools.StringToDate(dateString);
                DateTime shortDate = new(fullDate.Year, fullDate.Month, fullDate.Day);

                int? messageId = document.GetField(ProjectInfo.IdKey).GetInt32Value();
                var activeDate = new ActiveDate(shortDate, messageId ?? -1);

                dates.Add(activeDate);
            }

            return dates;
        }

        public static DynamicMessage GetMessage(int index)
        {
            List<string> msgData = new();
            Document? document = LuceneService.DirReader?.Document(index);

            foreach (var field in ProjectInfo.Data.SelectedFields)
            {
                msgData.Add(document?.GetField(field).GetStringValue() ?? string.Empty);
            }

            int? id = document?.GetField(ProjectInfo.IdKey).GetInt32Value();
            return new DynamicMessage(msgData, ProjectInfo.Data.SelectedFields, ProjectInfo.DateFieldKey, id ?? -1);
        }

        public static DynamicMessage GetFirstMessage()
        {
            return GetMessage(0);
        }

        public static DynamicMessage GetLastMessage()
        {
            int maxDoc = LuceneService.DirReader?.MaxDoc ?? 1;
            return GetMessage(maxDoc - 1);
        }

        public static List<DynamicMessage> LoadPreviousDocumentsFromIndex(int count)
        {
            if (_viewerReadIndex == 0)
                return new List<DynamicMessage>();

            var messages = new List<DynamicMessage>();
            int bottomEdge = Math.Max(_viewerReadIndex - count, -1);

            for (int i = _viewerReadIndex; i > bottomEdge; --i)
            {
                var msgData = new List<string>();
                Document? document = LuceneService.DirReader?.Document(i);

                foreach (var field in ProjectInfo.Data.SelectedFields)
                {
                    msgData.Add(document?.GetField(field).GetStringValue() ?? string.Empty);
                }

                int? id = document?.GetField(ProjectInfo.IdKey).GetInt32Value();
                var msg = new DynamicMessage(msgData, ProjectInfo.Data.SelectedFields, ProjectInfo.DateFieldKey, id ?? -1);

                messages.Add(msg);
            }

            _viewerReadIndex = Math.Max(bottomEdge, 0);

            messages.Reverse();
            return messages;
        }

        public static List<DynamicMessage> LoadNextDocumentsFromIndex(int count)
        {
            if (_viewerReadIndex == LuceneService.DirReader?.MaxDoc - 1)
                return new List<DynamicMessage>();

            var messages = new List<DynamicMessage>();
            int topEdge = Math.Min(_viewerReadIndex + count, LuceneService.DirReader?.MaxDoc ?? int.MaxValue);

            for (int i = _viewerReadIndex; i < topEdge; ++i)
            {
                var msgData = new List<string>();
                Document? document = LuceneService.DirReader?.Document(i);

                foreach (var field in ProjectInfo.Data.SelectedFields)
                {
                    msgData.Add(document?.GetField(field).GetStringValue() ?? string.Empty);
                }

                int? id = document?.GetField(ProjectInfo.IdKey).GetInt32Value();
                var msg = new DynamicMessage(msgData, ProjectInfo.Data.SelectedFields, ProjectInfo.DateFieldKey, id ?? -1);

                messages.Add(msg);
            }

            int maxDoc = LuceneService.DirReader?.MaxDoc ?? int.MaxValue;
            _viewerReadIndex = Math.Min(topEdge, maxDoc - 1);

            return messages;
        }

        public static List<DynamicMessage> PeekPreviousDocumentsFromIndex(int count)
        {
            int savedReadIndex = _viewerReadIndex;
            List<DynamicMessage> loadedDocuments = LoadPreviousDocumentsFromIndex(count);

            _viewerReadIndex = savedReadIndex;
            return loadedDocuments;
        }

        public static List<DynamicMessage> PeekNextDocumentsFromIndex(int count)
        {
            int savedReadIndex = _viewerReadIndex;
            List<DynamicMessage> loadedDocuments = LoadNextDocumentsFromIndex(count);

            _viewerReadIndex = savedReadIndex;
            return loadedDocuments;
        }

        #endregion

        #region Populating

        private static void PopulateUserColors()
        {
            var colors = ColorEngine.ColorGenerator.GenerateHSLuvColors(ProjectInfo.Data.UserKeys.Count, false);
            int i = 0;

            foreach (var user in ProjectInfo.Data.UserKeys)
            {
                ProjectInfo.Data.UserColors.Add(user, colors[i]);
                i++;
            }
        }

        public static int PopulateIndex(string filePath, string[] allFields, bool header)
        {
            InitLookup(allFields);

            int result = 0;
            int count = 0;

            if (_lookup == null)
                return result;

            var activeDates = new HashSet<ActiveDate>();

            using (var fileReader = new CsvReader(filePath))
            {
                DateTime date;
                string[]? row = null;
                int indexingValue = 0;

                // Read header
                if (!header)
                    fileReader.ReadRow(ref row);

                while (fileReader.ReadRow(ref row))
                {
                    count++;

                    string dateStr = row?[_lookup[0]] ?? string.Empty;
                    date = DateTime.Parse(dateStr);

                    var shortDate = new DateTime(date.Year, date.Month, date.Day);
                    activeDates.Add(new ActiveDate(shortDate, indexingValue));

                    ProjectInfo.Data.UserKeys.Add(row?[_lookup[1]] ?? string.Empty);

                    var day = date.Date;

                    if (!ProjectInfo.Data.MessagesPerDay.ContainsKey(day))
                        ProjectInfo.Data.MessagesPerDay.Add(day, 1);
                    else
                        ProjectInfo.Data.MessagesPerDay[day]++;

                    var document = new Document()
                    {
                        new Int32Field(ProjectInfo.IdKey, indexingValue, Field.Store.YES)
                    };

                    indexingValue++;

                    for (int i = 0; i < row?.Length; ++i)
                    {
                        string rowText = row[i];
                        string value = rowText.Length <= MAX_TERM_LENGTH ? rowText : string.Empty;

                        if (_lookup.Contains(i))
                        {
                            if (i == _lookup[0])
                            {
                                string temp = DateTools.DateToString(date, DateTools.Resolution.SECOND);
                                document.Add(new StringField(allFields[i], temp, Field.Store.YES));
                            }
                            if (i == _lookup[1])
                            {
                                document.Add(new StringField(allFields[i], value, Field.Store.YES));
                            }
                            if (i == _lookup[2])
                            {
                                document.Add(new TextField(allFields[i], value, Field.Store.YES));
                            }
                        }
                        else
                        {
                            if (ProjectInfo.Data.SelectedFields.Contains(allFields[i]))
                                document.Add(new StringField(allFields[i], value, Field.Store.YES));
                        }
                    }

                    LuceneService.Writer?.AddDocument(document);

                    if (count % PERIOD_OF_SAVING_INTERMEDIATE_RESULTS_OF_POPULATING_INDEX == 0)
                        LuceneService.Writer?.Commit();
                }

                LuceneService.Writer?.Commit();
                LuceneService.Writer?.Flush(triggerMerge: false, applyAllDeletes: false);

                ProjectInfo.Data.LineCount = count;
                ProjectInfo.Data.ActiveDates = activeDates;

                PrepareDirectory();
                PopulateUserColors();
                SaveInfoToDisk();
                SaveFieldsToDisk();
                SaveUsersToDisk();
                SaveStatsToDisk();

                result = 1;
                return result;
            }
        }

        #endregion
    }
}
