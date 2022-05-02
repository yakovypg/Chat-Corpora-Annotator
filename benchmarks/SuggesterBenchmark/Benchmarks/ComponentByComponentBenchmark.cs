using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using SuggesterBenchmark.Benchmarks.Base;

namespace SuggesterBenchmark.Benchmarks
{
    using MsgGroupList = List<List<int>>;

    public class ComponentByComponentBenchmark : BenchmarkBase
    {
        private const string Q4 = "select " +
            "haswordofdict(job), haswordofdict(code), hasusermentioned(Kadams223) " +
            "unr inwin 40";

        private readonly ChatParser.QueryContext _q4Tree;
        private readonly ChatParser.RestrictionsContext _q4RestrictionsContext;
        private readonly ChatParser.RestrictionContext[] _q4RestrictionContext;
        private readonly ChatParser.ConditionContext[] _q4ConditionContext;
        private readonly List<MsgGroupList> _q4VisitRestrictionsResult;
        private readonly int[] _q4Inwins;

        private const string Q5 = "select " +
            "haswordofdict(job), haswordofdict(skill), haswordofdict(skill), haswordofdict(area), haswordofdict(money) " +
            "inwin 40";

        private readonly ChatParser.QueryContext _q5Tree;
        private readonly ChatParser.RestrictionsContext _q5RestrictionsContext;
        private readonly ChatParser.RestrictionContext[] _q5RestrictionContext;
        private readonly ChatParser.ConditionContext[] _q5ConditionContext;
        private readonly List<MsgGroupList> _q5VisitRestrictionsResult;
        private readonly int[] _q5Inwins;

        private const string Q6 = "select " +
            "(select haswordofdict(job), haswordofdict(skill), haswordofdict(code), byuser(Lumiras) inwin 60); " +
            "(select byuser(Lumiras) and haswordofdict(issue)) " +
            "inwin 200";

        private readonly ChatParser.QueryContext _q6Tree;
        private readonly ChatParser.Query_seqContext _q6QuerySeqContext;
        private readonly ChatParser.RestrictionsContext[] _q6RestrictionsContext;
        private readonly ChatParser.RestrictionContext[][] _q6RestrictionContext;
        private readonly ChatParser.ConditionContext[][] _q6ConditionContext;
        private readonly List<MsgGroupList>[] _q6VisitRestrictionsResult;
        private readonly int[] _q6Inwins;

        private const string Q7 = "select " +
            "(select haswordofdict(job), haswordofdict(skill), haswordofdict(code), byuser(Lumiras)); " +
            "(select haswordofdict(job), haswordofdict(skill), haswordofdict(code), byuser(odrisck) inwin 40); " +
            "(select haswordofdict(job), haswordofdict(skill), haswordofdict(code), byuser(odrisck) inwin 40) " +
            "inwin 300";

        private readonly ChatParser.QueryContext _q7Tree;
        private readonly ChatParser.Query_seqContext _q7QuerySeqContext;
        private readonly ChatParser.RestrictionsContext[] _q7RestrictionsContext;
        private readonly ChatParser.RestrictionContext[][] _q7RestrictionContext;
        private readonly ChatParser.ConditionContext[][] _q7ConditionContext;
        private readonly List<MsgGroupList>[] _q7VisitRestrictionsResult;
        private readonly int[] _q7Inwins;

        private readonly QueryContextVisitor _visitor;

