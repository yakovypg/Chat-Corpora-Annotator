using System.Windows;

namespace ChatCorporaAnnotator.Models.Messages
{
    internal class QuickMessage : IQuickMessage
    {
        public string Message { get; set; }

        public QuickMessage(string message = null)
        {
            Message = message;
        }

        public MessageBoxResult ShowEmpty(MessageBoxButton msgButton = MessageBoxButton.OK)
        {
            return MessageBox.Show(Message, string.Empty, msgButton, MessageBoxImage.None);
        }

        public MessageBoxResult ShowError(MessageBoxButton msgButton = MessageBoxButton.OK)
        {
            return MessageBox.Show(Message, "Error", msgButton, MessageBoxImage.Error);
        }

        public MessageBoxResult ShowWarning(MessageBoxButton msgButton = MessageBoxButton.OK)
        {
            return MessageBox.Show(Message, "Warning", msgButton, MessageBoxImage.Warning);
        }

        public MessageBoxResult ShowQuestion(MessageBoxButton msgButton = MessageBoxButton.YesNo)
        {
            return MessageBox.Show(Message, "Question", msgButton, MessageBoxImage.Question);
        }

        public MessageBoxResult ShowInformation(MessageBoxButton msgButton = MessageBoxButton.OK)
        {
            return MessageBox.Show(Message, "Information", msgButton, MessageBoxImage.Information);
        }
    }
}
