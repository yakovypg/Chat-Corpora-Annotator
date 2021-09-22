using ChatCorporaAnnotator.Data.Windows.UI;
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

        #region AddingCommands

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

        #endregion

        #region SelectionCommands

        public ICommand SelectAllUsersCommand { get; }
        public bool CanSelectAllUsersCommandExecute(object parameter)
        {
            return !Users.IsNullOrEmpty();
        }
        public void OnSelectAllUsersCommandExecuted(object parameter)
        {
            if (!CanSelectAllUsersCommandExecute(parameter))
                return;

            var selectedItemsOrganizer = new SelectedItemsOrganizer();
            selectedItemsOrganizer.SelectAll(Users);
        }

        public ICommand DeselectAllUsersCommand { get; }
        public bool CanDeselectAllUsersCommandExecute(object parameter)
        {
            return !Users.IsNullOrEmpty();
        }
        public void OnDeselectAllUsersCommandExecuted(object parameter)
        {
            if (!CanDeselectAllUsersCommandExecute(parameter))
                return;

            var selectedItemsOrganizer = new SelectedItemsOrganizer();
            selectedItemsOrganizer.DeselectAll(Users);
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

            ChatUser.SelectedUsers = SelectedUsers;

            SetUsersCommand = new RelayCommand(OnSetUsersCommandExecuted, CanSetUsersCommandExecute);
            AddUsersCommand = new RelayCommand(OnAddUsersCommandExecuted, CanAddUsersCommandExecute);

            SelectAllUsersCommand = new RelayCommand(OnSelectAllUsersCommandExecuted, CanSelectAllUsersCommandExecute);
            DeselectAllUsersCommand = new RelayCommand(OnDeselectAllUsersCommandExecuted, CanDeselectAllUsersCommandExecute);
            ChangeSelectedUsersCommand = new RelayCommand(OnChangeSelectedUsersCommandExecute, CanChangeSelectedUsersCommandExecute);
        }

        public void ClearData()
        {
            Users.Clear();
            SelectedUsers.Clear();
        }

        private IEnumerable<ChatUser> GetUsers()
        {
            HashSet<string> userKeys = ProjectInfo.Data.UserKeys;
            var userColors = ProjectInfo.Data.UserColors;

            if (userKeys.IsNullOrEmpty())
                return new ChatUser[0];

            if (userColors.IsNullOrEmpty())
                return userKeys.Select(t => new ChatUser(t));

            int userIndex = 0;
            var users = new ChatUser[userKeys.Count];

            foreach (string user in userKeys)
            {
                users[userIndex++] = userColors.TryGetValue(user, out var color)
                    ? new ChatUser(user, color)
                    : new ChatUser(user);
            }

            return users;
        }
    }
}
