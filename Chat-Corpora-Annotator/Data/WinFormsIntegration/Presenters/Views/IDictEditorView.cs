using ChatCorporaAnnotator.Data.WinFormsIntegration.AppEventArgs;
using System;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters.Views
{
    public interface IDictEditorView: IView
    {
        event UserDictsEventHandler AddWord;
        event UserDictsEventHandler DeleteWord;
        event UserDictsEventHandler ClearDict;
        event UserDictsEventHandler AddDict;
        event UserDictsEventHandler DeleteDict;
        event UserDictsEventHandler DictDoubleClick;
        event OpenEventHandler ImportDict;

        event EventHandler EditorClosing;
        void LoadDicts(List<string> keys);
        void LoadDict(List<string> words);
        void ClearDictPane();
        void UpdateDictView(string key);
        void UpdateWordView(string key);

        void UpdateWordViewList(List<string> keys);
        void UpdateDictViewList(List<string> keys);
    }
}
