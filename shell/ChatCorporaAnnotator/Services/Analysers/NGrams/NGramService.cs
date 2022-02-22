using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;
using IndexEngine.Data.Paths;
using IndexEngine.Search;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ChatCorporaAnnotator.Services.Analysers.NGrams
{
    internal class NGramService : INGramService
    {
        private BPlusTree<string, int> FullIndex { get; set; }
        private BPlusTree<string, int>.OptionsV2 Options { get; set; }
        private BulkInsertOptions BulkOptions { get; set; }

        public bool IndexExists { get; private set; }
        public bool IndexIsRead { get; private set; }

        public NGramService()
        {
            CheckIndex();
        }

        public void CheckIndex()
        {
            if (File.Exists(ProjectInfo.InfoPath + "index"))
            {
                IndexExists = true;
            }
            else
            {
                IndexExists = false;
                IndexIsRead = false;
            }

            if (IndexExists)
            {
                if (FullIndex != null)
                    IndexIsRead = true;
            }
            else
            {
                IndexIsRead = false;
            }
        }

        public void ReadIndexFromDisk()
        {
            SetTreeOptions();

            FullIndex = new BPlusTree<string, int>(Options);
            IndexExists = true;
            IndexIsRead = true;
        }

        private void SetTreeOptions()
        {
            Options = new BPlusTree<string, int>.OptionsV2(PrimitiveSerializer.String, PrimitiveSerializer.Int32);
            
            BulkOptions = new BulkInsertOptions
            {
                DuplicateHandling = DuplicateHandling.FirstValueWins
            };

            Options.CalcBTreeOrder(48, 4);
            Options.CreateFile = CreatePolicy.IfNeeded;
            Options.StoragePerformance = StoragePerformance.Fastest;
            Options.FileName = ProjectInfo.InfoPath + "index";
        }

        public void BuildFullIndex()
        {
            FullIndex?.Dispose();
            SetTreeOptions();

            FullIndex = new BPlusTree<string, int>(Options);

            var grams = new Dictionary<string, int>();

            for (int i = 0; i < LuceneService.DirReader.MaxDoc; i++)
            {
                var msg = LuceneService.DirReader.Document(i).GetField(ProjectInfo.TextFieldKey).GetStringValue();

                foreach (var gram in GetNGrams(ProjectInfo.TextFieldKey, msg))
                {
                    if (!grams.TryGetValue(gram, out int value))
                        grams.Add(gram, 1);
                    else
                        grams[gram]++;
                }
            }

            FullIndex.BulkInsert(grams, BulkOptions);

            IndexExists = true;
            IndexIsRead = true;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public List<string> GetNGrams(string TextFieldKey, string document)
        {
            var ngrams = new List<string>();

            if (LuceneService.NGrammer != null)
            {
                TokenStream stream = LuceneService.NGrammer.GetTokenStream(TextFieldKey, new StringReader(document));
                var charTermAttribute = stream.AddAttribute<ICharTermAttribute>();
                
                stream.Reset();

                while (stream.IncrementToken())
                {
                    string term = charTermAttribute.ToString();
                    ngrams.Add(term);
                }

                stream.ClearAttributes();
                stream.End();
                stream.Dispose();

                return ngrams;
            }
            else
            {
                throw new Exception("No ngrammer");
            }
        }

        public List<BTreeDictionary<string, int>> GetReadableResultsForTerm(string term)
        {
            var bi = new BTreeDictionary<string, int>();
            var tri = new BTreeDictionary<string, int>();
            var four = new BTreeDictionary<string, int>();
            var five = new BTreeDictionary<string, int>();

            foreach (var kvp in FullIndex)
            {
                var arr = kvp.Key.Split(' ');

                if (arr.Contains(term))
                {
                    switch (arr.Length)
                    {
                        case 2: bi.Add(kvp); break;
                        case 3: tri.Add(kvp); break;
                        case 4: four.Add(kvp); break;
                        case 5: five.Add(kvp); break;
                        default: throw new Exception("Unknown n-gram.");
                    }
                }
            }

            var output = new List<BTreeDictionary<string, int>>
            {
                bi,
                tri,
                four,
                five
            };

            return output;
        }
    }
}
