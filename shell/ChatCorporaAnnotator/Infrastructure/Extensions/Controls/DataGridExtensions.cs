using ChatCorporaAnnotator.Data.Windows.UI;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ChatCorporaAnnotator.Infrastructure.Extensions.Controls
{
    internal static class DataGridExtensions
    {
        public static Point GetPosition(this DataGrid dataGrid)
        {
            return dataGrid.PointFromScreen(new Point());
        }

        public static Point GetMirroredPosition(this DataGrid dataGrid)
        {
            var pos = dataGrid.PointFromScreen(new Point());
            pos.X *= -1;
            pos.Y *= -1;

            return pos;
        }

        public static void PerformActionAsync(this DataGrid dataGrid, Action action)
        {
            dataGrid.Dispatcher.BeginInvoke(DispatcherPriority.Send, action);
        }

        public static ScrollViewer GetScrollViewer(this DataGrid dataGrid)
        {
            return UIHelper.FindChildren<ScrollViewer>(dataGrid).First();
        }

        public static void AddScrollChangedEvent(this DataGrid dataGrid, ScrollChangedEventHandler scrollEvent)
        {
            dataGrid.GetScrollViewer().ScrollChanged += scrollEvent;
        }

        public static void RemoveScrollChangedEvent(this DataGrid dataGrid, ScrollChangedEventHandler scrollEvent)
        {
            dataGrid.GetScrollViewer().ScrollChanged -= scrollEvent;
        }

        public static void AddScrollChangedEventAsync(this DataGrid dataGrid, ScrollChangedEventHandler scrollEvent)
        {
            dataGrid.PerformActionAsync(() => dataGrid.GetScrollViewer().ScrollChanged += scrollEvent);
        }

        public static void RemoveScrollChangedEventAsync(this DataGrid dataGrid, ScrollChangedEventHandler scrollEvent)
        {
            dataGrid.PerformActionAsync(() => dataGrid.GetScrollViewer().ScrollChanged -= scrollEvent);
        }

        public static void ScrollToVerticalOffset(this DataGrid dataGrid, double offset)
        {
            dataGrid.GetScrollViewer().ScrollToVerticalOffset(offset);
        }

        public static void ScrollToHorizontalOffset(this DataGrid dataGrid, double offset)
        {
            dataGrid.GetScrollViewer().ScrollToHorizontalOffset(offset);
        }

        public static void ScrollToVerticalOffsetAsync(this DataGrid dataGrid, double offset)
        {
            dataGrid.PerformActionAsync(() => dataGrid.ScrollToVerticalOffset(offset));
        }

        public static void ScrollToHorizontalOffsetAsync(this DataGrid dataGrid, double offset)
        {
            dataGrid.PerformActionAsync(() => dataGrid.ScrollToHorizontalOffset(offset));
        }

        public static void ScrollToNearlyTopAsync(this DataGrid dataGrid)
        {
            dataGrid.PerformActionAsync(() => dataGrid.ScrollToNearlyTop());
        }

        public static void ScrollToNearlyBottomAsync(this DataGrid dataGrid)
        {
            dataGrid.PerformActionAsync(() => dataGrid.ScrollToNearlyBottom());
        }

        public static void ScrollToTopAsync(this DataGrid dataGrid)
        {
            dataGrid.PerformActionAsync(() => dataGrid.ScrollToTop());
        }

        public static void ScrollToBottomAsync(this DataGrid dataGrid)
        {
            dataGrid.PerformActionAsync(() => dataGrid.ScrollToBottom());
        }

        public static void ScrollToNearlyTop(this DataGrid dataGrid)
        {
            var scrollViewer = dataGrid.GetScrollViewer();
            scrollViewer.ScrollToTop();
            scrollViewer.LineDown();
        }

        public static void ScrollToNearlyBottom(this DataGrid dataGrid)
        {
            var scrollViewer = dataGrid.GetScrollViewer();
            scrollViewer.ScrollToBottom();
            scrollViewer.LineUp();
        }

        public static void ScrollToTop(this DataGrid dataGrid)
        {
            dataGrid.GetScrollViewer().ScrollToTop();
        }

        public static void ScrollToBottom(this DataGrid dataGrid)
        {
            dataGrid.GetScrollViewer().ScrollToBottom();
        }

        public static void ScrollToHome(this DataGrid dataGrid)
        {
            dataGrid.GetScrollViewer().ScrollToHome();
        }

        public static void ScrollToEnd(this DataGrid dataGrid)
        {
            dataGrid.GetScrollViewer().ScrollToEnd();
        }

        public static void ScrollToLeftEnd(this DataGrid dataGrid)
        {
            dataGrid.GetScrollViewer().ScrollToLeftEnd();
        }

        public static void ScrollToRightEnd(this DataGrid dataGrid)
        {
            dataGrid.GetScrollViewer().ScrollToRightEnd();
        }

        public static void PageUp(this DataGrid dataGrid)
        {
            dataGrid.GetScrollViewer().PageUp();
        }

        public static void PageDown(this DataGrid dataGrid)
        {
            dataGrid.GetScrollViewer().PageDown();
        }

        public static void PageLeft(this DataGrid dataGrid)
        {
            dataGrid.GetScrollViewer().PageLeft();
        }

        public static void PageRight(this DataGrid dataGrid)
        {
            dataGrid.GetScrollViewer().PageRight();
        }

        public static void LineUp(this DataGrid dataGrid)
        {
            dataGrid.GetScrollViewer().LineUp();
        }

        public static void LineDown(this DataGrid dataGrid)
        {
            dataGrid.GetScrollViewer().LineDown();
        }

        public static void LineLeft(this DataGrid dataGrid)
        {
            dataGrid.GetScrollViewer().LineLeft();
        }

        public static void LineRight(this DataGrid dataGrid)
        {
            dataGrid.GetScrollViewer().LineRight();
        }
    }
}
