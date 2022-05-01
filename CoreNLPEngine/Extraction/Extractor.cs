using CoreNLPClientDotNet;
using CoreNLPEngine.Extensions;
using CoreNLPEngine.Search;
using CSharpTest.Net.Collections;
using Edu.Stanford.Nlp.Pipeline;
using IndexEngine.Data.Paths;
using IndexEngine.Search;
using NounPhraseExtractionAlgorithm;

namespace CoreNLPEngine.Extraction
{
    public class Extractor
    {
        public delegate void SuccessfulExtractionHandler();
        public event SuccessfulExtractionHandler? SuccessfulExtraction;

        public delegate void FailedExtractionHandler();
        public event FailedExtractionHandler? FailedExtraction;

        public delegate void ProgressChangedHandler(long delta, long currValue);
        public event ProgressChangedHandler? ProgressChanged;

        private ExtractConfig _config;
        public ExtractConfig Config
        {
            get => _config;
            set
            {
                _config = value;
                _stopwords = _config.LoadStopwordsFromDisk();
            }
        }

        public int ProgressUpdateInterval { get; set; }

        public List<int> MessagesWithQuestion { get; set; }
        public List<List<string>> SelectedWords { get; set; }

        public BTreeDictionary<int, string> URLs { get; set; }
        public BTreeDictionary<int, string> Dates { get; set; }
        public BTreeDictionary<int, string> Times { get; set; }
        public BTreeDictionary<int, string> Locations { get; set; }
        public BTreeDictionary<int, string> Organisations { get; set; }
        public BTreeDictionary<int, List<string>> NounPhrases { get; set; }

        private Task? _extractionTask = null;
        public Task? ExtractionTask => _extractionTask;

        private bool _stopExtraction = false;
        private bool _isExtractionActive = false;

        private string[] _stopwords;

        public Extractor()
        {
            _config = ExtractConfig.Default;
            _stopwords = Array.Empty<string>();

            Config = ExtractConfig.Default;

            MessagesWithQuestion = new List<int>();
            SelectedWords = new List<List<string>>();

            URLs = new BTreeDictionary<int, string>();
            Dates = new BTreeDictionary<int, string>();
            Times = new BTreeDictionary<int, string>();
            Locations = new BTreeDictionary<int, string>();
            Organisations = new BTreeDictionary<int, string>();
            NounPhrases = new BTreeDictionary<int, List<string>>();
        }

        public void Clear()
        {
            MessagesWithQuestion.Clear();
            URLs.Clear();
            Dates.Clear();
            Times.Clear();
            Locations.Clear();
            Organisations.Clear();
            NounPhrases.Clear();
        }

        public async Task ExtractAsync()
        {
            _extractionTask = Task.Run(Extract);
            await _extractionTask;
        }

        public void StopExtraction()
        {
            _stopExtraction = true;
        }

        public void StopExtractionAndWait()
        {
            StopExtraction();
            _extractionTask?.Wait();
        }

        public void UpdateStopwords()
        {
            _stopwords = Config.LoadStopwordsFromDisk();
        }

        public void Extract()
        {
            if (LuceneService.DirReader == null || _isExtractionActive)
                return;

            _isExtractionActive = true;

            Clear();

            bool extractionSuccess = true;
            var coreNLPClient = CreateCoreNLPClient();

            for (int i = 0; i < LuceneService.DirReader.MaxDoc; i++)
            {
                if (_stopExtraction)
                {
                    coreNLPClient.Stop();
                    extractionSuccess = false;
                    break;
                }

                var msgDoc = LuceneService.DirReader.Document(i);
                int msgId = msgDoc.GetField(ProjectInfo.IdKey).GetInt32Value() ?? -1;
                string msgText = msgDoc.GetField(ProjectInfo.TextFieldKey).GetStringValue();

                Document? annDoc = GetAnnotatedDocument(msgText, coreNLPClient);

                if (annDoc == null)
                    continue;

                ExtractDataFromDocument(annDoc, msgId);

                int currProgressValue = i + 1;

                if (currProgressValue % ProgressUpdateInterval == 0)
                    ProgressChanged?.Invoke(ProgressUpdateInterval, currProgressValue);
            }

            var res = KeywordRanker.GetKeywords(SelectedWords);
            Console.Write(res);

            coreNLPClient.Dispose();

            if (extractionSuccess)
            {
                SuccessfulExtraction?.Invoke();
                RetrieversSearch.Extractor = this;
            }
            else
            {
                FailedExtraction?.Invoke();
            }
        }

        private void ExtractDataFromDocument(Document annDoc, int msgId)
        {
            List<string> keyPhrases = ExtractKeyPhrases(annDoc);
            NounPhrases.Add(msgId, keyPhrases);

            List<string> nouns = ExtractCandidateKeywords(annDoc);
            SelectedWords.Add(nouns);

            if (annDoc.HasQuestion())
                MessagesWithQuestion.Add(msgId);

            if (annDoc.Mentions != null)
            {
                foreach (var ner in annDoc.Mentions)
                    ExtractNERTags(ner, msgId);
            }
        }

        private void ExtractNERTags(NERMention mention, int msgId)
        {
            string nerType = mention.EntityType;
            string nerText = mention.EntityMentionText;

            var updateDictAction = delegate (BTreeDictionary<int, string> dict, int id, string value)
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

        private List<string> ExtractCandidateKeywords(Document document)
        {
            if (document.Sentence == null)
                return new List<string>();

            var cands = new List<string>();

            foreach (var sentence in document.Sentence)
            {
                if (sentence.Token == null)
                    continue;

                foreach (var token in sentence.Token)
                {
                    if (!token.HasPos)
                        continue;

                    // If the word is a noun Pos starts with "NN"
                    if (token.Pos.Contains("NN") || token.Pos.Contains("VB"))
                    {
                        string noun = token.Value;
                        // noun = noun.Remove(noun.Length - 2);

                        if (!_stopwords.Contains(noun))
                            cands.Add(noun);
                    }
                }
            }

            return cands;
        }

        private Document? GetAnnotatedDocument(string text, CoreNLPClient coreNLPClient)
        {
            return Config == null || string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text)
                ? null
                : (Document)coreNLPClient.Annotate(text, null, "", "", Config.CoreNLPClientProperties);
        }

        private CoreNLPClient CreateCoreNLPClient()
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
