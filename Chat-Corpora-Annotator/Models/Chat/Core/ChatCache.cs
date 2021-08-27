using ChatCorporaAnnotator.Data.Indexing;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ChatCorporaAnnotator.Models.Chat.Core
{
    internal class ChatCache : IChatCache, INotifyPropertyChanged
    {
        private int _currPackageIndex = 0;

        private readonly List<ChatMessage> _previousPackage;
        private readonly List<ChatMessage> _currentPackage;
        private readonly List<ChatMessage> _nextPackage;

        public int CurrentPackageCapacity => _currentPackage.Count;

        private IList<ChatMessage> _currentMessages;
        public IList<ChatMessage> CurrentMessages
        {
            get => _currentMessages;
            private set
            {
                _currentMessages = value;
                OnPropertyChanged(nameof(CurrentMessages));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public ChatCache(IEnumerable<ChatMessage> currentPackage)
        {
            if (currentPackage == null)
                currentPackage = new ChatMessage[0];

            _previousPackage = new List<ChatMessage>();
            _currentPackage = new List<ChatMessage>(currentPackage);
            _nextPackage = new List<ChatMessage>();
        }

        public IList<ChatMessage> MoveBack(int currOffset, int retainedItems)
        {
            if (_currPackageIndex == 0 || _currentPackage.Count == 0)
                return null;

            int loadingMessagesCount = IndexInteraction.LoadingMessagesCount - retainedItems;

            if (!IndexInteraction.TryLoadPreviousMessagesFromIndex(loadingMessagesCount))
                return null;

            var loadedMessages = IndexInteraction.GetMessages();

            if (loadedMessages.IsNullOrEmpty())
                return null;

            var cachedMessages = _currentPackage.Skip(currOffset + retainedItems);
            var retainedMessages = _currentPackage.Take(currOffset + retainedItems);
            var currentMessages = _previousPackage.Concat(retainedMessages);

            _previousPackage.Reset(loadedMessages);
            _currentPackage.Reset(currentMessages);
            _nextPackage.Reset(cachedMessages);

            _currPackageIndex--;
            CurrentMessages = _currentPackage;

            return CurrentMessages;
        }

        public IList<ChatMessage> MoveForward(int currOffset, int retainedItems)
        {
            if (_currentPackage.Count == 0)
                return null;

            if (_currPackageIndex == 0)
                LoadNextPackage();

            int loadingMessagesCount = IndexInteraction.LoadingMessagesCount - retainedItems;

            if (!IndexInteraction.TryLoadNextMessagesFromIndex(loadingMessagesCount))
                return null;

            var loadedMessages = IndexInteraction.GetMessages();

            if (loadedMessages.IsNullOrEmpty())
                return null;

            var cachedMessages = _currentPackage.Take(currOffset - retainedItems);
            var retainedMessages = _currentPackage.Skip(currOffset - retainedItems);
            var currentMessages = retainedMessages.Concat(_nextPackage);

            _previousPackage.Reset(cachedMessages);
            _currentPackage.Reset(currentMessages);
            _nextPackage.Reset(loadedMessages);

            _currPackageIndex++;
            CurrentMessages = _currentPackage;

            return CurrentMessages;
        }

        public void Reset(IEnumerable<ChatMessage> currentPackage, int messageReadIndex = 0)
        {
            if (currentPackage == null)
                currentPackage = new ChatMessage[0];

            IndexInteraction.ResetMessageReadIndex(messageReadIndex);

            _previousPackage.Clear();
            _currentPackage.Clear();
            _nextPackage.Clear();

            _currentPackage.AddRange(currentPackage);
            CurrentMessages = _currentPackage;
        }

        private void LoadNextPackage()
        {
            if (!IndexInteraction.TryLoadNextMessagesFromIndex())
                return;

            var loadedMessages = IndexInteraction.GetMessages();

            if (loadedMessages.IsNullOrEmpty())
                return;

            _nextPackage.Reset(loadedMessages);
        }
    }
}
