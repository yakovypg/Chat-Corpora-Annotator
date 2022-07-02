using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.Models.Messages;
using IndexEngine.Containers;
using IndexEngine.Indexes;
using System.Collections.Generic;
using System.Linq;

namespace ChatCorporaAnnotator.Data.Indexing
{
    internal static class IndexInteraction
    {
        public static readonly int DefaultLoadingMessagesCount = 500;
        public static int LoadingMessagesCount = DefaultLoadingMessagesCount;

        public static int GetMessageReadIndex()
        {
            return IndexHelper.GetViewerReadIndex();
        }

        public static void ResetMessageReadIndex()
        {
            ResetMessageReadIndex(ProjectInteraction.FirstMessageId);
        }

        public static void ResetMessageReadIndex(int messageId)
        {
            IndexHelper.ResetViewerReadIndex(messageId - ProjectInteraction.FirstMessageId);
        }

        public static void DecreaseMessageReadIndex(int count)
        {
            IncreaseMessageReadIndex(count * -1);
        }

        public static void IncreaseMessageReadIndex(int count)
        {
            int currIndex = GetMessageReadIndex();
            IndexHelper.ResetViewerReadIndex(currIndex + count);
        }

        public static int GetFirstMessageId()
        {
            return IndexHelper.GetFirstMessage().Id;
        }

        public static int GetLastMessageId()
        {
            return IndexHelper.GetLastMessage().Id;
        }

        public static void UploadDataToMessage(DynamicMessage message)
        {
            if (message == null)
                return;

            var invertedIndex = SituationIndex.GetInstance().InvertedIndex;

            if (!invertedIndex.ContainsKey(message.Id))
                return;

            var situations = invertedIndex[message.Id];

            foreach (var sit in situations)
                message.Situations.TryAdd(sit.Key, sit.Value);
        }

        public static IEnumerable<ChatMessage> GetMessages()
        {
            IEnumerable<DynamicMessage> messages = MessageContainer.Messages;

            return messages.IsNullOrEmpty()
                ? new ChatMessage[0]
                : messages.Select(t => new ChatMessage(t));
        }

        public static IEnumerable<ChatMessage> GetAllTaggedMessages()
        {
            List<DynamicMessage> messages = new List<DynamicMessage>();

            var indexItems = SituationIndex.GetInstance().InvertedIndex.Where(t => t.Value.Count > 0);
            var taggedMsgIds = indexItems.Select(t => t.Key).ToArray();

            foreach (int id in taggedMsgIds)
            {
                DynamicMessage msg = IndexHelper.GetMessage(id - ProjectInteraction.FirstMessageId);
                UploadDataToMessage(msg);

                messages.Add(msg);
            }

            return messages.Select(t => new ChatMessage(t));
        }

        public static IEnumerable<ChatMessage> GetMessagesByTag(string tagHeader)
        {
            List<int> msgIds = new List<int>();
            List<DynamicMessage> messages = new List<DynamicMessage>();

            var index = SituationIndex.GetInstance().IndexCollection;

            foreach (var kvp in index)
            {
                if (!kvp.Key.Contains(tagHeader))
                    continue;

                foreach (var situationData in index[kvp.Key])
                {
                    msgIds.AddRange(situationData.Value);
                }
            }

            msgIds = msgIds.Distinct().ToList();

            foreach (int id in msgIds)
            {
                DynamicMessage msg = IndexHelper.GetMessage(id - ProjectInteraction.FirstMessageId);
                UploadDataToMessage(msg);

                messages.Add(msg);
            }

            return messages.Select(t => new ChatMessage(t));
        }

        public static IEnumerable<ChatMessage> GetMessagesBySituation(Situation situation)
        {
            List<DynamicMessage> messages = new List<DynamicMessage>();

            int sitId = situation.Id;
            string sitTag = situation.Header;
            List<int> msgIds = SituationIndex.GetInstance().IndexCollection[sitTag][sitId];

            foreach (int id in msgIds)
            {
                DynamicMessage msg = IndexHelper.GetMessage(id - ProjectInteraction.FirstMessageId);
                UploadDataToMessage(msg);

                messages.Add(msg);
            }

            return messages.Select(t => new ChatMessage(t));
        }

        public static bool TryLoadPreviousMessagesFromIndex()
        {
            return TryLoadPreviousMessagesFromIndex(LoadingMessagesCount);
        }

        public static bool TryLoadNextMessagesFromIndex()
        {
            return TryLoadNextMessagesFromIndex(LoadingMessagesCount);
        }

        public static bool TryLoadPreviousMessagesFromIndex(int count)
        {
            if (count < 0)
                return false;

            try
            {
                var list = IndexHelper.LoadPreviousDocumentsFromIndex(count);

                if (list.IsNullOrEmpty())
                    return false;

                MessageContainer.Messages = list;
                return true;
            }
            catch
            {
                new QuickMessage("Failed to load messages.").ShowError();
                return false;
            }
        }

        public static bool TryLoadNextMessagesFromIndex(int count)
        {
            if (count < 0)
                return false;

            try
            {
                var list = IndexHelper.LoadNextDocumentsFromIndex(count);

                if (list.IsNullOrEmpty())
                    return false;

                MessageContainer.Messages = list;
                return true;
            }
            catch
            {
                new QuickMessage("Failed to load messages.").ShowError();
                return false;
            }
        }

        public static IEnumerable<ChatMessage> PeekPreviousMessages()
        {
            return PeekPreviousMessages(LoadingMessagesCount);
        }

        public static IEnumerable<ChatMessage> PeekNextMessages()
        {
            return PeekNextMessages(LoadingMessagesCount);
        }

        public static IEnumerable<ChatMessage> PeekPreviousMessages(int count)
        {
            if (count < 0)
                return new ChatMessage[0];

            List<DynamicMessage> messages = IndexHelper.PeekPreviousDocumentsFromIndex(count);
            return messages.Select(t => new ChatMessage(t));
        }

        public static IEnumerable<ChatMessage> PeekNextMessages(int count)
        {
            if (count < 0)
                return new ChatMessage[0];

            List<DynamicMessage> messages = IndexHelper.PeekNextDocumentsFromIndex(count);
            return messages.Select(t => new ChatMessage(t));
        }
    }
}
