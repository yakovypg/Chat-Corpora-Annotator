using ChatCorporaAnnotator.Data.WinFormsIntegration.AppEventArgs;
using ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters.Parsers;
using ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters.Views;
using System;
using System.IO;
using System.Linq;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters
{
    public class SuggestPresenter
    {
        private readonly ISuggesterView _sugg;
        private readonly ITagView _tagger;
        private readonly IMainView _main;

        public SuggestPresenter(ISuggesterView sugg, ITagView tagger, IMainView main)
        {
            _sugg = sugg;
            _tagger = tagger;
            _main = main;

            _tagger.ShowSuggester += _tagger_ShowSuggester;
            _sugg.RunQuery += _sugg_RunQuery;
            _sugg.ShowMessageInMainWindow += _sugg_ShowMessageInMainWindow;
            _sugg.ImportQueryFile += _sugg_ImportQueryFile;
        }

        private void _sugg_ImportQueryFile(object sender, OpenEventArgs args)
        {
            _sugg.ImportedQueries = File.ReadAllLines(args.FilePath).ToList();
            _sugg.SetImportLabel(_sugg.ImportedQueries.Count);
        }

        private void _sugg_ShowMessageInMainWindow(object sender, FindEventArgs args)
        {
            _main.EnsureMessageIsVisible(args.id);
        }

        private void _sugg_RunQuery(object sender, EventArgs e)
        {
            //_service.Parse(_sugg.QueryString);
            //Run Parser from here
            _sugg.DisplayIndex = 0;
            //_sugg.GroupIndex = 0;

            if (_sugg.QueryResult != null)
            {
                _sugg.QueryResult.Clear();
            }

            _sugg.QueryResult = Parser.parse(_sugg.QueryString);
            _sugg.SetCounts();
            _sugg.DisplaySituation();
        }

        private void _tagger_ShowSuggester(object sender, EventArgs e)
        {
            //if(_main.InfoExtracted)
            //{
            //    _sugg.ShowView();
            //}
            //else
            //{
            //    _main.ShowSorryMessage();
            //}

            //Testing the window!!!
            _sugg.ShowView();
        }
    }
}
