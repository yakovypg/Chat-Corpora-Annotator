using ChatCorporaAnnotator.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ChatCorporaAnnotator.Views.Windows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DataGridRow_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!(sender is DataGridRow row))
                return;

            MessageExplorerWindowViewModel.OpenExplorer(row.Item);
        }
    }
}
