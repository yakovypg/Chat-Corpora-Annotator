using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ChatCorporaAnnotator.Data.Windows
{
    internal class WindowInteract
    {
        public Window Source { get; }

        public WindowInteract(Window window)
        {
            Source = window ?? throw new ArgumentNullException(nameof(window));
        }

        public void MoveToForeground(bool checkMinimized = true)
        {
            if (checkMinimized && Source.WindowState == WindowState.Minimized)
                Source.WindowState = WindowState.Normal;

            Source.Activate();
            Source.Topmost = true;
            Source.Topmost = false;
            Source.Focus();
        }

        public void MoveToForeground(int waitMilliseconds, bool checkMinimized = true)
        {
            Task.Run(delegate
            {
                Thread.Sleep(waitMilliseconds);
                Source.Dispatcher.Invoke(() => MoveToForeground(checkMinimized));
            });
        }

        public void ShowAfter(int waitMilliseconds)
        {
            Task.Run(delegate
            {
                Thread.Sleep(waitMilliseconds);
                Source.Dispatcher.Invoke(() => Source.Show());
            });
        }
    }
}
