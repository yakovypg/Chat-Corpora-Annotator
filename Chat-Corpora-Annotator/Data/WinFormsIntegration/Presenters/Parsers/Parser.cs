using Antlr4.Runtime;
using System.Collections.Generic;
using System.Text;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Presenters.Parsers
{
    class Parser
    {
        public static List<List<List<int>>> parse(string query)
        {
            StringBuilder text = new StringBuilder(query);

            AntlrInputStream inputStream = new AntlrInputStream(text.ToString());
            ChatLexer speakLexer = new ChatLexer(inputStream);
            CommonTokenStream commonTokenStream = new CommonTokenStream(speakLexer);
            ChatParser speakParser = new ChatParser(commonTokenStream);

            var tree = speakParser.query();

            var visitor = new MyChatVisitor();

            var result = (List<List<List<int>>>)visitor.Visit(tree);

            return result;
        }
    }
}
