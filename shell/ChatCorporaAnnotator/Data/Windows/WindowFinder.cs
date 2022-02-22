using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ChatCorporaAnnotator.Data.Windows
{
    internal class WindowFinder
    {
        public Window Find(Type windowType)
        {
            return windowType == null || Application.Current == null || Application.Current.Windows == null
                ? null
                : Application.Current.Windows.Cast<Window>().FirstOrDefault(t => t.GetType() == windowType);
        }

        public Window Find(Window window, IEqualityComparer<Window> comparer)
        {
            return window == null || Application.Current == null || Application.Current.Windows == null
                ? null
                : Application.Current.Windows.Cast<Window>().FirstOrDefault(t => comparer.Equals(t, window));
        }
    }
}