        public ComponentByComponentBenchmark()
        {
            _visitor = new QueryContextVisitor();

            _q4Tree = QueryParser.GetTree(Q4);
            _q4RestrictionsContext = _q4Tree.body().restrictions();
            _q4RestrictionContext = _q4RestrictionsContext.restriction();
            _q4ConditionContext = _q4RestrictionContext.Select(t => t.condition()).ToArray();
            _q4VisitRestrictionsResult = (List<MsgGroupList>)_visitor.VisitRestrictions(_q4RestrictionsContext);
            _q4Inwins = new int[] { 40 };

            _q5Tree = QueryParser.GetTree(Q5);
            _q5RestrictionsContext = _q5Tree.body().restrictions();
            _q5RestrictionContext = _q5RestrictionsContext.restriction();
            _q5ConditionContext = _q5RestrictionContext.Select(t => t.condition()).ToArray();
            _q5VisitRestrictionsResult = (List<MsgGroupList>)_visitor.VisitRestrictions(_q5RestrictionsContext);
            _q5Inwins = new int[] { 40 };

            _q6Tree = QueryParser.GetTree(Q6);
            _q6QuerySeqContext = _q6Tree.body().query_seq();
            _q6RestrictionsContext = _q6QuerySeqContext.query().Select(t => t.body().restrictions()).ToArray();
            _q6RestrictionContext = new ChatParser.RestrictionContext[][]
            {
                _q6RestrictionsContext[0].restriction(),
                new ChatParser.RestrictionContext[] { _q6RestrictionsContext[1].restriction(0).restriction(0), _q6RestrictionsContext[1].restriction(0).restriction(1) }
            };
            _q6ConditionContext = new ChatParser.ConditionContext[][]
            {
                _q6RestrictionContext[0].Select(t => t.condition()).ToArray(),
                _q6RestrictionContext[1].Select(t => t.condition()).ToArray(),
            };
            _q6VisitRestrictionsResult = new List<MsgGroupList>[]
            {
                (List<MsgGroupList>)_visitor.VisitRestrictions(_q6RestrictionsContext[0]),
                (List<MsgGroupList>)_visitor.VisitRestrictions(_q6RestrictionsContext[1]),
            };
            _q6Inwins = new int[] { 60, QueryContextVisitor.DEFAULT_WINDOW_SIZE };

            _q7Tree = QueryParser.GetTree(Q7);
            _q7QuerySeqContext = _q7Tree.body().query_seq();
            _q7RestrictionsContext = _q7QuerySeqContext.query().Select(t => t.body().restrictions()).ToArray();
            _q7RestrictionContext = _q7RestrictionsContext.Select(t => t.restriction()).ToArray();
            _q7ConditionContext = new ChatParser.ConditionContext[][]
            {
                _q7RestrictionContext[0].Select(t => t.condition()).ToArray(),
                _q7RestrictionContext[1].Select(t => t.condition()).ToArray(),
                _q7RestrictionContext[2].Select(t => t.condition()).ToArray(),
            };
            _q7VisitRestrictionsResult = new List<MsgGroupList>[]
            {
                (List<MsgGroupList>)_visitor.VisitRestrictions(_q7RestrictionsContext[0]),
                (List<MsgGroupList>)_visitor.VisitRestrictions(_q7RestrictionsContext[1]),
                (List<MsgGroupList>)_visitor.VisitRestrictions(_q7RestrictionsContext[2]),
            };
            _q7Inwins = new int[] { QueryContextVisitor.DEFAULT_WINDOW_SIZE, 40, 40 };
        }

        [Benchmark]
        public void Q4_VisitQuery()
        {
            var result = _visitor.VisitQuery(_q4Tree);
        }

        [Benchmark]
        public void Q4_VisitRestrictions()
        {
            var result = _visitor.VisitRestrictions(_q4RestrictionsContext);
        }

        [Benchmark]
        public void Q4_VisitCondition()
        {
            var result0 = _visitor.VisitCondition(_q4ConditionContext[0]);
            var result1 = _visitor.VisitCondition(_q4ConditionContext[1]);
            var result2 = _visitor.VisitCondition(_q4ConditionContext[2]);
        }

        [Benchmark]
        public void Q4_MergeRestrictions()
        {
            var result = _visitor.MergeRestrictions(_q4VisitRestrictionsResult[0], _q4Inwins[0]);
        }

        [Benchmark]
        public void Q5_VisitQuery()
        {
            var result = _visitor.VisitQuery(_q5Tree);
        }

        [Benchmark]
        public void Q5_VisitRestrictions()
        {
            var result = _visitor.VisitRestrictions(_q5RestrictionsContext);
        }

