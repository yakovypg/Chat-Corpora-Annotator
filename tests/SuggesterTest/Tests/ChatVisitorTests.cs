using ChatCorporaAnnotator.Data.Parsers.Suggester;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SuggesterTest.Tests.Base;
using System.Collections.Generic;
using System.Linq;

namespace SuggesterTest.Tests
{
    using MsgGroupList = List<List<int>>;

    [TestClass]
    public class ChatVisitorTests : TestBase
    {
        private void CheckCondition(string query, IEnumerable<int> expectedResult)
        {
            var tree = QueryParser.GetTree(query);
            var condition = tree.body().restrictions().restriction(0).condition();
            var visitor = new ChatVisitor();

            var visitResult = (HashSet<int>)visitor.VisitCondition(condition);

            Assert.IsNotNull(visitResult);

            var actual = visitResult.ToList();
            var expected = expectedResult.ToList();

            actual.Sort();
            expected.Sort();

            bool isCorrect = actual.SequenceEqual(expected);

            Assert.IsTrue(isCorrect);
        }

        private void CheckRestriction(string query, IEnumerable<int> expectedResult)
        {
            var tree = QueryParser.GetTree(query);
            var restriction = tree.body().restrictions().restriction(0);
            var visitor = new ChatVisitor();

            var visitResult = (IEnumerable<int>)visitor.VisitRestriction(restriction);

            Assert.IsNotNull(visitResult);

            var actual = visitResult.ToList();
            var expected = expectedResult.ToList();

            actual.Sort();
            expected.Sort();

            bool isCorrect = actual.SequenceEqual(expected);

            Assert.IsTrue(isCorrect);
        }

        private void CheckRestrictions(string query, List<MsgGroupList> expectedResult)
        {
            var tree = QueryParser.GetTree(query);
            var restrictions = tree.body().restrictions();
            var visitor = new ChatVisitor();

            var actualResult = (List<MsgGroupList>)visitor.VisitRestrictions(restrictions);

            Assert.IsNotNull(actualResult);
            Assert.AreEqual(expectedResult.Count, actualResult.Count);

            for (int i = 0; i < actualResult.Count; ++i)
            {
                Assert.AreEqual(expectedResult[i].Count, actualResult[i].Count);

                for (int j = 0; j < actualResult[i].Count; ++j)
                {
                    bool isCorrect = expectedResult[i][j].SequenceEqual(actualResult[i][j]);
                    Assert.IsTrue(isCorrect);
                }
            }
        }

        private void CheckRestrictionsWithUnr(string query, MsgGroupList expectedGroups)
        {
            var tree = QueryParser.GetTree(query);
            var restrictions = tree.body().restrictions();
            var visitor = new ChatVisitor();

            var actualResult = (List<MsgGroupList>)visitor.VisitRestrictions(restrictions);

            Assert.IsNotNull(actualResult);

            for (int i = 0; i < actualResult.Count; ++i)
            {
                Assert.AreEqual(expectedGroups.Count, actualResult[i].Count);

                bool isCorrect = false;

                for (int j = 0; j < actualResult[i].Count; ++j)
                {
                    if (expectedGroups.Any(t => t.SequenceEqual(actualResult[i][j])))
                    {
                        isCorrect = true;
                        break;
                    }
                }

                Assert.IsTrue(isCorrect);
            }
        }

        private void CheckMergedRestrictions(string query, int inwin, MsgGroupList expected)
        {
            var tree = QueryParser.GetTree(query);
            var restrictions = tree.body().restrictions();
            var visitor = new ChatVisitor();

            var visitResult = (List<MsgGroupList>)visitor.VisitRestrictions(restrictions);
            var mergeResult = visitor.MergeRestrictions(visitResult[0], inwin);

            Assert.IsNotNull(mergeResult);
            Assert.AreEqual(expected.Count, mergeResult.Count);

            for (int i = 0; i < expected.Count; ++i)
            {
                bool isCorrect = expected[i].SequenceEqual(mergeResult[i]);
                Assert.IsTrue(isCorrect);
            }
        }

        private void CheckMergedQueries(string query, int inwin, List<MsgGroupList> expected)
        {
            var tree = QueryParser.GetTree(query);
            var query_seq = tree.body().query_seq();
            var visitor = new ChatVisitor();

            var visitResult = (List<List<MsgGroupList>>)visitor.VisitQuery_seq(query_seq);
            List<MsgGroupList> mergeResult = visitor.MergeQueries(visitResult, inwin);

            Assert.IsNotNull(mergeResult);
            Assert.AreEqual(expected.Count, mergeResult.Count);

            for (int i = 0; i < expected.Count; ++i)
            {
                Assert.AreEqual(expected[i].Count, mergeResult[i].Count);

                for (int j = 0; j < expected[i].Count; ++j)
                {
                    bool isCorrect = expected[i][j].SequenceEqual(mergeResult[i][j]);
                    Assert.IsTrue(isCorrect);
                }
            }
        }

