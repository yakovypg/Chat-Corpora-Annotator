using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using SuggesterBenchmark.Benchmarks.Base;
using System.Collections.Generic;

namespace SuggesterBenchmark.Benchmarks
{
    using SubqueryResults = List<List<List<List<int>>>>;

    public class MergeQueriesBenchmark : BenchmarkBase
    {
        private const int INWIN = 1000;

        private readonly string[] _queries = new string[]
        {
            "select (select haswordofdict(area), haswordofdict(money) inwin 5)",
            "select (select haswordofdict(area), haswordofdict(money) inwin 5); (select haswordofdict(job) and haswordofdict(dev)) inwin " + INWIN,
            "select (select haswordofdict(area), haswordofdict(money) inwin 5); (select haswordofdict(job) and haswordofdict(dev)); (select haswordofdict(area), haswordofdict(money) inwin 5) inwin " + INWIN,
            "select (select haswordofdict(area), haswordofdict(money) inwin 5); (select haswordofdict(job) and haswordofdict(dev)); (select haswordofdict(area), haswordofdict(money) inwin 5); (select haswordofdict(money) or haswordofdict(dev)) inwin " + INWIN,
        };

        private readonly ChatVisitor _visitor;
        private readonly SubqueryResults[] _visitResults;

        public MergeQueriesBenchmark()
        {
            _visitor = new ChatVisitor();
            _visitResults = new SubqueryResults[_queries.Length];

            for (int i = 0; i < _queries.Length; ++i)
            {
                var tree = QueryParser.GetTree(_queries[i]);
                var res = (SubqueryResults)_visitor.VisitQuery_seq(tree.body().query_seq());

                _visitResults[i] = res;
            }
        }

        [Benchmark]
        public void MergeQueriesTest_0()
        {
            var result = _visitor.MergeQueries(_visitResults[0], INWIN);
        }

        [Benchmark]
        public void MergeQueriesTest_1()
        {
            var result = _visitor.MergeQueries(_visitResults[1], INWIN);
        }

        [Benchmark]
        public void MergeQueriesTest_2()
        {
            var result = _visitor.MergeQueries(_visitResults[2], INWIN);
        }

        [Benchmark]
        public void MergeQueriesTest_3()
        {
            var result = _visitor.MergeQueries(_visitResults[3], INWIN);
        }
    }
}
