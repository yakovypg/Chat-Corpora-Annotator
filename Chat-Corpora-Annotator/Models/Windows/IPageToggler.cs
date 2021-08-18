using System;

namespace ChatCorporaAnnotator.Models.Windows
{
    internal interface IPageToggler
    {
        int PagesCount { get; }
        bool CycleMode { get; set; }
        int CurrentIndex { get; set; }

        void BackPage();
        void NextPage();

        void SetPagesHideActions(Action[] hideActions);
        void SetPagesShowActions(Action[] showActions);
    }
}
