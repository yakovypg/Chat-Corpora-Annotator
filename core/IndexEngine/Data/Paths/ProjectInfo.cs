using IndexEngine.Containers;
using IndexEngine.Indexes;
using System.Reflection;

namespace IndexEngine.Data.Paths
{
    public static class ProjectInfo
    {
        public static ProjectData Data { get; private set; } = new ProjectData();

        public static string Name { get; private set; } = string.Empty;
        public static string Tagset { get; private set; } = string.Empty;

        public static string IndexPath { get; private set; } = string.Empty;
        public static string InfoPath { get; private set; } = string.Empty;
        public static string KeyPath { get; private set; } = string.Empty;
        public static string FieldsPath { get; private set; } = string.Empty;
        public static string UsersPath { get; private set; } = string.Empty;
        public static string StatsPath { get; private set; } = string.Empty;
        public static string IdKey { get; private set; } = string.Empty;
        public static string DateFieldKey { get; private set; } = string.Empty;
        public static string TextFieldKey { get; private set; } = string.Empty;
        public static string SenderFieldKey { get; private set; } = string.Empty;

        public static string SituationsPath { get; private set; } = string.Empty;
        public static string TagCountsPath { get; private set; } = string.Empty;
        public static string SavedTagsPath { get; private set; } = string.Empty;
        public static string SavedTagsPathTemp { get; private set; } = string.Empty;
        public static string TagsetPath { get; private set; } = string.Empty;
        public static string ActiveDatesPath { get; private set; } = string.Empty;

        public static string OutputXmlFilePath { get; private set; } = string.Empty;
        public static string OutputCsvFilePath { get; private set; } = string.Empty;
        public static string ExtractedDataPath { get; private set; } = string.Empty;

        public static bool IsTagsetSet => File.Exists(TagsetPath);

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

        public static bool TryUpdateTagset(string? tagset = null)
        {
            Tagset = tagset ?? (IsTagsetSet ? File.ReadAllText(TagsetPath) : string.Empty);

            if (TagsetIndex.GetInstance().IndexCollection.ContainsKey(Tagset))
                return true;

            try
            {
                File.Delete(TagsetPath);
                Tagset = string.Empty;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void UnloadData()
        {
            Data.LineCount = 0;

            Data.SelectedFields.Clear();
            Data.UserKeys.Clear();
            Data.UserColors.Clear();
            Data.ActiveDates.Clear();
            Data.MessagesPerDay.Clear();

            IndexHelper.UnloadData();

            foreach (PropertyInfo prop in typeof(ProjectInfo).GetProperties())
            {
                if (prop.PropertyType.Name == "String")
                    prop.SetValue(prop, string.Empty);
            }
        }

        private static void SetPaths(Project project)
        {
            IndexPath = project.WorkingDirectory;
            Name = project.Name;

            project.Paths.TryGetValue("Info", out string? infoFolder);
            InfoPath = Path.Combine(IndexPath, infoFolder ?? string.Empty);

            KeyPath = Path.Combine(InfoPath, $"{Name}-info.txt");
            FieldsPath = Path.Combine(InfoPath, $"{Name}-fields.txt");
            UsersPath = Path.Combine(InfoPath, $"{Name}-users.txt");
            StatsPath = Path.Combine(InfoPath, $"{Name}-stats.txt");

            SavedTagsPath = Path.Combine(InfoPath, $"{Name}-savedtags.txt");
            SavedTagsPathTemp = Path.Combine(InfoPath, $"{Name}-savedtagsnew.txt");
            TagCountsPath = Path.Combine(InfoPath, $"{Name}-tagcounts.txt");
            TagsetPath = Path.Combine(InfoPath, $"{Name}-tagset.txt");
            SituationsPath = Path.Combine(InfoPath, $"{Name}-situations.txt");
            ActiveDatesPath = Path.Combine(InfoPath, $"{Name}-activedates.txt");
            OutputXmlFilePath = Path.Combine(InfoPath, $"{Name}-outputXml.xml");
            OutputCsvFilePath = Path.Combine(InfoPath, $"{Name}-outputCsv.csv");
            ExtractedDataPath = Path.Combine(InfoPath, $"{Name}-extractedData.txt");
        }

        private static void SetKeys(string date, string sender, string text)
        {
            IdKey = "id";
            DateFieldKey = date;
            SenderFieldKey = sender;
            TextFieldKey = text;
        }

        private static void SetSelectedFields(List<string> fields)
        {
            Data.SelectedFields = fields;
        }
    }
}
