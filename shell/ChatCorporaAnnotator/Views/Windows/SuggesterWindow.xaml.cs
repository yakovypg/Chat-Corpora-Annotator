using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.ViewModels.Windows;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatCorporaAnnotator.Views.Windows
{
    public partial class SuggesterWindow : Window
    {
        public SuggesterWindow()
        {
            InitializeComponent();
        }

        internal SuggesterWindow(SuggesterWindowViewModel viewModel)
        {
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
        }

        private void MessagesDataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is DataGridRow row))
                return;

            if (!(DataContext is SuggesterWindowViewModel viewModel))
                return;

            if (!(row.Item is ChatMessage message))
                return;

            viewModel.MainWindowVM.ChatVM.ShiftChatPageCommand.Execute(message.Source.Id);
        }
    }
}
