using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Edu.Stanford.Nlp.Pipeline;
using NounPhraseExtractionAlgorithm;
using CoreNLPEngine.Extensions;
using Numpy;
using Wintellect.PowerCollections;


namespace CoreNLPEngine.Extraction
{
    public static class KeywordRanker
    {
        static readonly double d = 0.85; // damping coefficient, usually is .85
        static readonly double min_diff = 1e-5; // convergence threshold
        static readonly double steps = 10; // iteration steps
        static OrderedDictionary<string,double> node_weight = new OrderedDictionary<string, double>(); // save keywords and its weight
        static Dictionary<string, int> vocab = new Dictionary<string, int>();
        static int i = 0;
        private static void BuildVocabFromNouns(List<List<string>> nouns)
        {
            foreach (var sent in nouns)
            {
                foreach (var noun in sent)
                {
                    if (!vocab.ContainsKey(noun))
                    {
                        vocab[noun] = i;
                        i++;

                    }
                }
            
            }
        }


        private static List<Tuple<string, string>> GetTokenPairs(int window_size, List<List<string>> cands)
        {
            List<Tuple<string,string>> tuples = new List<Tuple<string,string>>();
            foreach (var sentence in cands)
            {
                if (sentence.Count >= window_size) {
                    for (int j = 0; j < sentence.Count; j++)
                    {
                        for (int k = j + 1; k < j + window_size; k++)
                        {
                            if (k >= sentence.Count) break;
                            var pair = Tuple.Create(sentence[j], sentence[k]);
                            if (!tuples.Contains(pair))
                            {
                                tuples.Add(pair);
                            }
                        }
                    }
                }
            }
            return tuples;
        }


        private static NDarray Symmetrize(NDarray a)
        {
            return a + a.T - np.diag(a.diagonal());
        }
        private static NDarray GetMatrix(Dictionary<string,int> vocab, List<Tuple<string,string>> token_pairs)
        {
            int vocab_size = vocab.Count;
            NDarray g = np.zeros((vocab_size,vocab_size));
            foreach (var tup in token_pairs)
            {
                var i = vocab[tup.Item1]; 
                var j = vocab[tup.Item2];
                g[i, j] = (NDarray)1;
               
            }
            g = Symmetrize(g);
            var norm = np.sum(g, 0);
            NDarray g_norm = new NDarray(g);
            NDarray nDarray = np.divide(g, norm, g_norm, norm.not_equals(0));

            
            return nDarray;

        }

        public static IOrderedEnumerable<KeyValuePair<string,double>> GetKeywords(List<List<string>> cands, int window_size = 4)
        {
            BuildVocabFromNouns(cands);
            var token_pairs = GetTokenPairs(window_size, cands);
            var g = GetMatrix(vocab, token_pairs);

            var pr = np.ones(vocab.Count);
            double prev = 0;
            for (int epoch = 0; epoch < steps; epoch++)
            {
                pr = (1 - d) + d * np.dot(g,pr);
                if (Math.Abs(prev - (double)np.sum(pr)) < min_diff) {
                    break;
                }
                else
                {
                    prev = (double)np.sum(pr);
                }
            }
            foreach (var item in vocab)
            {
                node_weight[item.Key] = (double)pr[item.Value];

            }

            var sortedDict = from entry in node_weight orderby entry.Value descending select entry;
            // [using System.Linq, as for me, is easier to read]
            // var sortedDictNew = node_weight.OrderByDescending(t => t.Value);
            // [test]
            // bool eq = sortedDictNew.SequenceEqual(sortedDict);

            return sortedDict;

        }
    }
}
