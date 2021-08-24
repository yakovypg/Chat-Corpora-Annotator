using ChatCorporaAnnotator.Data.WinFormsIntegration.AppEventArgs;
using System.Collections.Generic;
using System.Drawing;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters.Views
{
    public interface ITagsetView : IView
    {
        event TagsetUpdateEventHandler AddNewTagset;
        event TagsetUpdateEventHandler DeleteTagset;
        event TagsetUpdateEventHandler UpdateTagset;
        event TagsetUpdateEventHandler LoadExistingTagset;

        event TagsetUpdateEventHandler SetProjectTagset;
        void DisplayTagsetNames(List<string> names);
        void DisplayTagset(Dictionary<string,Color> tags);
        void DisplayProjectTagsetName(string name);
    }
}
