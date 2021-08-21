using ChatCorporaAnnotator.Data.Windows;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.ViewModels.Base;
using IndexEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Chat
{
    internal class MessagesViewModel : ViewModel
    {
        public ObservableCollection<ChatMessage> Messages { get; private set; }
        public ObservableCollection<ChatMessage> SelectedMessages { get; private set; }

        #region Commands

        public ICommand SetMessagesCommand { get; }
        public bool CanSetMessagesCommandExecute(object parameter)
        {
            return true;
        }
        public void OnSetMessagesCommandExecuted(object parameter)
        {
            if (!CanSetMessagesCommandExecute(parameter))
                return;

            IEnumerable<ChatMessage> newMessages = parameter is IEnumerable<ChatMessage> messages
                ? messages
                : GetMessages();

            if (newMessages.IsNullOrEmpty())
            {
                Messages.Clear();
                return;
            }

            Messages = new ObservableCollection<ChatMessage>(newMessages);
            OnPropertyChanged(nameof(Messages));
        }

        public ICommand AddMessagesCommand { get; }
        public bool CanAddMessagesCommandExecute(object parameter)
        {
            return parameter is IEnumerable<ChatMessage>;
        }
        public void OnAddMessagesCommandExecuted(object parameter)
        {
            if (!CanAddMessagesCommandExecute(parameter))
                return;

            IEnumerable<ChatMessage> addingMessages = parameter as IEnumerable<ChatMessage>;

            if (addingMessages.IsNullOrEmpty())
                return;

            Messages = new ObservableCollection<ChatMessage>(Messages.Concat(addingMessages));
            OnPropertyChanged(nameof(Messages));
        }

        #endregion

        #region SelectionCommands

        public ICommand ChangeSelectedMessagesCommand { get; }
        public bool CanChangeSelectedMessagesCommandExecute(object parameter)
        {
            return parameter is SelectionChangedEventArgs;
        }
        public void OnChangeSelectedMessagesCommandExecute(object parameter)
        {
            if (!CanChangeSelectedMessagesCommandExecute(parameter))
                return;

            var eventArgs = parameter as SelectionChangedEventArgs;
            var selectedItemsOrganizer = new SelectedItemsOrganizer();

            selectedItemsOrganizer.ChangeSelectedItems(SelectedMessages, eventArgs);
        }

        #endregion

        public MessagesViewModel()
        {
            Messages = new ObservableCollection<ChatMessage>();
            SelectedMessages = new ObservableCollection<ChatMessage>();

            SetMessagesCommand = new RelayCommand(OnSetMessagesCommandExecuted, CanSetMessagesCommandExecute);
            AddMessagesCommand = new RelayCommand(OnAddMessagesCommandExecuted, CanAddMessagesCommandExecute);

            ChangeSelectedMessagesCommand = new RelayCommand(OnChangeSelectedMessagesCommandExecute, CanChangeSelectedMessagesCommandExecute);
        }

        private IEnumerable<ChatMessage> GetMessages()
        {
            IEnumerable<DynamicMessage> messages = MessageContainer.Messages;

            return messages.IsNullOrEmpty()
                ? new ChatMessage[0]
                : messages.Select(t => new ChatMessage(t));
        }
    }
}
