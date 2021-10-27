using ChatCorporaAnnotator.ViewModels.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatCorporaAnnotator.Views.Windows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ChatDataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
                MessageExplorerWindowViewModel.OpenExplorer(row.Item);
        }
    }
}
