using IndexEngine.Data.Paths;
using Newtonsoft.Json;
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

                return new ExtractConfig(coreNLPPath, annotators)
                {
                    //CoreNLPClientProperties = GetCoreNLPClientDefaultProperties()
                };
            }
        }

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

        public void LoadConfigFromDisk()
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
            CoreNLPClientProperties = config.CoreNLPClientProperties;
        }

        public void SaveConfigToDisk()
        {
            string json = JsonConvert.SerializeObject(this);
            File.WriteAllText(ToolInfo.ExtractorConfigPath, json);
        }

        public bool TrySaveConfigToDisk()
        {
            try
            {
                SaveConfigToDisk();
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
