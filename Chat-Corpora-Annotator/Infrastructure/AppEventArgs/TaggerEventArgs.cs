using System;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Infrastructure.AppEventArgs
{
    internal class TaggerEventArgs : EventArgs
    {
        public int Id { get; set; }
        public string Tag { get; set; }
        public string Tagset { get; set; }

        public List<int> MessagesIds { get; set; }
        public Dictionary<string, object> AdditionalInfo { get; set; }
    }

    internal delegate void TaggerEventHandler(object sender, TaggerEventArgs args);
}
