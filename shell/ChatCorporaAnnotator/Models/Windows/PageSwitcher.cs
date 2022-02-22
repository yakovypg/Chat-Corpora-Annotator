using System;
using System.Collections.Generic;
using System.Windows;

namespace ChatCorporaAnnotator.Models.Windows
{
    internal class PageSwitcher : IPageSwitcher
    {
        private readonly IList<IPageExplorerStep> _pages;

        public int PagesCount => _pages.Count;
        public IEnumerable<IPageExplorerStep> Pages => _pages;

        public bool CycleMode { get; set; }

        private int _currentIndex;
        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if (value < 0 || value >= PagesCount)
                    throw new IndexOutOfRangeException(nameof(CurrentIndex));

                bool cancelShow = _pages[_currentIndex].Hide?.Invoke().Cancel ?? false;

                if (cancelShow)
                    return;

                _pages[_currentIndex].SetVisibility?.Invoke(Visibility.Hidden);

                _currentIndex = value;

                _pages[_currentIndex].SetVisibility?.Invoke(Visibility.Visible);
                _pages[_currentIndex].Show?.Invoke();
            }
        }

        public PageSwitcher(IEnumerable<IPageExplorerStep> pages, int currentIndex = 0, bool cycleMode = false)
        {
            _pages = pages != null
                ? new List<IPageExplorerStep>(pages)
                : new List<IPageExplorerStep>();

            CycleMode = cycleMode;
            CurrentIndex = currentIndex;
        }

        public int BackPage()
        {
            if (PagesCount == 0)
                return CurrentIndex;

            if (CurrentIndex == 0)
            {
                if (CycleMode)
                    CurrentIndex = PagesCount - 1;

                return CurrentIndex;
            }

            return --CurrentIndex;
        }

        public int NextPage()
        {
            if (PagesCount == 0)
                return CurrentIndex;

            if (CurrentIndex == PagesCount - 1)
            {
                if (CycleMode)
                    CurrentIndex = 0;

                return CurrentIndex;
            }

            return ++CurrentIndex;
        }
    }
}
