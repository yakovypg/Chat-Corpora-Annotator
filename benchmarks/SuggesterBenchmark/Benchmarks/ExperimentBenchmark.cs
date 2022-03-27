using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using SuggesterBenchmark.Benchmarks.Base;

namespace SuggesterBenchmark.Benchmarks
{
    public class ExperimentBenchmark : BenchmarkBase
    {
        private const string Q0 = "select " +
            "(select haswordofdict(job), haswordofdict(dev), haswordofdict(money) inwin 5);";

        private const string Q1 = "select " +
            "(select haswordofdict(job), haswordofdict(dev), haswordofdict(money) inwin 5); " +
            "(select haswordofdict(job), haswordofdict(area), haswordofdict(money) inwin 5); " +
            "inwin 1000";

        private const string Q2 = "select " +
            "(select haswordofdict(job), haswordofdict(dev), haswordofdict(money) inwin 5); " +
            "(select haswordofdict(job), haswordofdict(area), haswordofdict(money) inwin 5); " +
            "(select haswordofdict(job), haswordofdict(money), haswordofdict(area) inwin 5); " +
            "inwin 1000";

        private const string Q3 = "select " +
            "(select haswordofdict(job), haswordofdict(dev), haswordofdict(money) inwin 5); " +
            "(select haswordofdict(job), haswordofdict(area), haswordofdict(money) inwin 5); " +
            "(select haswordofdict(job), haswordofdict(money), haswordofdict(area) inwin 5); " +
            "(select haswordofdict(dev), haswordofdict(money), haswordofdict(area) inwin 5); " +
            "inwin 1000";

        private readonly string[] _queries = new string[]
        {
            Q0, Q1, Q2, Q3
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

        [Benchmark(Description = "Experiment_0")]
        public void ExperimentTest_0()
        {
            var result = _visitor.VisitQuery(_queryContexts[0]);
        }

        [Benchmark(Description = "Experiment_1")]
        public void ExperimentTest_1()
        {
            var result = _visitor.VisitQuery(_queryContexts[1]);
        }

        [Benchmark(Description = "Experiment_2")]
        public void ExperimentTest_2()
        {
            var result = _visitor.VisitQuery(_queryContexts[2]);
        }

        [Benchmark(Description = "Experiment_3")]
        public void ExperimentTest_3()
        {
            var result = _visitor.VisitQuery(_queryContexts[3]);
        }
    }
}
