using ChatCorporaAnnotator.Data.WinFormsIntegration.AppEventArgs;
using System;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters.Views
{
    public interface ITagView : IView
    {
        event WriteEventHandler WriteToDisk;
        event EventHandler SaveTagged;
        event EventHandler LoadTagged;
        event EventHandler TagsetClick;
        event TaggerEventHandler AddTag;
        event TaggerEventHandler RemoveTag;

        event TaggerEventHandler DeleteSituation;
        event TaggerEventHandler EditSituation;
        event SituationArrayEventHandler MergeSituations;

        event SituationArrayEventHandler CrossMergeSituations;

        event EventHandler ShowSuggester;

        bool SituationsLoaded { get; set; }
        int CurIndex { get; set; }
        //void DisplayTagsInDocuments();
        void DisplayTagsetColors(Dictionary<string, System.Drawing.Color> dict);

        event TaggerEventHandler LoadTagset;

        void ShowDates(List<DateTime> dates);
        //void UpdateTagIndex(List<string> tags);
        void DisplayTagset(List<string> tags);
        void DisplayTagErrorMessage();
        void AddSituationIndexItem(string s);
        void DeleteSituationIndexItem(string s);
        void UpdateSituationCount(int count);
    }
}
