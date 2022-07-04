using ChatCorporaAnnotator.Infrastructure.Extensions.Controls;
using ChatCorporaAnnotator.Views.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ChatCorporaAnnotator.Data.Windows
{
    internal class MainWindowInteract
    {
        private readonly MainWindow _mainWindow;

        public DataGrid ChatDataGrid => _mainWindow.ChatDataGrid;

        public MainWindowInteract()
        {
            var window = WindowFinder.Find(typeof(MainWindow)) as MainWindow;
            _mainWindow = window ?? throw new Exception("MainWindow not found.");
        }

        public MainWindow GetInstance()
        {
            return _mainWindow;
        }

        public Dispatcher GetDispatcher()
        {
            return _mainWindow.Dispatcher;
        }

        public void InvokeAction(Action action)
        {
            _mainWindow.Dispatcher.Invoke(action);
        }

        public void ResetChatDataGridSelectedItems()
        {
            _mainWindow.ChatDataGrid.SelectedItem = null;
        }

        public void ScrollChatDataGridToNearlyTop()
        {
            _mainWindow.ChatDataGrid.ScrollToNearlyTop();
        }

        public void ScrollChatDataGridToTop()
        {
            _mainWindow.ChatDataGrid.ScrollToTop();
        }

        public void ScrollChatDataIntoView(object item)
        {
            _mainWindow.ChatDataGrid.ScrollIntoView(item);
        }

        public void ScrollToVerticalOffset(double offset)
        {
            _mainWindow.ChatDataGrid.ScrollToVerticalOffset(offset);
        }
    }
}
