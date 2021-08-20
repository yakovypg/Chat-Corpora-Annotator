using ChatCorporaAnnotator.Data.Windows;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Extensions;
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
        public ObservableCollection<DynamicMessage> Messages { get; private set; }
        public ObservableCollection<DynamicMessage> SelectedMessages { get; private set; }

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

            IEnumerable<DynamicMessage> newMessages = parameter is IEnumerable<DynamicMessage> messages
                ? messages
                : GetMessages();

            if (newMessages.IsNullOrEmpty())
            {
                Messages.Clear();
                return;
            }

            Messages = new ObservableCollection<DynamicMessage>(newMessages);
            OnPropertyChanged(nameof(Messages));
        }

        public ICommand AddMessagesCommand { get; }
        public bool CanAddMessagesCommandExecute(object parameter)
        {
            return parameter is IEnumerable<DynamicMessage>;
        }
        public void OnAddMessagesCommandExecuted(object parameter)
        {
            if (!CanAddMessagesCommandExecute(parameter))
                return;

            IEnumerable<DynamicMessage> addingMessages = parameter as IEnumerable<DynamicMessage>;

            if (addingMessages.IsNullOrEmpty())
                return;

            Messages = new ObservableCollection<DynamicMessage>(Messages.Concat(addingMessages));
            OnPropertyChanged(nameof(Messages));
        }

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
            Messages = new ObservableCollection<DynamicMessage>();
            SelectedMessages = new ObservableCollection<DynamicMessage>();

            SetMessagesCommand = new RelayCommand(OnSetMessagesCommandExecuted, CanSetMessagesCommandExecute);
            AddMessagesCommand = new RelayCommand(OnAddMessagesCommandExecuted, CanAddMessagesCommandExecute);
            ChangeSelectedMessagesCommand = new RelayCommand(OnChangeSelectedMessagesCommandExecute, CanChangeSelectedMessagesCommandExecute);
        }

        private IEnumerable<DynamicMessage> GetMessages()
        {
            IEnumerable<DynamicMessage> messages = MessageContainer.Messages;

            return messages.IsNullOrEmpty()
                ? new DynamicMessage[0]
                : messages;
        }
    }
}
