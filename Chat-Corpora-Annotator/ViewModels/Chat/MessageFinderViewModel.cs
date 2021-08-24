using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.ViewModels.Base;
using IndexEngine;
using IndexEngine.Paths;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Chat
{
    internal class MessageFinderViewModel : ViewModel
    {
        private readonly ChatViewModel _chatVM;

        #region Items

        private string _query = string.Empty;
        public string Query
        {
            get => _query;
            set => SetValue(ref _query, value);
        }

        private bool _isStartDateChecked = false;
        public bool IsStartDateChecked
        {
            get => _isStartDateChecked;
            set => SetValue(ref _isStartDateChecked, value);
        }

        private bool _isEndDateChecked = false;
        public bool IsEndDateChecked
        {
            get => _isEndDateChecked;
            set => SetValue(ref _isEndDateChecked, value);
        }

        private DateTime _startDate = DateTime.Today;
        public DateTime StartDate
        {
            get => _startDate;
            set => SetValue(ref _startDate, value);
        }

        private DateTime _endDate = DateTime.Today;
        public DateTime EndDate
        {
            get => _endDate;
            set => SetValue(ref _endDate, value);
        }

        #endregion

        #region Commands

        public ICommand ClearFinderCommand { get; }
        public bool CanClearFinderCommandExecute(object parameter)
        {
            return true;
        }
        public void OnClearFinderCommandExecuted(object parameter)
        {
            if (!CanClearFinderCommandExecute(parameter))
                return;

            Query = string.Empty;

            IsStartDateChecked = false;
            IsEndDateChecked = false;
            StartDate = DateTime.Today;
            EndDate = DateTime.Today;

            _chatVM.UsersVM.DeselectAllUsersCommand?.Execute(null);
        }

        public ICommand FindMessagesCommand { get; }
        public bool CanFindMessagesCommandExecute(object parameter)
        {
            return _chatVM.MainWindowVM.IsFileLoaded;
        }
        public void OnFindMessagesCommandExecuted(object parameter)
        {
            if (!CanFindMessagesCommandExecute(parameter))
                return;

            //if (ChatVM.UsersVM.SelectedUsers.IsNullOrEmpty())
            //{
            //    ChatVM.MessagesVM.Messages.Clear();
            //    ChatVM.MessagesVM.SelectedMessages.Clear();
            //    return;
            //}

            //ChatVM.MessagesVM.Messages.Clear();
            //ChatVM.MessagesVM.SelectedMessages.Clear();
        }

        #endregion

        public MessageFinderViewModel(ChatViewModel chatVM)
        {
            _chatVM = chatVM ?? throw new ArgumentNullException(nameof(chatVM));

            ClearFinderCommand = new RelayCommand(OnClearFinderCommandExecuted, CanClearFinderCommandExecute);
            FindMessagesCommand = new RelayCommand(OnFindMessagesCommandExecuted, CanFindMessagesCommandExecute);
        }
    }
}
