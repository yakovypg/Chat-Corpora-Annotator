﻿using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.Models.Messages;
using IndexEngine;
using System.Collections.Generic;
using System.Linq;

namespace ChatCorporaAnnotator.Data.Indexing
{
    internal static class IndexInteraction
    {
        public static int LoadingMessagesCount = 100;

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
            try
            {
                var list = IndexHelper.LoadPreviousDocumentsFromIndex(count);
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
            try
            {
                var list = IndexHelper.LoadNextDocumentsFromIndex(count);
                MessageContainer.Messages = list;
                return true;
            }
            catch
            {
                new QuickMessage("Failed to load messages.").ShowError();
                return false;
            }
        }
    }
}
