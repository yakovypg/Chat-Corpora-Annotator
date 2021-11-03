using ChatCorporaAnnotator.Data.Indexing;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using IndexEngine.Paths;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ChatCorporaAnnotator.Models.Chat.Core
{
    internal class ChatCache : MobileMessageCollection, IChatCache, INotifyPropertyChanged
    {
        private readonly List<ChatMessage> _savedPackage;
        private readonly List<ChatMessage> _previousPackage;
        private readonly List<ChatMessage> _currentPackage;
        private readonly List<ChatMessage> _nextPackage;

        private int _previousPackageScrollIndex;

        public delegate void PackageChangedHandler();
        public event PackageChangedHandler PackageChanged;

        public bool IsPaused => _savedPackage.Count > 0;
        public int CurrentPackageItemsCount => _currentPackage.Count;

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

        public ChatCache(IEnumerable<ChatMessage> currentPackage, int retainedItemsCount = 1) : base(currentPackage)
        {
            if (currentPackage == null)
                currentPackage = new ChatMessage[0];

            _savedPackage = new List<ChatMessage>();
            _previousPackage = new List<ChatMessage>();
            _currentPackage = new List<ChatMessage>(currentPackage);
            _nextPackage = new List<ChatMessage>();

            RetainedItemsCount = retainedItemsCount;
        }

        public IList<ChatMessage> MoveBack(out int scrollIndex)
        {
            scrollIndex = _previousPackageScrollIndex;

            if (IsPaused || _currentPackage.Count == 0 || _previousPackage.Count == 0)
                return null;

            int cachedItemsCount = RetainedItemsCount;

            var nextMessages = _currentPackage.Skip(cachedItemsCount).ToArray();
            var retainedMessages = _currentPackage.Take(cachedItemsCount).ToArray();
            var currentMessages = _previousPackage.Concat(retainedMessages).ToArray();

            _nextPackage.Reset(nextMessages);
            _currentPackage.Reset(currentMessages);

            LoadPreviousPackage();
            SetMessages(_currentPackage);

            PackageChanged?.Invoke();
            return new List<ChatMessage>(currentMessages);
        }

        public IList<ChatMessage> MoveForward(out int scrollIndex)
        {
            scrollIndex = 0;

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

                scrollIndex += missingMessagesCount;
                _previousPackageScrollIndex = previousMessages.Length - missingMessagesCount;

                outputMessages.InsertRange(0, missingMessages);
            }
            else
            {
                _previousPackageScrollIndex = previousMessages.Length - 1;
            }

            _previousPackage.Reset(previousMessages);
            _currentPackage.Reset(currentMessages);

            LoadNextPackage();
            SetMessages(outputMessages);

            PackageChanged?.Invoke();
            return outputMessages;
        }

        public void Pause(IEnumerable<ChatMessage> tempMessages)
        {
            if (!IsPaused)
                _savedPackage.Reset(_currentPackage);

            _currentPackage.Reset(tempMessages ?? new ChatMessage[0]);

            SetMessages(_currentPackage);
        }

        public void Resume()
        {
            if (!IsPaused)
                return;

            _currentPackage.Reset(_savedPackage);
            _savedPackage.Clear();

            SetMessages(_currentPackage);
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
            SetMessages(currentMessages);

            LoadNextPackage();
            PackageChanged?.Invoke();
        }

        public void Shift(int messageIndex, out int scrollIndex)
        {
            if (messageIndex < 0 || messageIndex >= ProjectInteraction.MessagesCount)
                throw new IndexOutOfRangeException();

            int packageLength = IndexInteraction.LoadingMessagesCount;
            int packageNumber = messageIndex / packageLength;
            int firstMsgId = packageNumber * packageLength;

            scrollIndex = messageIndex % packageLength;

            _savedPackage.Clear();
            _previousPackage.Clear();
            _currentPackage.Clear();
            _nextPackage.Clear();

            IndexInteraction.ResetMessageReadIndex(firstMsgId - 1);

            IEnumerable<ChatMessage> previousMessages = IndexInteraction.PeekPreviousMessages();
            IEnumerable<ChatMessage> currMessages = null;
            IEnumerable<ChatMessage> nextMessages = null;

            IndexInteraction.ResetMessageReadIndex(firstMsgId);

            if (IndexInteraction.TryLoadNextMessagesFromIndex())
            {
                currMessages = IndexInteraction.GetMessages();

                if (IndexInteraction.TryLoadNextMessagesFromIndex())
                    nextMessages = IndexInteraction.GetMessages();
            }

            int currMessagesCount = currMessages.Count();
            var outputMessages = new List<ChatMessage>(currMessages);

            if (IndexInteraction.LoadingMessagesCount < ProjectInfo.Data.LineCount && currMessagesCount < IndexInteraction.LoadingMessagesCount)
            {
                int previousMessagesCount = previousMessages.Count();
                int missingMessagesCount = IndexInteraction.LoadingMessagesCount - currMessagesCount;
                var missingMessages = previousMessages.Skip(previousMessagesCount - missingMessagesCount).ToArray();

                scrollIndex += missingMessagesCount;
                _previousPackageScrollIndex = previousMessagesCount - missingMessagesCount;

                outputMessages.InsertRange(0, missingMessages);
            }
            else
            {
                _previousPackageScrollIndex = previousMessages.Count() - 1;
            }

            _previousPackage.Reset(previousMessages);
            _currentPackage.Reset(currMessages);
            _nextPackage.Reset(nextMessages);

            SetMessages(outputMessages);
            PackageChanged?.Invoke();
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
