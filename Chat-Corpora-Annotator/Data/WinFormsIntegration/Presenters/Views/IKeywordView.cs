using CSharpTest.Net.Collections;
using System;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters.Views
{
    public interface IKeywordView : IView
    {
        event EventHandler RakeClick;
        event EventHandler StanfordClick;

        int RakeWordCount { get; }
        int RakeLength { get; set; }

        BTreeDictionary<string, double> RakeKeywords { get; set; }
        Dictionary<List<string>, int> Keyphrases { get; set; }
        List<string> NounPhrases { get; set; }

        void DisplayRakeKeywords();
        void DisplayKeyPhrases();
    }
}
