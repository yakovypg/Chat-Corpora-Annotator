using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.ViewModels.Base;
using IndexEngine;
using IndexEngine.Paths;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Chat
{
    internal class ChatViewModel : ViewModel
    {
        private const double CHAT_COLUMN_MIN_WIDTH = 50;
        private const double CHAT_COLUMN_MAX_WIDTH = 500;

        private readonly MainWindowViewModel _mainWindowVM;

        public UsersViewModel UsersVM { get; }
        public DatesViewModel DatesVM { get; }
        public MessagesViewModel MessagesVM { get; }
        public TagsViewModel TagsVM { get; }
        public SituationsViewModel SituationsVM { get; }

        public ObservableCollection<DataGridColumn> ChatGataGridColumns { get; private set; }

        #region ChatViewCommands

        public ICommand UpdateChatViewCommand { get; }
        public bool CanUpdateChatViewCommandExecute(object parameter)
        {
            return true;
        }
        public void OnUpdateChatViewCommandExecuted(object parameter)
        {
            if (!CanUpdateChatViewCommandExecute(parameter))
                return;

            var tagColumn = new DataGridTextColumn()
            {
                Header = "Tag",
                MinWidth = CHAT_COLUMN_MIN_WIDTH,
                MaxWidth = CHAT_COLUMN_MAX_WIDTH
            };

            ChatGataGridColumns.Add(tagColumn);

            foreach (var field in ProjectInfo.Data.SelectedFields)
            {
                DataTemplate columnDataTemplate = new DataTemplate(typeof(DynamicMessage));

                var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
                textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding($"Contents[{field}]"));
                textBlockFactory.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);

                columnDataTemplate.VisualTree = textBlockFactory;

                var column = new DataGridTemplateColumn()
                {
                    Header = field,
                    MinWidth = CHAT_COLUMN_MIN_WIDTH,
                    MaxWidth = CHAT_COLUMN_MAX_WIDTH,
                    CellTemplate = columnDataTemplate
                };

                ChatGataGridColumns.Add(column);
            }
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
            ChatGataGridColumns = new System.Collections.ObjectModel.ObservableCollection<DataGridColumn>();
            _mainWindowVM = mainWindowVM ?? throw new ArgumentNullException(nameof(mainWindowVM));

            UsersVM = new UsersViewModel();
            DatesVM = new DatesViewModel();
            MessagesVM = new MessagesViewModel();

            TagsVM = new TagsViewModel(_mainWindowVM);
            SituationsVM = new SituationsViewModel(_mainWindowVM);

            UpdateChatViewCommand = new RelayCommand(OnUpdateChatViewCommandExecuted, CanUpdateChatViewCommandExecute);

            AddTagCommand = new RelayCommand(OnAddTagCommandExecuted, CanAddTagCommandExecute);
            RemoveTagCommand = new RelayCommand(OnRemoveTagCommandExecuted, CanRemoveTagCommandExecute);
        }
    }
}
