using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.ViewModels.Base;
using IndexEngine.Paths;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Chat
{
    internal class ChatViewModel : ViewModel
    {
        private readonly MainWindowViewModel _mainWindowVM;

        public UsersViewModel UsersVM { get; }
        public DatesViewModel DatesVM { get; }
        public MessagesViewModel MessagesVM { get; }
        public TagsViewModel TagsVM { get; }
        public SituationsViewModel SituationsVM { get; }

        private GridView _chatGridView = new GridView();
        public GridView ChatGridView
        {
            get => _chatGridView;
            private set => SetValue(ref _chatGridView, value);
        }

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

            var chatView = new GridView();

            var tagColumn = new GridViewColumn()
            {
                Header = "Tag",
                DisplayMemberBinding = new Binding($"TagCollection.Presenter")
            };

            chatView.Columns.Add(tagColumn);

            foreach (var field in ProjectInfo.Data.SelectedFields)
            {
                var column = new GridViewColumn()
                {
                    Header = field,
                };

                DataTemplate cardLayout = new DataTemplate(typeof(ChatMessage));

                FrameworkElementFactory text = new FrameworkElementFactory(typeof(TextBlock));
                text.SetBinding(TextBlock.TextProperty, new Binding($"Contents[{field}]"));
                text.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
                text.SetValue(FrameworkElement.MaxWidthProperty, 200.0);

                cardLayout.VisualTree = text;
                column.CellTemplate = cardLayout;

                chatView.Columns.Add(column);
            }

            ChatGridView = chatView;
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
