using ChatCorporaAnnotator.Data.Windows;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.ViewModels.Base;
using IndexEngine.Paths;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Chat
{
    internal class UsersViewModel : ViewModel
    {
        public ObservableCollection<ChatUser> Users { get; private set; }
        public ObservableCollection<ChatUser> SelectedUsers { get; private set; }

        #region Commands

        public ICommand SetUsersCommand { get; }
        public bool CanSetUsersCommandExecute(object parameter)
        {
            return true;
        }
        public void OnSetUsersCommandExecuted(object parameter)
        {
            if (!CanSetUsersCommandExecute(parameter))
                return;

            IEnumerable<ChatUser> newUsers = parameter is IEnumerable<ChatUser> users
                ? users
                : GetUsers();

            if (newUsers.IsNullOrEmpty())
            {
                Users.Clear();
                return;
            }

            Users = new ObservableCollection<ChatUser>(newUsers);
            OnPropertyChanged(nameof(Users));
        }

        public ICommand AddUsersCommand { get; }
        public bool CanAddUsersCommandExecute(object parameter)
        {
            return parameter is IEnumerable<ChatUser>;
        }
        public void OnAddUsersCommandExecuted(object parameter)
        {
            if (!CanAddUsersCommandExecute(parameter))
                return;

            IEnumerable<ChatUser> addingUsers = parameter as IEnumerable<ChatUser>;

            if (addingUsers.IsNullOrEmpty())
                return;

            Users = new ObservableCollection<ChatUser>(Users.Concat(addingUsers));
            OnPropertyChanged(nameof(Users));
        }

        public ICommand ChangeSelectedUsersCommand { get; }
        public bool CanChangeSelectedUsersCommandExecute(object parameter)
        {
            return parameter is SelectionChangedEventArgs;
        }
        public void OnChangeSelectedUsersCommandExecute(object parameter)
        {
            if (!CanChangeSelectedUsersCommandExecute(parameter))
                return;

            var eventArgs = parameter as SelectionChangedEventArgs;
            var selectedItemOrganizer = new SelectedItemsOrganizer();

            selectedItemOrganizer.InvertAddedItemsSelection<ChatUser>(eventArgs);
        }

        #endregion

        public UsersViewModel()
        {
            Users = new ObservableCollection<ChatUser>();
            SelectedUsers = new ObservableCollection<ChatUser>();

            SetUsersCommand = new RelayCommand(OnSetUsersCommandExecuted, CanSetUsersCommandExecute);
            AddUsersCommand = new RelayCommand(OnAddUsersCommandExecuted, CanAddUsersCommandExecute);
            ChangeSelectedUsersCommand = new RelayCommand(OnChangeSelectedUsersCommandExecute, CanChangeSelectedUsersCommandExecute);
        }

        private IEnumerable<ChatUser> GetUsers()
        {
            IEnumerable<string> userKeys = ProjectInfo.Data.UserKeys;

            return userKeys.IsNullOrEmpty()
                ? new ChatUser[0]
                : userKeys.Select(t => new ChatUser(t));
        }
    }
}
