using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace ChatCorporaAnnotator.Models.Chat.Core
{
    internal class MobileMessageCollection : INotifyPropertyChanged
    {
        public delegate void MessagesChangedHandler(ObservableCollection<ChatMessage> newMessages);
        public event MessagesChangedHandler MessagesChanged;

        private readonly CollectionViewSource _messageCollection;
        public ICollectionView MessageCollection => _messageCollection.View;

        public ObservableCollection<ChatMessage> CurrentMessages
        {
            get => _messageCollection.Source as ObservableCollection<ChatMessage>;
            private set
            {
                _messageCollection.Source = value;

                OnPropertyChanged(nameof(MessageCollection));
                OnPropertyChanged(nameof(CurrentMessages));

                MessagesChanged?.Invoke(value);
            }
        }

        private string _messageFilterText;
        public string MessageFilterText
        {
            get => _messageFilterText;
            set
            {
                _messageFilterText = value;
                _messageCollection.View.Refresh();

                OnPropertyChanged(nameof(MessageFilterText));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public MobileMessageCollection(IEnumerable<ChatMessage> currentMessages = null)
        {
            _messageCollection = new CollectionViewSource()
            {
                IsLiveSortingRequested = false,
                IsLiveGroupingRequested = false,
                IsLiveFilteringRequested = true
            };

            _messageCollection.Filter += OnMessagesFiltered;

            SetMessages(currentMessages);
        }

        public void SetMessages(IEnumerable<ChatMessage> messages)
        {
            CurrentMessages = new ObservableCollection<ChatMessage>(messages ?? new ChatMessage[0]);
        }

        public void SetSortDescriptions(SortDescription sortDescription)
        {
            _messageCollection.SortDescriptions.Clear();
            _messageCollection.SortDescriptions.Add(sortDescription);
        }

        public void SetGroupDescriptions(GroupDescription groupDescription)
        {
            _messageCollection.GroupDescriptions.Clear();
            _messageCollection.GroupDescriptions.Add(groupDescription);
        }

        private void OnMessagesFiltered(object sender, FilterEventArgs e)
        {
            if (!(e.Item is ChatMessage message))
            {
                e.Accepted = false;
                return;
            }

            string filter = MessageFilterText;

            if (string.IsNullOrEmpty(filter))
                return;

            if (message.Text.ToLower().Contains(filter))
                return;

            e.Accepted = false;
        }
    }
}
