using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using SuggesterBenchmark.Benchmarks.Base;

namespace SuggesterBenchmark.Benchmarks
{
    public class VisitQuerySequenceBenchmark : BenchmarkBase
    {
        private readonly string[] _queries = new string[]
        {
            "select (select haswordofdict(area), haswordofdict(money) inwin 5)",
            "select (select haswordofdict(area), haswordofdict(money) inwin 5); (select haswordofdict(job) and haswordofdict(dev)) inwin 1000",
            "select (select haswordofdict(area), haswordofdict(money) inwin 5); (select haswordofdict(job) and haswordofdict(dev)); (select haswordofdict(area), haswordofdict(money) inwin 5) inwin 1000",
            "select (select haswordofdict(area), haswordofdict(money) inwin 5); (select haswordofdict(job) and haswordofdict(dev)); (select haswordofdict(area), haswordofdict(money) inwin 5); (select haswordofdict(money) or haswordofdict(dev)) inwin 1000",
        };

        private readonly QueryContextVisitor _visitor;
        private readonly ChatParser.Query_seqContext[] _querySequences;

        public VisitQuerySequenceBenchmark()
        {
            _visitor = new QueryContextVisitor();
            _querySequences = new ChatParser.Query_seqContext[_queries.Length];

            for (int i = 0; i < _queries.Length; ++i)
            {
                var tree = QueryParser.GetTree(_queries[i]);
                var querySequence = tree.body().query_seq();

                _querySequences[i] = querySequence;
            }
        }

        [Benchmark]
        public void VisitQuerySequenceTest_0()
        {
            var result = _visitor.VisitQuery_seq(_querySequences[0]);
        }

        [Benchmark]
        public void VisitQuerySequenceTest_1()
        {
            var result = _visitor.VisitQuery_seq(_querySequences[1]);
        }

        [Benchmark]
        public void VisitQuerySequenceTest_2()
        {
            var result = _visitor.VisitQuery_seq(_querySequences[2]);
        }

        [Benchmark]
        public void VisitQuerySequenceTest_3()
        {
            var result = _visitor.VisitQuery_seq(_querySequences[3]);
        }
    }
}
