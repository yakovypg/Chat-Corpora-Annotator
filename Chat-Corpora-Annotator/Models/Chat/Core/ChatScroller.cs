using ChatCorporaAnnotator.Infrastructure.AppEventArgs;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Infrastructure.Extensions.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChatCorporaAnnotator.Models.Chat.Core
{
    internal class ChatScroller : IChatScroller
    {
        private readonly ChatCache _messagesCase;

        public static int EdgeDeviation = 5;
        public static int ScrollDeviation = 2;
        public static int RetainedItemsMultiplier = 1;

        public static int VerticalCursorDeviation = 96;
        public static int HorizontalCursorDeviation = 12;

        public ChatScroller(ChatCache messagesCase)
        {
            _messagesCase = messagesCase ?? throw new ArgumentNullException(nameof(messagesCase));
        }

        public void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange == 0)
                return;

            var scrollViewer = sender as ScrollViewer;
            var dataGrid = scrollViewer.TemplatedParent as DataGrid;

            var scrollArgs = new ChatScrollingEventArgs(dataGrid, e);
            ScrollMessages(scrollArgs);
        }

        public void ScrollMessages(ChatScrollingEventArgs scrollArgs)
        {
            if (_messagesCase.CurrentPackageCapacity == 0)
                return;

            ScrollChangedEventArgs e = scrollArgs.ScrollEventArgs;

            int extentHeight = (int)e.ExtentHeight;
            int viewportHeight = (int)e.ViewportHeight;
            int verticalOffset = (int)e.VerticalOffset;

            if (verticalOffset == 0)
                return;

            bool isScrollPositive = e.VerticalChange > 0;

            int topEdge = 3;
            int bottomEdge = extentHeight - viewportHeight;
            int retainedItems = 5;
            MessageBox.Show(verticalOffset + "");

            if (isScrollPositive && verticalOffset >= bottomEdge)
            {
                if (!_messagesCase.MoveForward(verticalOffset, retainedItems))
                    return;

                var currMessages = _messagesCase.CurrentPackage;

                if (currMessages.IsNullOrEmpty())
                    return;

                scrollArgs.ChatContainer.RemoveScrollChangedEvent(ScrollChanged);
                scrollArgs.ChatContainer.ScrollToTop();
                scrollArgs.ChatContainer.AddScrollChangedEventAsync(ScrollChanged);

                MoveCursorToTop(scrollArgs.ChatContainer.ActualWidth,
                    scrollArgs.ChatContainer.PointFromScreen(new Point()));
            }
            else if (!isScrollPositive && verticalOffset <= topEdge)
            {
                if (!_messagesCase.MoveBack(verticalOffset, retainedItems))
                    return;

                var currMessages = _messagesCase.CurrentPackage;

                if (currMessages.IsNullOrEmpty())
                    return;

                scrollArgs.ChatContainer.RemoveScrollChangedEvent(ScrollChanged);
                scrollArgs.ChatContainer.ScrollToBottom();
                scrollArgs.ChatContainer.AddScrollChangedEventAsync(ScrollChanged);

                MoveCursorToBottom(scrollArgs.ChatContainer.ActualWidth, scrollArgs.ChatContainer.ActualHeight,
                    scrollArgs.ChatContainer.PointFromScreen(new Point()));
            }
        }

        private void MoveCursorToTop(double actualWidth, Point position)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                return;

            var chatPos = position;

            chatPos.X += actualWidth - HorizontalCursorDeviation;
            chatPos.Y += VerticalCursorDeviation;

            var cursorPos = new System.Drawing.Point((int)chatPos.X, (int)chatPos.Y);
            System.Windows.Forms.Cursor.Position = cursorPos;
        }

        private void MoveCursorToBottom(double actualWidth, double actualHeight, Point position)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                return;

            var chatPos = position;

            chatPos.X += actualWidth - HorizontalCursorDeviation;
            chatPos.Y += actualHeight - VerticalCursorDeviation;

            var cursorPos = new System.Drawing.Point((int)chatPos.X, (int)chatPos.Y);
            System.Windows.Forms.Cursor.Position = cursorPos;
        }
    }
}
