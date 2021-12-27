using ChatCorporaAnnotator.Infrastructure.Enums;
using ChatCorporaAnnotator.Services.Analysers.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatCorporaAnnotator.Models.Statistics
{
    internal class StatisticsCalculator : ICalculator
    {
        public delegate void SuccessfulCalculatingHandler();
        public event SuccessfulCalculatingHandler SuccessfulCalculating;

        public delegate void FailedCalculatingHandler(Exception ex);
        public event FailedCalculatingHandler FailedCalculating;

        public delegate void ProgressChangedHandler(long delta, long currValue);
        public event ProgressChangedHandler ProgressChanged;

        public long CurrentProgressValue { get; protected set; }
        public long MaxProgressValue { get; protected set; }

        public OperationState CalculatingState { get; protected set; }

        public IEnumerable<StatisticsItem> CorpusStatistics { get; protected set; }
        public IEnumerable<StatisticsItem> DatasetStatistics { get; protected set; }

        public StatisticsCalculator()
        {
            CurrentProgressValue = 0;
            MaxProgressValue = GetMaxProgressValue();

            CalculatingState = OperationState.NotStarted;

            CorpusStatistics = new StatisticsItem[0];
            DatasetStatistics = new StatisticsItem[0];
        }

        public virtual async Task CalculateAsync()
        {
            if (CalculatingState == OperationState.InProcess)
                return;

            CurrentProgressValue = 0;
            CalculatingState = OperationState.InProcess;

            var corpusStatsService = new CorpusStatisticsService() { ProgressChangedEventInterval = 100 };
            var datasetStatsService = new DatasetStatisticsService() { ProgressChangedEventInterval = 100 };

            corpusStatsService.ProgressChanged += (delta, currValue) => IncreaseProgressValue(delta);
            datasetStatsService.ProgressChanged += (delta, currValue) => IncreaseProgressValue(delta);

            await Task.Run(delegate
            {
                try
                {
                    corpusStatsService.CalculateAll();
                    datasetStatsService.CalculateAll();

                    CorpusStatistics = ConvertToStatisticsItems(corpusStatsService.AllFields);
                    DatasetStatistics = ConvertToStatisticsItems(datasetStatsService.AllFields);

                    CalculatingState = OperationState.Success;
                    SuccessfulCalculating?.Invoke();
                }
                catch (Exception ex)
                {
                    CalculatingState = OperationState.Fail;
                    FailedCalculating?.Invoke(ex);
                }
            });
        }

        protected IEnumerable<StatisticsItem> ConvertToStatisticsItems(Dictionary<string, double> stats)
        {
            return stats?.Select(t => new StatisticsItem(t.Key, t.Value)).ToArray();
        }

        protected void IncreaseProgressValue(long delta)
        {
            CurrentProgressValue += delta;
            ProgressChanged?.Invoke(delta, CurrentProgressValue);
        }

        private long GetMaxProgressValue()
        {
            var corpusStatsService = new CorpusStatisticsService();
            var datasetStatsService = new DatasetStatisticsService();

            return corpusStatsService.MaxProgressValue + datasetStatsService.MaxProgressValue;
        }
    }
}
