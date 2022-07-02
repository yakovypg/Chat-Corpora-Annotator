using ChatCorporaAnnotator.Models.Indexing;
using IndexEngine.Containers;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Data.Indexing
{
    internal static class ProjectInteraction
    {
        public static ProjectInformation ProjectInfo = null;

        public static int MessagesCount => IndexEngine.Data.Paths.ProjectInfo.Data.LineCount;
        public static int FirstMessageId => IndexInteraction.GetFirstMessageId();
        public static int LastMessageId => IndexInteraction.GetLastMessageId();

        public static HashSet<ActiveDate> GetActiveDates()
        {
            string path = IndexEngine.Data.Paths.ProjectInfo.ActiveDatesPath;

            if (ActiveDateParser.TryParseFile(path, out HashSet<ActiveDate>? dates))
            {
                return dates ?? new HashSet<ActiveDate>();
            }

            HashSet<ActiveDate> activeDates = IndexEngine.Data.Paths.ProjectInfo.Data.ActiveDates;
            ActiveDateParser.TrySaveToFile(path, activeDates);

            return activeDates;
        }
    }
}
