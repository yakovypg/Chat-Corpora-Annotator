using System.Collections.Generic;

namespace ChatCorporaAnnotator.Models.Windows
{
    internal interface IPageSwitcher
    {
        int PagesCount { get; }
        IEnumerable<IPageExplorerStep> Pages { get; }

        bool CycleMode { get; set; }
        int CurrentIndex { get; set; }

        int BackPage();
        int NextPage();
    }
}
