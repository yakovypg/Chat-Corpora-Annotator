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

    public static class Retrievers
    {
        public static Extraction.Extractor Extractor { get; set; } = new Extraction.Extractor();

        public static readonly Dictionary<string, HashSet<int>> Cache = new();

        public static HashSet<int> HasWordOfList(string dictName, List<string> words)
        {
            if (Cache.ContainsKey(dictName))
                return Cache[dictName];

            string queryText = string.Join(' ', words);
            Query query = LuceneService.Parser.Parse(queryText);
            TopDocs docs = LuceneService.Searcher.Search(query, LuceneService.DirReader.MaxDoc);

            HashSet<int> results = new(docs.ScoreDocs.Length);

            for (var i = 0; i < docs.ScoreDocs.Length; i++)
                results.Add(docs.ScoreDocs[i].Doc);

            Cache.Add(dictName, results);
            return results;
        }

        public static HashSet<int> HasWordOfList(List<string> words)
        {
            string queryText = string.Join(' ', words);
            Query query = LuceneService.Parser.Parse(queryText);
            TopDocs docs = LuceneService.Searcher.Search(query, LuceneService.DirReader.MaxDoc);

            HashSet<int> results = new(docs.ScoreDocs.Length);

            for (var i = 0; i < docs.ScoreDocs.Length; i++)
                results.Add(docs.ScoreDocs[i].Doc);

            return results;
        }

        public static List<int> HasNERTag(NERLabels tag)
        {
            switch (tag)
            {
                case NERLabels.ORG: return Extractor.Organisations.Keys.ToList();
                case NERLabels.LOC: return Extractor.Locations.Keys.ToList();
                case NERLabels.TIME: return Extractor.Times.Keys.ToList();
                case NERLabels.URL: return Extractor.URLs.Keys.ToList();
                case NERLabels.DATE: return Extractor.Dates.Keys.ToList();

                default: return new List<int>();
            }
        }

        public static List<int> HasQuestion()
        {
            return Extractor.MessagesWithQuestion.ToList();
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
