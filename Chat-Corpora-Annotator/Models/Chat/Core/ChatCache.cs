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
        private readonly List<ChatMessage> _savedPackage;
        private readonly List<ChatMessage> _previousPackage;
        private readonly List<ChatMessage> _currentPackage;
        private readonly List<ChatMessage> _nextPackage;

        public delegate void PackageChangedHandler();
        public event PackageChangedHandler PackageChanged;

        public bool IsPaused => _savedPackage.Count > 0;
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

            _savedPackage = new List<ChatMessage>();
            _previousPackage = new List<ChatMessage>();
            _currentPackage = new List<ChatMessage>(currentPackage);
            _nextPackage = new List<ChatMessage>();

            RetainedItemsCount = retainedItemsCount;
        }

        public IList<ChatMessage> MoveBack()
        {
            if (IsPaused || _currentPackage.Count == 0 || _previousPackage.Count == 0)
                return null;

            int cachedItemsCount = RetainedItemsCount;

            var nextMessages = _currentPackage.Skip(cachedItemsCount).ToArray();
            var retainedMessages = _currentPackage.Take(cachedItemsCount).ToArray();
            var currentMessages = _previousPackage.Concat(retainedMessages).ToArray();

            _nextPackage.Reset(nextMessages);
            _currentPackage.Reset(currentMessages);

            LoadPreviousPackage();
            CurrentMessages = new ObservableCollection<ChatMessage>(_currentPackage);

            PackageChanged?.Invoke();
            return new List<ChatMessage>(currentMessages);
        }

        public IList<ChatMessage> MoveForward()
        {
            if (IsPaused || _currentPackage.Count == 0 || _nextPackage.Count == 0)
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

                outputMessages.InsertRange(0, missingMessages);
            }

            _previousPackage.Reset(previousMessages);
            _currentPackage.Reset(currentMessages);

            LoadNextPackage();
            CurrentMessages = new ObservableCollection<ChatMessage>(outputMessages);

            PackageChanged?.Invoke();
            return outputMessages;
        }

        public void Pause(IEnumerable<ChatMessage> tempMessages)
        {
            if (!IsPaused)
                _savedPackage.Reset(_currentPackage);

            _currentPackage.Reset(tempMessages ?? new ChatMessage[0]);

            CurrentMessages = new ObservableCollection<ChatMessage>(_currentPackage);
        }

        public void Resume()
        {
            if (!IsPaused)
                return;

            _currentPackage.Reset(_savedPackage);
            _savedPackage.Clear();

            CurrentMessages = new ObservableCollection<ChatMessage>(_currentPackage);
        }

        public void Reset()
        {
            _savedPackage.Clear();
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

        public void Shift(int messageReadIndex)
        {
            _savedPackage.Clear();
            _previousPackage.Clear();
            _currentPackage.Clear();
            _nextPackage.Clear();

            IndexInteraction.ResetMessageReadIndex(messageReadIndex);

            IEnumerable<ChatMessage> previousMessages = IndexInteraction.PeekPreviousMessages();
            IEnumerable<ChatMessage> currMessages = null;
            IEnumerable<ChatMessage> nextMessages = null;

            if (IndexInteraction.TryLoadNextMessagesFromIndex())
            {
                currMessages = IndexInteraction.GetMessages();

                if (IndexInteraction.TryLoadNextMessagesFromIndex())
                    nextMessages = IndexInteraction.GetMessages();
            }

            _previousPackage.Reset(previousMessages);
            _currentPackage.Reset(currMessages);
            _nextPackage.Reset(nextMessages);

            CurrentMessages = new ObservableCollection<ChatMessage>(currMessages);
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
