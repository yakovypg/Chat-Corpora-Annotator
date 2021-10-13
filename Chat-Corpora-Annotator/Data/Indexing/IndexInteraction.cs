using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.Models.Messages;
using IndexEngine;
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

        public static void ResetMessageReadIndex(int index = 0)
        {
            IndexHelper.ResetViewerReadIndex(index);
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

        public static IEnumerable<ChatMessage> GetMessages()
        {
            IEnumerable<DynamicMessage> messages = MessageContainer.Messages;

            return messages.IsNullOrEmpty()
                ? new ChatMessage[0]
                : messages.Select(t => new ChatMessage(t));
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
