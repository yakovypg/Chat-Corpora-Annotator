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
            "(select haswordofdict(job), haswordofdict(skill), haswordofdict(area), haswordofdict(money) unr inwin 60);" +
            "(select haswordofdict(area) and (haswordofdict(payment) or haswordofdict(money)))" +
            "inwin 200";

        private const string Q2 = "select" +
            "(select (byuser(Lumiras) or byuser(sircharleswatson) or byuser(Kadams223)) and hasusermentioned(odrisck));" +
            "(select haswordofdict(issue), haswordofdict(dev) unr inwin 15)";

        private const string Q3 = "select " +
            "(select (byuser(Lumiras) or byuser(iheartkode) or byuser(odrisck)) and haswordofdict(os));" +
            "(select haswordofdict(os), haswordofdict(issue) inwin 5);" +
            "(select haswordofdict(job))" +
            "inwin 40";

        private const string Q4 = "select " +
            "haswordofdict(job), haswordofdict(code), hasusermentioned(Kadams223) " +
            "unr inwin 40";

        private const string Q5 = "select " +
            "haswordofdict(job), haswordofdict(skill), haswordofdict(skill), haswordofdict(area), haswordofdict(money) " +
            "inwin 40";

        private const string Q6 = "select " +
            "(select haswordofdict(job), haswordofdict(skill), haswordofdict(code), byuser(Lumiras) inwin 60);" +
            "(select byuser(Lumiras) and haswordofdict(issue))" +
            "inwin 200";

        private const string Q7 = "select " +
            "(select haswordofdict(job), haswordofdict(skill), haswordofdict(code), byuser(Lumiras));" +
            "(select haswordofdict(job), haswordofdict(skill), haswordofdict(code), byuser(odrisck) inwin 40);" +
            "(select haswordofdict(job), haswordofdict(skill), haswordofdict(code), byuser(odrisck) inwin 40)" +
            "inwin 300";

        private readonly string[] _queries = new string[]
        {
            Q0, Q1, Q2, Q3, Q4, Q5, Q6, Q7
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

        [Benchmark]
        public void VisitQueryTest_4()
        {
            var result = _visitor.VisitQuery(_trees[4]);
        }

        [Benchmark]
        public void VisitQueryTest_5()
        {
            var result = _visitor.VisitQuery(_trees[5]);
        }

        [Benchmark]
        public void VisitQueryTest_6()
        {
            var result = _visitor.VisitQuery(_trees[6]);
        }

        [Benchmark]
        public void VisitQueryTest_7()
        {
            var result = _visitor.VisitQuery(_trees[7]);
        }
    }
}
