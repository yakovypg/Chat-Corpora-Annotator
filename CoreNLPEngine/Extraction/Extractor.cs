using CoreNLPClientDotNet;
using CoreNLPEngine.Extensions;
using CSharpTest.Net.Collections;
using Edu.Stanford.Nlp.Pipeline;
using IndexEngine.Data.Paths;
using IndexEngine.Search;
using NounPhraseExtractionAlgorithm;

namespace CoreNLPEngine.Extraction
{
    public static class Extractor
    {
        public static ExtractConfig Config { get; set; }

        public static List<int> MessagesWithQuestion { get; set; }

        public static BTreeDictionary<int, string> URLs { get; set; }
        public static BTreeDictionary<int, string> Dates { get; set; }
        public static BTreeDictionary<int, string> Times { get; set; }
        public static BTreeDictionary<int, string> Locations { get; set; }
        public static BTreeDictionary<int, string> Organisations { get; set; }
        public static BTreeDictionary<int, List<string>> NounPhrases { get; set; }

        static Extractor()
        {
            Config = ExtractConfig.Default;

            MessagesWithQuestion = new List<int>();

            URLs = new BTreeDictionary<int, string>();
            Dates = new BTreeDictionary<int, string>();
            Times = new BTreeDictionary<int, string>();
            Locations = new BTreeDictionary<int, string>();
            Organisations = new BTreeDictionary<int, string>();
            NounPhrases = new BTreeDictionary<int, List<string>>();
        }

        public static void Clear()
        {
            MessagesWithQuestion.Clear();
            URLs.Clear();
            Dates.Clear();
            Times.Clear();
            Locations.Clear();
            Organisations.Clear();
            NounPhrases.Clear();
        }

        public static Task ExtractAsync()
        {
            return Task.Run(Extract);
        }

        public static void Extract()
        {
            if (LuceneService.DirReader == null)
                return;

            Clear();

            var coreNLPClient = CreateCoreNLPClient();

            for (int i = 0; i < LuceneService.DirReader.MaxDoc; i++)
            {
                var msgDoc = LuceneService.DirReader.Document(i);
                int msgId = msgDoc.GetField(ProjectInfo.IdKey).GetInt32Value() ?? -1;
                string msgText = msgDoc.GetField(ProjectInfo.TextFieldKey).GetStringValue();

                Document? annDoc = GetAnnotatedDocument(msgText, coreNLPClient);

                if (annDoc == null)
                    continue;

                List<string> keyPhrases = ExtractKeyPhrases(annDoc);
                NounPhrases.Add(msgId, keyPhrases);

                //List<string> nouns = ExtractNouns(annDoc);
                //KeyPhrases.Add(msgId, nouns);

                if (annDoc.HasQuestion())
                    MessagesWithQuestion.Add(msgId);

                if (annDoc.Mentions != null)
                {
                    foreach (var ner in annDoc.Mentions)
                        ExtractNERTags(ner, msgId);
                }
            }

            coreNLPClient.Dispose();
        }

        private static void ExtractNERTags(NERMention mention, int msgId)
        {
            string nerType = mention.EntityType;
            string nerText = mention.EntityMentionText;

            var updateDictAction = delegate(BTreeDictionary<int, string> dict, int id, string value)
            {
                if (!dict.ContainsKey(id))
                    dict.Add(id, value);
                else
                    dict.TryUpdate(id, $"{dict[id]}, {value}");
            };

            switch (nerType)
            {
                case "DATE":
                    updateDictAction(Dates, msgId, nerText);
                    break;

                case "TIME":
                    updateDictAction(Times, msgId, nerText);
                    break;

                case "LOCATION":
                    updateDictAction(Locations, msgId, nerText);
                    break;

                case "ORGANIZATION":
                    updateDictAction(Organisations, msgId, nerText);
                    break;

                case "URL":
                    updateDictAction(URLs, msgId, nerText);
                    break;
            }
        }

        private static List<string> ExtractKeyPhrases(Document document)
        {
            if (document.Sentence == null)
                return new List<string>();

            var keyPhrases = new List<string>();

            foreach (var sentence in document.Sentence)
            {
                if (sentence.ParseTree == null)
                    continue;

                IEnumerable<ParseTree> nnxSubtrees = NounPhraseExtractor.getKeyPhrases(sentence.ParseTree);

                foreach (var subtree in nnxSubtrees)
                {
                    List<ParseTree> leaves = subtree.GetLeaves();
                    string keys = string.Join(' ', leaves);

                    if (!string.IsNullOrEmpty(keys))
                        keyPhrases.Add(keys);
                }
            }

            return keyPhrases;
        }

        private static List<string> ExtractNouns(Document document)
        {
            if (document.Sentence == null)
                return new List<string>();

            var nouns = new List<string>();

            foreach (var sentence in document.Sentence)
            {
                if (sentence.Token == null)
                    continue;

                foreach (var token in sentence.Token)
                {
                    if (!token.HasPos)
                        continue;

                    // If the word is a noun Pos starts with "NN"
                    if (token.Pos.Contains("NN"))
                    {
                        string noun = token.Value;
                        noun = noun.Remove(noun.Length - 2);

                        nouns.Add(noun);
                    }
                }
            }

            return nouns;
        }

        private static Document? GetAnnotatedDocument(string text, CoreNLPClient coreNLPClient)
        {
            return Config == null || string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text)
                ? null
                : (Document)coreNLPClient.Annotate(text);
        }

        private static CoreNLPClient CreateCoreNLPClient()
        {
            return new CoreNLPClient(
                startServer: StartServer.TryStart,
                annotators: Config.Annotators,
                timeout: Config.CoreNLPClientTimeout,
                memory: $"{Config.CoreNLPClientMemory}G",
                properties: Config.CoreNLPClientProperties,
                classPath: Config.CoreNLPPath);
        }
    }
}
