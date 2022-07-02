using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Shingle;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;


namespace IndexEngine.Search
{
    public class NGramAnalyzer : Analyzer
    {
        public int MinGramSize { get; set; } = 2;
        public int MaxGramSize { get; set; } = 5;
        public bool ShowUnigrams { get; set; } = false;

        private readonly LuceneVersion version = LuceneService.AppLuceneVersion;

        public NGramAnalyzer(ReuseStrategy reuseStrategy) : base(reuseStrategy)
        {
        }

        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            var tokenizer = new StandardTokenizer(version, reader);
            var shingler = new ShingleFilter(tokenizer, MinGramSize, MaxGramSize);

            shingler.SetOutputUnigrams(ShowUnigrams);

            var filter = new StopFilter(version, new LowerCaseFilter(version, shingler),
                StopAnalyzer.ENGLISH_STOP_WORDS_SET);

            return new TokenStreamComponents(tokenizer, filter);
        }
    }
}