        [Benchmark]
        public void Q5_VisitCondition()
        {
            var result0 = _visitor.VisitCondition(_q5ConditionContext[0]);
            var result1 = _visitor.VisitCondition(_q5ConditionContext[1]);
            var result2 = _visitor.VisitCondition(_q5ConditionContext[2]);
            var result3 = _visitor.VisitCondition(_q5ConditionContext[3]);
            var result4 = _visitor.VisitCondition(_q5ConditionContext[4]);
        }

        [Benchmark]
        public void Q5_MergeRestrictions()
        {
            var result = _visitor.MergeRestrictions(_q5VisitRestrictionsResult[0], _q5Inwins[0]);
        }

        [Benchmark]
        public void Q6_VisitQuery()
        {
            var result = _visitor.VisitQuery(_q6Tree);
        }

        [Benchmark]
        public void Q6_VisitRestrictions()
        {
            var result0 = _visitor.VisitRestrictions(_q6RestrictionsContext[0]);
            var result1 = _visitor.VisitRestrictions(_q6RestrictionsContext[1]);
        }

        [Benchmark]
        public void Q6_VisitCondition()
        {
            var result00 = _visitor.VisitCondition(_q6ConditionContext[0][0]);
            var result01 = _visitor.VisitCondition(_q6ConditionContext[0][1]);
            var result02 = _visitor.VisitCondition(_q6ConditionContext[0][2]);
            var result03 = _visitor.VisitCondition(_q6ConditionContext[0][3]);
            var result10 = _visitor.VisitCondition(_q6ConditionContext[1][0]);
            var result11 = _visitor.VisitCondition(_q6ConditionContext[1][1]);
        }

        [Benchmark]
        public void Q6_MergeRestrictions()
        {
            var result0 = _visitor.MergeRestrictions(_q6VisitRestrictionsResult[0][0], _q6Inwins[0]);
            var result1 = _visitor.MergeRestrictions(_q6VisitRestrictionsResult[1][0], _q6Inwins[1]);
        }

        [Benchmark]
        public void Q7_VisitQuery()
        {
            var result = _visitor.VisitQuery(_q7Tree);
        }

        [Benchmark]
        public void Q7_VisitRestrictions()
        {
            var result0 = _visitor.VisitRestrictions(_q7RestrictionsContext[0]);
            var result1 = _visitor.VisitRestrictions(_q7RestrictionsContext[1]);
            var result2 = _visitor.VisitRestrictions(_q7RestrictionsContext[2]);
        }

        [Benchmark]
        public void Q7_VisitCondition()
        {
            var result00 = _visitor.VisitCondition(_q7ConditionContext[0][0]);
            var result01 = _visitor.VisitCondition(_q7ConditionContext[0][1]);
            var result02 = _visitor.VisitCondition(_q7ConditionContext[0][2]);
            var result03 = _visitor.VisitCondition(_q7ConditionContext[0][3]);

            var result10 = _visitor.VisitCondition(_q7ConditionContext[1][0]);
            var result11 = _visitor.VisitCondition(_q7ConditionContext[1][1]);
            var result12 = _visitor.VisitCondition(_q7ConditionContext[1][2]);
            var result13 = _visitor.VisitCondition(_q7ConditionContext[1][3]);

            var result20 = _visitor.VisitCondition(_q7ConditionContext[2][0]);
            var result21 = _visitor.VisitCondition(_q7ConditionContext[2][1]);
            var result22 = _visitor.VisitCondition(_q7ConditionContext[2][2]);
            var result23 = _visitor.VisitCondition(_q7ConditionContext[2][3]);
        }

        [Benchmark]
        public void Q7_MergeRestrictions()
        {
            var result0 = _visitor.MergeRestrictions(_q7VisitRestrictionsResult[0][0], _q7Inwins[0]);
            var result1 = _visitor.MergeRestrictions(_q7VisitRestrictionsResult[1][0], _q7Inwins[1]);
            var result2 = _visitor.MergeRestrictions(_q7VisitRestrictionsResult[2][0], _q7Inwins[2]);
        }
    }
}
