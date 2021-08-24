using ChatCorporaAnnotator.Data.WinFormsIntegration.AppEventArgs;
using IndexEngine;
using System;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters.Views
{
    public interface ISuggesterView : IView
    {
        void DisplaySituation();

        event EventHandler RunQuery;
        event OpenEventHandler ImportQueryFile;
        //event OpenEventHandler ImportUserDict;

        event EventHandler ShowDictEditor;
        void DisplayUserDict(string key, List<string> value);
        void UpdateUserDict(string key, List<string> value);

        void DeleteUserDictFromPreview(string key);
        string QueryString { get; set; }
        List<DynamicMessage> CurrentSituation { get; set; }
        List<List<List<int>>> QueryResult { get; set; }

        List<string> ImportedQueries { get; set; }
        int DisplayIndex { get; set; }
        int GroupIndex { get; set; }
        //Dictionary<string, List<string>> UserDicts { get; set; }
        void SetCounts();
        void SetImportLabel(int count,int num=1);

        event FindEventHandler ShowMessageInMainWindow;
    }
}
