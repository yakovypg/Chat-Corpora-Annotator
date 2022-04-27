using Antlr4.Runtime;
using ChatCorporaAnnotator.Data.Parsers.Suggester.Histograms;
using IndexEngine.Data.Paths;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ChatCorporaAnnotator.Data.Parsers.Suggester
{
    using MsgGroupList = List<List<int>>;

    public static class QueryParser
    {
        public static List<MsgGroupList> Parse(string query)
        {
            TryLoadHistograms(out HashSet<MsgGroupHistogram> histograms);

            var tree = GetTree(query);
            var visitor = new QueryContextVisitor() { Histograms = histograms };
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

        public static bool TrySaveHistograms(HashSet<MsgGroupHistogram> histograms)
        {
            try
            {
                string json = JsonConvert.SerializeObject(histograms);
                File.WriteAllText(ToolInfo.HistogramsPath, json);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryLoadHistograms(out HashSet<MsgGroupHistogram> histograms)
        {
            try
            {
                string json = File.ReadAllText(ToolInfo.HistogramsPath);

                histograms = JsonConvert.DeserializeObject<HashSet<MsgGroupHistogram>>(json)
                    ?? new HashSet<MsgGroupHistogram>();

                return true;
            }
            catch
            {
                histograms = new HashSet<MsgGroupHistogram>();
                return false;
            }
        }
    }
}
