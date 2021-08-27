using ChatCorporaAnnotator.Models.Chat.Core;
using System;
using System.Windows.Controls;

namespace ChatCorporaAnnotator.Infrastructure.AppEventArgs
{
    internal class ChatScrollingEventArgs : EventArgs
    {
        public ChatPresenterInfo ChatInfo { get; }
        public ScrollChangedEventArgs ScrollEventArgs { get; }

        public ChatScrollingEventArgs(ChatPresenterInfo chatInfo, ScrollChangedEventArgs scrollEventArgs)
        {
            ChatInfo = chatInfo;
            ScrollEventArgs = scrollEventArgs;
        }
    }

    internal delegate void ChatScrollingEventHandler(object sender, ChatScrollingEventArgs args);
}
