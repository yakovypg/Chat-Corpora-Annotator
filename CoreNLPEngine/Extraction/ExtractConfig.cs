using IndexEngine.Data.Paths;
using Newtonsoft.Json.Linq;

namespace CoreNLPEngine.Extraction
{
    public class ExtractConfig
    {
        public string CoreNLPPath { get; set; }
        public string[] Annotators { get; set; }

        public int CoreNLPClientMemory { get; set; }
        public int CoreNLPClientTimeout { get; set; }

        public JObject? CoreNLPClientProperties { get; set; }

        public static ExtractConfig Default
        {
            get
            {
                string coreNLPPath = Environment.GetEnvironmentVariable("CORENLP_HOME") ?? string.Empty;

                string[] annotators = new string[]
                {
                    "tokenize", "ssplit", "pos", "lemma", "ner", "parse", "depparse", "coref"
                };

                return new ExtractConfig(coreNLPPath, annotators);
            }
        }

        public ExtractConfig(string coreNLPPath, int coreNLPClientMemory = 8, int coreNLPClientTimeout = 30000)
            : this(coreNLPPath, Array.Empty<string>(), coreNLPClientMemory, coreNLPClientTimeout)
        {
        }

        public ExtractConfig(string coreNLPPath, string[] annotators, int coreNLPClientMemory = 8, int coreNLPClientTimeout = 30000)
        {
            CoreNLPPath = coreNLPPath;
            Annotators = annotators;

            CoreNLPClientMemory = coreNLPClientMemory;
            CoreNLPClientTimeout = coreNLPClientTimeout;
        }

        public static JObject GetCoreNLPClientDefaultProperties()
        {
            var props = new JObject
            {
                { "annotators", "tokenize,ssplit,pos,lemma,ner,parse" },

                { "pos.model", ToolInfo.POSpath },
                { "ner.model", ToolInfo.NERpath },
                { "parse.model", ToolInfo.SRparserpath },

                { "ner.useSUTime", "true" },
                { "ner.applyFineGrained", "false" },

                { "sutime.binders", "0" },

                { "sutime.rules", ToolInfo.sutimeRules },
                { "sutime.markTimeRanges", "true" },
                { "sutime.includeNested", "true" }
            };

            return props;
        }
    }
}
