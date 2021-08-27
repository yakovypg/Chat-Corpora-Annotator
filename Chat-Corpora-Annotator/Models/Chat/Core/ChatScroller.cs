using ChatCorporaAnnotator.Infrastructure.AppEventArgs;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using System;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatCorporaAnnotator.Models.Chat.Core
{
    internal class ChatScroller : IChatScroller
    {
        private readonly ChatCache _messagesCase;

        public static int EdgeDeviation = 5;
        public static int ScrollDeviation = 2;
        public static int RetainedItemsMultiplier = 2;

        public static int VerticalCursorDeviation = 96;
        public static int HorizontalCursorDeviation = 12;

        public ChatScroller(ChatCache messagesCase)
        {
            _messagesCase = messagesCase ?? throw new ArgumentNullException(nameof(messagesCase));
        }

        public void ScrollMessages(ChatScrollingEventArgs scrollArgs)
        {
            System.Windows.MessageBox.Show("");
            if (_messagesCase.CurrentPackageCapacity == 0)
                return;

            ChatPresenterInfo chatInfo = scrollArgs.ChatInfo;
            ScrollChangedEventArgs e = scrollArgs.ScrollEventArgs;

            int extentHeight = (int)e.ExtentHeight;
            int viewportHeight = (int)e.ViewportHeight;
            int verticalOffset = (int)e.VerticalOffset;

            if (verticalOffset == 0)
                return;

            bool isScrollPositive = e.VerticalChange > 0;

            int topEdge = extentHeight - viewportHeight - EdgeDeviation;
            int bottomEdge = viewportHeight + 5;
            int retainedItems = viewportHeight * RetainedItemsMultiplier;

            if (isScrollPositive && verticalOffset >= topEdge)
            {
                var currMessages = _messagesCase.MoveForward(verticalOffset, retainedItems);

                if (currMessages.IsNullOrEmpty())
                    return;

                int viewIndex = retainedItems - ScrollDeviation;
                chatInfo.ScrollIntoView(currMessages[viewIndex]);

                MoveCursorToTop(chatInfo);
            }
            else if (!isScrollPositive && verticalOffset <= bottomEdge)
            {
                var currMessages = _messagesCase.MoveBack(verticalOffset, retainedItems);

                if (currMessages.IsNullOrEmpty())
                    return;

                int viewIndex = currMessages.Count - retainedItems + ScrollDeviation;
                chatInfo.ScrollIntoView(currMessages[viewIndex]);

                MoveCursorToBottom(chatInfo);
            }
            else
            {
                //Thread.Sleep(50);
            }
        }

        private void MoveCursorToTop(ChatPresenterInfo chatInfo)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                return;

            var chatPos = chatInfo.Position;

            chatPos.X += chatInfo.ActualWidth - HorizontalCursorDeviation;
            chatPos.Y += VerticalCursorDeviation;

            var cursorPos = new System.Drawing.Point((int)chatPos.X, (int)chatPos.Y);
            System.Windows.Forms.Cursor.Position = cursorPos;
        }

        private void MoveCursorToBottom(ChatPresenterInfo chatInfo)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                return;

            var chatPos = chatInfo.Position;

            chatPos.X += chatInfo.ActualWidth - HorizontalCursorDeviation;
            chatPos.Y += chatInfo.ActualHeight - VerticalCursorDeviation;

            var cursorPos = new System.Drawing.Point((int)chatPos.X, (int)chatPos.Y);
            System.Windows.Forms.Cursor.Position = cursorPos;
        }
    }
}
