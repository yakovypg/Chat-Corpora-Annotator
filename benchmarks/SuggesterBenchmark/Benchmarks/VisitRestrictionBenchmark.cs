using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using SuggesterBenchmark.Benchmarks.Base;

namespace SuggesterBenchmark.Benchmarks
{
    public class VisitRestrictionBenchmark : BenchmarkBase
    {
        private readonly string[] _queries = new string[]
        {
            "select haswordofdict(job)", // 25
            "select hasusermentioned(odrisck)", // 32
            "select byuser(odrisck)", // 22
            "select haswordofdict(job) or haswordofdict(skill)", // 49
            "select haswordofdict(job) and haswordofdict(skill)", // 50
            "select haswordofdict(job) or hasusermentioned(phgilliam)", // 56
        };

        private readonly ChatVisitor _visitor;
        private readonly ChatParser.RestrictionContext[] _restrictions;

        public VisitRestrictionBenchmark()
        {
            _visitor = new ChatVisitor();
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
    }
}
