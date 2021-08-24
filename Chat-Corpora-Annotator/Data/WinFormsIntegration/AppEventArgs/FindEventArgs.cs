using System;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.AppEventArgs
{
    public class FindEventArgs : EventArgs
    {
        public int id;
    }

    public delegate void FindEventHandler(object sender, FindEventArgs args);
}
