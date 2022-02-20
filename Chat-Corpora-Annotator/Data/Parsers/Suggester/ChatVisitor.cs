using Antlr4.Runtime.Misc;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using IndexEngine.Indexes;
using IndexEngine.Search;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatCorporaAnnotator.Data.Parsers.Suggester
{
    using MsgGroupList = List<List<int>>;

    public class ChatVisitor : ChatBaseVisitor<object>
    {
        public override object VisitQuery([NotNull] ChatParser.QueryContext context)
        {
            return VisitBody(context.body());
        }

        public override object VisitBody([NotNull] ChatParser.BodyContext context)
        {
            const int defaultWindowSize = 70;
            int windowSize = defaultWindowSize;

            if (context == null)
                return null;

            if (context.InWin() != null)
            {
                string text = context.number().GetText();

                if (!int.TryParse(text, out windowSize))
                {
                    windowSize = defaultWindowSize;
                    return null;
                }
            }

            if (context.restrictions() != null)
            {
                // If query/subquery contains only restrictions -- we need only merge restrictions
                // [x1...xn] -- result for restriction R1
                // [y1...ym] -- result for restriction R2
                // Select R1, R2 inwin W
                // Result is vector of vectors [[xi, yj]] where 0 < yj - xi <= W
                // So we have only one restriction group

                var groupListPermutations = (List<MsgGroupList>)VisitRestrictions(context.restrictions());
                var result = new List<MsgGroupList>();

                foreach (var groupList in groupListPermutations)
                {
                    var mergedRestrcitions = MergeRestrictions(groupList, windowSize);
                    var comp = new MsgGroupEqualityComparer<int>();

                    var uniqueMergedRestrcitions = mergedRestrcitions
                        .Where(x => !result.Any(y => y.Any(z => comp.Equals(x, z))))
                        .Select(t => new MsgGroupList { t });

                    result.AddRange(uniqueMergedRestrcitions);
                }

                result.Sort(new MsgGroupListComparer());
                return result;
            }
            else if (context.query_seq() != null)
            {
                // Now in this query subqueries only.
                // Also we have invariant: results of son's query are calculated correctlly.
                // It means that now we have only son's correct placement.

                var subqueryResults = (List<List<MsgGroupList>>)VisitQuery_seq(context.query_seq());
                return MergeQueries(subqueryResults, windowSize);
            }

            return null;
        }

        public override object VisitQuery_seq([NotNull] ChatParser.Query_seqContext context)
        {
            var contextQueries = context.query();
            var queryList = new List<List<MsgGroupList>>();

            foreach (var qery in contextQueries)
            {
                // VisitQuery returns all correct placement. 
                // For example, we have 3 group X, Y, Z:
                // select qX1,...,qXk inwin n1, qY1,...,qYs inwin n2, qZ1...qZm inwin n3
                // Suppose, that answer for separate groups is: [X1, X2] [Y1, Y2, Y3] [Z1, Z2]
                // Result of MergeRestrictionGroups([[X1, X2] [Y1, Y2, Y3] [Z1, Z2]]) is:
                // [ [X1, Y1, Z1],
                //   [X1, Y1, Z2],
                //   [X1, Y2, Z1],
                //   [X1, Y2, Z2],
                //   ............
                //   [X2, Y3, Z2] ]

                queryList.Add((List<MsgGroupList>)VisitQuery(qery));
            }

            return queryList;
        }

        public override object VisitRestrictions([NotNull] ChatParser.RestrictionsContext context)
        {
            var restrictions = context.restriction();
            var visitResults = new List<int>[restrictions.Length];

            for (int i = 0; i < restrictions.Length; ++i)
            {
                var group = (List<int>)VisitRestriction(restrictions[i]);
                group.Sort();

                visitResults[i] = group;
            }

            if (context.Unr() == null)
                return new List<MsgGroupList>() { visitResults.ToList() };

            var permutations = visitResults.GetPermutations();
            int permsCount = permutations.Count();

            var groupList = new List<MsgGroupList>(permsCount);

            foreach (var perm in permutations)
                groupList.Add(perm.ToList());

            return groupList;
        }

        public override object VisitRestriction([NotNull] ChatParser.RestrictionContext context)
        {
            if (context.And() != null)
            {
                List<int> lhs = (List<int>)VisitRestriction(context.restriction(0));
                List<int> rhs = (List<int>)VisitRestriction(context.restriction(1));

                return lhs.Intersect(rhs).ToList();
            }
            else if (context.Or() != null)
            {
                List<int> lhs = (List<int>)VisitRestriction(context.restriction(0));
                List<int> rhs = (List<int>)VisitRestriction(context.restriction(1));

                lhs.AddRange(rhs); // Do not use Union()
                return lhs;
            }
            else if (context.Not() != null)
            {
                const int defaultMsgCount = 1000000;
                int msgCount = Math.Min(defaultMsgCount, LuceneService.DirReader.MaxDoc);

                var numberList = Enumerable.Range(0, msgCount).ToList();
                var excludeList = (List<int>)VisitRestriction(context.restriction(0));

                return numberList.Except(excludeList).ToList();
            }
            else if (context.condition() != null)
            {
                var condition = context.condition();
                var visitResult = VisitCondition(condition) as HashSet<int>;

                return visitResult.ToList();
            }
            else
            {
                return VisitRestriction(context.restriction(0));
            }
        }

        public override object VisitCondition([NotNull] ChatParser.ConditionContext context)
        {
            const string errorString = "<missing STRING>";

            if (context.HasUserMentioned() != null)
            {
                string username = context.huser().GetText();

                return username != errorString
                    ? Retrievers.HasUserMentioned(username)
                    : null;
            }
            else if (context.ByUser() != null)
            {
                string username = context.huser().GetText();

                return username != errorString
                    ? Retrievers.HasUser(username)
                    : null;
            }
            else if (context.HasLocation() != null)
            {
                return Retrievers.HasNERTag(NER.LOC);
            }
            else if (context.HasOrganization() != null)
            {
                return Retrievers.HasNERTag(NER.ORG);
            }
            else if (context.HasTime() != null)
            {
                return Retrievers.HasNERTag(NER.TIME);
            }
            else if (context.HasURL() != null)
            {
                return Retrievers.HasNERTag(NER.URL);
            }
            else if (context.HasQuestion() != null)
            {
                return Retrievers.HasQuestion();
            }
            else if (context.HasWordOfDict() != null)
            {
                string dictname = context.hdict().GetText();

                if (dictname == errorString)
                    return null;

                var indexCollection = UserDictsIndex.GetInstance().IndexCollection;

                return indexCollection.TryGetValue(dictname, out List<string> words)
                    ? Retrievers.HasWordOfList(words)
                    : null;
            }
            else if (context.HasDate() != null)
            {
                return Retrievers.HasNERTag(NER.DATE);
            }
            else
            {
                return null;
            }
        }

        public MsgGroupList MergeRestrictions(MsgGroupList rList, int windowSize)
        {
            if (rList.Count == 0)
                return new MsgGroupList();

            if (rList.Count == 1)
                return rList.First().Select(t => new List<int>() { t }).ToList();

            List<int> firstGroup = rList[0];
            var result = new MsgGroupList();

            for (int i = 0; i < firstGroup.Count; i++)
            {
                var accumulatedMsgs = new List<int>() { firstGroup[i] };
                var newGroups = MergeRestrictions(rList, windowSize, accumulatedMsgs, 1);

                result.AddRange(newGroups);
            }

            return result.ToList();
        }

        private MsgGroupList MergeRestrictions(MsgGroupList groupList, int windowSize, IReadOnlyList<int> accumulatedMsgs, int startGroup)
        {
            if (startGroup >= groupList.Count)
                return new MsgGroupList() { accumulatedMsgs.ToList() };

            List<int> curGroup = groupList[startGroup];
            IEnumerable<List<int>> result = new MsgGroupList();

            int previousItem = accumulatedMsgs.Last();

            for (int i = 0; i < curGroup.Count; ++i)
            {
                var curItem = curGroup[i];

                if (curItem < previousItem || accumulatedMsgs.Contains(curItem))
                    continue;

                if (curItem - previousItem > windowSize)
                    break;

                var newAccumulatedMsgs = new List<int>(accumulatedMsgs) { curItem };

                var newGroups = MergeRestrictions(groupList, windowSize, newAccumulatedMsgs, startGroup + 1);
                result = result.Concat(newGroups);
            }

            return result.ToList();
        }

        public List<MsgGroupList> MergeQueries(List<List<MsgGroupList>> sqResults, int windowSize)
        {
            if (sqResults.Any(t => t.IsNullOrEmpty()))
                return new List<MsgGroupList>();

            List<MsgGroupList> firstSubquery = sqResults[0];
            List<MsgGroupList> result = new List<MsgGroupList>();

            for (int i = 0; i < firstSubquery.Count; ++i)
            {
                var accumulatedItems = new List<MsgGroupList>() { firstSubquery[i] };
                var newGroupLists = MergeQueries(sqResults, windowSize, accumulatedItems, 1);

                result.AddRange(newGroupLists);
            }

            return MergeGroupLists(result, sqResults.Count);
        }

        public List<MsgGroupList> MergeQueries(List<List<MsgGroupList>> sqResults, int windowSize, IReadOnlyList<MsgGroupList> accumulatedItems, int start)
        {
            if (start >= sqResults.Count)
                return new List<MsgGroupList>(accumulatedItems);

            List<MsgGroupList> currentSubquery = sqResults[start];
            List<MsgGroupList> result = new List<MsgGroupList>();

            MsgGroupList prevItem = accumulatedItems.Last();

            int prevItemLast = prevItem.Last().Last();
            int firstItemFirst = accumulatedItems.First().First().First();

            for (int i = 0; i < currentSubquery.Count; ++i)
            {
                MsgGroupList curItem = currentSubquery[i];

                int curItemFirst = curItem.First().First();
                int curItemLast = curItem.Last().Last();

                if (curItemFirst <= prevItemLast || accumulatedItems.Contains(curItem))
                    continue;

                if (curItemLast - firstItemFirst > windowSize)
                    break;

                var newAccumulated = new List<MsgGroupList>(accumulatedItems) { curItem };
                var newGroups = MergeQueries(sqResults, windowSize, newAccumulated, start + 1);

                result.AddRange(newGroups);
            }

            return result;
        }

        public List<MsgGroupList> MergeGroupLists(List<MsgGroupList> groupLists, int count)
        {
            if (groupLists.Count % count != 0)
                throw new ArgumentException();

            var result = new List<MsgGroupList>();

            for (int i = 0; i < groupLists.Count; i += count)
            {
                var groupList = new MsgGroupList();

                for (int j = i; j < i + count; ++j)
                {
                    var group = new List<int>();

                    foreach (var item in groupLists[j])
                        group.AddRange(item);

                    groupList.Add(group);
                }

                if (groupList.Count > 0)
                    result.Add(groupList);
            }

            return result;
        }
    }
}
