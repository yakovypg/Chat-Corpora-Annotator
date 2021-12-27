using ChatCorporaAnnotator.ViewModels.Windows;
using ScintillaNET;
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

            var concordanceViewer = (Scintilla)ConcordanceViewerHost.Child;

            concordanceViewer.ReadOnly = true;
            concordanceViewer.Margins.Left = 1;
            concordanceViewer.Margins.Right = 1;
            concordanceViewer.Margins.Capacity = 5;
            concordanceViewer.IndentationGuides = IndentView.LookBoth;
        }

        private void ChatDataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
                MessageExplorerWindowViewModel.OpenExplorer(row.Item);
        }
    }
}
