using System;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Infrastructure.AppEventArgs
{
    internal class TaggerEventArgs : EventArgs
    {
        public int SituationId { get; set; }
        public string TagHeader { get; set; }
        public List<int> MessagesIds { get; set; }
    }

    internal delegate void TaggerEventHandler(object sender, TaggerEventArgs args);
}
