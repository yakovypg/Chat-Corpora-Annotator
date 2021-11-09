using IndexEngine.Indexes;
using Lucene.Net.Documents;
using System;
using System.Collections.Generic;

namespace IndexEngine.Containers
{
    public static class MessageContainer
    {
        public static List<DynamicMessage> Messages { get; set; } = new List<DynamicMessage>();

        public static void InsertTagsInDynamicMessage(int id, int offset)
        {
            if (id <= Messages.Count + offset)
            {
                foreach (var str in SituationIndex.GetInstance().InvertedIndex[id])
                {
                    Messages[id].AddSituation(str.Key, str.Value);
                }
            }
        }

        public static void UpdateTagsInDynamicMessage(int id, int offset)
        {
            Messages[id].RemoveAllSituations();
            InsertTagsInDynamicMessage(id, offset);
        }
    }

    public class DynamicMessage
    {
        public int Id { get; set; }

        public Dictionary<string, int> Situations { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, object> Contents { get; }

        public DynamicMessage(string[] fields, string[] data)
        {
            Contents = new Dictionary<string, object>();

            for (int i = 0; i < fields.Length; i++)
            {
                Contents.Add(fields[i], data[i]);
            }
        }

        public DynamicMessage()
        {
        }

        public void AddSituation(string situation, int id)
        {
            if (!Situations.ContainsKey(situation))
                Situations.Add(situation, id);
        }

        public void RemoveAllSituations()
        {
            Situations.Clear();
        }

        public bool RemoveSituation(string situation)
        {
            return Situations.Remove(situation);
        }

        public DynamicMessage(List<string> data, List<string> selectedFields, string dateFieldKey, int id)
        {
            //this.Id = Guid.NewGuid();
            Id = id;

            if (data.Count != selectedFields.Count)
            {
                throw new Exception("Wrong array size");
            }
            else
            {
                Contents = new Dictionary<string, object>();

                for (int i = 0; i < data.Count; i++)
                {
                    if (selectedFields[i] == dateFieldKey)
                    {
                        Contents.Add(selectedFields[i], DateTools.StringToDate(data[i]));
                    }
                    else
                    {
                        Contents.Add(selectedFields[i], data[i]);
                    }
                }
            }
        }
    }
}