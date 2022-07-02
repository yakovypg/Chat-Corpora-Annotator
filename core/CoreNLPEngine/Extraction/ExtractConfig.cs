using IndexEngine.Data.Paths;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoreNLPEngine.Extraction
{
    public class ExtractConfig
    {
        public static readonly string[] DefaultAnnotators = new string[]
        {
            "tokenize", "ssplit", "pos", "lemma", "ner", "parse"
        };

        public static readonly string[] FullAnnotators = new string[]
        {
            "tokenize", "ssplit", "pos", "lemma", "ner", "parse", "depparse", "coref"
        };

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
                return new ExtractConfig(coreNLPPath, DefaultAnnotators);
            }
        }

        // This constructor is for serialization
        public ExtractConfig() : this(string.Empty)
        {
        }

        public ExtractConfig(string coreNLPPath, string[]? annotators = null, int coreNLPClientMemory = 8, int coreNLPClientTimeout = 30000)
        {
            CoreNLPPath = coreNLPPath;
            Annotators = annotators ?? Array.Empty<string>();

            CoreNLPClientMemory = coreNLPClientMemory;
            CoreNLPClientTimeout = coreNLPClientTimeout;
        }

        public string[] LoadStopwordsFromDisk()
        {
            try
            {
                string path = Path.Combine(CoreNLPPath, "patterns", "stopwords.txt");
                string[] fileLines = File.ReadAllLines(path);

                return fileLines.Where(t => !string.IsNullOrEmpty(t)).ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        public void LoadConfigFromDisk(bool loadProperties)
        {
            ExtractConfig config;

            try
            {
                string json = File.ReadAllText(ToolInfo.ExtractorConfigPath);
                config = JsonConvert.DeserializeObject<ExtractConfig>(json) ?? Default;
            }
            catch
            {
                config = Default;
            }

            CoreNLPPath = config.CoreNLPPath;
            Annotators = config.Annotators;
            CoreNLPClientMemory = config.CoreNLPClientMemory;
            CoreNLPClientTimeout = config.CoreNLPClientTimeout;

            if (loadProperties)
                CoreNLPClientProperties = config.CoreNLPClientProperties;
        }

        public bool TrySaveConfigToDisk()
        {
            try
            {
                string json = JsonConvert.SerializeObject(this);
                File.WriteAllText(ToolInfo.ExtractorConfigPath, json);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static JObject GetCoreNLPClientDefaultProperties()
        {
            var props = new JObject
            {
                ["parse.model"] = "edu/stanford/nlp/models/srparser/englishSR.ser.gz"
            };

            return props;
        }

        public static JObject GetCoreNLPClientFullProperties()
        {
            var props = new JObject
            {
                { "annotators", "tokenize,ssplit,pos,lemma,ner,parse" },

                { "pos.model", ToolInfo.PosPath },
                { "ner.model", ToolInfo.NerPath },
                { "parse.model", ToolInfo.SrParserPath },

                { "ner.useSUTime", "true" },
                { "ner.applyFineGrained", "false" },

                { "sutime.binders", "0" },

                { "sutime.rules", ToolInfo.SutimeRules },
                { "sutime.markTimeRanges", "true" },
                { "sutime.includeNested", "true" }
            };

            return props;
        }
    }
}
