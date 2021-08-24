using System;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.AppEventArgs
{
    public class TaggerEventArgs : EventArgs
    {
        public string Tagset;
        public string Tag;
        public List<int> messages;
        public int Id;
        public Dictionary<string, object> AdditionalInfo;
    }

    public delegate void TaggerEventHandler(object sender, TaggerEventArgs args);
}
