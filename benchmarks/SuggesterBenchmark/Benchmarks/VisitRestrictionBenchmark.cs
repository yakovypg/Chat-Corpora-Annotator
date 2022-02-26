using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using SuggesterBenchmark.Benchmarks.Base;

namespace SuggesterBenchmark.Benchmarks
{
    public class VisitRestrictionBenchmark : BenchmarkBase
    {
        private readonly string[] _queries = new string[]
        {
            "select haswordofdict(money) or haswordofdict(area)",
            "select haswordofdict(job) or haswordofdict(skill)",
            "select haswordofdict(area) or byuser(odrisck) or haswordofdict(money)",
            "select haswordofdict(money) and haswordofdict(area)",
            "select haswordofdict(job) and haswordofdict(skill)",
            "select haswordofdict(area) and byuser(odrisck) and haswordofdict(money)",
            "select (haswordofdict(skill) or haswordofdict(job)) and (byuser(odrisck) or byuser(cerissa))",
            "select (haswordofdict(dev) and haswordofdict(job)) or (haswordofdict(dev) and byuser(odrisck))",
        };

        private readonly QueryContextVisitor _visitor;
        private readonly ChatParser.RestrictionContext[] _restrictions;

        public VisitRestrictionBenchmark()
        {
            _visitor = new QueryContextVisitor();
            _restrictions = new ChatParser.RestrictionContext[_queries.Length];

            for (int i = 0; i < _queries.Length; ++i)
            {
                var tree = QueryParser.GetTree(_queries[i]);
                var restriction = tree.body().restrictions().restriction(0);

                _restrictions[i] = restriction;
            }
        }

        [Benchmark]
        public void VisitRestrictionTest_0()
        {
            var result = _visitor.VisitRestriction(_restrictions[0]);
        }

        [Benchmark]
        public void VisitRestrictionTest_1()
        {
            var result = _visitor.VisitRestriction(_restrictions[1]);
        }

        [Benchmark]
        public void VisitRestrictionTest_2()
        {
            var result = _visitor.VisitRestriction(_restrictions[2]);
        }

        [Benchmark]
        public void VisitRestrictionTest_3()
        {
            var result = _visitor.VisitRestriction(_restrictions[3]);
        }

        [Benchmark]
        public void VisitRestrictionTest_4()
        {
            var result = _visitor.VisitRestriction(_restrictions[4]);
        }

        [Benchmark]
        public void VisitRestrictionTest_5()
        {
            var result = _visitor.VisitRestriction(_restrictions[5]);
        }

        [Benchmark]
        public void VisitRestrictionTest_6()
        {
            var result = _visitor.VisitRestriction(_restrictions[6]);
        }

        [Benchmark]
        public void VisitRestrictionTest_7()
        {
            var result = _visitor.VisitRestriction(_restrictions[7]);
        }
    }
}
