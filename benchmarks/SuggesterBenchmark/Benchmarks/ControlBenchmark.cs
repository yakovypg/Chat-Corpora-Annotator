using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using SuggesterBenchmark.Benchmarks.Base;

namespace SuggesterBenchmark.Benchmarks
{
    using MsgGroupList = List<List<int>>;
    using SubqueryResults = List<List<List<List<int>>>>;

    public class ControlBenchmark : BenchmarkBase
    {
        private const int MERGE_QUERIES_INWIN = 1000;

        private readonly string[] _visitQueryQueries = new string[]
        {
            "select (select haswordofdict(skill), haswordofdict(job) unr inwin 5); (select haswordofdict(dev) and (byuser(odrisck) or byuser(jsonify)) inwin 3); inwin 50",
            "select (select haswordofdict(job), haswordofdict(dev) unr inwin 5); (select haswordofdict(skill), haswordofdict(dev), haswordofdict(money) inwin 10); (select haswordofdict(area) or haswordofdict(money)); (select haswordofdict(dev), haswordofdict(money) or byuser(odrisck) inwin 50) inwin 500",
            
            "select haswordofdict(dev), haswordofdict(area), haswordofdict(area), haswordofdict(money)",
            "select haswordofdict(dev), haswordofdict(area), haswordofdict(area), haswordofdict(money) unr"
        };

        private readonly string[] _visitRestrictionsQueries = new string[]
        {
            "select haswordofdict(dev), haswordofdict(area), haswordofdict(area), haswordofdict(money)",
            "select haswordofdict(dev), haswordofdict(area), haswordofdict(area), haswordofdict(money) unr"
        };

        private readonly string[] _mergeRestrictionsQueries = new string[]
        {
            "select haswordofdict(job), haswordofdict(dev), haswordofdict(job), haswordofdict(dev), haswordofdict(job)",
            "select haswordofdict(skill), haswordofdict(skill), haswordofdict(job), haswordofdict(dev), haswordofdict(area)"
        };

        private readonly string[] _mergeQueriesQueries = new string[]
        {
            "select (select haswordofdict(job), haswordofdict(dev) inwin 5); (select haswordofdict(dev), haswordofdict(job) inwin 5); (select haswordofdict(job), haswordofdict(dev) inwin 5); (select haswordofdict(dev), haswordofdict(job) inwin 5) inwin " + MERGE_QUERIES_INWIN,
            "select (select haswordofdict(skill) or byuser(odrisck)); (select haswordofdict(job), haswordofdict(skill) inwin 50); (select haswordofdict(area), haswordofdict(money) inwin 2); (select haswordofdict(dev) and haswordofdict(money)) inwin " + MERGE_QUERIES_INWIN
        };

        private readonly QueryContextVisitor _visitor;

        private readonly ChatParser.QueryContext[] _queryContexts;
        private readonly ChatParser.RestrictionsContext[] _restrictionsContexts;

        private readonly SubqueryResults[] _visitQueryResults;
        private readonly MsgGroupList[] _visitRestrictionsResults;

        public ControlBenchmark()
        {
            _visitor = new QueryContextVisitor();

            _queryContexts = new ChatParser.QueryContext[_visitQueryQueries.Length];
            _restrictionsContexts = new ChatParser.RestrictionsContext[_visitRestrictionsQueries.Length];

            _visitQueryResults = new SubqueryResults[_mergeQueriesQueries.Length];
            _visitRestrictionsResults = new MsgGroupList[_mergeRestrictionsQueries.Length];

            for (int i = 0; i < _visitQueryQueries.Length; ++i)
            {
                var tree = QueryParser.GetTree(_visitQueryQueries[i]);
                _queryContexts[i] = tree;
            }

            for (int i = 0; i < _visitRestrictionsQueries.Length; ++i)
            {
                var tree = QueryParser.GetTree(_visitRestrictionsQueries[i]);
                var restrictions = tree.body().restrictions();

                _restrictionsContexts[i] = restrictions;
            }

            for (int i = 0; i < _mergeQueriesQueries.Length; ++i)
            {
                var tree = QueryParser.GetTree(_mergeQueriesQueries[i]);
                var res = (SubqueryResults)_visitor.VisitQuery_seq(tree.body().query_seq());

                _visitQueryResults[i] = res;
            }

            for (int i = 0; i < _mergeRestrictionsQueries.Length; ++i)
            {
                var tree = QueryParser.GetTree(_mergeRestrictionsQueries[i]);
                var res = (List<MsgGroupList>)_visitor.VisitRestrictions(tree.body().restrictions());

                _visitRestrictionsResults[i] = res[0];
            }
        }

        [Benchmark(Description = "VisitQuery_0")]
        public void VisitQueryTest_0()
        {
            var result = _visitor.VisitQuery(_queryContexts[0]);
        }

        [Benchmark(Description = "VisitQuery_1")]
        public void VisitQueryTest_1()
        {
            var result = _visitor.VisitQuery(_queryContexts[1]);
        }

        [Benchmark(Description = "VisitQuery_2")]
        public void VisitQueryTest_2()
        {
            var result = _visitor.VisitQuery(_queryContexts[2]);
        }

        [Benchmark(Description = "VisitQuery_3")]
        public void VisitQueryTest_3()
        {
            var result = _visitor.VisitQuery(_queryContexts[3]);
        }

        [Benchmark(Description = "VisitRestrictions_0")]
        public void VisitRestrictionsTest_0()
        {
            var result = _visitor.VisitRestrictions(_restrictionsContexts[0]);
        }

        [Benchmark(Description = "VisitRestrictions_1")]
        public void VisitRestrictionsTest_1()
        {
            var result = _visitor.VisitRestrictions(_restrictionsContexts[1]);
        }

        [Benchmark(Description = "MergeRestrictions_0")]
        public void MergeRestrictionsTest_0()
        {
            var result = _visitor.MergeRestrictions(_visitRestrictionsResults[0], QueryContextVisitor.DEFAULT_WINDOW_SIZE);
        }

        [Benchmark(Description = "MergeRestrictions_1")]
        public void MergeRestrictionsTest_1()
        {
            var result = _visitor.MergeRestrictions(_visitRestrictionsResults[1], QueryContextVisitor.DEFAULT_WINDOW_SIZE);
        }

        [Benchmark(Description = "MergeQueries_0")]
        public void MergeQueriesTest_0()
        {
            var result = _visitor.MergeQueries(_visitQueryResults[0], MERGE_QUERIES_INWIN);
        }

        [Benchmark(Description = "MergeQueries_1")]
        public void MergeQueriesTest_1()
        {
            var result = _visitor.MergeQueries(_visitQueryResults[1], MERGE_QUERIES_INWIN);
        }
    }
}
