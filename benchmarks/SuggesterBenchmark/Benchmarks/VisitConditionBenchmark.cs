using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using SuggesterBenchmark.Benchmarks.Base;

namespace SuggesterBenchmark.Benchmarks
{
    public class VisitConditionBenchmark : BenchmarkBase
    {
        private readonly string[] _queries = new string[]
        {
            "select haswordofdict(job)",
            "select haswordofdict(skill)",
            "select haswordofdict(area)",
            "select hasusermentioned(odrisck)",
            "select hasusermentioned(trisell)",
            "select hasusermentioned(seahik)",
            "select byuser(odrisck)",
            "select byuser(jsonify)",
            "select byuser(cerissa)",
        };

        private readonly QueryContextVisitor _visitor;
        private readonly ChatParser.ConditionContext[] _conditions;

        public VisitConditionBenchmark()
        {
            _visitor = new QueryContextVisitor();
            _conditions = new ChatParser.ConditionContext[_queries.Length];

            for (int i = 0; i < _queries.Length; ++i)
            {
                var tree = QueryParser.GetTree(_queries[i]);
                var condition = tree.body().restrictions().restriction(0).condition();

                _conditions[i] = condition;
            }
        }

        [Benchmark]
        public void VisitConditionTest_0()
        {           
            var result = _visitor.VisitCondition(_conditions[0]);
        }

        [Benchmark]
        public void VisitConditionTest_1()
        {
            var result = _visitor.VisitCondition(_conditions[1]);
        }

        [Benchmark]
        public void VisitConditionTest_2()
        {
            var result = _visitor.VisitCondition(_conditions[2]);
        }

        [Benchmark]
        public void VisitConditionTest_3()
        {
            var result = _visitor.VisitCondition(_conditions[3]);
        }

        [Benchmark]
        public void VisitConditionTest_4()
        {
            var result = _visitor.VisitCondition(_conditions[4]);
        }

        [Benchmark]
        public void VisitConditionTest_5()
        {
            var result = _visitor.VisitCondition(_conditions[5]);
        }

        [Benchmark]
        public void VisitConditionTest_6()
        {
            var result = _visitor.VisitCondition(_conditions[6]);
        }

        [Benchmark]
        public void VisitConditionTest_7()
        {
            var result = _visitor.VisitCondition(_conditions[7]);
        }

        [Benchmark]
        public void VisitConditionTest_8()
        {
            var result = _visitor.VisitCondition(_conditions[8]);
        }
    }
}
