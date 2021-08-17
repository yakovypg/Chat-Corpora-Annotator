using System.Windows;

namespace ChatCorporaAnnotator.Models.Messages
{
    internal interface IQuickMessage
    {
        string Message { get; set; }

        MessageBoxResult ShowEmpty(MessageBoxButton msgButton = MessageBoxButton.OK);
        MessageBoxResult ShowError(MessageBoxButton msgButton = MessageBoxButton.OK);
        MessageBoxResult ShowWarning(MessageBoxButton msgButton = MessageBoxButton.OK);
        MessageBoxResult ShowQuestion(MessageBoxButton msgButton = MessageBoxButton.YesNo);
        MessageBoxResult ShowInformation(MessageBoxButton msgButton = MessageBoxButton.OK);
    }
}
