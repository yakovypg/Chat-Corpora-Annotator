using IndexEngine;
using System;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.AppEventArgs
{
    public class WriteEventArgs : EventArgs
    {
        public List<DynamicMessage> ids = new List<DynamicMessage>();
    }

    public delegate void WriteEventHandler(object sender, WriteEventArgs args);
}
