using Antlr4.Runtime;
using System.Collections.Generic;
using System.Text;

namespace ChatCorporaAnnotator.Data.Parsers.Suggester
{
    internal static class QueryParser
    {
        public static List<List<List<int>>> Parse(string query, bool disorderlyRestrictions = false)
        {
            StringBuilder text = new StringBuilder(query);

            AntlrInputStream inputStream = new AntlrInputStream(text.ToString());
            ChatLexer speakLexer = new ChatLexer(inputStream);
            CommonTokenStream commonTokenStream = new CommonTokenStream(speakLexer);
            ChatParser speakParser = new ChatParser(commonTokenStream);

            var tree = speakParser.query();

            var visitor = new MyChatVisitor() { DisorderlyRestrictionsMode = disorderlyRestrictions };

            var result = (List<List<List<int>>>)visitor.Visit(tree);

            return result;
        }
    }
}
