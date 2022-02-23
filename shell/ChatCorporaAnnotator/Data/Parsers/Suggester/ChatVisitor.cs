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
                    var comp = new MsgGroupEqualityComparer<int>();

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
                visitResult.Sort();

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
                    ? Retrievers.HasWordOfList(words)
                    : null;
            }

            if (context.ByUser() != null)
            {
                string username = context.huser().GetText();

                return username != errorString
                    ? Retrievers.HasUser(username)
                    : null;
            }

            if (context.HasUserMentioned() != null)
            {
                string username = context.huser().GetText();

                return username != errorString
                    ? Retrievers.HasUserMentioned(username)
                    : null;
            }

            if (context.HasQuestion() != null)
                return Retrievers.HasQuestion();

            if (context.HasDate() != null)
                return Retrievers.HasNERTag(NER.DATE);

            if (context.HasLocation() != null)
                return Retrievers.HasNERTag(NER.LOC);

            if (context.HasOrganization() != null)
                return Retrievers.HasNERTag(NER.ORG);

            if (context.HasTime() != null)
                return Retrievers.HasNERTag(NER.TIME);

            if (context.HasURL() != null)
                return Retrievers.HasNERTag(NER.URL);

            return null;
        }

        public MsgGroupList MergeRestrictions(MsgGroupList groupList, int windowSize)
        {
            if (groupList.Count == 0)
                return new MsgGroupList();

            if (groupList.Count == 1)
                return groupList.First().Select(t => new List<int>() { t }).ToList();

            List<int> firstGroup = groupList[0];
            var result = new MsgGroupList();

            for (int i = 0; i < firstGroup.Count; i++)
            {
                var accumulatedMsgs = new List<int>() { firstGroup[i] };
                var newGroups = MergeRestrictions(groupList, windowSize, accumulatedMsgs, 1);

                result.AddRange(newGroups);
            }

            return result;
        }

        private MsgGroupList MergeRestrictions(MsgGroupList groupList, int windowSize, IReadOnlyList<int> accumulatedMsgs, int startGroup)
        {
            if (startGroup >= groupList.Count)
                return new MsgGroupList() { accumulatedMsgs.ToList() };

            List<int> curGroup = groupList[startGroup];
            var result = new MsgGroupList();

            int firstItem = accumulatedMsgs[0];
            int previousItem = accumulatedMsgs[accumulatedMsgs.Count - 1];

            for (int i = 0; i < curGroup.Count; ++i)
            {
                int curItem = curGroup[i];

                if (curItem <= previousItem)
                    continue;

                if (curItem - firstItem > windowSize)
                    break;

                var newAccumulatedMsgs = new List<int>(accumulatedMsgs) { curItem };

                var newGroups = MergeRestrictions(groupList, windowSize, newAccumulatedMsgs, startGroup + 1);
                result.AddRange(newGroups);
            }

            return result;
        }

        public List<MsgGroupList> MergeQueries(List<List<MsgGroupList>> subqueryResults, int windowSize)
        {
            if (subqueryResults.Any(t => t.IsNullOrEmpty()))
                return new List<MsgGroupList>();

            var firstSubqueryResult = subqueryResults[0];
            var result = new List<MsgGroupList>();

            for (int i = 0; i < firstSubqueryResult.Count; ++i)
            {
                var accumulatedItems = new List<MsgGroupList>() { firstSubqueryResult[i] };
                var mergedSubqueryResults = MergeQueries(subqueryResults, windowSize, accumulatedItems, 1);

                result.AddRange(mergedSubqueryResults);
            }

            return CombineGroupLists(result, subqueryResults.Count);
        }

        public List<MsgGroupList> MergeQueries(List<List<MsgGroupList>> subqueryResults, int windowSize, IReadOnlyList<MsgGroupList> accumulatedItems, int start)
        {
            if (start >= subqueryResults.Count)
                return new List<MsgGroupList>(accumulatedItems);

            var currentSubqueryResult = subqueryResults[start];
            var result = new List<MsgGroupList>();

            MsgGroupList prevItem = accumulatedItems.Last();

            int prevItemLast = prevItem.Last().Last();
            int firstItemFirst = accumulatedItems.First().First().First();

            for (int i = 0; i < currentSubqueryResult.Count; ++i)
            {
                MsgGroupList curItem = currentSubqueryResult[i];

                int curItemFirst = curItem.First().First();
                int curItemLast = curItem.Last().Last();

                if (curItemFirst <= prevItemLast || accumulatedItems.Contains(curItem))
                    continue;

                if (curItemLast - firstItemFirst > windowSize)
                    break;

                var newAccumulatedItems = new List<MsgGroupList>(accumulatedItems) { curItem };
                var mergedSubqueryResults = MergeQueries(subqueryResults, windowSize, newAccumulatedItems, start + 1);

                result.AddRange(mergedSubqueryResults);
            }

            return result;
        }

        public List<MsgGroupList> CombineGroupLists(List<MsgGroupList> groupLists, int combineSectionLength)
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
