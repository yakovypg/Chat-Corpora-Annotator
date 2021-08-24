using ChatCorporaAnnotator.Models.Messages;
using IndexEngine;

namespace ChatCorporaAnnotator.Data.Indexing
{
    internal static class IndexInteraction
    {
        public static bool TryLoadMessagesFromIndex(int count)
        {
            try
            {
                var list = IndexHelper.LoadNDocumentsFromIndex(count);
                MessageContainer.Messages = list;
                return true;
            }
            catch
            {
                new QuickMessage("Failed to load messages.").ShowWarning();
                return false;
            }
        }
    }
}
