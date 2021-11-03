using ChatCorporaAnnotator.Infrastructure.Extensions.Controls;
using ChatCorporaAnnotator.Views.Windows;
using System;

namespace ChatCorporaAnnotator.Data.Windows
{
    internal class MainWindowInteract
    {
        private readonly MainWindow _mainWindow;

        public MainWindowInteract()
        {
            var window = new WindowFinder().Find(typeof(MainWindow)) as MainWindow;
            _mainWindow = window ?? throw new Exception("MainWindow not found.");
        }

        public MainWindow GetInstance()
        {
            return _mainWindow;
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
