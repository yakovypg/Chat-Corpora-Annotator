using Numpy;
using Wintellect.PowerCollections;

namespace CoreNLPEngine.Extraction
{
    public static class KeywordRanker
    {
        private const double DAMPING_COEFFICIENT = 0.85; // damping coefficient, usually is 0.85
        private const double MIN_DIFF = 1e-5; // convergence threshold
        private const double ITERATION_STEPS = 10; // iteration steps

        private static int VocabIndex { get; set; } = 0;

        private static OrderedDictionary<string, double> NodeWeight { get; } = new(); // save keywords and its weight
        private static Dictionary<string, int> Vocabulary { get; } = new();

        private static void BuildVocabularyFromNouns(List<List<string>> nouns)
        {
            foreach (var sent in nouns)
            {
                foreach (var noun in sent)
                {
                    if (!Vocabulary.ContainsKey(noun))
                        Vocabulary[noun] = VocabIndex++;
                }
            }
        }

        private static List<Tuple<string, string>> GetTokenPairs(int windowSize, List<List<string>> candidates)
        {
            var tokenPairs = new List<Tuple<string, string>>();

            foreach (var sentence in candidates)
            {
                if (sentence.Count < windowSize)
                    continue;

                for (int i = 0; i < sentence.Count; ++i)
                {
                    for (int j = i + 1; j < i + windowSize; ++j)
                    {
                        if (j >= sentence.Count)
                            break;

                        var pair = Tuple.Create(sentence[i], sentence[j]);

                        if (!tokenPairs.Contains(pair))
                            tokenPairs.Add(pair);
                    }
                }
            }

            return tokenPairs;
        }

        private static NDarray GetMatrix(Dictionary<string, int> vocabulary, List<Tuple<string, string>> tokenPairs)
        {
            int vocab_size = vocabulary.Count;

            NDarray g = np.zeros((vocab_size, vocab_size));

            foreach (var tup in tokenPairs)
            {
                var i = vocabulary[tup.Item1];
                var j = vocabulary[tup.Item2];

                g[i, j] = (NDarray)1;
            }

            g = Symmetrize(g);
            var norm = np.sum(g, 0);

            NDarray gNorm = new(g);
            NDarray nDarray = np.divide(g, norm, gNorm, norm.not_equals(0));

            return nDarray;
        }

        public static IOrderedEnumerable<KeyValuePair<string, double>> GetKeywords(List<List<string>> candidates, int windowSize = 4)
        {
            BuildVocabularyFromNouns(candidates);

            var tokenPairs = GetTokenPairs(windowSize, candidates);
            var g = GetMatrix(Vocabulary, tokenPairs);
            var pr = np.ones(Vocabulary.Count);

            double prev = 0;

            for (int epoch = 0; epoch < ITERATION_STEPS; epoch++)
            {
                pr = (1 - DAMPING_COEFFICIENT) + DAMPING_COEFFICIENT * np.dot(g, pr);

                if (Math.Abs(prev - (double)np.sum(pr)) < MIN_DIFF)
                    break;
                else
                    prev = (double)np.sum(pr);
            }

            foreach (var item in Vocabulary)
                NodeWeight[item.Key] = (double)pr[item.Value];

            var sortedDict = NodeWeight.OrderByDescending(t => t.Value); //from entry in NodeWeight orderby entry.Value descending select entry;
            return sortedDict;
        }

        private static NDarray Symmetrize(NDarray a)
        {
            return a + a.T - np.diag(a.diagonal());
        }
    }
}
