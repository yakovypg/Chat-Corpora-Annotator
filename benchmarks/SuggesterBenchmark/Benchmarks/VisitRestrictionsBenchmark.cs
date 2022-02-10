using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using SuggesterBenchmark.Benchmarks.Base;

namespace SuggesterBenchmark.Benchmarks
{
    public class VisitRestrictionsBenchmark : BenchmarkBase
    {
        private readonly string[] _queries = new string[]
        {
            "select haswordofdict(job)", // 25
            "select haswordofdict(job), haswordofdict(skill)", // 47
            "select haswordofdict(job) or haswordofdict(dev), haswordofdict(skill)", // 69
            "select haswordofdict(job), haswordofdict(skill), haswordofdict(dev)", // 67
            "select haswordofdict(job), haswordofdict(skill), haswordofdict(dev), haswordofdict(skill)", // 89
        };

        private readonly ChatVisitor _visitor;
        private readonly ChatParser.RestrictionsContext[] _restrictions;

        public VisitRestrictionsBenchmark()
        {
            _visitor = new ChatVisitor();
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
    }
}
