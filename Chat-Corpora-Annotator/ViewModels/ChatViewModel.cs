using ChatCorporaAnnotator.Data.Windows;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels
{
    internal class ChatViewModel : ViewModel
    {
        public MainWindowViewModel MainWindowVM { get; }

        public ObservableCollection<object> CurrentChatItems { get; private set; }
        public ObservableCollection<object> ChatSelectedItems { get; private set; }

        public ObservableCollection<object> ActiveDates { get; private set; }
        public ObservableCollection<object> Situations { get; private set; }

        #region TagCommands

        public ICommand AddTagCommand { get; }
        public bool CanAddTagCommandExecute(object parameter)
        {
            return ChatSelectedItems.Count > 0;
        }
        public void OnAddTagCommandExecuted(object parameter)
        {
            if (!CanAddTagCommandExecute(parameter))
                return;
        }

        public ICommand RemoveTagCommand { get; }
        public bool CanRemoveTagCommandExecute(object parameter)
        {
            return ChatSelectedItems.Count > 0;
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

            selectedItemsOrganizer.ChangeSelectedItems(ChatSelectedItems, eventArgs);
        }

        #endregion

        public ChatViewModel(MainWindowViewModel mainWindowVM)
        {
            MainWindowVM = mainWindowVM ?? throw new ArgumentNullException(nameof(mainWindowVM));

            CurrentChatItems = new ObservableCollection<object>();
            ChatSelectedItems = new ObservableCollection<object>();

            ActiveDates = new ObservableCollection<object>();
            Situations = new ObservableCollection<object>();

            AddTagCommand = new RelayCommand(OnAddTagCommandExecuted, CanAddTagCommandExecute);
            RemoveTagCommand = new RelayCommand(OnRemoveTagCommandExecuted, CanRemoveTagCommandExecute);

            ChangeChatSelectedItemsCommand = new RelayCommand(OnChangeChatSelectedItemsCommandExecute, CanChangeChatSelectedItemsCommandExecute);
        }
    }
}
