using System;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.AppEventArgs
{
    public class UserDictsEventArgs : EventArgs
    {
        public string Name { get; set; }
        public List<string> Words { get; set; }
        public string Word { get; set; }
    }

    public delegate void UserDictsEventHandler(object sender, UserDictsEventArgs args);
}
