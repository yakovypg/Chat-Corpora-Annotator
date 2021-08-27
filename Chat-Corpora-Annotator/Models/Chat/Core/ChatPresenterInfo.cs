using System;
using System.Windows;

namespace ChatCorporaAnnotator.Models.Chat.Core
{
    internal class ChatPresenterInfo
    {
        public Point Position { get; set; }
        public double ActualWidth { get; set; }
        public double ActualHeight { get; set; }

        public Action<object> ScrollIntoView { get; set; }

        public ChatPresenterInfo()
        {
        }
    }
}
