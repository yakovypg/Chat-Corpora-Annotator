using ChatCorporaAnnotator.Data.Indexing;
using ChatCorporaAnnotator.Data.Windows;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.Models.Chat.Core;
using ChatCorporaAnnotator.ViewModels.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Chat
{
    internal class MessagesViewModel : ViewModel
    {
        public ChatCache MessagesCase { get; private set; }
        public ObservableCollection<ChatMessage> SelectedMessages { get; private set; }

        #region AddingCommands

        public ICommand SetMessagesCommand { get; }
        public bool CanSetMessagesCommandExecute(object parameter)
        {
            return true;
        }
        public void OnSetMessagesCommandExecuted(object parameter)
        {
            if (!CanSetMessagesCommandExecute(parameter))
                return;

            var messages = GetMessages();
            MessagesCase.Reset(messages);
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
            MessagesCase = new ChatCache(null);
            SelectedMessages = new ObservableCollection<ChatMessage>();

            SetMessagesCommand = new RelayCommand(OnSetMessagesCommandExecuted, CanSetMessagesCommandExecute);
            ChangeSelectedMessagesCommand = new RelayCommand(OnChangeSelectedMessagesCommandExecute, CanChangeSelectedMessagesCommandExecute);
        }

        public void ClearData()
        {
            MessagesCase.Reset(null);
            SelectedMessages.Clear();
        }

        private IEnumerable<ChatMessage> GetMessages()
        {
            return IndexInteraction.TryLoadNextMessagesFromIndex()
                ? IndexInteraction.GetMessages()
                : new ChatMessage[0];
        }
    }
}