        [TestMethod]
        public void VisitRestrictionsTest_1()
        {
            string query = "select haswordofdict(fruit)";

            List<MsgGroupList> expectedResult = new List<MsgGroupList>()
            {
                new MsgGroupList()
                {
                    new List<int>() { 2, 8 }
                }
            };

            CheckRestrictions(query, expectedResult);
        }

        [TestMethod]
        public void VisitRestrictionsTest_2()
        {
            string query = "select haswordofdict(fruit), haswordofdict(vegetable)";

            List<MsgGroupList> expectedResult = new List<MsgGroupList>()
            {
                new MsgGroupList()
                {
                    new List<int>() { 2, 8 },
                    new List<int>() { 3, 5, 6, 7, 8, 9 }
                }
            };

            CheckRestrictions(query, expectedResult);
        }

        [TestMethod]
        public void VisitRestrictionsTest_3()
        {
            string query = "select haswordofdict(fruit), haswordofdict(vegetable), byuser(misha) unr";

            var expectedResult = new MsgGroupList()
            {
                new List<int>() { 2, 8 },
                new List<int>() { 3, 5, 6, 7, 8, 9 },
                new List<int>() { 0, 1, 2, 5 }
            };

            CheckRestrictionsWithUnr(query, expectedResult);
        }

        [TestMethod]
        public void VisitRestrictionTest_HasWordOfDict()
        {
            string query = "select haswordofdict(fruit)";
            var expectedResult = new List<int>() { 2, 8 };

            CheckRestriction(query, expectedResult);
        }

        [TestMethod]
        public void VisitRestrictionTest_ByUser()
        {
            string query = "select byuser(misha)";
            var expectedResult = new List<int>() { 0, 1, 2, 5 };

            CheckRestriction(query, expectedResult);
        }

        [TestMethod]
        public void VisitRestrictionTest_Or()
        {
            string query = "select haswordofdict(vegetable) or byuser(roma)";

            var vegetableList = new List<int> { 3, 5, 6, 7, 8, 9 };
            var romaList = new List<int>() { 4, 6, 9 };

            var expectedResult = vegetableList.Concat(romaList).ToList();

            CheckRestriction(query, expectedResult);
        }

        [TestMethod]
        public void VisitRestrictionTest_And()
        {
            string query = "select haswordofdict(vegetable) and byuser(roma)";

            var vegetableList = new List<int> { 3, 5, 6, 7, 8, 9 };
            var romaList = new List<int>() { 4, 6, 9 };

            var expectedResult = vegetableList.Intersect(romaList).ToList();

            CheckRestriction(query, expectedResult);
        }

        [TestMethod]
        public void VisitRestrictionTest_Empty_1()
        {
            string query = "select haswordofdict(job)";
            var expectedResult = new List<int>();

            CheckRestriction(query, expectedResult);
        }

        [TestMethod]
        public void VisitRestrictionTest_Empty_2()
        {
            string query = "select haswordofdict(shop) and byuser(ivan)";
            var expectedResult = new List<int>();

            CheckRestriction(query, expectedResult);
        }

        [TestMethod]
        public void VisitRestrictionTest_Not()
        {
            string query = "select not haswordofdict(vegetable)";

            var vegetableList = new List<int> { 3, 5, 6, 7, 8, 9 };
            var allMessages = Enumerable.Range(0, 12);

            var expectedResult = allMessages.Except(vegetableList);

            CheckRestriction(query, expectedResult);
        }

        [TestMethod]
        public void VisitConditionTest_HasWordOfDict_1()
        {
            string query = "select haswordofdict(fruit)";
            var expectedResult = new HashSet<int>() { 2, 8 };

            CheckCondition(query, expectedResult);
        }

        [TestMethod]
        public void VisitConditionTest_HasWordOfDict_2()
        {
            string query = "select haswordofdict(vegetable)";
            var expectedResult = new HashSet<int>() { 3, 5, 6, 7, 8, 9 };

            CheckCondition(query, expectedResult);
        }

        [TestMethod]
        public void VisitConditionTest_ByUser_1()
        {
            string query = "select byuser(misha)";
            var expectedResult = new HashSet<int>() { 0, 1, 2, 5 };

            CheckCondition(query, expectedResult);
        }

