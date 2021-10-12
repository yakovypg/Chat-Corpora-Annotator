﻿using IndexEngine;
using IndexEngine.Indexes;
using IndexEngine.Paths;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Services
{
    public class TaggedCollectionStatisticsService : ITaggedStatisticsService
    {
        public int WindowCount { get; private set; }

        public int AverageUsersPerSituation { get; private set; }
        public Dictionary<string, int> AverageUsersInSituationPerTag { get; private set; } = new Dictionary<string, int>();
        public Dictionary<string, int> SituationsPerTag { get; private set; } = new Dictionary<string, int>();

        public int NumberOfDocs { get; private set; }
        public long NumberOfSymbols { get; private set; }
        public ulong NumberOfTokens { get; private set; }
        public int NumberOfUsers { get; private set; }
        public double AverageLength { get; private set; }
        public double AverageMessagesPerUnit { get; private set; }

        public List<int> AllLengths => throw new NotImplementedException();
        public List<int> AllTokenNumbers => throw new NotImplementedException();
        public List<int> AllTokenLengths => throw new NotImplementedException();

        public Dictionary<string, double> AllFields { get; private set; } = new Dictionary<string, double>();

        public bool IsCalculated { get; private set; }
        public TimeSpan Duration { get; private set; }
        public int AverageWindowLength { get; private set; }
        public int IntertwinedCount { get; private set; }

        private readonly HashSet<Tuple<int, int>> intervals = new HashSet<Tuple<int, int>>();
        private readonly HashSet<Tuple<Tuple<int, int>, Tuple<int, int>>> mem = new HashSet<Tuple<Tuple<int, int>, Tuple<int, int>>>();

        public void CalculateAll()
        {
            AllFields.Clear();
            SetSimpleCounts();
            CountIntertwined();
            IsCalculated = true;
        }

        private void SetSimpleCounts()
        {
            int prev = 0; //prev id
            List<int> windows = new List<int>(); // all windows lengths;
            int count = 0; //all messages in all situations
            int symcount = 0; //all symbols in all situations
            int sitcount = SituationIndex.GetInstance().ItemCount; //the number of all situations

            HashSet<string> usersInSituation = new HashSet<string>(); //counts all unique users in a situation
            Dictionary<string, int> userPerSituationPerTagCounts = new Dictionary<string, int>(); //the sum of unique users in each tag

            foreach (var kvp in SituationIndex.GetInstance().IndexCollection)
            {
                userPerSituationPerTagCounts.Add(kvp.Key, 0);
                AverageUsersInSituationPerTag.Add(kvp.Key, 0);
            }

            foreach (var kvp in SituationIndex.GetInstance().IndexCollection)
            {
                foreach (var dict in kvp.Value)
                {
                    count += dict.Value.Count;
                    prev = dict.Value[0];
                    foreach (var index in dict.Value)
                    {
                        if (index - prev > 1)
                            windows.Add(index - prev);

                        prev = index;
                        symcount += LuceneService.DirReader.Document(index).GetField(ProjectInfo.TextFieldKey).GetStringValue().Length;
                        usersInSituation.Add(LuceneService.DirReader.Document(index).GetField(ProjectInfo.SenderFieldKey).GetStringValue());
                    }

                    userPerSituationPerTagCounts[kvp.Key] += usersInSituation.Count;
                    usersInSituation.Clear();
                }
            }

            NumberOfDocs = count;
            AverageMessagesPerUnit = count / sitcount;
            NumberOfSymbols = symcount;
            AverageLength = symcount / count;
            AverageWindowLength = windows.Sum() / windows.Count;
            AverageUsersPerSituation = userPerSituationPerTagCounts.Values.Sum() / sitcount;

            foreach (var kvp in userPerSituationPerTagCounts)
            {
                AverageUsersInSituationPerTag[kvp.Key] = kvp.Value / SituationIndex.GetInstance().GetValueCount(kvp.Key);
                SituationsPerTag.Add(kvp.Key, SituationIndex.GetInstance().GetValueCount(kvp.Key));
                AllFields.Add("Average number of users in " + kvp.Key, AverageUsersInSituationPerTag[kvp.Key]);
                AllFields.Add(kvp.Key, SituationIndex.GetInstance().GetValueCount(kvp.Key));
            }

            AllFields.Add("Number of tagged messages", NumberOfDocs);
            AllFields.Add("Number of symbols", NumberOfSymbols);
            AllFields.Add("Average number of messages per situation", AverageMessagesPerUnit);
            AllFields.Add("Average length of a tagged message", AverageLength);
            AllFields.Add("Average window length", AverageWindowLength);
            AllFields.Add("Number of windows", windows.Count);

        }
        private void CountIntertwined()
        {
            int count = 0;

            if (!IsCalculated)
            {
                Tuple<int, int> tuple;

                foreach (var kvp in SituationIndex.GetInstance().IndexCollection)
                {
                    foreach (var dict in kvp.Value)
                    {
                        tuple = new Tuple<int, int>(dict.Value.First(), dict.Value.Last());
                        intervals.Add(tuple);
                    }
                }
            }

            foreach (var tup in intervals)
            {
                foreach (var tup2 in intervals)
                {
                    if (!mem.Contains(new Tuple<Tuple<int, int>, Tuple<int, int>>(tup, tup2)))
                    {
                        if (!(tup.Item1 == tup2.Item1 && tup.Item2 == tup2.Item2))
                        {
                            if (tup.Item1 <= tup2.Item2 && tup2.Item1 <= tup.Item2)
                                count++;

                            mem.Add(new Tuple<Tuple<int, int>, Tuple<int, int>>(tup, tup2));
                        }
                    }
                }
            }

            IntertwinedCount = count;
            AllFields.Add("Intertwined situations", count);
        }
    }
}

