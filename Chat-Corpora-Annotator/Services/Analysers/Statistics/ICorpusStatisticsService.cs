using System.Collections.Generic;

namespace ChatCorporaAnnotator.Services.Analysers.Statistics
{
    internal interface ICorpusStatisticsService : IStatisticsService
    {
        int WindowCount { get; }
        int AverageWindowLength { get; }
        int IntertwinedCount { get; }
        int AverageUsersPerSituation { get; }

        Dictionary<string, int> AverageUsersInSituationPerTag { get; }
        Dictionary<string, int> SituationsPerTag { get; }
    }
}
