using ChatCorporaAnnotator.Data.WinFormsIntegration.AppEventArgs;
using ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters.Views;
using ChatCorporaAnnotator.Data.WinFormsIntegration.Services;
using IndexEngine;
using IndexEngine.Indexes;
using IndexEngine.NLP;
using IndexEngine.Paths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters
{
    public class MainPresenter
    {
        private readonly IMainView _main;
        private readonly ISearchService _searcher;
        private readonly ITagView _tagger;
        private readonly ITagService _service;
        private readonly FolderService _folder;
        private readonly IStatisticsService _dataset;
        private readonly ITaggedStatisticsService _corpus;
        private readonly IConcordanceService _concordancer;
        private readonly INGramService _ngrammer;

        public MainPresenter(IMainView view, ITagView tagger, ITagService service, ISearchService searcher, FolderService folder, IStatisticsService dataset, ITaggedStatisticsService corpus, IConcordanceService concordancer, INGramService ngrammer)
        {
            _tagger = tagger;
            _service = service;
            _main = view;
            _searcher = searcher;
            _folder = folder;
            _dataset = dataset;
            _corpus = corpus;
            _concordancer = concordancer;
            _ngrammer = ngrammer;

            _main.FindClick += _view_FindClick;
            _main.OpenIndexedCorpus += _view_OpenIndexedCorpus;
            _main.ConcordanceClick += _main_ConcordanceClick;
            _main.NGramClick += _main_NGramClick;
            //_main.KeywordClick += _main_KeywordClick;
            _main.LoadStatistics += _main_LoadStatistics;
            _main.ExtractInfoClick += _main_ExtractInfoClick;
            _main.BuildIndexClick += _main_BuildIndexClick;
            _main.CheckNgramState += _main_CheckNgramState;
            _main.LoadMore += _main_LoadMore;

            _folder.CheckFolder();
        }

        private void _main_LoadMore(object sender, EventArgs e)
        {
            AddDocumentsToDisplay(2000);
            _main.ShowDates(ProjectInfo.Data.MessagesPerDay.Keys.ToList());
        }

        private void _main_CheckNgramState(object sender, EventArgs e)
        {
            _ngrammer.CheckIndex();
            _main.UpdateNgramState(_ngrammer.IndexExists, _ngrammer.IndexIsRead);
        }

        private void _main_BuildIndexClick(object sender, EventArgs e)
        {
            if (_ngrammer.IndexExists)
            {
                _ngrammer.ReadIndexFromDisk();
                _main.UpdateNgramState(_ngrammer.IndexExists, _ngrammer.IndexIsRead);
            }
            else
            {
                Thread t = new Thread(_ngrammer.BuildFullIndex);
                t.Start();
                _main.UpdateNgramState(_ngrammer.IndexExists, _ngrammer.IndexIsRead);
            }
        }

        private void _main_ExtractInfoClick(object sender, EventArgs e)
        {
            if (!_main.InfoExtracted)
            {
                Extractor.CreatePipeline();
                Extractor.Extract();
                _main.InfoExtracted = true;
            }
            else
            {
                _main.ShowExtractedMessage();
            }
        }

        private void _main_LoadStatistics(object sender, EventArgs e)
        {
            _corpus.CalculateAll();
            _dataset.CalculateAll();

            _main.DisplayStatistics(0, _dataset.AllFields);
            _main.DisplayStatistics(1, _corpus.AllFields);
        }

        //private void _main_KeywordClick(object sender, EventArgs e)
        //{
        //    IKeywordView _keyword = _main.CreateKeywordView();
        //    IKeywordService keywordService = new KeywordService();
        //    KeywordPresenter pres = new KeywordPresenter(_main, keywordService, _keyword);
        //    _keyword.ShowView();
        //    _main.ShowKeywordView(_keyword);
        //}

        private void _main_NGramClick(object sender, NgramEventArgs e)
        {
            var result = _ngrammer.GetReadableResultsForTerm(e.Term);
            _main.DisplayNGrams(result);
        }

        private void _main_ConcordanceClick(object sender, ConcordanceEventArgs e)
        {
            _concordancer.ConQuery = LuceneService.Parser.Parse(e.Term);
            _concordancer.FindConcordance(e.Term, ProjectInfo.TextFieldKey, e.Chars);

            _main.DisplayConcordance(_concordancer.Concordance.ToArray());
        }

        private void _view_OpenIndexedCorpus(object sender, OpenEventArgs e)
        {
            //ProjectInfo.CurrentIndexPath = _main.CurrentIndexPath;
            //ProjectInfo.UnloadData();

            _service.UnloadData();
            SituationIndex.GetInstance().UnloadData();
            ProjectInfo.LoadProject(e.Path);

            if (LuceneService.OpenIndex())
            {
                MessageContainer.Messages = new List<DynamicMessage>();
                _main.SetLineCount(ProjectInfo.Data.LineCount);
                _main.FileLoadState = true;

                AddDocumentsToDisplay(2000);
                _main.ShowDates(ProjectInfo.Data.MessagesPerDay.Keys.ToList());
                //_main.UpdateNgramState(_ngrammer.IndexExists);
            }
            else
            {
                MessageBox.Show("No index");
            }
        }

        public void AddDocumentsToDisplay(int count)
        {
            var list = IndexHelper.LoadNDocumentsFromIndex(count);
            MessageContainer.Messages.AddRange(list);
            _main.DisplayDocuments();
            ShowTags(count);
        }

        private void ShowTags(int count)
        {
            for (int i = _tagger.CurIndex; i < _tagger.CurIndex + count; i++)
            {
                if (_service.TaggedIds.Contains(i))
                {
                    MessageContainer.InsertTagsInDynamicMessage(i, count);
                }
            }
            _tagger.CurIndex += count;
        }

        private void _view_FindClick(object sender, LuceneQueryEventArgs e)
        {
            _searcher.UserQuery = LuceneService.Parser.Parse(e.Query);

            if (!e.FilteredByDate && !e.FilteredByUser)
            {
                _searcher.SearchText(e.Count);
            }
            else if (e.FilteredByDate && !e.FilteredByUser)
            {
                _searcher.ConstructDateFilter(ProjectInfo.DateFieldKey, e.Start, e.Finish);
                _searcher.SearchText_DateFilter(e.Count);
            }
            else if (!e.FilteredByDate && e.FilteredByUser)
            {
                _searcher.ConstructUserFilter(ProjectInfo.SenderFieldKey, e.Users);
                _searcher.SearchText_UserFilter(e.Count);
            }
            else if (e.FilteredByDate && e.FilteredByUser)
            {
                _searcher.ConstructDateFilter(ProjectInfo.DateFieldKey, e.Start, e.Finish);
                _searcher.ConstructUserFilter(ProjectInfo.SenderFieldKey, e.Users);
                _searcher.SearchText_UserDateFilter(e.Count);
            }

            var result = _searcher.MakeSearchResultsReadable();

            _main.SearchResults = result;
            _main.DisplaySearchResults();
        }
    }
}
