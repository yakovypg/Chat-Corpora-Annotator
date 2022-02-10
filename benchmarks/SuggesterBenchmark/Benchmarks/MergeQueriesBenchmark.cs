using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using SuggesterBenchmark.Benchmarks.Base;
using System.Collections.Generic;

namespace SuggesterBenchmark.Benchmarks
{
    using SubqueryResults = List<List<List<List<int>>>>;

    public class MergeQueriesBenchmark : BenchmarkBase
    {
        [Params(5, 50)]
        public int Inwin;

        private readonly string[] _queries = new string[]
        {
            "select (select haswordofdict(job), haswordofdict(skill) inwin 5); (select haswordofdict(area) or haswordofdict(money) or haswordofdict(dev)) inwin 5", // 148
            "select (select haswordofdict(job), haswordofdict(skill) inwin 5); (select haswordofdict(area) or haswordofdict(dev), haswordofdict(money) or haswordofdict(dev) inwin 5) inwin 5", // 169
            "select (select haswordofdict(job), haswordofdict(skill) inwin 5); (select haswordofdict(area) or haswordofdict(dev), haswordofdict(dev) mess inwin 5) inwin 5", // 150
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
            var result = _visitor.MergeQueries(_visitResults[0], Inwin);
        }

        [Benchmark]
        public void MergeQueriesTest_1()
        {
            var result = _visitor.MergeQueries(_visitResults[1], Inwin);
        }

        [Benchmark]
        public void MergeQueriesTest_2()
        {
            var result = _visitor.MergeQueries(_visitResults[2], Inwin);
        }
    }
}
