using ChatCorporaAnnotator.Data.Indexing;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var grouppedMessages = GroupMessages(foundMessages);

            _chatVM.MessagesVM.MessagesCase.Pause(grouppedMessages);
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
            var grouppedMessages = GroupMessages(foundMessages);

            _chatVM.MessagesVM.MessagesCase.Pause(grouppedMessages);
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
            var grouppedMessages = GroupMessages(foundMessages);

            _chatVM.MessagesVM.MessagesCase.Pause(grouppedMessages);
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

        private IEnumerable<ChatMessage> GroupMessages(IEnumerable<ChatMessage> messages)
        {
            List<ChatMessage> output = new List<ChatMessage>();

            if (messages.IsNullOrEmpty())
                return output;

            var groups = messages.GroupBy(t => t.Source.Situations.First());

            foreach (var group in groups)
            {
                output.AddRange(group);
                output.Add(new ChatMessage());
            }

            if (output.Count != 0)
                output.RemoveAt(output.Count - 1);

            return output;
        }
    }
}
