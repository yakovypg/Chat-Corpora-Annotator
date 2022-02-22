using System;
using System.ComponentModel;
using System.Windows;

namespace ChatCorporaAnnotator.Models.Windows
{
    internal interface IPageExplorerStep
    {
        string Header { get; set; }

        Func<EventArgs> Show { get; set; }
        Func<CancelEventArgs> Hide { get; set; }
        Action<Visibility> SetVisibility { get; set; }
    }
}
