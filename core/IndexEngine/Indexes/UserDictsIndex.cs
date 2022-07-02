using IndexEngine.Data.Paths;
using Newtonsoft.Json;

namespace IndexEngine.Indexes
{
    public class UserDictsIndex : IIndex<string, List<string>>
    {
        private static readonly Lazy<UserDictsIndex> lazy = new(() => new UserDictsIndex());

        public int ItemCount => IndexCollection.Count;
        public IDictionary<string, List<string>> IndexCollection { get; private set; }

        private UserDictsIndex()
        {
            IndexCollection = new Dictionary<string, List<string>>();
        }

        public static UserDictsIndex GetInstance()
        {
            return lazy.Value;
        }

        public bool AddWordToIndexEntry(string key, string word)
        {
            if (!IndexCollection.TryGetValue(key, out List<string>? wordList))
                return false;

            wordList.Add(word);
            return true;
        }

        public bool RemoveWordFromIndexEntry(string key, string word)
        {
            return IndexCollection.TryGetValue(key, out List<string>? wordList) &&
                wordList.Remove(word);
        }

        public void AddIndexEntry(string key, List<string> value)
        {
            IndexCollection.Add(key, value);
        }

        public bool DeleteIndexEntry(string key)
        {
            return IndexCollection.Remove(key);
        }

        public void UpdateIndexEntry(string key, List<string> value)
        {
            if (!IndexCollection.ContainsKey(key))
                AddIndexEntry(key, value);
            else
                IndexCollection[key] = value;
        }

        public int GetValueCount(string key)
        {
            return IndexCollection.ContainsKey(key)
                ? IndexCollection[key].Count
                : -1;
        }

        public void ImportIndex(string path)
        {
            var json = File.ReadAllText(path);

            IndexCollection = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json)
                ?? new Dictionary<string, List<string>>();
        }

        public void ExportIndex(string path)
        {
            var jsonString = JsonConvert.SerializeObject(IndexCollection);
            File.WriteAllText(path, jsonString);
        }

        public bool TryImportIndex(string path)
        {
            try
            {
                ImportIndex(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryExportIndex(string path)
        {
            try
            {
                ExportIndex(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void UnloadData()
        {
            IndexCollection.Clear();
        }

        public bool CheckDirectory()
        {
            return Directory.Exists(ProjectInfo.InfoPath);
        }

        public bool CheckFiles()
        {
            return File.Exists(ToolInfo.UserDictsPath);
        }

        public void FlushIndexToDisk()
        {
            ExportIndex(ToolInfo.UserDictsPath);
        }

        public void ReadIndexFromDisk()
        {
            if (!CheckFiles())
                return;

            ImportIndex(ToolInfo.UserDictsPath);
        }
    }
}
