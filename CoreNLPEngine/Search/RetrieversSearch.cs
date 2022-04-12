using IndexEngine.Data.Paths;
using IndexEngine.Search;
using Lucene.Net.Documents;
using Lucene.Net.Search;

namespace CoreNLPEngine.Search
{
    public enum NERLabels
    {
        ORG,
        LOC,
        TIME,
        URL,
        DATE
    }

    public static class RetrieversSearch
    {
        public static HashSet<int> HasWordOfList(List<string> words)
        {
            HashSet<int> results = new HashSet<int>();

            foreach (var word in words)
            {

                //TermQuery query = new TermQuery(new Lucene.Net.Index.Term(word));
                //BooleanQuery boolquery = new BooleanQuery();
                //boolquery.Add(query, Occur.MUST);

                //А на самом деле можно построить фильтр сразу на весь список слов без цикла.
                //string[] temp = new string[1];
                //temp[0] = word;
                //FieldCacheTermsFilter filter = new FieldCacheTermsFilter(ProjectInfo.TextFieldKey, temp);
                //var boolFilter = new BooleanFilter();
                //boolFilter.Add(new FilterClause(filter, Occur.MUST));

                Query query = LuceneService.Parser.Parse(word);
                TopDocs docs = LuceneService.Searcher.Search(query, LuceneService.DirReader.MaxDoc);

                foreach (var doc in docs.ScoreDocs)
                {
                    Document idoc = LuceneService.Searcher.IndexReader.Document(doc.Doc);
                    results.Add(idoc.GetField(ProjectInfo.IdKey).GetInt32Value().Value);
                }
            }

            return results;
        }

        public static List<int> HasNERTag(NERLabels tag)
        {
            switch (tag)
            {
                case NERLabels.ORG: return Extraction.Extractor.Organisations.Keys.ToList();
                case NERLabels.LOC: return Extraction.Extractor.Locations.Keys.ToList();
                case NERLabels.TIME: return Extraction.Extractor.Times.Keys.ToList();
                case NERLabels.URL: return Extraction.Extractor.URLs.Keys.ToList();
                case NERLabels.DATE: return Extraction.Extractor.Dates.Keys.ToList();

                default: return new List<int>();
            }
        }

        public static List<int> HasQuestion()
        {
            return Extraction.Extractor.MessagesWithQuestion.ToList();
        }

        public static HashSet<int> HasUser(string user)
        {
            HashSet<int> results = new HashSet<int>();
            TermQuery query = new TermQuery(new Lucene.Net.Index.Term(ProjectInfo.SenderFieldKey, user));

            BooleanQuery boolquery = new BooleanQuery();
            boolquery.Add(query, Occur.MUST);

            TopDocs docs = LuceneService.Searcher.Search(boolquery, LuceneService.DirReader.MaxDoc);

            foreach (var doc in docs.ScoreDocs)
            {
                Document idoc = LuceneService.Searcher.IndexReader.Document(doc.Doc);
                results.Add(idoc.GetField(ProjectInfo.IdKey).GetInt32Value().Value);
            }

            return results;
        }

        public static HashSet<int> HasUserMentioned(string user)
        {
            HashSet<int> results = new HashSet<int>();
            Query query = LuceneService.Parser.Parse(user);
            TopDocs docs = LuceneService.Searcher.Search(query, LuceneService.DirReader.MaxDoc);

            foreach (var doc in docs.ScoreDocs)
            {
                Document idoc = LuceneService.Searcher.IndexReader.Document(doc.Doc);
                results.Add(idoc.GetField(ProjectInfo.IdKey).GetInt32Value().Value);
            }

            return results;
        }
    }
}
