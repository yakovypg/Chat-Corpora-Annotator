using System;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.AppEventArgs
{
    public class SituationArrayEventArgs : EventArgs
    {
        public List<TaggerEventArgs> args = new List<TaggerEventArgs>();
    }

    public delegate void SituationArrayEventHandler(object sender, SituationArrayEventArgs args);
}
