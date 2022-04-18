using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using SuggesterBenchmark.Benchmarks.Base;

namespace SuggesterBenchmark.Benchmarks
{
    public class NaturalQueriesBenchmark : BenchmarkBase
    {
        private const string Q0 = "select " +
            "haswordofdict(job), haswordofdict(dev), haswordofdict(money) " +
            "unr inwin 20";

        private const string Q1 = "select " +
            "(select haswordofdict(dev) or haswordofdict(area), haswordofdict(job));" +
            "(select haswordofdict(area) and haswordofdict(money))" +
            "inwin 40";

        private const string Q2 = "select" +
            "(select (byuser(Lumiras) or byuser(sircharleswatson) or byuser(Kadams223)) and hasusermentioned(odrisck));" +
            "(select haswordofdict(issue), haswordofdict(dev) unr inwin 15)";

        private const string Q3 = "select " +
            "(select (byuser(Lumiras) or byuser(iheartkode) or byuser(odrisck)) and haswordofdict(os));" +
            "(select haswordofdict(os), haswordofdict(issue) inwin 5);" +
            "(select haswordofdict(job))" +
            "inwin 40";

        private readonly string[] _queries = new string[]
        {
            Q0, Q1, Q2, Q3
        };

        private readonly QueryContextVisitor _visitor;
        private readonly ChatParser.QueryContext[] _trees;

        public NaturalQueriesBenchmark()
        {
            _visitor = new QueryContextVisitor();
            _trees = new ChatParser.QueryContext[_queries.Length];

            for (int i = 0; i < _queries.Length; ++i)
            {
                var tree = QueryParser.GetTree(_queries[i]);
                _trees[i] = tree;
            }
        }

        [Benchmark]
        public void VisitQueryTest_0()
        {
            var result = _visitor.VisitQuery(_trees[0]);
        }

        [Benchmark]
        public void VisitQueryTest_1()
        {
            var result = _visitor.VisitQuery(_trees[1]);
        }

        [Benchmark]
        public void VisitQueryTest_2()
        {
            var result = _visitor.VisitQuery(_trees[2]);
        }

        [Benchmark]
        public void VisitQueryTest_3()
        {
            var result = _visitor.VisitQuery(_trees[3]);
        }
    }
}
