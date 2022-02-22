using System;
using System.Windows.Controls;

namespace ChatCorporaAnnotator.Infrastructure.AppEventArgs
{
    internal class ChatScrollingEventArgs : EventArgs
    {
        public DataGrid ChatContainer { get; }
        public ScrollChangedEventArgs ScrollEventArgs { get; }

        public ChatScrollingEventArgs(DataGrid chatContainer, ScrollChangedEventArgs scrollEventArgs)
        {
            ChatContainer = chatContainer;
            ScrollEventArgs = scrollEventArgs;
        }
    }

    internal delegate void ChatScrollingEventHandler(object sender, ChatScrollingEventArgs args);
}
