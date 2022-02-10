using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using SuggesterBenchmark.Benchmarks.Base;

namespace SuggesterBenchmark.Benchmarks
{
    public class VisitQueryAndBodyBenchmark : BenchmarkBase
    {
        private readonly string[] _queries = new string[]
        {
            // "query", // query_length
            "select haswordofdict(job)", // 25
            "select hasusermentioned(odrisck)", // 32
            "select byuser(odrisck)", // 22
            "select haswordofdict(job) or haswordofdict(skill)", // 49
            "select haswordofdict(job) or byuser(phgilliam)", // 46
            "select haswordofdict(job) and haswordofdict(skill)", // 50
            "select haswordofdict(job) and not haswordofdict(skill)", // 54
            "select haswordofdict(job), haswordofdict(skill) inwin 3", // 55
            "select haswordofdict(job), haswordofdict(skill) inwin 50", // 56
            "select haswordofdict(job), haswordofdict(skill) mess inwin 3", // 60
            "select haswordofdict(job), haswordofdict(skill)", // 47
            "select haswordofdict(job), haswordofdict(skill), haswordofdict(dev)", // 67
            "select haswordofdict(job), haswordofdict(skill), haswordofdict(dev) mess", // 72
            "select haswordofdict(job) or haswordofdict(dev), haswordofdict(skill)", // 69
            "select haswordofdict(job) or haswordofdict(dev), haswordofdict(skill) mess inwin 3", // 82
            "select (select haswordofdict(job), haswordofdict(skill) inwin 5); (select haswordofdict(area) or haswordofdict(money) or haswordofdict(dev)) inwin 5", // 148
            "select (select haswordofdict(job), haswordofdict(skill) inwin 5); (select haswordofdict(area) or haswordofdict(dev), haswordofdict(money) or haswordofdict(dev) inwin 5) inwin 5", // 169
            "select (select haswordofdict(job), haswordofdict(skill) inwin 5); (select haswordofdict(area) or haswordofdict(dev), haswordofdict(dev) mess inwin 5) inwin 5", // 150
        };

        private readonly ChatVisitor _visitor;
        private readonly ChatParser.QueryContext[] _trees;
        private readonly ChatParser.BodyContext[] _bodies;

        public VisitQueryAndBodyBenchmark()
        {
            _visitor = new ChatVisitor();
            _trees = new ChatParser.QueryContext[_queries.Length];
            _bodies = new ChatParser.BodyContext[_queries.Length];

            for (int i = 0; i < _queries.Length; ++i)
            {
                var tree = QueryParser.GetTree(_queries[i]);
                var body = tree.body();

                _trees[i] = tree;
                _bodies[i] = body;
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

        [Benchmark]
        public void VisitQueryTest_9()
        {
            var result = _visitor.VisitQuery(_trees[9]);
        }

        [Benchmark]
        public void VisitQueryTest_10()
        {
            var result = _visitor.VisitQuery(_trees[10]);
        }

        [Benchmark]
        public void VisitQueryTest_11()
        {
            var result = _visitor.VisitQuery(_trees[11]);
        }

        [Benchmark]
        public void VisitQueryTest_12()
        {
            var result = _visitor.VisitQuery(_trees[12]);
        }

        [Benchmark]
        public void VisitQueryTest_13()
        {
            var result = _visitor.VisitQuery(_trees[13]);
        }

        [Benchmark]
        public void VisitQueryTest_14()
        {
            var result = _visitor.VisitQuery(_trees[14]);
        }

        [Benchmark]
        public void VisitQueryTest_15()
        {
            var result = _visitor.VisitQuery(_trees[15]);
        }

        [Benchmark]
        public void VisitQueryTest_16()
        {
            var result = _visitor.VisitQuery(_trees[16]);
        }

        [Benchmark]
        public void VisitQueryTest_17()
        {
            var result = _visitor.VisitQuery(_trees[17]);
        }

        [Benchmark]
        public void VisitBodyTest_0()
        {
            var result = _visitor.VisitBody(_bodies[0]);
        }

        [Benchmark]
        public void VisitBodyTest_1()
        {
            var result = _visitor.VisitBody(_bodies[1]);
        }

        [Benchmark]
        public void VisitBodyTest_2()
        {
            var result = _visitor.VisitBody(_bodies[2]);
        }

        [Benchmark]
        public void VisitBodyTest_3()
        {
            var result = _visitor.VisitBody(_bodies[3]);
        }

        [Benchmark]
        public void VisitBodyTest_4()
        {
            var result = _visitor.VisitBody(_bodies[4]);
        }

        [Benchmark]
        public void VisitBodyTest_5()
        {
            var result = _visitor.VisitBody(_bodies[5]);
        }

        [Benchmark]
        public void VisitBodyTest_6()
        {
            var result = _visitor.VisitBody(_bodies[6]);
        }

        [Benchmark]
        public void VisitBodyTest_7()
        {
            var result = _visitor.VisitBody(_bodies[7]);
        }

        [Benchmark]
        public void VisitBodyTest_8()
        {
            var result = _visitor.VisitBody(_bodies[8]);
        }

        [Benchmark]
        public void VisitBodyTest_9()
        {
            var result = _visitor.VisitBody(_bodies[9]);
        }

        [Benchmark]
        public void VisitBodyTest_10()
        {
            var result = _visitor.VisitBody(_bodies[10]);
        }

        [Benchmark]
        public void VisitBodyTest_11()
        {
            var result = _visitor.VisitBody(_bodies[11]);
        }

        [Benchmark]
        public void VisitBodyTest_12()
        {
            var result = _visitor.VisitBody(_bodies[12]);
        }

        [Benchmark]
        public void VisitBodyTest_13()
        {
            var result = _visitor.VisitBody(_bodies[13]);
        }

        [Benchmark]
        public void VisitBodyTest_14()
        {
            var result = _visitor.VisitBody(_bodies[14]);
        }

        [Benchmark]
        public void VisitBodyTest_15()
        {
            var result = _visitor.VisitBody(_bodies[15]);
        }

        [Benchmark]
        public void VisitBodyTest_16()
        {
            var result = _visitor.VisitBody(_bodies[16]);
        }

        [Benchmark]
        public void VisitBodyTest_17()
        {
            var result = _visitor.VisitBody(_bodies[17]);
        }
    }
}
