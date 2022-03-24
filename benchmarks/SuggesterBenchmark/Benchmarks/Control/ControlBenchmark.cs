using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using SuggesterBenchmark.Benchmarks.Base;

namespace SuggesterBenchmark.Benchmarks.Control
{
    using MsgGroupList = List<List<int>>;
    using SubqueryResults = List<List<List<List<int>>>>;

    public class ControlBenchmark : BenchmarkBase
    {
        private const int MERGE_QUERIES_QUERIES_INWIN = 1000;


        private readonly string[] _visitRestrictionQueries = new string[]
        {
            "select (haswordofdict(skill) or haswordofdict(job)) and (byuser(odrisck) or byuser(cerissa))",
            "select (haswordofdict(dev) and haswordofdict(job)) or (haswordofdict(dev) and byuser(odrisck))"
        };

        private readonly string[] _visitRestrictionsQueries = new string[]
        {
            "select haswordofdict(job), haswordofdict(skill), haswordofdict(dev), haswordofdict(area)",
            "select haswordofdict(job), haswordofdict(skill), haswordofdict(dev), haswordofdict(area) unr"
        };

        private readonly string[] _mergeRestrictionsQueries = new string[]
        {
            "select haswordofdict(skill), haswordofdict(skill), haswordofdict(job), haswordofdict(skill)",
            "select haswordofdict(skill), haswordofdict(skill), haswordofdict(job), haswordofdict(skill), haswordofdict(dev)"
        };

        private readonly string[] _mergeQueriesQueries = new string[]
        {
            "select (select haswordofdict(area), haswordofdict(money) inwin 5); (select haswordofdict(job) and haswordofdict(dev)); (select haswordofdict(area), haswordofdict(money) inwin 5) inwin " + MERGE_QUERIES_QUERIES_INWIN,
            "select (select haswordofdict(area), haswordofdict(money) inwin 5); (select haswordofdict(job) and haswordofdict(dev)); (select haswordofdict(area), haswordofdict(money) inwin 5); (select haswordofdict(money) or haswordofdict(dev)) inwin " + MERGE_QUERIES_QUERIES_INWIN
        };

        private readonly QueryContextVisitor _visitor;

        private readonly ChatParser.RestrictionContext[] _restrictionContexts;
        private readonly ChatParser.RestrictionsContext[] _restrictionsContexts;

        private readonly SubqueryResults[] _visitQueryResults;
        private readonly MsgGroupList[] _visitRestrictionsResults;

        public ControlBenchmark()
        {
            _visitor = new QueryContextVisitor();

            _restrictionContexts = new ChatParser.RestrictionContext[_visitRestrictionQueries.Length];
            _restrictionsContexts = new ChatParser.RestrictionsContext[_visitRestrictionsQueries.Length];

            _visitQueryResults = new SubqueryResults[_mergeQueriesQueries.Length];
            _visitRestrictionsResults = new MsgGroupList[_mergeRestrictionsQueries.Length];

            for (int i = 0; i < _visitRestrictionQueries.Length; ++i)
            {
                var tree = QueryParser.GetTree(_visitRestrictionQueries[i]);
                var restriction = tree.body().restrictions().restriction(0);

                _restrictionContexts[i] = restriction;
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

        [Benchmark(Description = "VisitRestriction_0")]
        public void VisitRestrictionTest_0()
        {
            var result = _visitor.VisitRestriction(_restrictionContexts[0]);
        }

        [Benchmark(Description = "VisitRestriction_1")]
        public void VisitRestrictionTest_1()
        {
            var result = _visitor.VisitRestriction(_restrictionContexts[1]);
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
            var result = _visitor.MergeQueries(_visitQueryResults[0], MERGE_QUERIES_QUERIES_INWIN);
        }

        [Benchmark(Description = "MergeQueries_1")]
        public void MergeQueriesTest_1()
        {
            var result = _visitor.MergeQueries(_visitQueryResults[1], MERGE_QUERIES_QUERIES_INWIN);
        }
    }
}
