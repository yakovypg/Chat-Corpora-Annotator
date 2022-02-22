using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace ChatCorporaAnnotator.Infrastructure.Behaviors.Static
{
    internal class DataGridColumnsBehavior
    {
        public static readonly DependencyProperty BindableColumnsProperty = DependencyProperty.RegisterAttached("BindableColumns",
            typeof(ObservableCollection<DataGridColumn>),
            typeof(DataGridColumnsBehavior),
            new UIPropertyMetadata(null, BindableColumnsPropertyChanged));

        private static DataGrid _source;

        private static void BindableColumnsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (!(source is DataGrid dataGrid))
                return;

            if (!(e.NewValue is ObservableCollection<DataGridColumn> columns))
                return;

            _source = dataGrid;
            ResetColumns(columns);

            columns.CollectionChanged += ChangeCollection;
        }

        public static void SetBindableColumns(DependencyObject element, ObservableCollection<DataGridColumn> value)
        {
            element.SetValue(BindableColumnsProperty, value);
        }

        public static ObservableCollection<DataGridColumn> GetBindableColumns(DependencyObject element)
        {
            return element.GetValue(BindableColumnsProperty) as ObservableCollection<DataGridColumn>;
        }

        private static void ChangeCollection(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_source == null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddColumns(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RemoveColumns(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    ResetColumns(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Move:
                    MoveColumn(e.OldStartingIndex, e.NewStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    ReplaceColumn(e.NewStartingIndex, e.NewItems[0] as DataGridColumn);
                    break;
            }
        }

        private static void AddColumns(IList columns)
        {
            if (columns == null)
                return;

            foreach (DataGridColumn column in columns)
                _source.Columns.Add(column);
        }

        private static void RemoveColumns(IList columns)
        {
            if (columns == null)
                return;

            foreach (DataGridColumn column in columns)
                _source.Columns.Remove(column);
        }

        private static void ResetColumns(IList columns)
        {
            _source.Columns.Clear();
            AddColumns(columns);
        }

        private static void MoveColumn(int oldIndex, int newIndex)
        {
            _source.Columns.Move(oldIndex, newIndex);
        }

        private static void ReplaceColumn(int index, DataGridColumn newColumn)
        {
            _source.Columns[index] = newColumn;
        }
    }
}
