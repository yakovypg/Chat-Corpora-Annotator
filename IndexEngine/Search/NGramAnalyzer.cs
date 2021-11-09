using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Shingle;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using System.IO;


namespace IndexEngine.Search
{
    public class NGramAnalyzer : Analyzer
    {
        private readonly LuceneVersion version = LuceneService.AppLuceneVersion;

        public NGramAnalyzer(ReuseStrategy reuseStrategy) : base(reuseStrategy)
        {
        }

        public int minGramSize { get; set; } = 2;
        public int maxGramSize { get; set; } = 5;
        public bool ShowUnigrams { get; set; } = false;

        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            var tokenizer = new StandardTokenizer(version, reader);
            var shingler = new ShingleFilter(tokenizer, minGramSize, maxGramSize);

            if (!ShowUnigrams)
            {
                shingler.SetOutputUnigrams(false);
            }
            else
            {
                shingler.SetOutputUnigrams(true);
            }

            var filter = new StopFilter(version, new LowerCaseFilter(version, shingler),
                StopAnalyzer.ENGLISH_STOP_WORDS_SET);

            return new TokenStreamComponents(tokenizer, filter);
        }
    }
}
