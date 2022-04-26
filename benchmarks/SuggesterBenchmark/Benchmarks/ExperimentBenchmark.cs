using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using SuggesterBenchmark.Benchmarks.Base;

namespace SuggesterBenchmark.Benchmarks
{
    public class ExperimentBenchmark : BenchmarkBase
    {
        // private const string Q1 = "select haswordofdict(test), haswordofdict(test), haswordofdict(skill) inwin 30"; // 1 100k
        // private const string Q1 = "select haswordofdict(test500), haswordofdict(test500), haswordofdict(job) inwin 15"; // 2 500k
        // private const string Q1 = "select haswordofdict(testX), haswordofdict(testX), haswordofdict(dev) inwin 10"; // 3 1kk
        // private const string Q1 = "select haswordofdict(testX), haswordofdict(dev), haswordofdict(testZ)"; // 4 1kk

        private const string Q1 = "select " +
            "(select haswordofdict(job), haswordofdict(dev), haswordofdict(area) inwin 10);";

        private const string Q2 = "select " +
            "(select haswordofdict(job), haswordofdict(dev), haswordofdict(area) inwin 10); " +
            "(select haswordofdict(job), haswordofdict(dev), haswordofdict(area) inwin 10); " +
            "inwin 1000";

        private const string Q3 = "select " +
            "(select haswordofdict(job), haswordofdict(dev), haswordofdict(area) inwin 10); " +
            "(select haswordofdict(job), haswordofdict(dev), haswordofdict(area) inwin 10); " +
            "(select haswordofdict(dev), haswordofdict(dev), haswordofdict(area) inwin 10); " +
            "inwin 1000";

        private const string Q4 = "select " +
            "(select haswordofdict(job), haswordofdict(dev), haswordofdict(area) inwin 10); " +
            "(select haswordofdict(job), haswordofdict(dev), haswordofdict(area) inwin 10); " +
            "(select haswordofdict(dev), haswordofdict(dev), haswordofdict(area) inwin 10); " +
            "(select haswordofdict(dev), haswordofdict(dev), haswordofdict(area) inwin 10); " +
            "inwin 1000";

        private readonly string[] _queries = new string[]
        {
            Q1, Q2, Q3, Q4
        };

        private readonly QueryContextVisitor _visitor;
        private readonly ChatParser.QueryContext[] _queryContexts;

        public ExperimentBenchmark()
        {
            _visitor = new QueryContextVisitor();
            _queryContexts = new ChatParser.QueryContext[_queries.Length];

            for (int i = 0; i < _queries.Length; ++i)
            {
                var tree = QueryParser.GetTree(_queries[i]);
                _queryContexts[i] = tree;
            }
        }

        [Benchmark(Description = "Subqueries-1")]
        public void ExperimentTest_0()
        {
            var result = _visitor.VisitQuery(_queryContexts[0]);
        }

        [Benchmark(Description = "Subqueries-2")]
        public void ExperimentTest_1()
        {
            var result = _visitor.VisitQuery(_queryContexts[1]);
        }

        [Benchmark(Description = "Subqueries-3")]
        public void ExperimentTest_2()
        {
            var result = _visitor.VisitQuery(_queryContexts[2]);
        }

        [Benchmark(Description = "Subqueries-4")]
        public void ExperimentTest_3()
        {
            var result = _visitor.VisitQuery(_queryContexts[3]);
        }
    }
}
