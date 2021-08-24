using System;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.AppEventArgs
{
    public class ConcordanceEventArgs : EventArgs
    {
        public string Term;
        public int Chars;
    }

    public delegate void ConcordanceEventHandler(object sender, ConcordanceEventArgs args);
}
