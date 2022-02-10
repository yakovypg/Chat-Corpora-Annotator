using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using SuggesterBenchmark.Benchmarks.Base;

namespace SuggesterBenchmark.Benchmarks
{
    public class VisitConditionBenchmark : BenchmarkBase
    {
        private readonly string[] _queries = new string[]
        {
            "select haswordofdict(job)", // 25
            "select hasusermentioned(odrisck)", // 32
            "select byuser(odrisck)" // 22
        };

        private readonly ChatVisitor _visitor;
        private readonly ChatParser.ConditionContext[] _conditions;

        public VisitConditionBenchmark()
        {
            _visitor = new ChatVisitor();
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
    }
}
