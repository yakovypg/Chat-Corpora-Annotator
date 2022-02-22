using ChatCorporaAnnotator.Infrastructure.Enums;
using System;

namespace ChatCorporaAnnotator.Infrastructure.AppEventArgs
{
    internal class ProjectSavingStateEventArgs : EventArgs
    {
        public SaveProjectState OldState { get; }
        public SaveProjectState NewState { get; }

        public ProjectSavingStateEventArgs(SaveProjectState oldState, SaveProjectState newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }

    internal delegate void ProjectSavingStateEventHandler(object sender, ProjectSavingStateEventArgs args);
}
