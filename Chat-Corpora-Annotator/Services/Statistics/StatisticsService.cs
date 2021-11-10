using System;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Services.Statistics
{
    internal abstract class StatisticsService : IStatisticsService
    {
        public delegate void ProgressChangedHandler(long delta, long currValue);
        public event ProgressChangedHandler ProgressChanged;

        public uint ProgressChangedEventInterval { get; set; }

        public long CurrentProgressValue { get; protected set; }
        public long MaxProgressValue { get; protected set; }

        public bool IsCalculated { get; protected set; }
        public TimeSpan Duration { get; protected set; }

        public int NumberOfDocs { get; protected set; }
        public long NumberOfSymbols { get; protected set; }
        public ulong NumberOfTokens { get; protected set; }
        public int NumberOfUsers { get; protected set; }

        public double AverageLength { get; protected set; }
        public double AverageMessagesPerUnit { get; protected set; }

        public List<int> AllLengths { get; protected set; }
        public List<int> AllTokenNumbers { get; protected set; }
        public List<int> AllTokenLengths { get; protected set; }
        public Dictionary<string, double> AllFields { get; protected set; }

        public StatisticsService()
        {
            CurrentProgressValue = 0;
            ProgressChangedEventInterval = 1;

            AllLengths = new List<int>();
            AllTokenNumbers = new List<int>();
            AllTokenLengths = new List<int>();
            AllFields = new Dictionary<string, double>();
        }

        protected void IncreaseProgressValue()
        {
            if (CurrentProgressValue >= MaxProgressValue)
                return;

            CurrentProgressValue += 1;

            if (CurrentProgressValue % ProgressChangedEventInterval == 0)
                ProgressChanged?.Invoke(ProgressChangedEventInterval, CurrentProgressValue);
        }

        protected void IncreaseProgressValueToMax()
        {
            CurrentProgressValue = MaxProgressValue;
            ProgressChanged?.Invoke(ProgressChangedEventInterval, CurrentProgressValue);
        }

        protected int SafeDivide(int x, int y)
        {
            return y != 0 ? (x / y) : 0;
        }

        protected double SafeDivide(double x, double y)
        {
            return y != 0 ? (x / y) : 0;
        }

        public abstract void CalculateAll();
    }
}
