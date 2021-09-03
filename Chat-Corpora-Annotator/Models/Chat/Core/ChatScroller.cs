using ChatCorporaAnnotator.Data.Windows;
using ChatCorporaAnnotator.Infrastructure.AppEventArgs;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Infrastructure.Extensions.Controls;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ChatCorporaAnnotator.Models.Chat.Core
{
    internal class ChatScroller : IChatScroller
    {
        private const int TOP_CURSOR_DEVIATION = 45;
        private const int BOTTOM_CURSOR_DEVIATION = 20;
        private const int HORIZONTAL_CURSOR_DEVIATION = 12;
        private const int RETAINED_ITEMS_COUNT = 2;

        private readonly ChatCache _messagesCase;

        public ChatScroller(ChatCache messagesCase)
        {
            _messagesCase = messagesCase ?? throw new ArgumentNullException(nameof(messagesCase));
            _messagesCase.RetainedItemsCount = RETAINED_ITEMS_COUNT;
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
            if (_messagesCase.CurrentPackageItemsCount == 0)
                return;

            ScrollChangedEventArgs e = scrollArgs.ScrollEventArgs;

            int extentHeight = (int)e.ExtentHeight;
            int viewportHeight = (int)e.ViewportHeight;
            int verticalOffset = (int)e.VerticalOffset;

            if (e.VerticalChange == 0)
                return;

            bool isScrollPositive = e.VerticalChange > 0;

            int topEdge = 0;
            int bottomEdge = extentHeight - viewportHeight;

            if (isScrollPositive && verticalOffset >= bottomEdge)
            {
                var currMessages = _messagesCase.MoveForward(out int pageStartIndex);

                if (currMessages.IsNullOrEmpty())
                    return;

                scrollArgs.ChatContainer.RemoveScrollChangedEvent(ScrollChanged);
                scrollArgs.ChatContainer.ScrollToVerticalOffset(pageStartIndex - 1);
                scrollArgs.ChatContainer.AddScrollChangedEventAsync(ScrollChanged);

                double actualWidth = scrollArgs.ChatContainer.ActualWidth;
                var pos = scrollArgs.ChatContainer.GetMirroredPosition();

                if (MouseInteract.IsLeftPressed)
                    MoveCursorToTop(actualWidth, pos);
            }
            else if (!isScrollPositive && verticalOffset <= topEdge)
            {
                var currMessages = _messagesCase.MoveBack(out int pageStartIndex);

                if (currMessages.IsNullOrEmpty())
                    return;

                scrollArgs.ChatContainer.RemoveScrollChangedEvent(ScrollChanged);
                scrollArgs.ChatContainer.ScrollToVerticalOffset(pageStartIndex);
                scrollArgs.ChatContainer.AddScrollChangedEventAsync(ScrollChanged);

                double actualWidth = scrollArgs.ChatContainer.ActualWidth;
                double actualHeight = scrollArgs.ChatContainer.ActualHeight;
                var pos = scrollArgs.ChatContainer.GetMirroredPosition();

                if (MouseInteract.IsLeftPressed)
                    MoveCursorToBottom(actualWidth, actualHeight, pos);
            }
        }

        private void MoveCursorToTop(double chatWidth, Point position)
        {
            int x = (int)(position.X + chatWidth - HORIZONTAL_CURSOR_DEVIATION);
            int y = (int)(position.Y + TOP_CURSOR_DEVIATION);

            MouseInteract.MoveCursor(x, y);
        }

        private void MoveCursorToBottom(double chatWidth, double chatHeight, Point position)
        {
            int x = (int)(position.X + chatWidth - HORIZONTAL_CURSOR_DEVIATION);
            int y = (int)(position.Y + chatHeight - BOTTOM_CURSOR_DEVIATION);

            MouseInteract.MoveCursor(x, y);
        }
    }
}
