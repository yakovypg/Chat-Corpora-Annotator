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
        static LuceneService()
        {
            AppLuceneVersion = LuceneVersion.LUCENE_48;
        }

        public static LuceneVersion AppLuceneVersion;

        public static FSDirectory Dir { get; set; }

        public static StandardAnalyzer Analyzer { get; set; }
        public static IndexWriterConfig IndexConfig { get; set; }
        public static IndexWriter Writer { get; set; }
        public static DirectoryReader DirReader { get; set; }
        public static QueryParser Parser { get; set; }

        public static QueryParser UserParser { get; set; }
        public static IndexSearcher Searcher { get; set; }

        public static IndexSearcher NGramSearcher { get; set; }
        public static FSDirectory NGramDir { get; set; }
        public static NGramAnalyzer NGrammer { get; set; }
        public static DirectoryReader NGramReader { get; set; }

        public static void Dispose()
        {
            Analyzer.Dispose();
            NGrammer.Dispose();
            DirReader.Dispose();
            Writer.Dispose();
        }

        public static Dictionary<int, List<int>> GetTokenDataForDoc(string document)
        {
            Dictionary<int, List<int>> res = new Dictionary<int, List<int>>();
            List<int> list = new List<int>();

            TokenStream stream = Analyzer.GetTokenStream(ProjectInfo.TextFieldKey, new StringReader(document));

            var index = 0;
            var charTermAttribute = stream.AddAttribute<ICharTermAttribute>();

            stream.Reset();

            while (stream.IncrementToken())
            {
                //Token token = new Token();
                //int startOffset = offsetAttribute.StartOffset;
                //int endOffset = offsetAttribute.EndOffset;
                string term = charTermAttribute.ToString();
                index++;
                list.Add(term.Length);
            }

            res.Add(index, list);
            stream.ClearAttributes();
            stream.End();
            stream.Dispose();

            return res;
        }

        internal static void OpenParser()
        {
            if (Analyzer != null)
            {
                Parser = new QueryParser(AppLuceneVersion, ProjectInfo.TextFieldKey, Analyzer);
                UserParser = new QueryParser(AppLuceneVersion, ProjectInfo.SenderFieldKey, Analyzer);
            }
        }

        internal static void OpenAnalyzers()
        {
            Analyzer = new StandardAnalyzer(AppLuceneVersion);
            NGrammer = new NGramAnalyzer(StandardAnalyzer.PER_FIELD_REUSE_STRATEGY);
        }

        internal static void OpenWriter()
        {
            OpenAnalyzers();

            IndexConfig = new IndexWriterConfig(AppLuceneVersion, Analyzer);
            IndexConfig.MaxBufferedDocs = IndexWriterConfig.DISABLE_AUTO_FLUSH;
            IndexConfig.RAMBufferSizeMB = 50.0;
            IndexConfig.OpenMode = OpenMode.CREATE;
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
            var query = NumericRangeQuery.NewInt32Range(ProjectInfo.IdKey, id, id, true, true);

            ScoreDoc doc = LuceneService.Searcher.Search(query, 1).ScoreDocs.FirstOrDefault();
            List<string> data = new List<string>();
            Document idoc = LuceneService.Searcher.Doc(doc.Doc);

            foreach (var field in ProjectInfo.Data.SelectedFields)
            {
                data.Add(idoc.GetField(field).GetStringValue());
            }

            return new DynamicMessage(data, ProjectInfo.Data.SelectedFields, ProjectInfo.DateFieldKey, idoc.GetField(ProjectInfo.IdKey).GetInt32Value().Value);
        }

        public static bool OpenIndex()
        {
            OpenDirectory();

            if (Dir != null)
            {
                if (DirectoryReader.IndexExists(Dir))
                {
                    OpenAnalyzers();
                    OpenReader();
                    OpenParser();
                    return true;

                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
