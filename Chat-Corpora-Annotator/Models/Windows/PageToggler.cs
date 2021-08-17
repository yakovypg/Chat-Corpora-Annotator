using ChatCorporaAnnotator.Infrastructure.Extensions;
using System;
using System.Linq;
using System.Windows;

namespace ChatCorporaAnnotator.Models.Windows
{
    internal class PageToggler : IPageToggler
    {
        private readonly Action<Visibility>[] _pageVisibilitySetters;

        public int PagesCount { get; }
        public bool CycleMode { get; set; }

        private int _currentIndex;
        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if (value < 0 || value >= PagesCount)
                    throw new IndexOutOfRangeException(nameof(CurrentIndex));

                _pageVisibilitySetters[_currentIndex].Invoke(Visibility.Hidden);
                _pageVisibilitySetters[value].Invoke(Visibility.Visible);

                _currentIndex = value;
            }
        }

        public PageToggler(Action<Visibility>[] pageVisibilitySetters, int currentIndex = 0, bool cycleMode = false)
        {
            if (pageVisibilitySetters.IsNullOrEmpty())
                throw new ArgumentException("The number of page visibility setters must be greater than zero.");

            int pagesCount = pageVisibilitySetters.Count();

            if (currentIndex < 0 || currentIndex >= pagesCount)
                throw new IndexOutOfRangeException(nameof(currentIndex));

            _pageVisibilitySetters = pageVisibilitySetters;

            PagesCount = pagesCount;
            CycleMode = cycleMode;
            CurrentIndex = currentIndex;
        }

        public void BackPage()
        {
            if (CurrentIndex == 0)
            {
                if (CycleMode)
                    CurrentIndex = PagesCount - 1;

                return;
            }

            CurrentIndex--;
        }

        public void NextPage()
        {
            if (CurrentIndex == PagesCount - 1)
            {
                if (CycleMode)
                    CurrentIndex = 0;

                return;
            }

            CurrentIndex++;
        }
    }
}
