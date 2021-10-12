using ChatCorporaAnnotator.Infrastructure.AppEventArgs;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Enums;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.Models.Messages;
using ChatCorporaAnnotator.Services;
using ChatCorporaAnnotator.ViewModels.Base;
using IndexEngine;
using IndexEngine.Paths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace ChatCorporaAnnotator.ViewModels.Chat
{
    internal class MessageFinderViewModel : ViewModel
    {
        private readonly ChatViewModel _chatVM;
        private readonly ISearchService _messageSearcher;

        #region HighlightOptions

        public bool IgnoreCase { get; set; } = true;
        public string HighlightText { get; set; } = string.Empty;
        public Brush HighlightBrush { get; set; } = Brushes.Gold;

        #endregion

        #region FindOptions

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

            HighlightText = string.Empty;

            _chatVM.UpdateColumnsTemplate();
            _chatVM.MessagesVM.MessagesCase.Resume();
            _chatVM.SituationsVM.UpdateMessagesTags();
        }

        public ICommand FindMessagesCommand { get; }
        public bool CanFindMessagesCommandExecute(object parameter)
        {
            return _chatVM.MainWindowVM.ProjectFileLoadState == FileLoadState.Loaded;
        }
        public void OnFindMessagesCommandExecuted(object parameter)
        {
            if (!CanFindMessagesCommandExecute(parameter))
                return;

            if (string.IsNullOrEmpty(Query))
            {
                new QuickMessage("Query is empty.").ShowInformation();
                return;
            }

            int messagesCount = LuceneService.DirReader.MaxDoc;

            DateTime[] dates = null;
            string[] selectedUsers = null;

            var selectedChatUsers = _chatVM.UsersVM.SelectedUsers;

            if (!selectedChatUsers.IsNullOrEmpty())
                selectedUsers = selectedChatUsers.Select(t => t.Name).ToArray();

            if (IsStartDateChecked || IsEndDateChecked)
            {
                dates = new DateTime[2];
                dates[0] = IsStartDateChecked ? StartDate : DateTime.MinValue;
                dates[1] = IsEndDateChecked ? EndDate : DateTime.MaxValue;
            }

            var args = new LuceneQueryEventArgs(Query, messagesCount, selectedUsers, dates);
            var foundMessages = FindMessages(args).Select(t => new ChatMessage(t)).ToArray();

            if (foundMessages.Length == 0)
            {
                string text = GetQueryTextCore();

                foundMessages = _chatVM.MessagesVM.MessagesCase.CurrentMessages
                    .Where(t => t.Text.Contains(text)).ToArray();
            }

            _chatVM.MessagesVM.MessagesCase.Pause(foundMessages);

            UpdateHighlightRules();
            _chatVM.SituationsVM.UpdateMessagesTags();
        }

        #endregion

        public MessageFinderViewModel(ChatViewModel chatVM)
        {
            _chatVM = chatVM ?? throw new ArgumentNullException(nameof(chatVM));
            _messageSearcher = new SearchService();

            ClearFinderCommand = new RelayCommand(OnClearFinderCommandExecuted, CanClearFinderCommandExecute);
            FindMessagesCommand = new RelayCommand(OnFindMessagesCommandExecuted, CanFindMessagesCommandExecute);
        }

        private string[] SplitQuery()
        {
            char[] separators = { ',', '"', ':' };
            return Query.Trim().Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        private string GetQueryTextCore()
        {
            string[] words = SplitQuery();
            return words.LastOrDefault();
        }

        private void UpdateHighlightRules()
        {
            HighlightText = GetQueryTextCore();
            _chatVM.UpdateColumnsTemplate();
        }

        private List<DynamicMessage> FindMessages(LuceneQueryEventArgs e)
        {
            _messageSearcher.UserQuery = LuceneService.Parser.Parse(e.Query);

            if (!e.FilteredByDate && !e.FilteredByUser)
            {
                _messageSearcher.SearchText(e.MessagesCount);
            }
            else if (e.FilteredByDate && !e.FilteredByUser)
            {
                _messageSearcher.ConstructDateFilter(ProjectInfo.DateFieldKey, e.StartDate, e.EndDate);
                _messageSearcher.SearchText_DateFilter(e.MessagesCount);
            }
            else if (!e.FilteredByDate && e.FilteredByUser)
            {
                _messageSearcher.ConstructUserFilter(ProjectInfo.SenderFieldKey, e.Users);
                _messageSearcher.SearchText_UserFilter(e.MessagesCount);
            }
            else if (e.FilteredByDate && e.FilteredByUser)
            {
                _messageSearcher.ConstructDateFilter(ProjectInfo.DateFieldKey, e.StartDate, e.EndDate);
                _messageSearcher.ConstructUserFilter(ProjectInfo.SenderFieldKey, e.Users);
                _messageSearcher.SearchText_UserDateFilter(e.MessagesCount);
            }

            return _messageSearcher.MakeSearchResultsReadable();
        }
    }
}
