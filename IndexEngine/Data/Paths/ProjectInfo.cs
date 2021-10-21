using CSharpTest.Net.Collections;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace IndexEngine.Paths
{
    public class ProjectData
    {
        public BTreeDictionary<DateTime, int> MessagesPerDay { get; set; } = new BTreeDictionary<DateTime, int>();
        public HashSet<string> UserKeys { get; set; } = new HashSet<string>();
        public Dictionary<string, Color> UserColors { get; set; } = new Dictionary<string, Color>();
        public List<string> SelectedFields { get; set; } = new List<string>();
        public int LineCount { get; set; }
    }

    public static class ProjectInfo
    {
        public static void LoadProject(string path)
        {
            UnloadData();
            SetPaths(path);

            var list = IndexHelper.LoadInfoFromDisk(KeyPath);
            SetKeys(list["DateFieldKey"], list["SenderFieldKey"], list["TextFieldKey"]);

            Data.LineCount = int.Parse(list["LineCount"]);
            Data.MessagesPerDay = IndexHelper.LoadStatsFromDisk(StatsPath);
            Data.UserKeys = IndexHelper.LoadUsersFromDisk(UsersPath);
            Data.SelectedFields = IndexHelper.LoadFieldsFromDisk(FieldsPath);

            if (TagsetSet)
                Tagset = File.ReadAllText(TagsetPath);
        }

        public static void CreateNewProject(string path, string date, string sender, string text, List<string> fields)
        {
            UnloadData();
            SetPaths(path);
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

        private static void SetSelectedFields(List<string> fields)
        {
            Data.SelectedFields = fields;
        }
        private static void SetPaths(string path)
        {
            IndexPath = path;
            Name = Path.GetFileNameWithoutExtension(IndexPath);

            InfoPath = IndexPath + @"\info\";
            KeyPath = InfoPath + Name + @"-info.txt";
            FieldsPath = InfoPath + Name + @"-fields.txt";
            UsersPath = InfoPath + Name + @"-users.txt";
            StatsPath = InfoPath + Name + @"-stats.txt";

            SavedTagsPath = InfoPath + Name + @"-savedtags.txt";
            SavedTagsPathTemp = InfoPath + Name + @"-savedtagsnew.txt";
            TagCountsPath = InfoPath + Name + @"-tagcounts.txt";
            TagsetPath = InfoPath + Name + @"-tagset.txt";
            SituationsPath = InfoPath + Name + @"-situations.txt";
            ActiveDatesPath = InfoPath + Name + @"-activedates.txt";
            OutputXmlFilePath = InfoPath + @"\output.xml";
        }

        private static void SetKeys(string date, string sender, string text)
        {
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

        public static string SituationsPath { get; private set; }
        public static string TagCountsPath { get; private set; }
        public static string SavedTagsPath { get; private set; }
        public static string SavedTagsPathTemp { get; private set; }
        public static string TagsetPath { get; private set; }
        public static string ActiveDatesPath { get; private set; }
        public static string OutputXmlFilePath { get; private set; }

        public static string Tagset { get; private set; }
        public static bool TagsetSet { get { return File.Exists(TagsetPath); } }
    }
}
