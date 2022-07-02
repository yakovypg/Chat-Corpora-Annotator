using ColorEngine;
using IndexEngine.Data.Paths;
using Newtonsoft.Json;
using System.Drawing;

namespace IndexEngine.Indexes
{
    public class TagsetIndex : INestedIndex<string, Dictionary<string, Color>, string, Color>
    {
        public const string DEFAILT_TAGSET_NAME = "Default";
        public const string NOT_SELECTED_TAGSET_NAME = "Not selected";

        private static readonly Lazy<TagsetIndex> _lazy = new(() => new TagsetIndex());

        public int ItemCount => IndexCollection.Count;
        public IDictionary<string, Dictionary<string, Color>> IndexCollection { get; private set; }

        private TagsetIndex()
        {
            IndexCollection = new Dictionary<string, Dictionary<string, Color>>();

            if (File.Exists(ToolInfo.TagsetColorIndexPath))
                ReadIndexFromDisk();

            if (!IndexCollection.ContainsKey(NOT_SELECTED_TAGSET_NAME))
                AddIndexEntry(NOT_SELECTED_TAGSET_NAME, new Dictionary<string, Color>());

            if (IndexCollection.Count == 1)
                AddDefaultTagset();
        }

        public static TagsetIndex GetInstance()
        {
            return _lazy.Value;
        }

        private void AddDefaultTagset()
        {
            var names = new List<string>
            {
                "JobSearch",
                "CodeHelp",
                "FCCBug",
                "SoftwareSupport",
                "OSSelection",
                "Meeting"
            };

            AddIndexEntry(DEFAILT_TAGSET_NAME, new Dictionary<string, Color>());

            foreach (var tag in names)
            {
                var color = ColorGenerator.GenerateHSLuvColor();
                AddInnerIndexEntry(DEFAILT_TAGSET_NAME, tag, color);
            }
        }

        public bool CheckFiles()
        {
            return File.Exists(ToolInfo.TagsetColorIndexPath);
        }

        public bool CheckDirectory()
        {
            return Directory.Exists(ToolInfo.ModelsRootDirectory);
        }

        public void ReadIndexFromDisk()
        {
            if (!CheckFiles())
                return;

            var jsonString = File.ReadAllText(ToolInfo.TagsetColorIndexPath);

            IndexCollection = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Color>>>(jsonString)
                ?? new Dictionary<string, Dictionary<string, Color>>();
        }

        public void FlushIndexToDisk()
        {
            var jsonString = JsonConvert.SerializeObject(IndexCollection);
            File.WriteAllText(ToolInfo.TagsetColorIndexPath, jsonString);
        }

        public void AddIndexEntry(string key, Dictionary<string, Color>? value)
        {
            IndexCollection.Add(key, value ?? new Dictionary<string, Color>());
        }

        public void AddInnerIndexEntry(string key, string inkey, Color invalue)
        {
            IndexCollection[key].TryAdd(inkey, invalue);
        }

        public void AddInnerIndexEntry(string key, string inkey)
        {
            AddInnerIndexEntry(key, inkey, ColorGenerator.GenerateHSLuvColor());
        }

        public bool DeleteIndexEntry(string key)
        {
            return IndexCollection.Remove(key);
        }

        public bool DeleteInnerIndexEntry(string key, string inkey)
        {
            return IndexCollection.ContainsKey(key) &&
                   IndexCollection[key].Remove(inkey);
        }

        public void InitializeIndex(List<string> list)
        {
            foreach (var tag in list)
                AddIndexEntry(tag, null);
        }

        public void UpdateIndexEntry(string key, Dictionary<string, Color> value)
        {
            IndexCollection[key] = value;
        }

        public int GetValueCount(string key)
        {
            return IndexCollection.ContainsKey(key)
                ? IndexCollection[key].Count
                : -1;
        }

        public int GetInnerValueCount(string key, string inkey)
        {
            return IndexCollection.ContainsKey(key)
                ? IndexCollection[key].Count
                : -1;
        }

        public void UnloadData()
        {
            IndexCollection.Clear();
        }
    }
}