        [TestMethod]
        public void VisitConditionTest_ByUser_2()
        {
            string query = "select byuser(roma)";
            var expectedResult = new HashSet<int>() { 4, 6, 9 };

            CheckCondition(query, expectedResult);
        }

        [TestMethod]
        public void MergeRestrictionsTest_Count1()
        {
            string query = "select haswordofdict(vegetable)";

            var expectedResult = new MsgGroupList()
            {
                new List<int>() { 3 },
                new List<int>() { 5 },
                new List<int>() { 6 },
                new List<int>() { 7 },
                new List<int>() { 8 },
                new List<int>() { 9 }
            };

            CheckMergedRestrictions(query, 1, expectedResult);
        }

        [TestMethod]
        public void MergeRestrictionsTest_Count2_1()
        {
            string query = "select haswordofdict(shop), haswordofdict(vegetable) inwin 10";

            var expectedResult = new MsgGroupList()
            {
                new List<int>() { 4, 5 },
                new List<int>() { 4, 6 },
                new List<int>() { 4, 7 },
                new List<int>() { 4, 8 },
                new List<int>() { 4, 9 }
            };

            CheckMergedRestrictions(query, 10, expectedResult);
        }

        [TestMethod]
        public void MergeRestrictionsTest_Count2_2()
        {
            string query = "select haswordofdict(shop), haswordofdict(vegetable) inwin 2";

            var expectedResult = new MsgGroupList()
            {
                new List<int>() { 4, 5 },
                new List<int>() { 4, 6 }
            };

            CheckMergedRestrictions(query, 2, expectedResult);
        }

        [TestMethod]
        public void MergeRestrictionsTest_Count3_1()
        {
            string query = "select byuser(misha) and not haswordofdict(vegetable), haswordofdict(fruit) or haswordofdict(shop), haswordofdict(vegetable) inwin 10";

            var expectedResult = new MsgGroupList()
            {
                new List<int>() { 0, 2, 3 },
                new List<int>() { 0, 2, 5 },
                new List<int>() { 0, 2, 6 },
                new List<int>() { 0, 2, 7 },
                new List<int>() { 0, 2, 8 },
                new List<int>() { 0, 2, 9 },

                new List<int>() { 0, 4, 5 },
                new List<int>() { 0, 4, 6 },
                new List<int>() { 0, 4, 7 },
                new List<int>() { 0, 4, 8 },
                new List<int>() { 0, 4, 9 },

                new List<int>() { 0, 8, 9 },

                new List<int>() { 1, 2, 3 },
                new List<int>() { 1, 2, 5 },
                new List<int>() { 1, 2, 6 },
                new List<int>() { 1, 2, 7 },
                new List<int>() { 1, 2, 8 },
                new List<int>() { 1, 2, 9 },

                new List<int>() { 1, 4, 5 },
                new List<int>() { 1, 4, 6 },
                new List<int>() { 1, 4, 7 },
                new List<int>() { 1, 4, 8 },
                new List<int>() { 1, 4, 9 },

                new List<int>() { 1, 8, 9 },

                new List<int>() { 2, 4, 5 },
                new List<int>() { 2, 4, 6 },
                new List<int>() { 2, 4, 7 },
                new List<int>() { 2, 4, 8 },
                new List<int>() { 2, 4, 9 },

                new List<int>() { 2, 8, 9 }
            };

            CheckMergedRestrictions(query, 10, expectedResult);
        }

        [TestMethod]
        public void MergeRestrictionsTest_Count3_2()
        {
            string query = "select byuser(misha) and not haswordofdict(vegetable), haswordofdict(fruit) or haswordofdict(shop), haswordofdict(vegetable) inwin 3";

            var expectedResult = new MsgGroupList()
            {
                new List<int>() { 0, 2, 3 },
                new List<int>() { 0, 2, 5 },

                new List<int>() { 1, 2, 3 },
                new List<int>() { 1, 2, 5 },

                new List<int>() { 1, 4, 5 },
                new List<int>() { 1, 4, 6 },
                new List<int>() { 1, 4, 7 },

                new List<int>() { 2, 4, 5 },
                new List<int>() { 2, 4, 6 },
                new List<int>() { 2, 4, 7 }
            };

            CheckMergedRestrictions(query, 3, expectedResult);
        }

        [TestMethod]
        public void MergeQueriesTest_Count1()
        {
            string query = "select (select haswordofdict(fruit))";

            var expectedResult = new List<MsgGroupList>()
            {
                new MsgGroupList()
                {
                    new List<int>() { 2 }
                },

                new MsgGroupList()
                {
                    new List<int>() { 8 }
                }
            };

            CheckMergedQueries(query, 3, expectedResult);
        }

