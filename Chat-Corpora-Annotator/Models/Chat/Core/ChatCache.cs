using ChatCorporaAnnotator.Data.Indexing;
using ChatCorporaAnnotator.Infrastructure.Extensions;
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

        public int CurrentPackageCapacity => _currentPackage.Count;
        public IList<ChatMessage> CurrentPackage => _currentPackage;

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

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public ChatCache(IEnumerable<ChatMessage> currentPackage)
        {
            if (currentPackage == null)
                currentPackage = new ChatMessage[0];

            _currentMessages = new ObservableCollection<ChatMessage>(currentPackage);

            _previousPackage = new List<ChatMessage>();
            _currentPackage = new List<ChatMessage>(currentPackage);
            _nextPackage = new List<ChatMessage>();
        }

        //public List<ChatMessage> MoveBack(int currOffset, int retainedItems)
        //{
        //    if (_currPackageIndex == 0 || _currentPackage.Count == 0)
        //        return null;

        //    int loadingMessagesCount = IndexInteraction.LoadingMessagesCount - retainedItems;

        //    if (!IndexInteraction.TryLoadPreviousMessagesFromIndex(loadingMessagesCount))
        //        return null;

        //    var loadedMessages = IndexInteraction.GetMessages();

        //    if (loadedMessages.IsNullOrEmpty())
        //        return null;

        //    var cachedMessages = _currentPackage.Skip(currOffset + retainedItems).ToArray();
        //    var retainedMessages = _currentPackage.Take(currOffset + retainedItems).ToArray();
        //    var currentMessages = _previousPackage.Concat(retainedMessages).ToArray();

        //    _previousPackage.Reset(loadedMessages);
        //    _currentPackage.Reset(currentMessages);
        //    _nextPackage.Reset(cachedMessages);

        //    _currPackageIndex--;
        //    CurrentMessages = new ObservableCollection<ChatMessage>(_currentPackage);

        //    return _currentPackage;
        //}

        public bool MoveBack(int currOffset, int retainedItems)
        {
            if (_currentPackage.Count == 0 || _previousPackage.Count == 0)
                return false;

            _nextPackage.Reset(_currentPackage);
            _currentPackage.Reset(_previousPackage);

            LoadPreviousPackage();
            CurrentMessages = new ObservableCollection<ChatMessage>(_currentPackage);

            return true;
        }

        public bool MoveForward(int currOffset, int retainedItems)
        {
            if (_currentPackage.Count == 0 || _nextPackage.Count == 0)
                return false;

            _previousPackage.Reset(_currentPackage);
            _currentPackage.Reset(_nextPackage);

            LoadNextPackage();
            CurrentMessages = new ObservableCollection<ChatMessage>(_currentPackage);

            return true;
        }

        //public List<ChatMessage> MoveForward(int currOffset, int retainedItems)
        //{
        //    if (_currentPackage.Count == 0)
        //        return null;

        //    if (_currPackageIndex == 0)
        //        LoadNextPackage();

        //    int loadingMessagesCount = IndexInteraction.LoadingMessagesCount - retainedItems;

        //    if (!IndexInteraction.TryLoadNextMessagesFromIndex(loadingMessagesCount))
        //        return null;

        //    var loadedMessages = IndexInteraction.GetMessages();

        //    if (loadedMessages.IsNullOrEmpty())
        //        return null;

        //    var cachedMessages = _currentPackage.Take(currOffset - retainedItems).ToArray();
        //    var retainedMessages = _currentPackage.Skip(currOffset - retainedItems).ToArray();
        //    var currentMessages = retainedMessages.Concat(_nextPackage).ToArray();

        //    _previousPackage.Reset(cachedMessages);
        //    _currentPackage.Reset(currentMessages);
        //    _nextPackage.Reset(loadedMessages);

        //    _currPackageIndex++;
        //    CurrentMessages = new ObservableCollection<ChatMessage>(_currentPackage);

        //    return _currentPackage;
        //}

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
                int index = _nextPackage.Last().Source.Id;
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
