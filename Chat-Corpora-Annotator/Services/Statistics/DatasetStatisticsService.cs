using IndexEngine.Data.Paths;
using IndexEngine.Search;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using System;

namespace ChatCorporaAnnotator.Services.Statistics
{
    internal class DatasetStatisticsService : StatisticsService
    {
        public DatasetStatisticsService()
        {
            MaxProgressValue = LuceneService.DirReader.MaxDoc * 2 + 5;
        }

        public override void CalculateAll()
        {
            if (IsCalculated)
                return;

            AllFields.Clear();
            CurrentProgressValue = 0;

            SetDuration();
            SetMessageNumber();
            SetUserNumber();
            SetLengthData();
            SetAvgLength();
            SetAverageMessagesPerDay();
            SetTokenNumber();

            IsCalculated = true;
            IncreaseProgressValueToMax();
        }

        private void SetDuration()
        {
            var test = LuceneService.DirReader.Document(LuceneService.DirReader.MaxDoc - 1).GetField(ProjectInfo.DateFieldKey).GetStringValue();
            var end = DateTools.StringToDate(test);
            var start = DateTools.StringToDate(LuceneService.DirReader.Document(0).GetField(ProjectInfo.DateFieldKey).GetStringValue());

            Duration = end - start;
            AllFields.Add("Duration in days", Math.Ceiling(Duration.TotalDays));

            IncreaseProgressValue();
        }

        private void SetMessageNumber()
        {
            NumberOfDocs = LuceneService.DirReader.NumDocs;
            AllFields.Add("Number of messages", NumberOfDocs);

            IncreaseProgressValue();
        }

        private void SetUserNumber()
        {
            NumberOfUsers = ProjectInfo.Data.UserKeys.Count;
            AllFields.Add("Number of users", NumberOfUsers);

            IncreaseProgressValue();
        }

        private void SetLengthData()
        {
            long length = 0;

            for (int i = 0; i < NumberOfDocs; i++)
            {
                var temp = LuceneService.DirReader.Document(i).GetField(ProjectInfo.TextFieldKey).GetStringValue().Length;
                length += temp;
                AllLengths.Add(temp);

                IncreaseProgressValue();
            }

            NumberOfSymbols = length;
            AllFields.Add("Number of symbols in corpus", NumberOfSymbols);
        }

        private void SetAvgLength()
        {
            AverageLength = SafeDivide(NumberOfSymbols, NumberOfDocs);
            AllFields.Add("Average length of a message", AverageLength);

            IncreaseProgressValue();
        }

        private void SetAverageMessagesPerDay()
        {
            AverageMessagesPerUnit = SafeDivide(NumberOfDocs, ProjectInfo.Data.MessagesPerDay.Keys.Count);
            AllFields.Add("Average number of messages per day", AverageMessagesPerUnit);

            IncreaseProgressValue();
        }

        private void SetTokenNumber()
        {
            CollectionStatistics stat = LuceneService.Searcher.CollectionStatistics(ProjectInfo.TextFieldKey);
            NumberOfTokens = (ulong)stat.SumTotalTermFreq;

            for (int i = 0; i < LuceneService.DirReader.MaxDoc; i++)
            {
                LuceneService.GetTokenDataForDoc(LuceneService.DirReader.Document(i).GetField(ProjectInfo.TextFieldKey).GetStringValue());
                IncreaseProgressValue();
            }

            AllFields.Add("Tokens", NumberOfTokens);
        }
    }
}
