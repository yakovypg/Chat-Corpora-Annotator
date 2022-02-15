using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using SuggesterBenchmark.Benchmarks.Base;

namespace SuggesterBenchmark.Benchmarks
{
    public class VisitQueryBenchmark : BenchmarkBase
    {
        private readonly string[] _queries = new string[]
        {
            "select haswordofdict(job)",
            "select haswordofdict(job) or haswordofdict(skill)",
            "select haswordofdict(job) and haswordofdict(skill)",
            "select haswordofdict(job), haswordofdict(skill) inwin 5",
            "select haswordofdict(job), haswordofdict(skill) inwin 50",
            "select haswordofdict(job), haswordofdict(skill) unr inwin 5",
            "select haswordofdict(job), haswordofdict(skill), haswordofdict(dev)",
            "select (select haswordofdict(job), haswordofdict(skill) inwin 5); (select haswordofdict(area) or haswordofdict(money) or haswordofdict(dev)) inwin 15",
            "select (select haswordofdict(job), haswordofdict(skill) inwin 5); (select haswordofdict(area) or haswordofdict(money), haswordofdict(dev) unr inwin 5) inwin 15",
        };

        private readonly ChatVisitor _visitor;
        private readonly ChatParser.QueryContext[] _trees;

        public VisitQueryBenchmark()
        {
            _visitor = new ChatVisitor();
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

        [Benchmark]
        public void VisitQueryTest_8()
        {
            var result = _visitor.VisitQuery(_trees[8]);
        }
    }
}
