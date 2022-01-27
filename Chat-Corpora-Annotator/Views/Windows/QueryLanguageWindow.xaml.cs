using ChatCorporaAnnotator.ViewModels.Windows;
using System;
using System.Windows;

namespace ChatCorporaAnnotator.Views.Windows
{
    public partial class QueryLanguageWindow : Window
    {
        public QueryLanguageWindow()
        {
            InitializeComponent();
        }

        internal QueryLanguageWindow(QueryLanguageWindowViewModel viewModel)
        {
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
        }
    }
}
