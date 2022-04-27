using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using ChatCorporaAnnotator.Data.Parsers.Suggester.Histograms;
using IndexEngine.Data.Paths;
using SuggesterBenchmark.Benchmarks.Base;

namespace SuggesterBenchmark.Benchmarks
{
    public class HistogramAlgorithmBenchmark : BenchmarkBase
    {
        private const string Q1 = "select haswordofdict(test), haswordofdict(test), haswordofdict(skill) inwin 30"; // 1 100k
        private const string Q2 = "select haswordofdict(test500), haswordofdict(test500), haswordofdict(job) inwin 15"; // 2 500k
        private const string Q3 = "select haswordofdict(testX), haswordofdict(testX), haswordofdict(dev) inwin 10"; // 3 1kk
        private const string Q4 = "select haswordofdict(testX), haswordofdict(dev), haswordofdict(testZ)"; // 4 1kk

        private const string Q5 = "select " +
            "byuser(sludge256), " +
            "byuser(sludge256), " +
            "byuser(trisell) or byuser(seahik) or byuser(odrisck) or byuser(jsonify) or byuser(cerissa) or byuser(mykey007) or byuser(AhsanBudhani) or hasusermentioned(seahik) " +
            "inwin 50";

        private readonly string[] _queries = new string[]
        {
            Q1, Q2, Q3, Q4, Q5
        };

        private readonly QueryContextVisitor _visitor;
        private readonly ChatParser.QueryContext[] _queryContexts;

        public HistogramAlgorithmBenchmark()
        {
            QueryParser.TryLoadHistograms(out HashSet<MsgGroupHistogram> histograms);

            Console.WriteLine($"\n{histograms.Count} histograms were loaded from {ToolInfo.HistogramsPath}\n");

            _visitor = new QueryContextVisitor()/* { Histograms = histograms }*/;
            _queryContexts = new ChatParser.QueryContext[_queries.Length];

            for (int i = 0; i < _queries.Length; ++i)
            {
                var tree = QueryParser.GetTree(_queries[i]);
                _queryContexts[i] = tree;
            }
        }

        [Benchmark(Description = "Query-1")]
        public void Test_1()
        {
            var result = _visitor.VisitQuery(_queryContexts[0]);
        }

        [Benchmark(Description = "Query-2")]
        public void Test_2()
        {
            var result = _visitor.VisitQuery(_queryContexts[1]);
        }

        [Benchmark(Description = "Query-3")]
        public void Test_3()
        {
            var result = _visitor.VisitQuery(_queryContexts[2]);
        }

        [Benchmark(Description = "Query-4")]
        public void Test_4()
        {
            var result = _visitor.VisitQuery(_queryContexts[3]);
        }

        [Benchmark(Description = "Query-5")]
        public void Test_5()
        {
            var result = _visitor.VisitQuery(_queryContexts[4]);
        }
    }
}
