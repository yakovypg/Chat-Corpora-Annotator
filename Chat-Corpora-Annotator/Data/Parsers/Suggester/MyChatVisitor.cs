using Antlr4.Runtime.Misc;
using IndexEngine.Indexes;
using IndexEngine.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ChatCorporaAnnotator.Data.Parsers.Suggester
{
    using MsgGroups = List<List<int>>;

    public class MyChatVisitor : ChatBaseVisitor<object>
    {
        public override object VisitQuery([NotNull] ChatParser.QueryContext context)
        {
            return VisitBody(context.body());
        }

        public override object VisitBody([NotNull] ChatParser.BodyContext context)
        {
            // Default window size is 70
            int windowSize = 70;

            if (context != null)
            {
                if (context.InWin() != null)
                {
                    try
                    {
                        windowSize = Int32.Parse(context.number().GetText());
                    }
                    catch (FormatException)
                    {
                        MessageBox.Show("Incorrect query");
                    }
                }




                if (context.restrictions() != null)
                {
                    // If query / subquery contains only restrictions -- we need only merge restrictions
                    // [x1...xn] -- result for restriction R1
                    // [y1...ym] -- result for restriction R2
                    // Select R1, R2 inwin W
                    // Result is vector of vectors [[xi, yj]] where 0 < yj - xi <= W
                    // So we have only one restriction group

                    var onlyRestrictions = (MsgGroups)VisitRestrictions(context.restrictions());
                    var mergedRestrcitions = MergeRestrictions(onlyRestrictions, windowSize);

                    return OnlyRestrictionsToList(mergedRestrcitions);
                }
                else if (context.query_seq() != null)
                {
                    // Now in this query subqueries only.
                    // Also we have invariant: results of son's query are calculated correctlly.
                    // It means that now we have only son's correct accomadation.

                    var subQueryResults = (List<List<MsgGroups>>)VisitQuery_seq(context.query_seq());

                    return MergeQueries(subQueryResults, windowSize);
                }
            }
            else
            {

                return null;
            }
            return null;
        }

        public override object VisitQuery_seq([NotNull] ChatParser.Query_seqContext context)
        {
            var qList = new List<List<MsgGroups>>();

            foreach (var q in context.query())
            {
                // VisitQuery returns all correct accomodation. 
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
                qList.Add((List<MsgGroups>)VisitQuery(q));
            }

            return qList;
        }

        public override object VisitRestrictions([NotNull] ChatParser.RestrictionsContext context)
        {
            var rList = new MsgGroups();

            foreach (var r in context.restriction())
            {
                var newList = (List<int>)VisitRestriction(r);
                newList.Sort();
                rList.Add(newList);

            }

            return rList;
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
                lhs.AddRange(rhs);
                lhs.Sort();

                return lhs;
            }
            else if (context.Not() != null)
            {
                long tmp = LuceneService.DirReader.MaxDoc - 1;
                int msgCount = 1000000;
                if (msgCount > tmp)
                {
                    msgCount = (int)tmp;
                }

                var numberList = Enumerable.Range(1, msgCount).ToList();
                var excludeList = (List<int>)VisitRestriction(context.restriction(0));

                return numberList.Except(excludeList).ToList();
            }
            else if (context.condition() != null)
            {
                return VisitCondition(context.condition());
            }
            else
            {
                return VisitRestriction(context.restriction(0));
            }
        }

        public override object VisitCondition([NotNull] ChatParser.ConditionContext context)
        {
            if (context.HasUserMentioned() != null)
            {
                string username = context.huser().GetText();
                if (username != "<missing STRING>")
                {
                    return Retrievers.HasUserMentioned(username);
                }
                else
                {
                    return new List<int>();
                }
            }
            else if (context.ByUser() != null)
            {
                string username = context.huser().GetText();
                if (username != "<missing STRING>")
                {
                    return Retrievers.HasUser(username);
                }
                else
                {
                    return new List<int>();
                }
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
                List<string> list = new List<string>();
                if (dictname != "<missing STRING>")
                {
                    if (UserDictsIndex.GetInstance().IndexCollection.TryGetValue(dictname, out list))
                    {
                        return Retrievers.HasWordOfList(list);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }



            }
            else if (context.HasDate() != null)
            {
                return Retrievers.HasNERTag(NER.DATE);
            }
            else
            {
                // Message about incorrect query
                return null;
            }

        }


        private List<MsgGroups> OnlyRestrictionsToList(MsgGroups rList)
        {
            var result = new List<MsgGroups>();

            foreach (var r in rList)
            {
                result.Add(new MsgGroups { r });
            }

            return result;
        }

        private MsgGroups MergeRestrictions(MsgGroups rList, int windowSize)
        {
            int _size = rList.Count;
            var result = new MsgGroups();

            if (_size == 1)
            {
                foreach (var r in rList[0])
                {
                    result.Add(new List<int> { r });
                }

                return result;
            }


            for (int fstInd = 0; fstInd < rList[0].Count; fstInd++)
            {
                int curPos = rList[0][fstInd];
                int fstPos = rList[0][fstInd];
                List<int> curMsgs = new List<int> { curPos };
                for (int i = 1; i < _size; i++)
                {
                    foreach (var _id in rList[i])
                    {
                        if (_id <= curPos)
                        {
                            continue;
                        }

                        if (_id - fstPos <= windowSize)
                        {
                            curMsgs.Add(_id);
                            curPos = _id;
                            break;
                        }
                    }
                }

                if (curMsgs.Count == _size)
                {
                    result.Add(curMsgs);
                }
            }

            return result;
        }

        private List<MsgGroups> MergeQueries(List<List<MsgGroups>> sqResults, int windowSize)
        {
            var result = new List<MsgGroups>();
            int _size = sqResults.Count();
            List<int> curIndex = new List<int>();

            for (int i = 0; i < _size; i++)
            {
                curIndex.Add(0);
            }

            while (true)
            {
                bool can_end = true;

                var curAccomadtion = new List<MsgGroups>();

                for (int i = 0; i < _size; i++)
                {
                    var elem = sqResults[i][curIndex[i]];
                    curAccomadtion.Add(elem);
                }

                if (isCorrectAccomodation(curAccomadtion, windowSize))
                {
                    var current = new MsgGroups();
                    foreach (var group in curAccomadtion)
                    {
                        current.AddRange(group);
                    }

                    result.Add(current);
                }

                for (int i = _size - 1; i >= 0; i--)
                {
                    if (curIndex[i] < sqResults[i].Count - 1)
                    {
                        can_end = false;
                        curIndex[i]++;
                        for (int j = i + 1; j < _size; j++)
                        {
                            curIndex[j] = 0;
                        }

                        break;
                    }
                }


                if (can_end)
                {
                    break;
                }
            }


            return result;
        }

        private bool isCorrectAccomodation(List<MsgGroups> acc, int windowSize)
        {
            int _size = acc.Count();

            for (int i = 1; i < _size; i++)
            {
                int prevLast = acc[i - 1].Last().Last();
                int curFirst = acc[i].First().First();

                if (prevLast >= curFirst)
                {
                    return false;
                }
            }

            int fstFirst = acc.First().First().First();
            int lastLast = acc.Last().Last().Last();

            bool correctWindow = ((lastLast - fstFirst) <= windowSize);

            return correctWindow;
        }
    }
}
