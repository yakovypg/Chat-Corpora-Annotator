using ChatCorporaAnnotator.ViewModels.Windows;
using System;
using System.Windows;

namespace ChatCorporaAnnotator.Views.Windows
{
    public partial class ExtractorWindow : Window
    {
        public ExtractorWindow()
        {
            InitializeComponent();
        }

        internal ExtractorWindow(ExtractorWindowViewModel viewModel)
        {
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
        }
    }
}
