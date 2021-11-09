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

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Services
{
    public class NGramService : INGramService
    {
        private BPlusTree<string, int> FullIndex { get; set; }
        private BPlusTree<string, int>.OptionsV2 Options { get; set; }
        private BulkInsertOptions bulkOptions { get; set; }

        public bool IndexExists { get; private set; }
        public bool IndexIsRead { get; private set; } = false;

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
            bulkOptions = new BulkInsertOptions();
            bulkOptions.DuplicateHandling = DuplicateHandling.FirstValueWins;
            Options.CalcBTreeOrder(48, 4);
            Options.CreateFile = CreatePolicy.IfNeeded;
            Options.StoragePerformance = StoragePerformance.Fastest;
            Options.FileName = ProjectInfo.InfoPath + "index";
        }

        public void BuildFullIndex()
        {
            SetTreeOptions();
            FullIndex = new BPlusTree<string, int>(Options);
            Dictionary<string, int> grams = new Dictionary<string, int>();

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

            FullIndex.BulkInsert(grams, bulkOptions);
            grams = null;
            IndexExists = true;
            IndexIsRead = true;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public List<string> GetNGrams(string TextFieldKey, string document)
        {
            List<string> ngrams = new List<string>();

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
            BTreeDictionary<string, int> bi = new BTreeDictionary<string, int>();
            BTreeDictionary<string, int> tri = new BTreeDictionary<string, int>();
            BTreeDictionary<string, int> four = new BTreeDictionary<string, int>();
            BTreeDictionary<string, int> five = new BTreeDictionary<string, int>();

            foreach (var kvp in FullIndex)
            {
                var arr = kvp.Key.Split(' ');

                if (arr.Contains(term))
                {
                    switch (arr.Length)
                    {
                        case 2:
                            bi.Add(kvp);
                            break;
                        case 3:
                            tri.Add(kvp);
                            break;
                        case 4:
                            four.Add(kvp);
                            break;
                        case 5:
                            five.Add(kvp);
                            break;
                        default:
                            Console.WriteLine("Whoops");
                            break;
                    }
                }
            }

            var ret = new List<BTreeDictionary<string, int>>();
            ret.Add(bi);
            ret.Add(tri);
            ret.Add(four);
            ret.Add(five);

            return ret;
        }
    }

    public interface INGramService
    {
        List<BTreeDictionary<string, int>> GetReadableResultsForTerm(string term);
        void ReadIndexFromDisk();

        void BuildFullIndex();
        void CheckIndex();
        bool IndexExists { get; }
        bool IndexIsRead { get; }
    }
}