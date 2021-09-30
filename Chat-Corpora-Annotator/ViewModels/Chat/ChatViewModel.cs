using ChatCorporaAnnotator.Data.Windows.Controls;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models.Chat.Core;
using ChatCorporaAnnotator.ViewModels.Base;
using IndexEngine;
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

        #region TagCommands

        public ICommand AddTagCommand { get; }
        public bool CanAddTagCommandExecute(object parameter)
        {
            return MessagesVM.SelectedMessages.Count > 0;
        }
        public void OnAddTagCommandExecuted(object parameter)
        {
            if (!CanAddTagCommandExecute(parameter))
                return;
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
        }

        #endregion

        public ChatViewModel(MainWindowViewModel mainWindowVM)
        {
            MainWindowVM = mainWindowVM ?? throw new ArgumentNullException(nameof(mainWindowVM));

            UsersVM = new UsersViewModel();
            DatesVM = new DatesViewModel();

            TagsVM = new TagsViewModel(MainWindowVM);
            SituationsVM = new SituationsViewModel(MainWindowVM);

            MessageFinderVM = new MessageFinderViewModel(this);
            MessagesVM = new MessagesViewModel(this);

            Scroller = new ChatScroller(MessagesVM.MessagesCase);
            ChatColumns = new ObservableCollection<DataGridColumn>();

            SetChatColumnsCommand = new RelayCommand(OnSetChatColumnsCommandExecuted, CanSetChatColumnsCommandExecute);

            AddTagCommand = new RelayCommand(OnAddTagCommandExecuted, CanAddTagCommandExecute);
            RemoveTagCommand = new RelayCommand(OnRemoveTagCommandExecuted, CanRemoveTagCommandExecute);
        }

        public void ClearData()
        {
            TagsVM.ClearData();
            DatesVM.ClearData();
            SituationsVM.ClearData();
            MessagesVM.ClearData();
            UsersVM.ClearData();

            ChatColumns.Clear();
        }

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

        public void ResetChatColumns(IEnumerable<DataGridColumn> columns)
        {
            ChatColumns = new ObservableCollection<DataGridColumn>(columns);
            OnPropertyChanged(nameof(ChatColumns));
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
