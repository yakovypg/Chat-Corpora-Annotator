using System;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Services.Statistics
{
    internal interface IStatisticsService
    {
        bool IsCalculated { get; }
        TimeSpan Duration { get; }

        int NumberOfDocs { get; }
        long NumberOfSymbols { get; }
        ulong NumberOfTokens { get; }
        int NumberOfUsers { get; }

        double AverageLength { get; }
        double AverageMessagesPerUnit { get; }

        List<int> AllLengths { get; }
        List<int> AllTokenNumbers { get; }
        List<int> AllTokenLengths { get; }
        Dictionary<string, double> AllFields { get; }

        void CalculateAll();
    }
}
