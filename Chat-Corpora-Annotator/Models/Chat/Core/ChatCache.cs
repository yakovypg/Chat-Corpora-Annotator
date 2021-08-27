using ChatCorporaAnnotator.Data.Indexing;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using System;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Models.Chat.Core
{
    internal class ChatCache
    {
        private readonly int _currPackageIndex = 0;

        private readonly List<ChatMessage> _previousPackage;
        private readonly List<ChatMessage> _currentPackage;
        private readonly List<ChatMessage> _nextPackage;

        public IEnumerable<ChatMessage> CurrentPackage => _currentPackage;

        public ChatCache(IEnumerable<ChatMessage> currentPackage)
        {
            _currentPackage = new List<ChatMessage>(currentPackage) ?? throw new ArgumentNullException(nameof(currentPackage));

            _previousPackage = new List<ChatMessage>();
            _nextPackage = new List<ChatMessage>();
        }

        public IEnumerable<ChatMessage> MoveBack()
        {
            if (_currPackageIndex == 0)
                return null;

            if (!IndexInteraction.TryLoadPreviousMessagesFromIndex())
                return null;

            var loadedMessages = IndexInteraction.GetMessages();

            if (loadedMessages.IsNullOrEmpty())
                return null;

            _nextPackage.Reset(_currentPackage);
            _currentPackage.Reset(_previousPackage);
            _previousPackage.Reset(loadedMessages);

            return CurrentPackage;
        }

        public IEnumerable<ChatMessage> MoveForward()
        {
            if (!IndexInteraction.TryLoadNextMessagesFromIndex())
                return null;

            var loadedMessages = IndexInteraction.GetMessages();

            if (loadedMessages.IsNullOrEmpty())
                return null;

            _nextPackage.Reset(_currentPackage);
            _currentPackage.Reset(_previousPackage);
            _previousPackage.Reset(loadedMessages);

            return CurrentPackage;
        }
    }
}
