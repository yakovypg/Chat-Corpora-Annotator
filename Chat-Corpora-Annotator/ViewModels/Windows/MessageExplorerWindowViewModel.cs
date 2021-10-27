using ChatCorporaAnnotator.Data.Windows;
using ChatCorporaAnnotator.Infrastructure.Commands;
using ChatCorporaAnnotator.Models.Chat;
using ChatCorporaAnnotator.Models.Messages;
using ChatCorporaAnnotator.ViewModels.Base;
using ChatCorporaAnnotator.Views.Windows;
using IndexEngine.Paths;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ChatCorporaAnnotator.ViewModels.Windows
{
    internal class MessageExplorerWindowViewModel : ViewModel
    {
        public static bool IsWarningsEnable { get; set; } = true;
        public static int MaxOpenedWindowCount { get; set; } = 5;
        public static uint CurrentFreeID { get; private set; } = 0;

        public static SortedDictionary<uint, MessageExplorerWindow> OpenedWindows { get; private set; } =
            new SortedDictionary<uint, MessageExplorerWindow>();

        public uint ID { get; }
        public string Text { get; }
        public string Sender { get; }
        public ChatMessage SourceMessage { get; }

        #region Commands

        public ICommand ExplorerClosingCommand { get; }
        public bool CanExplorerClosingCommandExecute(object parameter)
        {
            return true;
        }
        public void OnExplorerClosingCommandExecuted(object parameter)
        {
            if (!CanExplorerClosingCommandExecute(parameter))
                return;

            OpenedWindows.Remove(ID);
        }

        #endregion

        public MessageExplorerWindowViewModel(uint id, ChatMessage message)
        {
            if (message == null)
                return;

            message.Source.Contents.TryGetValue(ProjectInfo.TextFieldKey, out object text);
            message.Source.Contents.TryGetValue(ProjectInfo.SenderFieldKey, out object sender);
            message.Source.Contents.TryGetValue(ProjectInfo.DateFieldKey, out object sentDate);

            ID = id;
            SourceMessage = message;
            Text = text?.ToString();
            Sender = sender?.ToString();

            ExplorerClosingCommand = new RelayCommand(OnExplorerClosingCommandExecuted, CanExplorerClosingCommandExecute);
        }

        public static void OpenExplorer(object msg)
        {
            if (!(msg is ChatMessage chatMessage))
                return;

            if (OpenedWindows.Count >= MaxOpenedWindowCount)
            {
                if (!IsWarningsEnable)
                    return;

                var msgRes = new QuickMessage("The maximum number of open windows has been reached. Close the first one?").ShowQuestion();

                if (msgRes == MessageBoxResult.No || OpenedWindows.Count == 0)
                    return;

                var firstWindow = OpenedWindows.First().Value;
                firstWindow.Close();
            }

            var vm = new MessageExplorerWindowViewModel(CurrentFreeID, chatMessage);
            var window = new MessageExplorerWindow(vm);

            OpenedWindows.Add(CurrentFreeID++, window);
            new WindowInteract(window).ShowAfter(100);
        }

        public static void CloseAllExplorers()
        {
            while (OpenedWindows.Count > 0)
            {
                var window = OpenedWindows.First().Value;
                window.Close();
            }
        }
    }
}
