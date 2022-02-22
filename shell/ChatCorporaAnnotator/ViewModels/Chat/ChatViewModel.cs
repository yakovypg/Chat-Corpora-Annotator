using ChatCorporaAnnotator.Data.Windows;
using ChatCorporaAnnotator.Infrastructure.AppEventArgs;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.Models.Chat.Core;
using ChatCorporaAnnotator.Models.Serialization;
using ChatCorporaAnnotator.ViewModels.Base;
using ChatCorporaAnnotator.ViewModels.Windows;
using IndexEngine.Data.Paths;
using IndexEngine.Indexes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Chat
{
    internal class ChatViewModel : ViewModel
    {
        #region ViewModels

        public MainWindowViewModel MainWindowVM { get; }

        public UsersViewModel UsersVM { get; }
        public DatesViewModel DatesVM { get; }
        public MessagesViewModel MessagesVM { get; }
        public MessageFilterViewModel MessageFilterVM { get; }
        public MessageFinderViewModel MessageFinderVM { get; }
        public TagsViewModel TagsVM { get; }
        public SituationsViewModel SituationsVM { get; }

        #endregion

        #region ItemsVisibilities

        private Visibility _activeDatesWaitingImageGridVisibility = Visibility.Visible;
        public Visibility ActiveDatesWaitingImageGridVisibility
        {
            get => _activeDatesWaitingImageGridVisibility;
            set => SetValue(ref _activeDatesWaitingImageGridVisibility, value);
        }

        #endregion

        #region ChatView

        private readonly ChatColumnCreator _chatColumnCreator;

        public ChatScroller Scroller { get; private set; }
        public ObservableCollection<DataGridColumn> ChatColumns { get; private set; }

        #endregion

        #region ChatViewCommands

        public ICommand SetChatColumnsCommand { get; }
        public bool CanSetChatColumnsCommandExecute(object parameter)
        {
            return true;
        }
        public void OnSetChatColumnsCommandExecuted(object parameter)
        {
            if (!CanSetChatColumnsCommandExecute(parameter))
                return;

            List<string> selectedFields = parameter is List<string> fields
                ? fields
                : ProjectInfo.Data.SelectedFields;

            var columns = _chatColumnCreator.GenerateChatColumns(selectedFields, true, true);

            ChatColumns.Clear();

            foreach (var column in columns)
                ChatColumns.Add(column);
        }

        #endregion

        #region DataCommands

        public ICommand ResetDataCommand { get; }
        public bool CanResetDataCommandExecute(object parameter)
        {
            return true;
        }
        public void OnResetDataCommandExecuted(object parameter)
        {
            if (!CanResetDataCommandExecute(parameter))
                return;

            ClearData();
            SetChatColumnsCommand.Execute(null);

            MessagesVM.MessagesCase.Reset();
            TagsVM.SetTagsetCommand.Execute(null);
            DatesVM.SetAllActiveDatesCommand.Execute(null);
            SituationsVM.SetSituationsCommand.Execute(null);
            UsersVM.SetUsersCommand.Execute(null);

            MainWindowVM.MessagesCount = ProjectInfo.Data.LineCount;
        }

        public ICommand ShiftChatPageCommand { get; }
        public bool CanShiftChatPageCommandExecute(object parameter)
        {
            return parameter is int;
        }
        public void OnShiftChatPageCommandExecuted(object parameter)
        {
            if (!CanShiftChatPageCommandExecute(parameter))
                return;

            int shiftIndex = (int)parameter;
            MessagesVM.MessagesCase.Shift(shiftIndex, out int scrollIndex);

            var mainWindowInteract = new MainWindowInteract();
            mainWindowInteract.ScrollToVerticalOffset(scrollIndex);
        }

        #endregion

        #region TagCommands

        public ICommand AddTagCommand { get; }
        public bool CanAddTagCommandExecute(object parameter)
        {
            return !MainWindowVM.StatisticsVM.IsStatisticsCaulculatingActive &&
                   (parameter is SituationData || (MessagesVM.SelectedMessages.Count > 0 && TagsVM.SelectedTag != null));
        }
        public void OnAddTagCommandExecuted(object parameter)
        {
            if (!CanAddTagCommandExecute(parameter))
                return;

            TaggerEventArgs args;

            if (parameter is SituationData sitData)
            {
                args = new TaggerEventArgs()
                {
                    SituationId = sitData.Id,
                    TagHeader = sitData.Header,
                    MessagesIds = sitData.Messages
                };
            }
            else
            {
                args = new TaggerEventArgs()
                {
                    SituationId = SituationIndex.GetInstance().GetValueCount(TagsVM.SelectedTag.Header),
                    TagHeader = TagsVM.SelectedTag.Header,
                    MessagesIds = new List<int>()
                };

                foreach (var msg in MessagesVM.SelectedMessages)
                {
                    if (!msg.Source.Situations.ContainsKey(args.TagHeader))
                        args.MessagesIds.Add(msg.Source.Id);
                }
            }

            if (args.MessagesIds.Count == 0)
                return;

            AddTag(args);
            MainWindowVM.IsProjectChanged = true;
        }

        public ICommand RemoveTagCommand { get; }
        public bool CanRemoveTagCommandExecute(object parameter)
        {
            return !MainWindowVM.StatisticsVM.IsStatisticsCaulculatingActive && MessagesVM.SelectedMessages.Count > 0;
        }
        public void OnRemoveTagCommandExecuted(object parameter)
        {
            if (!CanRemoveTagCommandExecute(parameter))
                return;

            var args = new TaggerEventArgs();

            foreach (var msg in MessagesVM.SelectedMessages)
            {
                if (msg.Source.Situations.Count == 0)
                    continue;

                args.MessagesIds = new List<int> { msg.Source.Id };

                foreach (var situation in msg.Source.Situations.ToArray())
                {
                    args.TagHeader = situation.Key;
                    args.SituationId = situation.Value;

                    RemoveTag(args);
                }
            }

            MainWindowVM.IsProjectChanged = true;
        }

        public ICommand RemoveAllTagsCommand { get; }
        public bool CanRemoveAllTagsCommandExecute(object parameter)
        {
            return !MainWindowVM.StatisticsVM.IsStatisticsCaulculatingActive && SituationsVM.TaggedMessagesIds.Count > 0;
        }
        public void OnRemoveAllTagsCommandExecuted(object parameter)
        {
            if (!CanRemoveAllTagsCommandExecute(parameter))
                return;

            foreach (var msg in MessagesVM.MessagesCase.CurrentMessages)
            {
                msg.RemoveAllSituations();
            }

            SituationsVM.Situations.Clear();
            SituationsVM.TaggedMessagesIds.Clear();

            SituationIndex.GetInstance().UnloadData();
            MainWindowVM.IsProjectChanged = true;
        }

        #endregion

        public ChatViewModel(MainWindowViewModel mainWindowVM)
        {
            MainWindowVM = mainWindowVM ?? throw new ArgumentNullException(nameof(mainWindowVM));

            UsersVM = new UsersViewModel();
            DatesVM = new DatesViewModel(this);

            TagsVM = new TagsViewModel(MainWindowVM);
            SituationsVM = new SituationsViewModel(MainWindowVM);

            MessageFilterVM = new MessageFilterViewModel(this);
            MessageFinderVM = new MessageFinderViewModel(this);
            MessagesVM = new MessagesViewModel(SituationsVM.UpdateMessagesTags);

            Scroller = new ChatScroller(MessagesVM.MessagesCase);
            ChatColumns = new ObservableCollection<DataGridColumn>();

            _chatColumnCreator = new ChatColumnCreator();

            SetChatColumnsCommand = new RelayCommand(OnSetChatColumnsCommandExecuted, CanSetChatColumnsCommandExecute);

            ResetDataCommand = new RelayCommand(OnResetDataCommandExecuted, CanResetDataCommandExecute);
            ShiftChatPageCommand = new RelayCommand(OnShiftChatPageCommandExecuted, CanShiftChatPageCommandExecute);

            AddTagCommand = new RelayCommand(OnAddTagCommandExecuted, CanAddTagCommandExecute);
            RemoveTagCommand = new RelayCommand(OnRemoveTagCommandExecuted, CanRemoveTagCommandExecute);
            RemoveAllTagsCommand = new RelayCommand(OnRemoveAllTagsCommandExecuted, CanRemoveAllTagsCommandExecute);
        }

        #region DataMethods

        public void ClearData()
        {
            TagsVM.ClearData();
            DatesVM.ClearData();
            SituationsVM.ClearData();
            MessagesVM.ClearData();
            UsersVM.ClearData();

            ChatColumns.Clear();
        }

        #endregion

        #region TagsMethods

        public void ShiftSituationsIds(string tag, int startId)
        {
            for (int i = startId; i <= SituationIndex.GetInstance().GetValueCount(tag); ++i)
            {
                var messagesIds = SituationIndex.GetInstance().IndexCollection[tag][i];

                foreach (var msgId in messagesIds)
                {
                    ChatMessage msg = MessagesVM.MessagesCase.CurrentMessages.FirstOrDefault(t => t.Source.Id == msgId);

                    if (msg != null)
                        msg.Source.Situations[tag]--;
                }

                SituationIndex.GetInstance().DeleteInnerIndexEntry(tag, i);
                SituationIndex.GetInstance().AddInnerIndexEntry(tag, i - 1, messagesIds);

                SituationsVM.RemoveSituationsCommand?.Execute(new Situation(i, tag));
                SituationsVM.AddSituationsCommand?.Execute(new Situation(i - 1, tag));
            }
        }

        public void DeleteTag(TaggerEventArgs e)
        {
            RemoveSituationFromMessages(e);
            MainWindowVM.SituationsCount = SituationIndex.GetInstance().ItemCount;
            RemoveSituation(e);
        }

        public void ChangeSituationTag(TagEditingEventArgs e)
        {
            RemoveSituationFromMessages(e);
            EditTag(e);
            RemoveSituation(e);
        }

        private void AddTag(TaggerEventArgs e)
        {
            SituationIndex.GetInstance().AddInnerIndexEntry(e.TagHeader, e.SituationId, e.MessagesIds);

            foreach (var msgId in e.MessagesIds)
            {
                ChatMessage msg = MessagesVM.MessagesCase.CurrentMessages.FirstOrDefault(t => t.Source.Id == msgId);

                if (msg == null)
                    continue;

                var situation = new Situation(e.SituationId, e.TagHeader);
                msg.AddSituation(situation, TagsVM.CurrentTagset);

                SituationsVM.TaggedMessagesIds.Add(msgId);
            }

            int sitId = SituationIndex.GetInstance().GetValueCount(e.TagHeader) - 1;
            var sit = new Situation(sitId, e.TagHeader);

            SituationsVM.AddSituationsCommand?.Execute(sit);
        }

        private void EditTag(TagEditingEventArgs e)
        {
            string tag = e.NewHeader;
            int count = SituationIndex.GetInstance().GetValueCount(tag);
            List<int> msgIds = SituationIndex.GetInstance().IndexCollection[e.TagHeader][e.SituationId];

            SituationIndex.GetInstance().AddInnerIndexEntry(tag, count, msgIds);

            foreach (var id in msgIds)
            {
                ChatMessage msg = MessagesVM.MessagesCase.CurrentMessages.FirstOrDefault(t => t.Source.Id == id);

                if (msg == null)
                    continue;

                var sit = new Situation(count, tag);
                msg.AddSituation(sit, TagsVM.CurrentTagset);
            }

            SituationsVM.AddSituationsCommand.Execute(new Situation(count, tag));
        }

        private void RemoveTag(TaggerEventArgs e)
        {
            ChatMessage msg = MessagesVM.MessagesCase.CurrentMessages.FirstOrDefault(t => t.Source.Id == e.MessagesIds[0]);

            if (msg == null)
                return;

            msg.RemoveSituation(e.TagHeader, TagsVM.CurrentTagset);

            if (msg.Source.Situations.Count == 0)
                SituationsVM.TaggedMessagesIds.Remove(msg.Source.Id);

            SituationIndex.GetInstance().DeleteMessageFromSituationAndIndex(e.TagHeader, e.SituationId, e.MessagesIds[0]);

            if (SituationIndex.GetInstance().GetInnerValueCount(e.TagHeader, e.SituationId) == 0)
                DeleteTag(e);
        }

        private void RemoveSituationFromMessages(TaggerEventArgs e)
        {
            foreach (var id in SituationIndex.GetInstance().IndexCollection[e.TagHeader][e.SituationId])
            {
                ChatMessage msg = MessagesVM.MessagesCase.CurrentMessages.FirstOrDefault(t => t.Source.Id == id);

                if (msg != null)
                    msg.RemoveSituation(e.TagHeader, TagsVM.CurrentTagset);
            }
        }

        private void RemoveSituation(TaggerEventArgs e)
        {
            SituationIndex.GetInstance().DeleteInnerIndexEntry(e.TagHeader, e.SituationId);
            SituationsVM.RemoveSituationsCommand.Execute(new Situation(e.SituationId, e.TagHeader));

            if (e.SituationId >= SituationIndex.GetInstance().GetValueCount(e.TagHeader) + 1)
                return;

            ShiftSituationsIds(e.TagHeader, e.SituationId + 1);
        }

        #endregion

        #region ChatColumnMethods

        public void UpdateColumnsTemplate()
        {
            var textColumn = ChatColumns.FirstOrDefault(t => t.Header?.ToString() == ProjectInfo.TextFieldKey);

            if (!(textColumn is DataGridTemplateColumn textTemplateColumn))
                return;

            var newTextColumn = _chatColumnCreator.CreateHighlightedChatColumn(ProjectInfo.TextFieldKey,
                MessageFinderVM.HighlightText, MessageFinderVM.IgnoreCase, MessageFinderVM.HighlightBrush);

            textTemplateColumn.CellTemplate = newTextColumn.CellTemplate;
        }

        #endregion
    }
}
