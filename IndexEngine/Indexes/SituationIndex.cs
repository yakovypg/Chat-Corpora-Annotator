using IndexEngine.Paths;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.ComponentModel;

namespace IndexEngine.Indexes
{
    public class SituationIndex : INestedIndex<string, Dictionary<int, List<int>>, int, List<int>>
    {
        private static readonly Lazy<SituationIndex> lazy = new Lazy<SituationIndex>(() => new SituationIndex());

        public static SituationIndex GetInstance()
        {
            return lazy.Value;
        }

        private SituationIndex() { }

        public IDictionary<string, Dictionary<int, List<int>>> IndexCollection { get; private set; } = new Dictionary<string, Dictionary<int, List<int>>>();

        public IDictionary<int, Dictionary<string, int>> InvertedIndex { get; private set; } = new Dictionary<int, Dictionary<string, int>>();

        
        public int ItemCount
        {
            get
            {
                int count = 0;
                foreach (var kvp in IndexCollection)
                {
                    count += kvp.Value.Count;
                }
                return count;
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
                foreach(var kvp in value)
                {
                    AddInnerIndexEntry(key, kvp.Key, kvp.Value);
                }
            }
            foreach(var kvp in value)
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
                IndexCollection.Add(key, new Dictionary<int, List<int>>());
                IndexCollection[key].Add(sid, messages);
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
                    InvertedIndex[id].Add(key, sid);
                }
            }
        }

        public void CrossMergeItems(string key1, int id1, string key2, int id2)
        {

            AddInvertedIndexEntry(key2, id2, IndexCollection[key1][id1]);
            AddInvertedIndexEntry(key1, id1, IndexCollection[key2][id2]);
            
        }

        public void DeleteIndexEntry(string key)
        {
            IndexCollection.Remove(key);
            foreach(var kvp in InvertedIndex)
            {
                if (kvp.Value.ContainsKey(key))
                {
                    kvp.Value.Remove(key);
                }
            }

        }

        public void DeleteInnerIndexEntry(string key, int sid)
        {
            IndexCollection[key].Remove(sid);
            foreach(var kvp in InvertedIndex)
            {
                if (kvp.Value.ContainsKey(key)){
                    if(kvp.Value[key] == sid)
                    {
                        kvp.Value.Remove(key);
                    }
                }
            }
        }


        public void FlushIndexToDisk()
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(IndexCollection);
            File.WriteAllText(ProjectInfo.SituationsPath, json);

            json = JsonConvert.SerializeObject(InvertedIndex);
            File.WriteAllText(ProjectInfo.SavedTagsPathTemp, json);
        }

        public  void ReadIndexFromDisk()
        {
            if (CheckFiles())
            {
                InvertedIndex = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<string, int>>>(File.ReadAllText(ProjectInfo.SavedTagsPathTemp));

                IndexCollection = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<int, List<int>>>>(File.ReadAllText(ProjectInfo.SituationsPath));


            }
        }

        public  void UnloadData()
        {
            IndexCollection.Clear();
            InvertedIndex.Clear();
        }

        public void UpdateIndexEntry(string key, Dictionary<int, List<int>> value)
        {
            throw new System.NotImplementedException();
        }

        public bool CheckDirectory()
        {
            if (Directory.Exists(ProjectInfo.InfoPath))
            {
                return true;
            }
            else return false;
        }

        public bool CheckFiles()
        {
            if(File.Exists(ProjectInfo.SituationsPath) && File.Exists(ProjectInfo.SavedTagsPathTemp))
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        public  void InitializeIndex(List<string> list)
        {
            foreach(var str in list)
            {
                IndexCollection.Add(str, new Dictionary<int, List<int>>());
            }
        }

        public void DeleteMessageFromSituation(string tag, int id, int messageid) 
        {
            if (IndexCollection.ContainsKey(tag))
            {
                if (IndexCollection[tag].ContainsKey(id))
                {
                    IndexCollection[tag][id].Remove(messageid);
                }
            }
        }
        public void AddMessageToSituation(string tag, int id, int messageid) 
        {
            if (IndexCollection.ContainsKey(tag))
            {
                if (IndexCollection[tag].ContainsKey(id))
                {
                    if (!IndexCollection[tag][id].Contains(messageid))
                    {
                        IndexCollection[tag][id].Add(messageid);
                        IndexCollection[tag][id].Sort();
                    }
                }
            }
        }
        public  int GetValueCount(string key)
        {
            if (IndexCollection.ContainsKey(key))
            {
                return IndexCollection[key].Count;
            }
            else
            {
                return 0;
            }
        }

        public  int GetInnerValueCount(string key, int inkey)
        {
            if (IndexCollection.ContainsKey(key))
            {
                if (IndexCollection[key].ContainsKey(inkey))
                {
                    return IndexCollection[key][inkey].Count;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return - 1;
            }
        }

        public void RemakeOldIndexFile()
        {
            using (StreamReader reader = new StreamReader(ProjectInfo.SavedTagsPath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var MessageIdAndSituations = line.Split(' ');

                    var situationsSet = MessageIdAndSituations[1].Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
                    var id = int.Parse(MessageIdAndSituations[0]);
                    InvertedIndex.Add(id, new Dictionary<string, int>());
                    foreach (var s in situationsSet)
                    {
                        var item = s.Split('-');
                        InvertedIndex[id].Add(item[0], int.Parse(item[1]));
                    }

                }
            }
        }

        public void RemakeFromInverted()
        {
            List<string> list = new List<string> { "JobSearch", "FCCBug", "CodeHelp", "Meeting", "OSSelection", "SoftwareSupport" };
            InitializeIndex(list);
            foreach (var kvp in InvertedIndex)
            {
                foreach (var pair in kvp.Value)
                {
                    if (IndexCollection[pair.Key].ContainsKey(pair.Value))
                    {
                        AddMessageToSituation(pair.Key, pair.Value, kvp.Key);
                    }
                    else
                    {
                        AddInnerIndexEntry(pair.Key, pair.Value, new List<int>());
                        AddMessageToSituation(pair.Key, pair.Value, kvp.Key);
                    }
                }
            }
        }

    }
}
