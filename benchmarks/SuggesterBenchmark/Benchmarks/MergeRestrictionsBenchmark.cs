using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using SuggesterBenchmark.Benchmarks.Base;
using System.Collections.Generic;

namespace SuggesterBenchmark.Benchmarks
{
    using MsgGroupList = List<List<int>>;

    public class MergeRestrictionsBenchmark : BenchmarkBase
    {
        [Params(5, 50)]
        public int Inwin;

        private readonly string[] _queries = new string[]
        {
            "select haswordofdict(job), haswordofdict(skill)", // 47
            "select haswordofdict(job) or haswordofdict(dev), haswordofdict(skill)", // 69
            "select haswordofdict(job), haswordofdict(skill), haswordofdict(dev)", // 67
            "select haswordofdict(job), haswordofdict(skill), haswordofdict(dev), haswordofdict(skill)", // 89
        };

        private readonly ChatVisitor _visitor;
        private readonly MsgGroupList[] _visitResults;

        public MergeRestrictionsBenchmark()
        {
            _visitor = new ChatVisitor();
            _visitResults = new MsgGroupList[_queries.Length];

            for (int i = 0; i < _queries.Length; ++i)
            {
                var tree = QueryParser.GetTree(_queries[i]);
                var res = (List<MsgGroupList>)_visitor.VisitRestrictions(tree.body().restrictions());

                _visitResults[i] = res[0];
            }
        }

        [Benchmark]
        public void MergeRestrictionsTest_0()
        {
            var result = _visitor.MergeRestrictions(_visitResults[0], Inwin);
        }

        [Benchmark]
        public void MergeRestrictionsTest_1()
        {
            var result = _visitor.MergeRestrictions(_visitResults[1], Inwin);
        }

        [Benchmark]
        public void MergeRestrictionsTest_2()
        {
            var result = _visitor.MergeRestrictions(_visitResults[2], Inwin);
        }

        [Benchmark]
        public void MergeRestrictionsTest_3()
        {
            var result = _visitor.MergeRestrictions(_visitResults[3], Inwin);
        }
    }
}
