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

        public void ScrollChatDataGridToNearlyTop()
        {
            _mainWindow.ChatDataGrid.ScrollToNearlyTop();
        }

        public void ScrollChatDataGridToTop()
        {
            _mainWindow.ChatDataGrid.ScrollToTop();
        }
    }
}
