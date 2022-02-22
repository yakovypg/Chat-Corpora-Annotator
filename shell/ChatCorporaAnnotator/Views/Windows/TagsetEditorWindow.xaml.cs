using ChatCorporaAnnotator.ViewModels.Windows;
using System;
using System.Windows;

namespace ChatCorporaAnnotator.Views.Windows
{
    public partial class TagsetEditorWindow : Window
    {
        public TagsetEditorWindow()
        {
            InitializeComponent();
        }

        internal TagsetEditorWindow(TagsetEditorWindowViewModel viewModel)
        {
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
        }
    }
}
