using ChatCorporaAnnotator.Data.WinFormsIntegration.AppEventArgs;
using CSharpTest.Net.Collections;
using IndexEngine;
using System;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters.Views
{
    public interface IMainView : IView
    {
        bool FileLoadState { get; set; }
        List<DynamicMessage> SearchResults { get; set; }
        bool InfoExtracted { get; set; }

        void EnsureMessageIsVisible(int id);

        // will be replaced by ShowProjectData()
        void SetLineCount(int count);
        void SetTagsetLabel(string tagset);
        void ShowDates(List<DateTime> dates);
        void DisplayDocuments();
        void DisplaySearchResults();
        void DisplayStatistics(int type, Dictionary<string, double> args);

        //IKeywordView CreateKeywordView();
        //void ShowKeywordView(IKeywordView key);

        void ShowSorryMessage();
        void ShowExtractedMessage();

        void DisplayConcordance(string[] con);
        void DisplayNGrams(List<BTreeDictionary<string, int>> grams);
        event EventHandler CheckNgramState;
        void UpdateNgramState(bool state, bool readstate);
        event OpenEventHandler FileAndIndexSelected;
        event OpenEventHandler OpenIndexedCorpus;

        event EventHandler ChartClick;
        event EventHandler HeatmapClick;
        event LuceneQueryEventHandler FindClick;

        event EventHandler LoadMore;

        event ConcordanceEventHandler ConcordanceClick;
        event NgramEventHandler NGramClick;
        event EventHandler BuildIndexClick;
        //event EventHandler KeywordClick;
        event EventHandler LoadStatistics;

        event EventHandler ExtractInfoClick;

        //void VisualizeHist(PointPairList list, string name);
    }
}
