using System;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.AppEventArgs
{
    public class TagsetUpdateEventArgs : EventArgs
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public string Tag { get; set; }
    }

    public delegate void TagsetUpdateEventHandler(object sender, TagsetUpdateEventArgs args);
}

