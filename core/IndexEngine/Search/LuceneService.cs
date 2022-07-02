using IndexEngine.Containers;
using IndexEngine.Data.Paths;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace IndexEngine.Search
{
    public static class LuceneService
    {
        public static LuceneVersion AppLuceneVersion { get; set; } = LuceneVersion.LUCENE_48;

        public static FSDirectory? Dir { get; set; }

        public static StandardAnalyzer? Analyzer { get; set; }
        public static IndexWriterConfig? IndexConfig { get; set; }
        public static IndexWriter? Writer { get; set; }
        public static DirectoryReader? DirReader { get; set; }
        public static QueryParser? Parser { get; set; }

        public static QueryParser? UserParser { get; set; }
        public static IndexSearcher? Searcher { get; set; }

        public static IndexSearcher? NGramSearcher { get; set; }
        public static FSDirectory? NGramDir { get; set; }
        public static NGramAnalyzer? NGrammer { get; set; }
        public static DirectoryReader? NGramReader { get; set; }

        public static void Dispose()
        {
            Analyzer?.Dispose();
            NGrammer?.Dispose();
            DirReader?.Dispose();
            Writer?.Dispose();
        }

        public static Dictionary<int, List<int>> GetTokenDataForDoc(string document)
        {
            if (Analyzer == null)
                throw new NullReferenceException(nameof(Analyzer));

            TokenStream stream = Analyzer.GetTokenStream(ProjectInfo.TextFieldKey, new StringReader(document));

            int tokenIndex = 0;
            ICharTermAttribute? charTermAttribute = stream.AddAttribute<ICharTermAttribute>();

            stream.Reset();

            var termLengths = new List<int>();

            while (stream.IncrementToken())
            {
                //Token token = new Token();
                //int startOffset = offsetAttribute.StartOffset;
                //int endOffset = offsetAttribute.EndOffset;

                string term = charTermAttribute.ToString();
                tokenIndex++;
                termLengths.Add(term.Length);
            }

            var result = new Dictionary<int, List<int>>
            {
                { tokenIndex, termLengths }
            };

            stream.ClearAttributes();
            stream.End();
            stream.Dispose();

            return result;
        }

        internal static void OpenParser()
        {
            if (Analyzer == null)
                return;

            Parser = new QueryParser(AppLuceneVersion, ProjectInfo.TextFieldKey, Analyzer);
            UserParser = new QueryParser(AppLuceneVersion, ProjectInfo.SenderFieldKey, Analyzer);
        }

        internal static void OpenAnalyzers()
        {
            Analyzer = new StandardAnalyzer(AppLuceneVersion);
            NGrammer = new NGramAnalyzer(Lucene.Net.Analysis.Analyzer.PER_FIELD_REUSE_STRATEGY);
        }

        internal static void OpenWriter()
        {
            OpenAnalyzers();

            IndexConfig = new IndexWriterConfig(AppLuceneVersion, Analyzer)
            {
                MaxBufferedDocs = IndexWriterConfig.DISABLE_AUTO_FLUSH,
                RAMBufferSizeMB = 50.0,
                OpenMode = OpenMode.CREATE
            };

            Writer = new IndexWriter(Dir, IndexConfig);

            OpenParser();
        }

        public static void OpenReader()
        {
            DirReader = DirectoryReader.Open(Dir);
            Searcher = new IndexSearcher(DirReader);
        }

        internal static void OpenDirectory()
        {
            Dir = FSDirectory.Open(ProjectInfo.IndexPath);
        }

        public static void OpenNewIndex()
        {
            OpenDirectory();
            OpenWriter();
        }

        public static DynamicMessage RetrieveMessageById(int id)
        {
            if (Searcher == null)
                throw new NullReferenceException(nameof(Searcher));

            var query = NumericRangeQuery.NewInt32Range(ProjectInfo.IdKey, id, id, true, true);

            ScoreDoc doc = Searcher.Search(query, 1).ScoreDocs.First();
            Document idoc = Searcher.Doc(doc.Doc);

            var data = new List<string>();
            int msgId = idoc.GetField(ProjectInfo.IdKey).GetInt32Value() ?? -1;

            foreach (var field in ProjectInfo.Data.SelectedFields)
                data.Add(idoc.GetField(field).GetStringValue());

            return new DynamicMessage(data, ProjectInfo.Data.SelectedFields, ProjectInfo.DateFieldKey, msgId);
        }

        public static bool OpenIndex()
        {
            OpenDirectory();

            if (Dir == null || !DirectoryReader.IndexExists(Dir))
                return false;

            OpenAnalyzers();
            OpenReader();
            OpenParser();

            return true;
        }
    }
}
