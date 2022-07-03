using IndexEngine.Data.Paths;
using IndexEngine.Search;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using CoreNLPEngine.Extraction;

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
        public static Extractor Extractor { get; set; } = new Extractor();

        public static readonly Dictionary<string, HashSet<int>> Cache = new();

        public static HashSet<int> HasWordOfList(string dictName, List<string> words)
        {
            if (Cache.ContainsKey(dictName))
                return Cache[dictName];

            if (LuceneService.Parser is null || LuceneService.Searcher is null)
                return new HashSet<int>();

            string queryText = string.Join(' ', words);
            Query query = LuceneService.Parser.Parse(queryText);
            TopDocs docs = LuceneService.Searcher.Search(query, LuceneService.DirReader?.MaxDoc ?? 0);

            HashSet<int> results = new(docs.ScoreDocs.Length);

            for (var i = 0; i < docs.ScoreDocs.Length; i++)
                results.Add(docs.ScoreDocs[i].Doc);

            Cache.Add(dictName, results);
            return results;
        }

        public static HashSet<int> HasWordOfList(List<string> words)
        {
            if (LuceneService.Parser is null || LuceneService.Searcher is null)
                return new HashSet<int>();

            string queryText = string.Join(' ', words);
            Query query = LuceneService.Parser.Parse(queryText);
            TopDocs docs = LuceneService.Searcher.Search(query, LuceneService.DirReader?.MaxDoc ?? 0);

            HashSet<int> results = new(docs.ScoreDocs.Length);

            for (var i = 0; i < docs.ScoreDocs.Length; i++)
                results.Add(docs.ScoreDocs[i].Doc);

            return results;
        }

        public static List<int> HasNERTag(NERLabels tag)
        {
            return tag switch
            {
                NERLabels.ORG => Extractor.Organisations.Keys.ToList(),
                NERLabels.LOC => Extractor.Locations.Keys.ToList(),
                NERLabels.TIME => Extractor.Times.Keys.ToList(),
                NERLabels.URL => Extractor.URLs.Keys.ToList(),
                NERLabels.DATE => Extractor.Dates.Keys.ToList(),
                _ => new List<int>(),
            };
        }

        public static List<int> HasQuestion()
        {
            return Extractor.MessagesWithQuestion.ToList();
        }

        public static HashSet<int> HasUser(string user)
        {
            if (LuceneService.Searcher is null)
                return new HashSet<int>();

            var results = new HashSet<int>();
            var query = new TermQuery(new Lucene.Net.Index.Term(ProjectInfo.SenderFieldKey, user));

            var boolquery = new BooleanQuery
            {
                { query, Occur.MUST }
            };

            TopDocs docs = LuceneService.Searcher.Search(boolquery, LuceneService.DirReader?.MaxDoc ?? 0);

            foreach (var doc in docs.ScoreDocs)
            {
                Document idoc = LuceneService.Searcher.IndexReader.Document(doc.Doc);
                results.Add(idoc.GetField(ProjectInfo.IdKey).GetInt32Value() ?? -1);
            }

            return results;
        }

        public static HashSet<int> HasUserMentioned(string user)
        {
            if (LuceneService.Parser is null || LuceneService.Searcher is null)
                return new HashSet<int>();

            var results = new HashSet<int>();

            Query query = LuceneService.Parser.Parse(user);
            TopDocs docs = LuceneService.Searcher.Search(query, LuceneService.DirReader?.MaxDoc ?? 0);

            foreach (var doc in docs.ScoreDocs)
            {
                Document idoc = LuceneService.Searcher.IndexReader.Document(doc.Doc);
                results.Add(idoc.GetField(ProjectInfo.IdKey).GetInt32Value() ?? -1);
            }

            return results;
        }
    }
}
