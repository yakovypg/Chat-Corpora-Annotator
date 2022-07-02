using IndexEngine.Data.Paths;
using Newtonsoft.Json;

namespace IndexEngine.Indexes
{
    public class SituationIndex : INestedIndex<string, Dictionary<int, List<int>>, int, List<int>>
    {
        private static readonly Lazy<SituationIndex> _lazy = new(() => new SituationIndex());

        public int ItemCount => IndexCollection.Sum(t => t.Value.Count);

        public IDictionary<string, Dictionary<int, List<int>>> IndexCollection { get; private set; }
        public IDictionary<int, Dictionary<string, int>> InvertedIndex { get; private set; }

        private SituationIndex()
        {
            IndexCollection = new Dictionary<string, Dictionary<int, List<int>>>();
            InvertedIndex = new Dictionary<int, Dictionary<string, int>>();
        }

        public static SituationIndex GetInstance()
        {
            return _lazy.Value;
        }

        public void InitializeIndex(List<string> list)
        {
            foreach (var str in list)
            {
                IndexCollection.Add(str, new Dictionary<int, List<int>>());
            }
        }

        public void AddIndexEntry(string key, Dictionary<int, List<int>> value)
        {
            if (!IndexCollection.ContainsKey(key))
            {
                IndexCollection.Add(key, value);
            }
            else
            {
                foreach (var kvp in value)
                {
                    AddInnerIndexEntry(key, kvp.Key, kvp.Value);
                }
            }

            foreach (var kvp in value)
            {
                AddInvertedIndexEntry(key, kvp.Key, kvp.Value);
            }
        }

        public void AddInnerIndexEntry(string key, int sid, List<int> messages)
        {
            if (IndexCollection.ContainsKey(key))
            {
                IndexCollection[key].Add(sid, messages);
            }
            else
            {
                var msgs = new Dictionary<int, List<int>>()
                {
                    { sid, messages }
                };

                IndexCollection.Add(key, msgs);
            }

            AddInvertedIndexEntry(key, sid, messages);
        }

        private void AddInvertedIndexEntry(string key, int sid, List<int> messages)
        {
            foreach (var id in messages)
            {
                if (!InvertedIndex.ContainsKey(id))
                {
                    InvertedIndex.Add(id, new Dictionary<string, int>());
                    InvertedIndex[id].Add(key, sid);
                }
                else
                {
                    if (!InvertedIndex[id].ContainsKey(key))
                        InvertedIndex[id].Add(key, sid);
                }
            }
        }

        public bool DeleteIndexEntry(string key)
        {
            if (!IndexCollection.Remove(key))
                return false;

            foreach (var kvp in InvertedIndex)
            {
                if (kvp.Value.ContainsKey(key))
                    kvp.Value.Remove(key);
            }

            return true;
        }

        public bool DeleteInnerIndexEntry(string key, int sid)
        {
            if (!IndexCollection[key].Remove(sid))
                return false;

            foreach (var kvp in InvertedIndex)
            {
                if (kvp.Value.ContainsKey(key))
                {
                    if (kvp.Value[key] == sid)
                        kvp.Value.Remove(key);
                }
            }

            return true;
        }

        public void UpdateIndexEntry(string key, Dictionary<int, List<int>> value)
        {
            IndexCollection[key] = value;
        }

        public void CrossMergeItems(string key1, int id1, string key2, int id2)
        {
            AddInvertedIndexEntry(key2, id2, IndexCollection[key1][id1]);
            AddInvertedIndexEntry(key1, id1, IndexCollection[key2][id2]);
        }

        public void MergeItems(string key1, int id1, string key2, int id2)
        {
            List<int> firstSitMsgIds = new(IndexCollection[key1][id1]);
            List<int> secondSitMsgIds = IndexCollection[key2][id2];

            foreach (int id in firstSitMsgIds.ToArray())
            {
                if (InvertedIndex[id].ContainsKey(key2))
                    firstSitMsgIds.Remove(id);
            }

            secondSitMsgIds.AddRange(firstSitMsgIds);

            int[] distinctedIds = secondSitMsgIds.Distinct().ToArray();

            if (secondSitMsgIds.Count != distinctedIds.Length)
            {
                secondSitMsgIds.Clear();
                secondSitMsgIds.AddRange(distinctedIds);
            }

            secondSitMsgIds.Sort();

            DeleteInnerIndexEntry(key1, id1);
            AddInvertedIndexEntry(key2, id2, firstSitMsgIds);
        }

        public void DeleteMessageFromSituationAndIndex(string tag, int id, int messageid)
        {
            if (IndexCollection.ContainsKey(tag) && IndexCollection[tag].ContainsKey(id))
                IndexCollection[tag][id].Remove(messageid);

            if (InvertedIndex.ContainsKey(messageid) && InvertedIndex[messageid].ContainsKey(tag))
                InvertedIndex[messageid].Remove(tag);
        }

        public int GetValueCount(string key)
        {
            return IndexCollection.ContainsKey(key)
                ? IndexCollection[key].Count
                : 0;
        }

        public int GetInnerValueCount(string key, int inkey)
        {
            if (!IndexCollection.ContainsKey(key) || !IndexCollection[key].ContainsKey(inkey))
                return -1;

            int count = IndexCollection[key][inkey].Count;
            return count;
        }

        public void UnloadData()
        {
            IndexCollection.Clear();
            InvertedIndex.Clear();
        }

        public bool CheckDirectory()
        {
            return Directory.Exists(ProjectInfo.InfoPath);
        }

        public bool CheckFiles()
        {
            return File.Exists(ProjectInfo.SituationsPath) && File.Exists(ProjectInfo.SavedTagsPathTemp);
        }

        public void FlushIndexToDisk()
        {
            var json = JsonConvert.SerializeObject(IndexCollection);
            File.WriteAllText(ProjectInfo.SituationsPath, json);

            json = JsonConvert.SerializeObject(InvertedIndex);
            File.WriteAllText(ProjectInfo.SavedTagsPathTemp, json);
        }

        public void ReadIndexFromDisk()
        {
            if (!CheckFiles())
                return;

            string indexJson = File.ReadAllText(ProjectInfo.SituationsPath);
            string invIndexJson = File.ReadAllText(ProjectInfo.SavedTagsPathTemp);

            InvertedIndex = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<string, int>>>(invIndexJson)
                ?? new Dictionary<int, Dictionary<string, int>>();

            IndexCollection = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<int, List<int>>>>(indexJson)
                ?? new Dictionary<string, Dictionary<int, List<int>>>();
        }
    }
}
