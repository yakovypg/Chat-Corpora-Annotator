using IndexEngine.Containers;
using IndexEngine.Indexes;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace IndexEngine.Data.Paths
{
    public static class ProjectInfo
    {
        public static void LoadProject(string path)
        {
            UnloadData();

            var project = new Project(path);
            SetPaths(project);

            var list = IndexHelper.LoadInfoFromDisk(KeyPath);
            SetKeys(list["DateFieldKey"], list["SenderFieldKey"], list["TextFieldKey"]);

            Data.LineCount = int.Parse(list["LineCount"]);
            Data.MessagesPerDay = IndexHelper.LoadStatsFromDisk(StatsPath);
            Data.UserKeys = IndexHelper.LoadUsersFromDisk(UsersPath);
            Data.SelectedFields = IndexHelper.LoadFieldsFromDisk(FieldsPath);

            TryUpdateTagset();
        }

        public static void CreateNewProject(string path, string date, string sender, string text, List<string> fields)
        {
            UnloadData();

            var project = Project.Create(path);

            SetPaths(project);
            SetKeys(date, sender, text);
            SetSelectedFields(fields);
        }

        public static void UnloadData()
        {
            Data.MessagesPerDay.Clear();
            Data.UserColors.Clear();
            Data.UserKeys.Clear();
            Data.SelectedFields = new List<string>();
            Data.LineCount = 0;

            IndexHelper.UnloadData();

            foreach (PropertyInfo prop in typeof(ProjectInfo).GetProperties())
            {
                if (prop.PropertyType.Name == "String")
                    prop.SetValue(prop, "");
            }
        }

        public static bool TryUpdateTagset(string tagset = null)
        {
            Tagset = tagset ?? (TagsetSet ? File.ReadAllText(TagsetPath) : null);

            if (!TagsetIndex.GetInstance().IndexCollection.ContainsKey(Tagset))
            {
                try
                {
                    File.Delete(TagsetPath);
                    Tagset = null;
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        private static void SetSelectedFields(List<string> fields)
        {
            Data.SelectedFields = fields;
        }

        private static void SetPaths(Project project)
        {
            IndexPath = project.WorkingDirectory;
            Name = project.Name;

            project.Paths.TryGetValue("Info", out string infoFolder);
            InfoPath = Path.Combine(IndexPath, infoFolder) + @"\";

            KeyPath = Path.Combine(InfoPath, Name + "-info.txt");
            FieldsPath = Path.Combine(InfoPath, Name + "-fields.txt");
            UsersPath = Path.Combine(InfoPath, Name + "-users.txt");
            StatsPath = Path.Combine(InfoPath, Name + "-stats.txt");

            SavedTagsPath = Path.Combine(InfoPath, Name + "-savedtags.txt");
            SavedTagsPathTemp = Path.Combine(InfoPath, Name + "-savedtagsnew.txt");
            TagCountsPath = Path.Combine(InfoPath, Name + "-tagcounts.txt");
            TagsetPath = Path.Combine(InfoPath, Name + "-tagset.txt");
            SituationsPath = Path.Combine(InfoPath, Name + "-situations.txt");
            ActiveDatesPath = Path.Combine(InfoPath, Name + "-activedates.txt");
            OutputXmlFilePath = Path.Combine(InfoPath, "outputXml.xml");
            OutputCsvFilePath = Path.Combine(InfoPath, "outputCsv.csv");
            ExtractedDataPath = Path.Combine(InfoPath, "extractedData.txt");
        }

        private static void SetKeys(string date, string sender, string text)
        {
            IdKey = "id";
            DateFieldKey = date;
            SenderFieldKey = sender;
            TextFieldKey = text;
        }

        public static string Name { get; private set; }
        public static ProjectData Data { get; private set; } = new ProjectData();
        public static string IndexPath { get; private set; }
        public static string InfoPath { get; private set; }
        public static string KeyPath { get; private set; }
        public static string FieldsPath { get; private set; }
        public static string UsersPath { get; private set; }
        public static string StatsPath { get; private set; }
        public static string DateFieldKey { get; private set; }
        public static string TextFieldKey { get; private set; }
        public static string SenderFieldKey { get; private set; }
        public static string IdKey { get; private set; }

        public static string SituationsPath { get; private set; }
        public static string TagCountsPath { get; private set; }
        public static string SavedTagsPath { get; private set; }
        public static string SavedTagsPathTemp { get; private set; }
        public static string TagsetPath { get; private set; }
        public static string ActiveDatesPath { get; private set; }
        public static string OutputXmlFilePath { get; private set; }
        public static string OutputCsvFilePath { get; private set; }
        public static string ExtractedDataPath { get; private set; }

        public static string Tagset { get; private set; }
        public static bool TagsetSet { get { return File.Exists(TagsetPath); } }
    }
}
