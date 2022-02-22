using System.Windows;

namespace ChatCorporaAnnotator.Views.Windows
{
    public partial class IndexFileWindow : Window
    {
        public IndexFileWindow()
        {
            InitializeComponent();
        }

        internal IndexFileWindow(object dataContext)
        {
            DataContext = dataContext;
            InitializeComponent();
        }
    }
}
