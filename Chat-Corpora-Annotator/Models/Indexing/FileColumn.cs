using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChatCorporaAnnotator.Models.Indexing
{
    internal class FileColumn : IFileColumn, INotifyPropertyChanged
    {
        public static Action<FileColumn, bool> ChangeSelectedColumnsAction { get; set; } 

        public string Header { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));

                ChangeSelectedColumnsAction?.Invoke(this, value);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public FileColumn(string header)
        {
            Header = header;
        }

        public override string ToString()
        {
            return Header;
        }

        //Do not override Equals and GetHashCode. If you still decide to do this, change the method of changing the selected columns.
    }
}
