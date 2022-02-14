using ChatCorporaAnnotator.Data.Parsers.Suggester;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SuggesterTests.Tests.Base;
using System;
using System.Collections.Generic;

namespace SuggesterTests.Tests
{
    [TestClass]
    public class QueryParserTests : TestBase
    {
        private void CheckResult(List<List<List<int>>> result, int resultCount, int groupsCount, int[] messagesInGroups)
        {
            if (groupsCount > 0 && messagesInGroups.Length != groupsCount)
                throw new ArgumentException("groupsCount != messagesInGroups.Length");

            Assert.AreEqual(resultCount, result.Count);

            for (int i = 0; i < result.Count; ++i)
            {
                Assert.AreEqual(groupsCount, result[i].Count);

                for (int j = 0; j < result[i].Count; ++j)
                    Assert.AreEqual(messagesInGroups[j], result[i][j].Count);
            }
        }

        [TestMethod]
        public void ParseTest_IncorrectQuery()
        {
            string query = "incorrect query";
            var result = QueryParser.Parse(query);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ParseTest_NotFound()
        {
            string query = "select haswordofdict(shop) and haswordofdict(vegetable)";
            var result = QueryParser.Parse(query);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void ParseTest_HasWordOfDict_1()
        {
            string query = "select haswordofdict(fruit)";
            var result = QueryParser.Parse(query);

            Assert.IsNotNull(result);

            CheckResult(result, 2, 1, new int[] { 1 });

            Assert.AreEqual(2, result[0][0][0]);
            Assert.AreEqual(8, result[1][0][0]);
        }

        [TestMethod]
        public void ParseTest_HasWordOfDict_2()
        {
            string query = "select haswordofdict(vegetable)";
            var result = QueryParser.Parse(query);

            Assert.IsNotNull(result);

            CheckResult(result, 6, 1, new int[] { 1 });

            Assert.AreEqual(3, result[0][0][0]);
            Assert.AreEqual(5, result[1][0][0]);
            Assert.AreEqual(6, result[2][0][0]);
            Assert.AreEqual(7, result[3][0][0]);
            Assert.AreEqual(8, result[4][0][0]);
            Assert.AreEqual(9, result[5][0][0]);
        }

        [TestMethod]
        public void ParseTest_Or_1()
        {
            string query = "select haswordofdict(shop) or haswordofdict(os)";
            var result = QueryParser.Parse(query);

            Assert.IsNotNull(result);

            CheckResult(result, 2, 1, new int[] { 1 });

            Assert.AreEqual(2, result[0][0][0]);
            Assert.AreEqual(4, result[1][0][0]);
        }

        [TestMethod]
        public void ParseTest_And_1()
        {
            string query = "select haswordofdict(fruit) and haswordofdict(vegetable)";
            var result = QueryParser.Parse(query);

            Assert.IsNotNull(result);

            CheckResult(result, 1, 1, new int[] { 1 });

            Assert.AreEqual(8, result[0][0][0]);
        }

        [TestMethod]
        public void ParseTest_And_Not_1()
        {
            string query = "select (byuser(misha) and not haswordofdict(vegetable)) and not haswordofdict(os)";
            var result = QueryParser.Parse(query);

            Assert.IsNotNull(result);

            CheckResult(result, 2, 1, new int[] { 1 });

            Assert.AreEqual(0, result[0][0][0]);
            Assert.AreEqual(1, result[1][0][0]);
        }

        [TestMethod]
        public void ParseTest_Not_1()
        {
            string query = "select not haswordofdict(vegetable)";
            var result = QueryParser.Parse(query);

            Assert.IsNotNull(result);

            CheckResult(result, 6, 1, new int[] { 1 });

            Assert.AreEqual(0, result[0][0][0]);
            Assert.AreEqual(1, result[1][0][0]);
            Assert.AreEqual(2, result[2][0][0]);
            Assert.AreEqual(4, result[3][0][0]);
            Assert.AreEqual(10, result[4][0][0]);
            Assert.AreEqual(11, result[5][0][0]);
        }

        [TestMethod]
        public void ParseTest_Comma_1()
        {
            string query = "select haswordofdict(shop), haswordofdict(vegetable)";
            var result = QueryParser.Parse(query);

            Assert.IsNotNull(result);

            CheckResult(result, 5, 1, new int[] { 2 });

            for (int i = 0; i < 5; ++i)
                Assert.AreEqual(4, result[i][0][0]);

            Assert.AreEqual(5, result[0][0][1]);
            Assert.AreEqual(6, result[1][0][1]);
            Assert.AreEqual(7, result[2][0][1]);
            Assert.AreEqual(8, result[3][0][1]);
            Assert.AreEqual(9, result[4][0][1]);
        }

        [TestMethod]
        public void ParseTest_Comma_2()
        {
            string query = "select haswordofdict(vegetable), haswordofdict(shop)";
            var result = QueryParser.Parse(query);

            Assert.IsNotNull(result);

            CheckResult(result, 1, 1, new int[] { 2 });

            Assert.AreEqual(3, result[0][0][0]);
            Assert.AreEqual(4, result[0][0][1]);
        }

        [TestMethod]
        public void ParseTest_Comma_3()
        {
            string query = "select haswordofdict(shop) or byuser(ivan), haswordofdict(vegetable)";
            var result = QueryParser.Parse(query);

            Assert.IsNotNull(result);

            CheckResult(result, 13, 1, new int[] { 2 });

            Assert.AreEqual(3, result[0][0][0]);
            Assert.AreEqual(5, result[0][0][1]);

            Assert.AreEqual(3, result[1][0][0]);
            Assert.AreEqual(6, result[1][0][1]);

            Assert.AreEqual(3, result[2][0][0]);
            Assert.AreEqual(7, result[2][0][1]);

            Assert.AreEqual(3, result[3][0][0]);
            Assert.AreEqual(8, result[3][0][1]);

            Assert.AreEqual(3, result[4][0][0]);
            Assert.AreEqual(9, result[4][0][1]);

            Assert.AreEqual(4, result[5][0][0]);
            Assert.AreEqual(5, result[5][0][1]);

            Assert.AreEqual(4, result[6][0][0]);
            Assert.AreEqual(6, result[6][0][1]);

            Assert.AreEqual(4, result[7][0][0]);
            Assert.AreEqual(7, result[7][0][1]);

            Assert.AreEqual(4, result[8][0][0]);
            Assert.AreEqual(8, result[8][0][1]);

            Assert.AreEqual(4, result[9][0][0]);
            Assert.AreEqual(9, result[9][0][1]);

            Assert.AreEqual(7, result[10][0][0]);
            Assert.AreEqual(8, result[10][0][1]);

            Assert.AreEqual(7, result[11][0][0]);
            Assert.AreEqual(9, result[11][0][1]);

            Assert.AreEqual(8, result[12][0][0]);
            Assert.AreEqual(9, result[12][0][1]);
        }

        [TestMethod]
        public void ParseTest_Comma_4()
        {
            string query = "select byuser(misha), haswordofdict(shop) or haswordofdict(fruit), haswordofdict(shop) or haswordofdict(fruit)";
            var result = QueryParser.Parse(query);

            Assert.IsNotNull(result);

            CheckResult(result, 7, 1, new int[] { 3 });

            Assert.AreEqual(0, result[0][0][0]);
            Assert.AreEqual(2, result[0][0][1]);
            Assert.AreEqual(4, result[0][0][2]);

            Assert.AreEqual(0, result[1][0][0]);
            Assert.AreEqual(2, result[1][0][1]);
            Assert.AreEqual(8, result[1][0][2]);

            Assert.AreEqual(0, result[2][0][0]);
            Assert.AreEqual(4, result[2][0][1]);
            Assert.AreEqual(8, result[2][0][2]);

            Assert.AreEqual(1, result[3][0][0]);
            Assert.AreEqual(2, result[3][0][1]);
            Assert.AreEqual(4, result[3][0][2]);

            Assert.AreEqual(1, result[4][0][0]);
            Assert.AreEqual(2, result[4][0][1]);
            Assert.AreEqual(8, result[4][0][2]);

            Assert.AreEqual(1, result[5][0][0]);
            Assert.AreEqual(4, result[5][0][1]);
            Assert.AreEqual(8, result[5][0][2]);

            Assert.AreEqual(2, result[6][0][0]);
            Assert.AreEqual(4, result[6][0][1]);
            Assert.AreEqual(8, result[6][0][2]);
        }

        [TestMethod]
        public void ParseTest_Unr_1()
        {
            string query = "select haswordofdict(shop), haswordofdict(vegetable) unr";
            var result = QueryParser.Parse(query);

            Assert.IsNotNull(result);

            CheckResult(result, 6, 1, new int[] { 2 });

            Assert.AreEqual(3, result[0][0][0]);
            Assert.AreEqual(4, result[0][0][1]);

            Assert.AreEqual(4, result[1][0][0]);
            Assert.AreEqual(5, result[1][0][1]);

            Assert.AreEqual(4, result[2][0][0]);
            Assert.AreEqual(6, result[2][0][1]);

            Assert.AreEqual(4, result[3][0][0]);
            Assert.AreEqual(7, result[3][0][1]);

            Assert.AreEqual(4, result[4][0][0]);
            Assert.AreEqual(8, result[4][0][1]);

            Assert.AreEqual(4, result[5][0][0]);
            Assert.AreEqual(9, result[5][0][1]);
        }

        [TestMethod]
        public void ParseTest_Unr_2()
        {
            string query = "select haswordofdict(shop), haswordofdict(fruit), haswordofdict(os) unr";
            var result = QueryParser.Parse(query);

            Assert.IsNotNull(result);

            CheckResult(result, 1, 1, new int[] { 3 });

            Assert.AreEqual(2, result[0][0][0]);
            Assert.AreEqual(4, result[0][0][1]);
            Assert.AreEqual(8, result[0][0][2]);
        }

        [TestMethod]
        public void ParseTest_Unr_3()
        {
            string query = "select haswordofdict(vegetable), haswordofdict(shop), haswordofdict(fruit) unr";
            var result = QueryParser.Parse(query);

            Assert.IsNotNull(result);

            CheckResult(result, 11, 1, new int[] { 3 });

            Assert.AreEqual(2, result[0][0][0]);
            Assert.AreEqual(3, result[0][0][1]);
            Assert.AreEqual(4, result[0][0][2]);

            Assert.AreEqual(2, result[1][0][0]);
            Assert.AreEqual(4, result[1][0][1]);
            Assert.AreEqual(5, result[1][0][2]);

            Assert.AreEqual(2, result[2][0][0]);
            Assert.AreEqual(4, result[2][0][1]);
            Assert.AreEqual(6, result[2][0][2]);

            Assert.AreEqual(2, result[3][0][0]);
            Assert.AreEqual(4, result[3][0][1]);
            Assert.AreEqual(7, result[3][0][2]);

            Assert.AreEqual(2, result[4][0][0]);
            Assert.AreEqual(4, result[4][0][1]);
            Assert.AreEqual(8, result[4][0][2]);

            Assert.AreEqual(2, result[5][0][0]);
            Assert.AreEqual(4, result[5][0][1]);
            Assert.AreEqual(9, result[5][0][2]);

            Assert.AreEqual(3, result[6][0][0]);
            Assert.AreEqual(4, result[6][0][1]);
            Assert.AreEqual(8, result[6][0][2]);

            Assert.AreEqual(4, result[7][0][0]);
            Assert.AreEqual(5, result[7][0][1]);
            Assert.AreEqual(8, result[7][0][2]);

            Assert.AreEqual(4, result[8][0][0]);
            Assert.AreEqual(6, result[8][0][1]);
            Assert.AreEqual(8, result[8][0][2]);

            Assert.AreEqual(4, result[9][0][0]);
            Assert.AreEqual(7, result[9][0][1]);
            Assert.AreEqual(8, result[9][0][2]);

            Assert.AreEqual(4, result[10][0][0]);
            Assert.AreEqual(8, result[10][0][1]);
            Assert.AreEqual(9, result[10][0][2]);
        }

        [TestMethod]
        public void ParseTest_Unr_Inwin_1()
        {
            string query = "select haswordofdict(shop), haswordofdict(vegetable) or haswordofdict(fruit) unr inwin 3";
            var result = QueryParser.Parse(query);

            Assert.IsNotNull(result);

            CheckResult(result, 5, 1, new int[] { 2 });

            Assert.AreEqual(2, result[0][0][0]);
            Assert.AreEqual(4, result[0][0][1]);

            Assert.AreEqual(3, result[1][0][0]);
            Assert.AreEqual(4, result[1][0][1]);

            Assert.AreEqual(4, result[2][0][0]);
            Assert.AreEqual(5, result[2][0][1]);


            Assert.AreEqual(4, result[3][0][0]);
            Assert.AreEqual(6, result[3][0][1]);

            Assert.AreEqual(4, result[4][0][0]);
            Assert.AreEqual(7, result[4][0][1]);
        }

        [TestMethod]
        public void ParseTest_Inwin_1()
        {
            string query = "select haswordofdict(shop), haswordofdict(vegetable) inwin 1";
            var result = QueryParser.Parse(query);

            Assert.IsNotNull(result);

            CheckResult(result, 1, 1, new int[] { 2 });

            Assert.AreEqual(4, result[0][0][0]);
            Assert.AreEqual(5, result[0][0][1]);
        }

        [TestMethod]
        public void ParseTest_Inwin_2()
        {
            string query = "select haswordofdict(shop), haswordofdict(vegetable) inwin 3";
            var result = QueryParser.Parse(query);

            Assert.IsNotNull(result);

            CheckResult(result, 3, 1, new int[] { 2 });

            Assert.AreEqual(4, result[0][0][0]);
            Assert.AreEqual(5, result[0][0][1]);

            Assert.AreEqual(4, result[1][0][0]);
            Assert.AreEqual(6, result[1][0][1]);

            Assert.AreEqual(4, result[2][0][0]);
            Assert.AreEqual(7, result[2][0][1]);
        }

        [TestMethod]
        public void ParseTest_Subquery_1()
        {
            string query = "select (select haswordofdict(shop), haswordofdict(vegetable) inwin 2); (select haswordofdict(vegetable), haswordofdict(vegetable) inwin 1) inwin 5";
            var result = QueryParser.Parse(query);

            Assert.IsNotNull(result);

            CheckResult(result, 5, 2, new int[] { 2, 2 });

            Assert.AreEqual(4, result[0][0][0]);
            Assert.AreEqual(5, result[0][0][1]);
            Assert.AreEqual(6, result[0][1][0]);
            Assert.AreEqual(7, result[0][1][1]);

            Assert.AreEqual(4, result[1][0][0]);
            Assert.AreEqual(5, result[1][0][1]);
            Assert.AreEqual(7, result[1][1][0]);
            Assert.AreEqual(8, result[1][1][1]);

            Assert.AreEqual(4, result[2][0][0]);
            Assert.AreEqual(5, result[2][0][1]);
            Assert.AreEqual(8, result[2][1][0]);
            Assert.AreEqual(9, result[2][1][1]);

            Assert.AreEqual(4, result[3][0][0]);
            Assert.AreEqual(6, result[3][0][1]);
            Assert.AreEqual(7, result[3][1][0]);
            Assert.AreEqual(8, result[3][1][1]);

            Assert.AreEqual(4, result[4][0][0]);
            Assert.AreEqual(6, result[4][0][1]);
            Assert.AreEqual(8, result[4][1][0]);
            Assert.AreEqual(9, result[4][1][1]);
        }

        [TestMethod]
        public void ParseTest_Subquery_2()
        {
            string query = "select (select haswordofdict(shop), haswordofdict(vegetable) unr inwin 2); (select haswordofdict(vegetable), haswordofdict(vegetable) inwin 1) inwin 3";
            var result = QueryParser.Parse(query);

            Assert.IsNotNull(result);

            CheckResult(result, 2, 2, new int[] { 2, 2 });

            Assert.AreEqual(3, result[0][0][0]);
            Assert.AreEqual(4, result[0][0][1]);
            Assert.AreEqual(5, result[0][1][0]);
            Assert.AreEqual(6, result[0][1][1]);

            Assert.AreEqual(4, result[1][0][0]);
            Assert.AreEqual(5, result[1][0][1]);
            Assert.AreEqual(6, result[1][1][0]);
            Assert.AreEqual(7, result[1][1][1]);
        }
    }
}
