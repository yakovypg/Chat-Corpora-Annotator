using ChatCorporaAnnotator.Models.Indexing;
using IndexEngine;
using IndexingServices.Containers;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Data.Indexing
{
    internal static class ProjectInteraction
    {
        public static ProjectInformation ProjectInfo = null;

        public static int MessagesCount => IndexEngine.Paths.ProjectInfo.Data.LineCount;

        public static HashSet<ActiveDate> GetActiveDates()
        {
            string path = IndexEngine.Paths.ProjectInfo.ActiveDatesPath;

            if (ActiveDateParser.TryParseFile(path, out HashSet<ActiveDate> dates))
            {
                return dates;
            }

            HashSet<ActiveDate> activeDates = IndexHelper.LoadAllActiveDates();
            ActiveDateParser.TrySaveToFile(path, activeDates);

            return activeDates;
        }
    }
}
