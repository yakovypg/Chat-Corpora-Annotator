using ChatCorporaAnnotator.Models.Indexing.Comparers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ChatCorporaAnnotator.Models.Indexing
{
    internal class FileColumn : IFileColumn, INotifyPropertyChanged
    {
        public static bool AcceptRepeatedColumns { get; set; }
        public static ICollection<FileColumn> SelectedColumns { get; set; }

        public string Header { get; }

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
                    AddToSelectedColumns();
                else
                    RemoveFromSelectedColumns();
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

        // Do not override Equals and GetHashCode. If you still decide to do this, change the method of changing the selected columns.

        private void AddToSelectedColumns()
        {
            if (SelectedColumns == null)
                return;

            var comparer = new FileColumnEqualityComparer();

            if (SelectedColumns.Contains(this, comparer) && !AcceptRepeatedColumns)
                return;

            SelectedColumns.Add(this);
        }

        private void RemoveFromSelectedColumns()
        {
            if (SelectedColumns != null && SelectedColumns.Contains(this))
                SelectedColumns.Remove(this);
        }
    }
}
