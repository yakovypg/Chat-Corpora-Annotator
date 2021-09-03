using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ChatCorporaAnnotator.Data.Windows
{
    internal static class UIHelper
    {
        public static IList<T> FindChildren<T>(DependencyObject element) where T : FrameworkElement
        {
            var children = new List<T>();
            int childrenCount = VisualTreeHelper.GetChildrenCount(element);

            for (int i = 0; i < childrenCount; ++i)
            {
                if (!(VisualTreeHelper.GetChild(element, i) is FrameworkElement child))
                    continue;

                if (child is T correctlyTyped)
                    children.Add(correctlyTyped);
                else
                    children.AddRange(FindChildren<T>(child));
            }

            return children;
        }

        public static T FindParent<T>(DependencyObject element) where T : FrameworkElement
        {
            var parent = VisualTreeHelper.GetParent(element) as FrameworkElement;

            while (parent != null)
            {
                return parent is T correctlyTyped
                    ? correctlyTyped
                    : FindParent<T>(parent);
            }

            return null;
        }
    }
}
