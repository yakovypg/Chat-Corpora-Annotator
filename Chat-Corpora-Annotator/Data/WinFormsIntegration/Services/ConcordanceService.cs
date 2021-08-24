using IndexEngine;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Services
{
    public interface IConcordanceService
    {
        Query ConQuery { get; set; }
        List<string> Concordance { get; set; }
        void FindConcordance(string query, string TextFieldKey, int count);
    }

    public class ConcordanceService : IConcordanceService
    {
        public Query ConQuery { get; set; }
        public List<string> Concordance { get; set; }

        public void FindConcordance(string query, string TextFieldKey, int count)
        {
            Concordance = new List<string>();
            TopDocs Hits = LuceneService.Searcher.Search(ConQuery, LuceneService.DirReader.MaxDoc);

            for (int i = 0; i < Hits.ScoreDocs.Length; i++)
            {
                ScoreDoc d = Hits.ScoreDocs[i];
                Document idoc = LuceneService.Searcher.Doc(d.Doc);
                Concordance.Add(idoc.GetField(TextFieldKey).GetStringValue());
            }
        }
    }
}
