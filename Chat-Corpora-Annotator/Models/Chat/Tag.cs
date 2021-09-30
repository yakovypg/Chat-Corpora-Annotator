using ChatCorporaAnnotator.Data.Imaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace ChatCorporaAnnotator.Models.Chat
{
    internal class Tag : ITag, INotifyPropertyChanged
    {
        public Brush BackgroundBrush => new SolidColorBrush(ColorTransformer.ToWindowsColor(BackgroundColor));

        private string _header;
        public string Header
        {
            get => _header;
            set
            {
                _header = value;
                OnPropertyChanged(nameof(Header));
            }
        }

        private System.Drawing.Color _backgroundColor;
        public System.Drawing.Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;

                OnPropertyChanged(nameof(BackgroundColor));
                OnPropertyChanged(nameof(BackgroundBrush));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public Tag(string header) : this(header, System.Drawing.Color.Transparent)
        {
        }

        public Tag(string header, System.Drawing.Color color)
        {
            Header = header;
            BackgroundColor = color;
        }
    }
}
