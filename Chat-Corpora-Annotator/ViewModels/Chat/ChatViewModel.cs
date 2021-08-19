using ChatCorporaAnnotator.Data.Windows;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.ViewModels.Base;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Chat
{
    internal class ChatViewModel : ViewModel
    {
        private readonly MainWindowViewModel _mainWindowVM;

        public TagsViewModel TagsVM { get; }
        public DatesViewModel DatesVM { get; }
        public SituationsViewModel SituationsVM { get; }
        public MessagesViewModel MessagesVM { get; }

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

        #region ChatCommands

        public ICommand ChangeChatSelectedItemsCommand { get; }
        public bool CanChangeChatSelectedItemsCommandExecute(object parameter)
        {
            return parameter is SelectionChangedEventArgs;
        }
        public void OnChangeChatSelectedItemsCommandExecute(object parameter)
        {
            if (!CanChangeChatSelectedItemsCommandExecute(parameter))
                return;

            var eventArgs = parameter as SelectionChangedEventArgs;
            var selectedItemsOrganizer = new SelectedItemsOrganizer();

            selectedItemsOrganizer.ChangeSelectedItems(MessagesVM.SelectedMessages, eventArgs);
        }

        #endregion

        public ChatViewModel(MainWindowViewModel mainWindowVM)
        {
            _mainWindowVM = mainWindowVM ?? throw new ArgumentNullException(nameof(mainWindowVM));

            DatesVM = new DatesViewModel();
            MessagesVM = new MessagesViewModel();
            TagsVM = new TagsViewModel(_mainWindowVM);
            SituationsVM = new SituationsViewModel(_mainWindowVM);

            AddTagCommand = new RelayCommand(OnAddTagCommandExecuted, CanAddTagCommandExecute);
            RemoveTagCommand = new RelayCommand(OnRemoveTagCommandExecuted, CanRemoveTagCommandExecute);

            ChangeChatSelectedItemsCommand = new RelayCommand(OnChangeChatSelectedItemsCommandExecute, CanChangeChatSelectedItemsCommandExecute);
        }
    }
}
