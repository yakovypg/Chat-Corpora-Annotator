using System;
using System.ComponentModel;
using System.Windows;

namespace ChatCorporaAnnotator.Models.Windows
{
    internal class PageExplorerStep : IPageExplorerStep
    {
        public string Header { get; set; }

        public Func<EventArgs> Show { get; set; }
        public Func<CancelEventArgs> Hide { get; set; }
        public Action<Visibility> SetVisibility { get; set; }

        public PageExplorerStep(string header = null)
        {
            Header = header;
        }
    }
}
