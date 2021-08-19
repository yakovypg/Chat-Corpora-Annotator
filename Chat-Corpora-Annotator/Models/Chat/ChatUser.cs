using ChatCorporaAnnotator.Data.Imaging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace ChatCorporaAnnotator.Models.Chat
{
    internal class ChatUser : IChatUser, INotifyPropertyChanged
    {
        public static ICollection<ChatUser> SelectedUsers { get; set; }

        public string Name { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                    return;

                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));

                if (value)
                    AddToSelectedUsers();
                else
                    RemoveFromSelectedUsers();
            }
        }

        public System.Drawing.Color BackgroundColor { get; set; }
        public Brush BackgroundBrush => new SolidColorBrush(ColorTransformer.ToWindowsColor(BackgroundColor));

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public ChatUser(string name) : this(name, System.Drawing.Color.Transparent)
        {
        }

        public ChatUser(string name, System.Drawing.Color color)
        {
            Name = name;
            BackgroundColor = color;
        }

        private void AddToSelectedUsers()
        {
            if (SelectedUsers != null)
                SelectedUsers.Add(this);
        }

        private void RemoveFromSelectedUsers()
        {
            if (SelectedUsers != null && SelectedUsers.Contains(this))
                SelectedUsers.Remove(this);
        }
    }
}
