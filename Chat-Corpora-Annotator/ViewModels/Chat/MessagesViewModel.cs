﻿using ChatCorporaAnnotator.Data.Windows;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.Models.Chat.Core;
using ChatCorporaAnnotator.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Chat
{
    internal class MessagesViewModel : ViewModel
    {
        public ChatCache MessagesCase { get; private set; }
        public ObservableCollection<ChatMessage> SelectedMessages { get; private set; }

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

            ChangeSelectedMessagesCommand = new RelayCommand(OnChangeSelectedMessagesCommandExecute, CanChangeSelectedMessagesCommandExecute);
        }

        public void ClearData()
        {
            MessagesCase.Reset();
            SelectedMessages.Clear();
        }
    }
}