        [TestMethod]
        public void MergeQueriesTest_Count2_1()
        {
            string query = "select (select haswordofdict(shop)); (select haswordofdict(vegetable)) inwin 3";

            var expectedResult = new List<MsgGroupList>()
            {
                new MsgGroupList()
                {
                    new List<int>() { 4 },
                    new List<int>() { 5 }
                },

                new MsgGroupList()
                {
                    new List<int>() { 4 },
                    new List<int>() { 6 }
                },

                new MsgGroupList()
                {
                    new List<int>() { 4 },
                    new List<int>() { 7 }
                }
            };

            CheckMergedQueries(query, 3, expectedResult);
        }

        [TestMethod]
        public void MergeQueriesTest_Count2_2()
        {
            string query = "select (select haswordofdict(fruit) or haswordofdict(shop), haswordofdict(vegetable) inwin 2); (select haswordofdict(vegetable)) inwin 10";

            var expectedResult = new List<MsgGroupList>()
            {
                new MsgGroupList()
                {
                    new List<int>() { 2, 3 },
                    new List<int>() { 5 }
                },

                new MsgGroupList()
                {
                    new List<int>() { 2, 3 },
                    new List<int>() { 6 }
                },

                new MsgGroupList()
                {
                    new List<int>() { 2, 3 },
                    new List<int>() { 7 }
                },

                new MsgGroupList()
                {
                    new List<int>() { 2, 3 },
                    new List<int>() { 8 }
                },

                new MsgGroupList()
                {
                    new List<int>() { 2, 3 },
                    new List<int>() { 9 }
                },

                new MsgGroupList()
                {
                    new List<int>() { 4, 5 },
                    new List<int>() { 6 }
                },

                new MsgGroupList()
                {
                    new List<int>() { 4, 5 },
                    new List<int>() { 7 }
                },

                new MsgGroupList()
                {
                    new List<int>() { 4, 5 },
                    new List<int>() { 8 }
                },

                new MsgGroupList()
                {
                    new List<int>() { 4, 5 },
                    new List<int>() { 9 }
                },

                new MsgGroupList()
                {
                    new List<int>() { 4, 6 },
                    new List<int>() { 7 }
                },

                new MsgGroupList()
                {
                    new List<int>() { 4, 6 },
                    new List<int>() { 8 }
                },

                new MsgGroupList()
                {
                    new List<int>() { 4, 6 },
                    new List<int>() { 9 }
                },
            };

            CheckMergedQueries(query, 10, expectedResult);
        }

        [TestMethod]
        public void MergeQueriesTest_Count3_1()
        {
            string query = "select (select byuser(misha)); (select not haswordofdict(vegetable), haswordofdict(shop) inwin 2); (select haswordofdict(vegetable), haswordofdict(vegetable), haswordofdict(vegetable), haswordofdict(vegetable) inwin 1) inwin 7";

            var expectedResult = new List<MsgGroupList>()
            {
                new MsgGroupList()
                {
                    new List<int>() { 1 },
                    new List<int>() { 2, 4 },
                    new List<int>() { 5, 6, 7, 8 }
                },
            };

            CheckMergedQueries(query, 7, expectedResult);
        }

        [TestMethod]
        public void MergeQueriesTest_Count3_2()
        {
            string query = "select (select byuser(misha)); (select not haswordofdict(vegetable), haswordofdict(shop) inwin 2); (select haswordofdict(vegetable), haswordofdict(vegetable), haswordofdict(vegetable), haswordofdict(vegetable) inwin 1) inwin 8";

            var expectedResult = new List<MsgGroupList>()
            {
                new MsgGroupList()
                {
                    new List<int>() { 0 },
                    new List<int>() { 2, 4 },
                    new List<int>() { 5, 6, 7, 8 }
                },

                new MsgGroupList()
                {
                    new List<int>() { 1 },
                    new List<int>() { 2, 4 },
                    new List<int>() { 5, 6, 7, 8 }
                },

                new MsgGroupList()
                {
                    new List<int>() { 1 },
                    new List<int>() { 2, 4 },
                    new List<int>() { 6, 7, 8, 9 }
                },
            };

            CheckMergedQueries(query, 8, expectedResult);
        }

        [TestMethod]
        public void MergeQueriesTest_Empty()
        {
            string query = "select (select haswordofdict(fruit)); (select haswordofdict(os) and byuser(roma)); (select haswordofdict(vegetable)) inwin 10";

            var expectedResult = new List<MsgGroupList>();

            CheckMergedQueries(query, 10, expectedResult);
        }
    }
}
