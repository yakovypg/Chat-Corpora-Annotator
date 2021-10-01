using IndexEngine.Indexes;
using Lucene.Net.Documents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IndexEngine
{
    public static class MessageContainer
    {

        public static List<DynamicMessage> Messages
        { get; set; } = new List<DynamicMessage>();
        public static void InsertTagsInDynamicMessage(int id, int offset)
        {

            if (id <= Messages.Count + offset)
            {
                foreach (var str in SituationIndex.GetInstance().InvertedIndex[id])
                {
                    if (!Messages[id].Situations.ContainsKey(str.Key))
                    {
                        Messages[id].Situations.Add(str.Key, str.Value);

                    }
                }
            }
        }

    }

        public class DynamicMessage
        {
            public int Id { get; set; }

            public Dictionary<string, int> Situations { get; set; } = new Dictionary<string, int>();

            private Dictionary<string, object> contents;

            public Dictionary<string, object> Contents { get { return contents; } }

            public DynamicMessage(string[] fields, string[] data)
            {

                contents = new Dictionary<string, object>();
                for (int i = 0; i < fields.Length; i++)
                {
                    contents.Add(fields[i], data[i]);
                }

            }
            public DynamicMessage()
            {


            }


            public void AddSituation(string situation, int id)
            {
                if (!Situations.ContainsKey(situation))
                {
                    this.Situations.Add(situation, id);
                }
            }

            public bool RemoveSituation(string situation)
            {
                return Situations.Remove(situation);
            }

            public DynamicMessage(List<string> data, List<string> selectedFields, string dateFieldKey, int id)
            {
                //this.Id = Guid.NewGuid();
                this.Id = id;

                if (data.Count != selectedFields.Count)
                {
                    throw new Exception("Wrong array size");

                }
                else
                {
                    this.contents = new Dictionary<string, object>();
                    for (int i = 0; i < data.Count; i++)
                    {
                        if (selectedFields[i] == dateFieldKey)
                        {
                            contents.Add(selectedFields[i], DateTools.StringToDate(data[i]));

                        }
                        else
                        {
                            contents.Add(selectedFields[i], data[i]);
                        }
                    }
                }
            }

        }
    }





