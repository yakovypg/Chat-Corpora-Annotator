using IndexEngine.Data.Paths;
using Newtonsoft.Json;

namespace IndexEngine.Indexes
{
    public class UserDictsIndex : IIndex<string, List<string>>
    {
        private static readonly Lazy<UserDictsIndex> lazy = new Lazy<UserDictsIndex>(() => new UserDictsIndex());

        public static UserDictsIndex GetInstance()
        {
            return lazy.Value;
        }

        private UserDictsIndex()
        {
        }

        public IDictionary<string, List<string>> IndexCollection { get; private set; } = new Dictionary<string, List<string>>();
        public int ItemCount { get { return IndexCollection.Count; } }

        public bool AddWordToIndexEntry(string key, string word)
        {
            if (!IndexCollection.TryGetValue(key, out List<string> wordList))
                return false;

            wordList.Add(word);
            return true;
        }

        public bool RemoveWordFromIndexEntry(string key, string word)
        {
            return IndexCollection.TryGetValue(key, out List<string> wordList) &&
                wordList.Remove(word);
        }

        public void AddIndexEntry(string key, List<string> value)
        {
            if (!IndexCollection.ContainsKey(key))
                IndexCollection.Add(key, value);
        }

        public bool CheckDirectory()
        {
            return Directory.Exists(ProjectInfo.InfoPath);
        }

        public bool CheckFiles()
        {
            return File.Exists(ToolInfo.UserDictsPath);
        }

        public void DeleteIndexEntry(string key)
        {
            if (IndexCollection.ContainsKey(key))
                IndexCollection.Remove(key);
        }

        public void FlushIndexToDisk()
        {
            ExportIndex(ToolInfo.UserDictsPath);
        }

        public int GetValueCount(string key)
        {
            return IndexCollection.ContainsKey(key)
                ? IndexCollection[key].Count
                : -1;
        }

        public void ReadIndexFromDisk()
        {
            if (!CheckFiles())
                return;

            ImportIndex(ToolInfo.UserDictsPath);
        }

        public void ImportIndex(string path)
        {
            var jsonString = File.ReadAllText(path);

            IndexCollection = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(jsonString)
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

        public void UpdateIndexEntry(string key, List<string> value)
        {
            if (!IndexCollection.ContainsKey(key))
                AddIndexEntry(key, value);
            else
                IndexCollection[key] = value;
        }

        public string ImportNewDictFromFile(string path)
        {
            if (File.Exists(path))
            {
                var arr = File.ReadAllLines(path);
                var value = arr.Skip(1);

                AddIndexEntry(arr[0], value.ToList());
                return arr[0];
            }
            else
            {
                return null;
            }
        }
    }
}
