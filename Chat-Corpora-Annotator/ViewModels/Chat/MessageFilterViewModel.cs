using ChatCorporaAnnotator.Data.Indexing;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.ViewModels.Base;
using System;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Chat
{
    internal class MessageFilterViewModel : ViewModel
    {
        private readonly ChatViewModel _chatVM;

        #region Commands

        public ICommand ClearFilterCommand { get; }
        public bool CanClearFilterCommandExecute(object parameter)
        {
            return true;
        }
        public void OnClearFilterCommandExecuted(object parameter)
        {
            if (!CanClearFilterCommandExecute(parameter))
                return;

            _chatVM.MessagesVM.MessagesCase.Resume();
            _chatVM.SituationsVM.UpdateMessagesTags();
        }

        public ICommand ShowAllTaggedMessagesCommand { get; }
        public bool CanShowAllTaggedMessagesCommandExecute(object parameter)
        {
            return _chatVM.SituationsVM.TaggedMessagesIds.Count > 0;
        }
        public void OnShowAllTaggedMessagesCommandExecuted(object parameter)
        {
            if (!CanShowAllTaggedMessagesCommandExecute(parameter))
                return;

            var foundMessages = IndexInteraction.GetAllTaggedMessages();

            _chatVM.MessagesVM.MessagesCase.Pause(foundMessages);
            _chatVM.SituationsVM.UpdateMessagesTags();
        }

        public ICommand ShowMessagesByTagCommand { get; }
        public bool CanShowMessagesByTagCommandExecute(object parameter)
        {
            return _chatVM.SituationsVM.TaggedMessagesIds.Count > 0 && _chatVM.TagsVM.SelectedTag != null;
        }
        public void OnShowMessagesByTagCommandExecuted(object parameter)
        {
            if (!CanShowMessagesByTagCommandExecute(parameter))
                return;

            string tagHeader = _chatVM.TagsVM.SelectedTag.Header;
            var foundMessages = IndexInteraction.GetMessagesByTag(tagHeader);

            _chatVM.MessagesVM.MessagesCase.Pause(foundMessages);
            _chatVM.SituationsVM.UpdateMessagesTags();
        }

        public ICommand ShowMessagesBySituationCommand { get; }
        public bool CanShowMessagesBySituationCommandExecute(object parameter)
        {
            return _chatVM.SituationsVM.TaggedMessagesIds.Count > 0 && _chatVM.SituationsVM.SelectedSituation != null;
        }
        public void OnShowMessagesBySituationCommandExecuted(object parameter)
        {
            if (!CanShowMessagesBySituationCommandExecute(parameter))
                return;

            var situation = _chatVM.SituationsVM.SelectedSituation;
            var foundMessages = IndexInteraction.GetMessagesBySituation(situation);

            _chatVM.MessagesVM.MessagesCase.Pause(foundMessages);
            _chatVM.SituationsVM.UpdateMessagesTags();
        }

        #endregion

        public MessageFilterViewModel(ChatViewModel chatVM)
        {
            _chatVM = chatVM ?? throw new ArgumentNullException(nameof(chatVM));

            ClearFilterCommand = new RelayCommand(OnClearFilterCommandExecuted, CanClearFilterCommandExecute);
            ShowAllTaggedMessagesCommand = new RelayCommand(OnShowAllTaggedMessagesCommandExecuted, CanShowAllTaggedMessagesCommandExecute);
            ShowMessagesByTagCommand = new RelayCommand(OnShowMessagesByTagCommandExecuted, CanShowMessagesByTagCommandExecute);
            ShowMessagesBySituationCommand = new RelayCommand(OnShowMessagesBySituationCommandExecuted, CanShowMessagesBySituationCommandExecute);
        }
    }
}
