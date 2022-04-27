using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using ChatCorporaAnnotator.Data.Parsers.Suggester.Histograms;
using IndexEngine.Data.Paths;
using SuggesterBenchmark.Benchmarks.Base;

namespace SuggesterBenchmark.Benchmarks
{
    using MsgGroupList = List<List<int>>;

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
        private readonly MsgGroupList[] _visitResults;

        private readonly int[] _inwins;

        public HistogramAlgorithmBenchmark()
        {
            QueryParser.TryLoadHistograms(out HashSet<MsgGroupHistogram> histograms);

            Console.WriteLine($"\n{histograms.Count} histograms were loaded from {ToolInfo.HistogramsPath}\n");

            _visitor = new QueryContextVisitor() { Histograms = histograms };
            _queryContexts = new ChatParser.QueryContext[_queries.Length];
            _visitResults = new MsgGroupList[_queries.Length];
            _inwins = new int[_queries.Length];

            for (int i = 0; i < _queries.Length; ++i)
            {
                var tree = QueryParser.GetTree(_queries[i]);
                _queryContexts[i] = tree;

                var res = (List<MsgGroupList>)_visitor.VisitRestrictions(tree.body().restrictions());
                _visitResults[i] = res[0];

                string inwinTemplate = "inwin ";
                string curQuery = _queries[i].ToLower();

                _inwins[i] = QueryContextVisitor.DEFAULT_WINDOW_SIZE;

                if (curQuery.Contains(inwinTemplate))
                {
                    int index = curQuery.LastIndexOf(inwinTemplate);
                    string inwinText = curQuery.Substring(index + inwinTemplate.Length);

                    if (!int.TryParse(inwinText, out _inwins[i]))
                        throw new Exception($"There is incorrect INWIN in '{_queries[i]}'");
                }
            }
        }

        [Benchmark(Description = "VisitQuery-1")]
        public void Test_1()
        {
            var result = _visitor.VisitQuery(_queryContexts[0]);
        }

        [Benchmark(Description = "VisitQuery-2")]
        public void Test_2()
        {
            var result = _visitor.VisitQuery(_queryContexts[1]);
        }

        [Benchmark(Description = "VisitQuery-3")]
        public void Test_3()
        {
            var result = _visitor.VisitQuery(_queryContexts[2]);
        }

        [Benchmark(Description = "VisitQuery-4")]
        public void Test_4()
        {
            var result = _visitor.VisitQuery(_queryContexts[3]);
        }

        [Benchmark(Description = "VisitQuery-5")]
        public void Test_5()
        {
            var result = _visitor.VisitQuery(_queryContexts[4]);
        }

        [Benchmark(Description = "MergeRestrictions-Q1")]
        public void MergeRestrictionsTest_1()
        {
            var result = _visitor.MergeRestrictions(_visitResults[0], _inwins[0]);
        }

        [Benchmark(Description = "MergeRestrictions-Q2")]
        public void MergeRestrictionsTest_2()
        {
            var result = _visitor.MergeRestrictions(_visitResults[1], _inwins[1]);
        }

        [Benchmark(Description = "MergeRestrictions-Q3")]
        public void MergeRestrictionsTest_3()
        {
            var result = _visitor.MergeRestrictions(_visitResults[2], _inwins[2]);
        }

        [Benchmark(Description = "MergeRestrictions-Q4")]
        public void MergeRestrictionsTest_4()
        {
            var result = _visitor.MergeRestrictions(_visitResults[3], _inwins[3]);
        }

        [Benchmark(Description = "MergeRestrictions-Q5")]
        public void MergeRestrictionsTest_5()
        {
            var result = _visitor.MergeRestrictions(_visitResults[4], _inwins[4]);
        }
    }
}
