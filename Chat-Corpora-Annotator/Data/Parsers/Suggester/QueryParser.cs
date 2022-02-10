using Antlr4.Runtime;
using System.Collections.Generic;
using System.Text;

namespace ChatCorporaAnnotator.Data.Parsers.Suggester
{
    using MsgGroupList = List<List<int>>;

    public static class QueryParser
    {
        public static List<MsgGroupList> Parse(string query, bool unorderedRestrictionsMode = false)
        {
            var tree = GetTree(query);
            var visitor = new ChatVisitor(unorderedRestrictionsMode);
            var result = (List<MsgGroupList>)visitor.Visit(tree);

            return result;
        }

        public static ChatParser.QueryContext GetTree(string query)
        {
            var text = new StringBuilder(query);

            var inputStream = new AntlrInputStream(text.ToString());
            var speakLexer = new ChatLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(speakLexer);
            var speakParser = new ChatParser(commonTokenStream);

            var tree = speakParser.query();
            return tree;
        }
    }
}
