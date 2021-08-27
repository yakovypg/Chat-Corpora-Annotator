using ChatCorporaAnnotator.Infrastructure.AppEventArgs;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.Models.Chat.Core;
using ChatCorporaAnnotator.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ChatCorporaAnnotator.Views.Windows
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel MainWindowVM;

        public MainWindow()
        {
            InitializeComponent();
            MainWindowVM = DataContext as MainWindowViewModel;
        }

        private void ChatDataGridRow_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!(sender is DataGridRow row))
                return;

            MessageExplorerWindowViewModel.OpenExplorer(row.Item);
        }

        private void ChatDataGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var chatInfo = new ChatPresenterInfo()
            {
                ActualWidth = ChatDataGrid.ActualWidth,
                ActualHeight = ChatDataGrid.ActualHeight,
                Position = ChatDataGrid.PointToScreen(new Point()),
                ScrollIntoView = t => ChatDataGrid.ScrollIntoView(t)
            };

            var scrollArgs = new ChatScrollingEventArgs(chatInfo, e);

            MainWindowVM.ChatVM.Scroller.ScrollMessages(scrollArgs);
        }

        private void SetMsg(System.Collections.Generic.IEnumerable<ChatMessage> messages)
        {
            ChatDataGrid.ItemsSource = messages;
        }
    }
}
