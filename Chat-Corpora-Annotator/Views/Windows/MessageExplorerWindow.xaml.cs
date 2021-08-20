using System.Windows;

namespace ChatCorporaAnnotator.Views.Windows
{
    public partial class MessageExplorerWindow : Window
    {
        public MessageExplorerWindow()
        {
            InitializeComponent();
        }

        internal MessageExplorerWindow(object dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
        }
    }
}
