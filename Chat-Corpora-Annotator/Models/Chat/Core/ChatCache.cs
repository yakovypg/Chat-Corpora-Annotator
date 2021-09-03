using ChatCorporaAnnotator.Data.Indexing;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ChatCorporaAnnotator.Models.Chat.Core
{
    internal class ChatCache : IChatCache, INotifyPropertyChanged
    {
        private readonly List<ChatMessage> _previousPackage;
        private readonly List<ChatMessage> _currentPackage;
        private readonly List<ChatMessage> _nextPackage;

        public int CurrentPackageItemsCount => _currentPackage.Count;

        private ObservableCollection<ChatMessage> _currentMessages;
        public ObservableCollection<ChatMessage> CurrentMessages
        {
            get => _currentMessages;
            private set
            {
                _currentMessages.Clear();
                _currentMessages = value;

                OnPropertyChanged(nameof(CurrentMessages));
            }
        }

        private int _retainedItemsCount = 1;
        public int RetainedItemsCount
        {
            get => _retainedItemsCount;
            set
            {
                if (value < 0)
                    throw new ArgumentException("The value must be greater than zero.");

                _retainedItemsCount = value;

                IndexInteraction.LoadingMessagesCount =
                    IndexInteraction.DefaultLoadingMessagesCount - RetainedItemsCount;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public ChatCache(IEnumerable<ChatMessage> currentPackage, int retainedItemsCount = 1)
        {
            if (currentPackage == null)
                currentPackage = new ChatMessage[0];

            _currentMessages = new ObservableCollection<ChatMessage>(currentPackage);

            _previousPackage = new List<ChatMessage>();
            _currentPackage = new List<ChatMessage>(currentPackage);
            _nextPackage = new List<ChatMessage>();

            RetainedItemsCount = retainedItemsCount;
        }

        public IList<ChatMessage> MoveBack(out int pageStartIndex)
        {
            pageStartIndex = 0;

            if (_currentPackage.Count == 0 || _previousPackage.Count == 0)
                return null;

            int cachedItemsCount = RetainedItemsCount;

            var nextMessages = _currentPackage.Skip(cachedItemsCount).ToArray();
            var retainedMessages = _currentPackage.Take(cachedItemsCount).ToArray();
            var currentMessages = _previousPackage.Concat(retainedMessages).ToArray();

            _nextPackage.Reset(nextMessages);
            _currentPackage.Reset(currentMessages);

            LoadPreviousPackage();

            pageStartIndex = _currentPackage.Count - RetainedItemsCount;
            CurrentMessages = new ObservableCollection<ChatMessage>(_currentPackage);

            return new List<ChatMessage>(currentMessages);
        }

        public IList<ChatMessage> MoveForward(out int pageStartIndex)
        {
            pageStartIndex = RetainedItemsCount;

            if (_currentPackage.Count == 0 || _nextPackage.Count == 0)
                return null;

            int cachedItemsCount = CurrentPackageItemsCount - RetainedItemsCount;

            var previousMessages = _currentPackage.Take(cachedItemsCount).ToArray();
            var retainedMessages = _currentPackage.Skip(cachedItemsCount).ToArray();
            var currentMessages = retainedMessages.Concat(_nextPackage).ToArray();

            var outputMessages = new List<ChatMessage>(currentMessages);

            if (currentMessages.Length < IndexInteraction.LoadingMessagesCount)
            {
                int missingMessagesCount = IndexInteraction.LoadingMessagesCount - currentMessages.Length;
                var missingMessages = previousMessages.Skip(previousMessages.Length - missingMessagesCount).ToArray();

                pageStartIndex = missingMessagesCount;
                outputMessages.InsertRange(0, missingMessages.Concat(currentMessages).ToArray());
            }

            _previousPackage.Reset(previousMessages);
            _currentPackage.Reset(currentMessages);

            LoadNextPackage();
            CurrentMessages = new ObservableCollection<ChatMessage>(outputMessages);

            return outputMessages;
        }

        public void Reset()
        {
            _previousPackage.Clear();
            _currentPackage.Clear();
            _nextPackage.Clear();

            IndexInteraction.ResetMessageReadIndex();

            if (!IndexInteraction.TryLoadNextMessagesFromIndex())
                return;

            var currentMessages = IndexInteraction.GetMessages();

            _currentPackage.Reset(currentMessages);
            CurrentMessages = new ObservableCollection<ChatMessage>(currentMessages);

            LoadNextPackage();
        }

        private bool LoadPreviousPackage()
        {
            if (_previousPackage.Count > 0)
            {
                int index = _previousPackage.First().Source.Id - 1;
                IndexInteraction.ResetMessageReadIndex(index);
            }

            _previousPackage.Reset(null);

            if (!IndexInteraction.TryLoadPreviousMessagesFromIndex())
                return false;

            var loadedMessages = IndexInteraction.GetMessages();

            if (loadedMessages.IsNullOrEmpty())
                return false;

            _previousPackage.Reset(loadedMessages);
            return true;
        }

        private bool LoadNextPackage()
        {
            if (_nextPackage.Count > 0)
            {
                int index = _nextPackage.Last().Source.Id + 1;
                IndexInteraction.ResetMessageReadIndex(index);
            }

            _nextPackage.Reset(null);

            if (!IndexInteraction.TryLoadNextMessagesFromIndex())
                return false;

            var loadedMessages = IndexInteraction.GetMessages();

            if (loadedMessages.IsNullOrEmpty())
                return false;

            _nextPackage.Reset(loadedMessages);
            return true;
        }
    }
}
