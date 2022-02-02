using IndexEngine.Data.Paths;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            var jsonString = JsonConvert.SerializeObject(IndexCollection);
            File.WriteAllText(ToolInfo.UserDictsPath, jsonString);
        }

        public int GetValueCount(string key)
        {
            return IndexCollection.ContainsKey(key)
                ? IndexCollection[key].Count
                : -1;
        }

        public void ReadIndexFromDisk()
        {
            if (CheckFiles())
            {
                var jsonString = File.ReadAllText(ToolInfo.UserDictsPath);
                IndexCollection = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(jsonString);
            }
        }

        public void UnloadData()
        {
            IndexCollection.Clear();
        }

        public void RedefineData(Dictionary<string, List<string>> newIndexCollection)
        {
            IndexCollection.Clear();
            IndexCollection = newIndexCollection ?? new Dictionary<string, List<string>>();
        }

        public void UpdateIndexEntry(string key, List<string> value)
        {
            throw new NotImplementedException();
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
