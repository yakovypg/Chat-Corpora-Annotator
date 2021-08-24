using ChatCorporaAnnotator.Data.WinFormsIntegration.AppEventArgs;
using ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters.Views;
using IndexEngine.Indexes;
using IndexEngine.Paths;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters
{
    public class DictEditorPresenter
    {
        private readonly IDictEditorView _editor;
        private readonly ISuggesterView _main;

        public DictEditorPresenter(IDictEditorView editor, ISuggesterView main)
        {
            _main = main;
            _editor = editor;

            _main.ShowDictEditor += _main_ShowDictEditor;
            _editor.ImportDict += _editor_ImportDict;
            _editor.DeleteWord += _editor_DeleteWord;
            _editor.DeleteDict += _editor_DeleteDict;
            _editor.ClearDict += _editor_ClearDict;
            _editor.AddDict += _editor_AddDict;
            _editor.AddWord += _editor_AddWord;
            _editor.DictDoubleClick += _editor_DictDoubleClick;
            _editor.EditorClosing += _editor_EditorClosing;


            if (File.Exists(ToolInfo.UserDictsPath))
            {
                UserDictsIndex.GetInstance().ReadIndexFromDisk();

                foreach (var kvp in UserDictsIndex.GetInstance().IndexCollection)
                {
                    _main.DisplayUserDict(kvp.Key, kvp.Value);
                }

                _editor.LoadDicts(UserDictsIndex.GetInstance().IndexCollection.Keys.ToList());
            }
        }

        private void _editor_EditorClosing(object sender, EventArgs e)
        {
            UserDictsIndex.GetInstance().FlushIndexToDisk();
        }

        private void _editor_DictDoubleClick(object sender, UserDictsEventArgs args)
        {
            _editor.LoadDict(UserDictsIndex.GetInstance().IndexCollection[args.Name]);
        }

        private void _editor_AddWord(object sender, UserDictsEventArgs args)
        {
            if (!UserDictsIndex.GetInstance().IndexCollection[args.Name].Contains(args.Word))
            {
                UserDictsIndex.GetInstance().IndexCollection[args.Name].Add(args.Word);
                _main.UpdateUserDict(args.Name, UserDictsIndex.GetInstance().IndexCollection[args.Name]);
                _editor.UpdateWordView(args.Word);
            }
            else
            {
                MessageBox.Show("Word already in list");
            }
        }

        private void _editor_AddDict(object sender, UserDictsEventArgs args)
        {
            if (!UserDictsIndex.GetInstance().IndexCollection.Keys.Contains(args.Name))
            {
                UserDictsIndex.GetInstance().AddIndexEntry(args.Name, new List<string>());
                _main.DisplayUserDict(args.Name, new List<string>());
                _editor.UpdateDictView(args.Name);
            }
        }

        private void _editor_ClearDict(object sender, UserDictsEventArgs args)
        {
            _editor.ClearDictPane();
            UserDictsIndex.GetInstance().IndexCollection[args.Name].Clear();
            _main.UpdateUserDict(args.Name, new List<string>());
        }

        private void _editor_DeleteDict(object sender, UserDictsEventArgs args)
        {
            _editor.ClearDictPane();
            UserDictsIndex.GetInstance().DeleteIndexEntry(args.Name);
            _main.DeleteUserDictFromPreview(args.Name);
        }

        private void _editor_DeleteWord(object sender, UserDictsEventArgs args)
        {
            UserDictsIndex.GetInstance().IndexCollection[args.Name].Remove(args.Word);
            _main.UpdateUserDict(args.Name, UserDictsIndex.GetInstance().IndexCollection[args.Name]);
            _editor.UpdateWordViewList(UserDictsIndex.GetInstance().IndexCollection[args.Name]);
        }

        private void _editor_ImportDict(object sender, OpenEventArgs args)
        {
            var res = UserDictsIndex.GetInstance().ImportNewDictFromFile(args.FilePath);

            if (res != null)
            {
                _main.DisplayUserDict(res, UserDictsIndex.GetInstance().IndexCollection[res]);
                _editor.UpdateDictView(res);
            }
        }

        private void _main_ShowDictEditor(object sender, EventArgs e)
        {
            _editor.ShowView();
        }
    }
}
