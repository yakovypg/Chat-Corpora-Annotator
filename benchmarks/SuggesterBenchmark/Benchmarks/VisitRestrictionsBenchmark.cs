using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using SuggesterBenchmark.Benchmarks.Base;

namespace SuggesterBenchmark.Benchmarks
{
    public class VisitRestrictionsBenchmark : BenchmarkBase
    {
        private readonly string[] _queries = new string[]
        {
            "select haswordofdict(job), haswordofdict(skill)",
            "select haswordofdict(job), haswordofdict(skill), haswordofdict(dev)",
            "select haswordofdict(job), haswordofdict(skill), haswordofdict(dev), haswordofdict(area)",
            "select haswordofdict(job), haswordofdict(skill), haswordofdict(dev), haswordofdict(area), haswordofdict(money)",

            "select haswordofdict(area), haswordofdict(os) unr",
            "select haswordofdict(area), haswordofdict(os), haswordofdict(dev) unr",
            "select haswordofdict(area), haswordofdict(os), haswordofdict(dev), haswordofdict(money) unr",
        };

        private readonly QueryContextVisitor _visitor;
        private readonly ChatParser.RestrictionsContext[] _restrictions;

        public VisitRestrictionsBenchmark()
        {
            _visitor = new QueryContextVisitor();
            _restrictions = new ChatParser.RestrictionsContext[_queries.Length];

            for (int i = 0; i < _queries.Length; ++i)
            {
                var tree = QueryParser.GetTree(_queries[i]);
                var restrictions = tree.body().restrictions();

                _restrictions[i] = restrictions;
            }
        }

        [Benchmark]
        public void VisitRestrictionsTest_0()
        {
            var result = _visitor.VisitRestrictions(_restrictions[0]);
        }

        [Benchmark]
        public void VisitRestrictionsTest_1()
        {
            var result = _visitor.VisitRestrictions(_restrictions[1]);
        }

        [Benchmark]
        public void VisitRestrictionsTest_2()
        {
            var result = _visitor.VisitRestrictions(_restrictions[2]);
        }

        [Benchmark]
        public void VisitRestrictionsTest_3()
        {
            var result = _visitor.VisitRestrictions(_restrictions[3]);
        }

        [Benchmark]
        public void VisitRestrictionsTest_4()
        {
            var result = _visitor.VisitRestrictions(_restrictions[4]);
        }

        [Benchmark]
        public void VisitRestrictionsTest_5()
        {
            var result = _visitor.VisitRestrictions(_restrictions[5]);
        }

        [Benchmark]
        public void VisitRestrictionsTest_6()
        {
            var result = _visitor.VisitRestrictions(_restrictions[6]);
        }
    }
}
