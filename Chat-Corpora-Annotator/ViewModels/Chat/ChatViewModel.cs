using ChatCorporaAnnotator.Data.Windows;
using ChatCorporaAnnotator.Data.Windows.Controls;
using ChatCorporaAnnotator.Infrastructure.AppEventArgs;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.Models.Chat.Core;
using ChatCorporaAnnotator.Models.Serialization;
using ChatCorporaAnnotator.ViewModels.Base;
using IndexEngine;
using IndexEngine.Indexes;
using IndexEngine.Paths;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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

        public const string TAG_COLUMN_HEADER = "Tag";

        public ChatScroller Scroller { get; private set; }
        public ObservableCollection<DataGridColumn> ChatColumns { get; private set; }

        public double ChatTextFontSize { get; set; } = 14.0;
        public Thickness ChatTextPadding { get; set; } = new Thickness(5, 3, 5, 3);

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

            var columns = GenerateChatColumns(selectedFields);
            SetChatColumns(columns);
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
            return parameter is SituationData || (MessagesVM.SelectedMessages.Count > 0 && TagsVM.SelectedTag != null);
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
                    Id = sitData.Id,
                    Tag = sitData.Header,
                    MessagesIds = sitData.Messages
                };
            }
            else
            {
                args = new TaggerEventArgs()
                {
                    Id = SituationIndex.GetInstance().GetValueCount(TagsVM.SelectedTag.Header),
                    Tag = TagsVM.SelectedTag.Header,
                    MessagesIds = new List<int>()
                };

                foreach (var msg in MessagesVM.SelectedMessages)
                    args.MessagesIds.Add(msg.Source.Id);
            }

            AddTag(args);
            MainWindowVM.IsProjectChanged = true;
        }

        public ICommand RemoveTagCommand { get; }
        public bool CanRemoveTagCommandExecute(object parameter)
        {
            return MessagesVM.SelectedMessages.Count > 0;
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

                if (msg.Source.Situations.Count == 1)
                {
                    var firstSituationData = msg.Source.Situations.FirstOrDefault();

                    args.Tag = firstSituationData.Key;
                    args.Id = firstSituationData.Value;

                    RemoveTag(args);
                }
                else
                {
                    //todo: remove multitag
                }
            }

            MainWindowVM.IsProjectChanged = true;
        }

        public ICommand RemoveAllTagsCommand { get; }
        public bool CanRemoveAllTagsCommandExecute(object parameter)
        {
            return SituationsVM.TaggedMessagesIds.Count > 0;
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

            MessageFinderVM = new MessageFinderViewModel(this);
            MessagesVM = new MessagesViewModel(SituationsVM.UpdateMessagesTags);

            Scroller = new ChatScroller(MessagesVM.MessagesCase);
            ChatColumns = new ObservableCollection<DataGridColumn>();

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

        public void DeleteOrEditTag(TaggerEventArgs e, bool type, int index = -1)
        {
            RemoveSituationFromMessages(e);

            if (type)
            {
                MainWindowVM.SituationsCount = SituationIndex.GetInstance().ItemCount;
            }
            else if (index == -1)
            {
                EditTag(e);
            }

            RemoveSituation(e);
        }

        private void AddTag(TaggerEventArgs e)
        {
            SituationIndex.GetInstance().AddInnerIndexEntry(e.Tag, e.Id, e.MessagesIds);

            foreach (var msgId in e.MessagesIds)
            {
                ChatMessage msg = MessagesVM.MessagesCase.CurrentMessages.FirstOrDefault(t => t.Source.Id == msgId);

                if (msg == null)
                    continue;

                var situation = new Situation(e.Id, e.Tag);
                msg.AddSituation(situation, TagsVM.CurrentTagset);

                SituationsVM.TaggedMessagesIds.Add(msgId);
            }

            int sitId = SituationIndex.GetInstance().GetValueCount(e.Tag) - 1;
            var sit = new Situation(sitId, e.Tag);

            SituationsVM.AddSituationsCommand?.Execute(sit);
        }

        private void EditTag(TaggerEventArgs e)
        {
            var tag = e.AdditionalInfo["Change"].ToString();
            var count = SituationIndex.GetInstance().GetValueCount(tag);
            var list = SituationIndex.GetInstance().IndexCollection[e.Tag][e.Id];

            SituationIndex.GetInstance().AddInnerIndexEntry(tag, count, list);

            foreach (var id in list)
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

            msg.RemoveSituation(e.Tag, TagsVM.CurrentTagset);

            if (msg.Source.Situations.Count == 0)
                SituationsVM.TaggedMessagesIds.Remove(msg.Source.Id);

            SituationIndex.GetInstance().DeleteMessageFromSituationAndIndex(e.Tag, e.Id, e.MessagesIds[0]);

            if (SituationIndex.GetInstance().GetInnerValueCount(e.Tag, e.Id) == 0)
                DeleteOrEditTag(e, true);
        }

        private void RemoveSituationFromMessages(TaggerEventArgs e)
        {
            foreach (var id in SituationIndex.GetInstance().IndexCollection[e.Tag][e.Id])
            {
                ChatMessage msg = MessagesVM.MessagesCase.CurrentMessages.FirstOrDefault(t => t.Source.Id == id);

                if (msg != null)
                    msg.RemoveSituation(e.Tag, TagsVM.CurrentTagset);
            }
        }

        private void RemoveSituation(TaggerEventArgs e)
        {
            SituationIndex.GetInstance().DeleteInnerIndexEntry(e.Tag, e.Id);
            SituationsVM.RemoveSituationsCommand.Execute(new Situation(e.Id, e.Tag));

            if (e.Id >= SituationIndex.GetInstance().GetValueCount(e.Tag) + 1)
                return;

            for (int i = e.Id + 1; i <= SituationIndex.GetInstance().GetValueCount(e.Tag); ++i)
            {
                var messagesIds = SituationIndex.GetInstance().IndexCollection[e.Tag][i];

                foreach (var msgId in messagesIds)
                {
                    ChatMessage msg = MessagesVM.MessagesCase.CurrentMessages.FirstOrDefault(t => t.Source.Id == msgId);

                    if (msg != null)
                        msg.Source.Situations[e.Tag]--;
                }

                SituationIndex.GetInstance().DeleteInnerIndexEntry(e.Tag, i);
                SituationIndex.GetInstance().AddInnerIndexEntry(e.Tag, i - 1, messagesIds);

                SituationsVM.RemoveSituationsCommand?.Execute(new Situation(i, e.Tag));
                SituationsVM.AddSituationsCommand?.Execute(new Situation(i - 1, e.Tag));
            }
        }

        #endregion

        #region ColumnsUpdatingMethods

        public void UpdateColumnsTemplate()
        {
            var textColumn = ChatColumns.FirstOrDefault(t => t.Header?.ToString() == ProjectInfo.TextFieldKey);

            if (!(textColumn is DataGridTemplateColumn textTemplateColumn))
                return;

            var newTextColumn = CreateHighlightedChatColumn(ProjectInfo.TextFieldKey);
            textTemplateColumn.CellTemplate = newTextColumn.CellTemplate;
        }

        public void SetChatColumns(IEnumerable<DataGridColumn> columns)
        {
            ChatColumns.Clear();

            foreach (var column in columns)
                ChatColumns.Add(column);
        }

        #endregion

        #region ColumnsGenerationMethods

        private DataGridTemplateColumn[] GenerateChatColumns(List<string> selectedFields)
        {
            if (selectedFields.IsNullOrEmpty())
                return new DataGridTemplateColumn[0];

            var fields = new List<string>(selectedFields);

            if (fields.Remove(ProjectInfo.TextFieldKey))
                fields.Insert(0, ProjectInfo.TextFieldKey);

            if (fields.Remove(ProjectInfo.DateFieldKey))
                fields.Insert(0, ProjectInfo.DateFieldKey);

            if (fields.Remove(ProjectInfo.SenderFieldKey))
                fields.Insert(0, ProjectInfo.SenderFieldKey);

            fields.Insert(0, TAG_COLUMN_HEADER);

            var columns = new DataGridTemplateColumn[fields.Count];

            for (int i = 0; i < fields.Count; ++i)
            {
                string currField = fields[i];

                var column = currField == ProjectInfo.TextFieldKey
                    ? CreateHighlightedChatColumn(currField)
                    : CreateDefaultChatColumn(currField);

                columns[i] = column;
            }

            return columns;
        }

        private DataGridTemplateColumn CreateDefaultChatColumn(string fieldKey)
        {
            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetValue(TextBlock.PaddingProperty, ChatTextPadding);
            textBlockFactory.SetValue(TextBlock.FontSizeProperty, ChatTextFontSize);
            textBlockFactory.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);

            if (fieldKey != TAG_COLUMN_HEADER)
                textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding($"Source.Contents[{fieldKey}]"));
            else
                textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding($"TagsPresenter"));

            if (fieldKey == ProjectInfo.SenderFieldKey)
                textBlockFactory.SetBinding(TextBlock.ForegroundProperty, new Binding($"SenderColor"));

            var columnDataTemplate = new DataTemplate(typeof(DynamicMessage))
            {
                VisualTree = textBlockFactory
            };

            var column = new DataGridTemplateColumn()
            {
                Header = fieldKey,
                CellTemplate = columnDataTemplate
            };

            return column;
        }

        private DataGridTemplateColumn CreateHighlightedChatColumn(string fieldKey)
        {
            var textBlockFactory = new FrameworkElementFactory(typeof(HighlightedTextBlock));

            textBlockFactory.SetValue(Control.PaddingProperty, ChatTextPadding);
            textBlockFactory.SetValue(Control.FontSizeProperty, ChatTextFontSize);
            textBlockFactory.SetValue(HighlightedTextBlock.TextWrappingProperty, TextWrapping.Wrap);

            textBlockFactory.SetValue(HighlightedTextBlock.IgnoreCaseProperty, MessageFinderVM.IgnoreCase);
            textBlockFactory.SetValue(HighlightedTextBlock.HighlightBrushProperty, MessageFinderVM.HighlightBrush);
            textBlockFactory.SetValue(HighlightedTextBlock.HighlightedTextProperty, MessageFinderVM.HighlightText);

            if (fieldKey != TAG_COLUMN_HEADER)
                textBlockFactory.SetBinding(HighlightedTextBlock.TextProperty, new Binding($"Source.Contents[{fieldKey}]"));

            if (fieldKey == ProjectInfo.SenderFieldKey)
                textBlockFactory.SetBinding(Control.ForegroundProperty, new Binding($"SenderColor"));

            var columnDataTemplate = new DataTemplate(typeof(DynamicMessage))
            {
                VisualTree = textBlockFactory
            };

            var column = new DataGridTemplateColumn()
            {
                Header = fieldKey,
                CellTemplate = columnDataTemplate
            };

            return column;
        }

        #endregion
    }
}
