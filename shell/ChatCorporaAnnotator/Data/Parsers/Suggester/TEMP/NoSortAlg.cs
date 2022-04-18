using Antlr4.Runtime.Misc;
using ChatCorporaAnnotator.Data.Parsers.Suggester.Comparers;
using ChatCorporaAnnotator.Infrastructure.Extensions;
using CoreNLPEngine.Search;
using IndexEngine.Indexes;
using IndexEngine.Search;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatCorporaAnnotator.Data.Parsers.Suggester
{
    using MsgGroupList = List<List<int>>;

    public class QueryContextVisitor3 : ChatBaseVisitor<object>
    {
        public const int DEFAULT_WINDOW_SIZE = 70;
        public const int MAX_MESSAGES_RECEIVED_NOT = 1000 * 1000;

        public override object VisitQuery([NotNull] ChatParser.QueryContext context)
        {
            return VisitBody(context.body());
        }

        public override object VisitBody([NotNull] ChatParser.BodyContext context)
        {
            int windowSize = DEFAULT_WINDOW_SIZE;

            if (context == null)
                return null;

            if (context.InWin() != null)
            {
                string text = context.number().GetText();

                if (!int.TryParse(text, out windowSize))
                    return null;
            }

            if (context.restrictions() != null)
            {
                var restrictions = context.restrictions();
                var restrResultPermutations = (List<MsgGroupList>)VisitRestrictions(restrictions);
                var result = new List<MsgGroupList>();

                for (int i = 0; i < restrResultPermutations.Count; ++i)
                {
                    var mergedRestrcitions = MergeRestrictions(restrResultPermutations[i], windowSize);
                    var comp = new MsgGroupEqualityComparer();

                    var uniqueRestrcitions = mergedRestrcitions
                        .Where(x => !result.Any(y => y.Any(z => comp.Equals(x, z))))
                        .Select(t => new MsgGroupList { t });

                    result.AddRange(uniqueRestrcitions);
                }

                result.Sort(new MsgGroupListComparer());
                return result;
            }

            if (context.query_seq() != null)
            {
                var subqueryResults = (List<List<MsgGroupList>>)VisitQuery_seq(context.query_seq());
                return MergeQueries(subqueryResults, windowSize);
            }

            return null;
        }

        public override object VisitQuery_seq([NotNull] ChatParser.Query_seqContext context)
        {
            var queries = context.query();
            var result = new List<List<MsgGroupList>>();

            for (int i = 0; i < queries.Length; ++i)
            {
                var visitResult = (List<MsgGroupList>)VisitQuery(queries[i]);
                result.Add(visitResult);
            }

            return result;
        }

        public override object VisitRestrictions([NotNull] ChatParser.RestrictionsContext context)
        {
            var restrictions = context.restriction();
            var visitResults = new MsgGroupList(restrictions.Length);

            for (int i = 0; i < restrictions.Length; ++i)
            {
                var visitResult = ((IEnumerable<int>)VisitRestriction(restrictions[i])).ToList();
                //visitResult.Sort();

                visitResults.Add(visitResult);
            }

            if (context.Unr() == null)
                return new List<MsgGroupList>() { visitResults };

            var permutations = visitResults.GetPermutations();
            int permsCount = visitResults.GetPermutationsCount();

            var groupList = new List<MsgGroupList>(permsCount);

            foreach (var perm in permutations)
                groupList.Add(perm.ToList());

            return groupList;
        }

        public override object VisitRestriction([NotNull] ChatParser.RestrictionContext context)
        {
            if (context.And() != null)
            {
                var lhs = (IEnumerable<int>)VisitRestriction(context.restriction(0));
                var rhs = (IEnumerable<int>)VisitRestriction(context.restriction(1));

                return lhs.Intersect(rhs);
            }

            if (context.Or() != null)
            {
                var lhs = (IEnumerable<int>)VisitRestriction(context.restriction(0));
                var rhs = (IEnumerable<int>)VisitRestriction(context.restriction(1));

                return lhs.Concat(rhs);
            }

            if (context.Not() != null)
            {
                int msgCount = Math.Min(MAX_MESSAGES_RECEIVED_NOT, LuceneService.DirReader.MaxDoc);

                var allMessages = Enumerable.Range(0, msgCount);
                var excludeMessages = (IEnumerable<int>)VisitRestriction(context.restriction(0));

                return allMessages.Except(excludeMessages);
            }

            if (context.condition() != null)
            {
                var condition = context.condition();
                return VisitCondition(condition);
            }

            return VisitRestriction(context.restriction(0));
        }

        public override object VisitCondition([NotNull] ChatParser.ConditionContext context)
        {
            const string errorString = "<missing STRING>";

            if (context.HasWordOfDict() != null)
            {
                string dictname = context.hdict().GetText();

                if (dictname == errorString)
                    return null;

                var userDicts = UserDictsIndex.GetInstance().IndexCollection;

                return userDicts.TryGetValue(dictname, out List<string> words)
                    ? RetrieversSearch.HasWordOfList(words)
                    : null;
            }

            if (context.ByUser() != null)
            {
                string username = context.huser().GetText();

                return username != errorString
                    ? RetrieversSearch.HasUser(username)
                    : null;
            }

            if (context.HasUserMentioned() != null)
            {
                string username = context.huser().GetText();

                return username != errorString
                    ? RetrieversSearch.HasUserMentioned(username)
                    : null;
            }

            if (context.HasQuestion() != null)
                return RetrieversSearch.HasQuestion();

            if (context.HasDate() != null)
                return RetrieversSearch.HasNERTag(NERLabels.DATE);

            if (context.HasLocation() != null)
                return RetrieversSearch.HasNERTag(NERLabels.LOC);

            if (context.HasOrganization() != null)
                return RetrieversSearch.HasNERTag(NERLabels.ORG);

            if (context.HasTime() != null)
                return RetrieversSearch.HasNERTag(NERLabels.TIME);

            if (context.HasURL() != null)
                return RetrieversSearch.HasNERTag(NERLabels.URL);

            return null;
        }

        public MsgGroupList MergeRestrictions(MsgGroupList groupList, int windowSize)
        {
            if (groupList.Count == 0)
                return new MsgGroupList();

            if (groupList.Count == 1)
                return groupList[0].Select(t => new List<int>() { t }).ToList();

            var result = new MsgGroupList();
            int[] counts = new int[groupList.Count - 1];
            int[] placement = new int[groupList.Count - 1];

            for (int i = 1; i < groupList.Count; ++i)
            {
                counts[i - 1] = groupList[i].Count;
                placement[i - 1] = i;
            }

            Array.Sort(counts, placement);

            List<int> firstGroup = groupList[0];

            for (int i = 0; i < firstGroup.Count; ++i)
            {
                var accumulatedMsgs = new List<int>(groupList.Count) { firstGroup[i] };

                for (int j = 1; j < groupList.Count; ++j)
                    accumulatedMsgs.Add(-1);

                var newGroups = MergeRestrictions(groupList, placement, windowSize, accumulatedMsgs, 0);
                result.AddRange(newGroups);
            }

            return result;
        }

        public MsgGroupList MergeRestrictions(MsgGroupList groupList, int[] placement, int windowSize, List<int> accumulatedMsgs, int curIndex)
        {
            if (curIndex >= placement.Length)
                return new MsgGroupList() { accumulatedMsgs };

            var result = new MsgGroupList();

            int firstItem = accumulatedMsgs[0];
            int curGroupPosition = placement[curIndex];
            List<int> curGroup = groupList[curGroupPosition];

            (int? previousMsg, int? nextMsg) = GetPreviousAndNextMessage(accumulatedMsgs, curGroupPosition);

            for (int i = 0; i < curGroup.Count; ++i)
            {
                int curMsg = curGroup[i];

                if (curMsg <= previousMsg)
                    continue;

                if ((nextMsg.HasValue && curMsg >= nextMsg) ||
                    (curMsg - firstItem > windowSize))
                {
                    continue;
                }

                var newAccumulatedMsgs = new List<int>(accumulatedMsgs)
                {
                    [curGroupPosition] = curMsg
                };

                var newGroups = MergeRestrictions(groupList, placement, windowSize, newAccumulatedMsgs, curIndex + 1);
                result.AddRange(newGroups);
            }

            return result;
        }

        private static (int? previousItem, int? nextItem) GetPreviousAndNextMessage(List<int> list, int startIndex)
        {
            int? previousItem = null;
            int? nextItem = null;

            for (int i = startIndex + 1; i < list.Count; ++i)
            {
                if (list[i] < 0)
                    continue;

                nextItem = list[i];
                break;
            }

            for (int i = startIndex - 1; i >= 0; --i)
            {
                if (list[i] < 0)
                    continue;

                previousItem = list[i];
                break;
            }

            return (previousItem, nextItem);
        }

        //public MsgGroupList MergeRestrictions(MsgGroupList groupList, int windowSize)
        //{
        //    if (groupList.Count == 0)
        //        return new MsgGroupList();

        //    if (groupList.Count == 1)
        //        return groupList[0].Select(t => new List<int>() { t }).ToList();

        //    List<int> firstGroup = groupList[0];
        //    var result = new MsgGroupList();

        //    for (int i = 0; i < firstGroup.Count; i++)
        //    {
        //        var accumulatedMsgs = new List<int>() { firstGroup[i] };
        //        var newGroups = MergeRestrictions(groupList, windowSize, accumulatedMsgs, 1);

        //        result.AddRange(newGroups);
        //    }

        //    return result;
        //}

        //private MsgGroupList MergeRestrictions(MsgGroupList groupList, int windowSize, IReadOnlyList<int> accumulatedMsgs, int startGroup)
        //{
        //    if (startGroup >= groupList.Count)
        //        return new MsgGroupList() { accumulatedMsgs.ToList() };

        //    List<int> curGroup = groupList[startGroup];
        //    var result = new MsgGroupList();

        //    int firstItem = accumulatedMsgs[0];
        //    int previousItem = accumulatedMsgs[accumulatedMsgs.Count - 1];

        //    for (int i = 0; i < curGroup.Count; ++i)
        //    {
        //        int curItem = curGroup[i];

        //        if (curItem <= previousItem)
        //            continue;

        //        if (curItem - firstItem > windowSize)
        //            break;

        //        var newAccumulatedMsgs = new List<int>(accumulatedMsgs) { curItem };

        //        var newGroups = MergeRestrictions(groupList, windowSize, newAccumulatedMsgs, startGroup + 1);
        //        result.AddRange(newGroups);
        //    }

        //    return result;
        //}

        public List<MsgGroupList> MergeQueries(List<List<MsgGroupList>> subqueryResults, int windowSize)
        {
            if (subqueryResults.Any(t => t.IsNullOrEmpty()))
                return new List<MsgGroupList>();

            var firstSubqueryResult = subqueryResults[0];
            var result = new List<MsgGroupList>();

            for (int i = 0; i < firstSubqueryResult.Count; ++i)
            {
                var curItem = firstSubqueryResult[i];

                int windowStart = curItem[0][0];
                int curItemLast = curItem[^1][^1];

                var accumulatedItems = new List<MsgGroupList>() { curItem };
                var mergedSubqueryResults = MergeQueries(subqueryResults, windowSize, windowStart, curItemLast, accumulatedItems, 1);

                result.AddRange(mergedSubqueryResults);
            }

            return CombineGroupLists(result, subqueryResults.Count);
        }

        public List<MsgGroupList> MergeQueries(List<List<MsgGroupList>> subqueryResults, int windowSize, int windowStart, int prevLast, IReadOnlyList<MsgGroupList> accumulatedItems, int start)
        {
            if (start >= subqueryResults.Count)
                return new List<MsgGroupList>(accumulatedItems);

            var currentSubqueryResult = subqueryResults[start];
            var result = new List<MsgGroupList>();

            for (int i = 0; i < currentSubqueryResult.Count; ++i)
            {
                MsgGroupList curItem = currentSubqueryResult[i];

                int curItemFirst = curItem[0][0];
                int curItemLast = curItem[^1][^1];

                if (curItemFirst <= prevLast)
                    continue;

                if (curItemLast - windowStart > windowSize)
                    break;

                var newAccumulatedItems = new List<MsgGroupList>(accumulatedItems) { curItem };
                var mergedSubqueryResults = MergeQueries(subqueryResults, windowSize, windowStart, curItemLast, newAccumulatedItems, start + 1);

                result.AddRange(mergedSubqueryResults);
            }

            return result;
        }

        private static List<MsgGroupList> CombineGroupLists(List<MsgGroupList> groupLists, int combineSectionLength)
        {
            if (groupLists.Count % combineSectionLength != 0)
                throw new ArgumentException("The number of list items must be divided by the length of the section.");

            var result = new List<MsgGroupList>();

            for (int i = 0; i < groupLists.Count; i += combineSectionLength)
            {
                var groupList = new MsgGroupList();

                for (int j = i; j < i + combineSectionLength; ++j)
                {
                    var group = new List<int>();

                    foreach (var list in groupLists[j])
                        group.AddRange(list);

                    groupList.Add(group);
                }

                if (groupList.Count > 0)
                    result.Add(groupList);
            }

            return result;
        }
    }
}
