using IndexEngine.Data.Paths;
using IndexEngine.Search;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Services
{
    public interface IKeywordService
    {
        Dictionary<string, double> GetRakeKeywords(int length);
        void FlushKeywordsToDisk();
        List<string> ProcessKeywordList(List<string> keywords);
    }

    public class KeywordService : IKeywordService
    {
        public void FlushKeywordsToDisk()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, double> GetRakeKeywords(int length)
        {
            Rake generator = new Rake(ToolInfo.root + "\\SMARTstopset.txt", 3, length, 3);
            return generator.Run(GetList());
        }

        private string BuildBigString(List<string> list)
        {
            StringBuilder str = new StringBuilder();

            foreach (var item in list)
            {
                str.Append(item);
            }

            return str.ToString();
        }

        private List<string> GetList()
        {
            List<string> sents = new List<string>();

            for (int i = 0; i < LuceneService.DirReader.MaxDoc; i++)
            {
                var document = LuceneService.DirReader.Document(i);
                sents.Add(document.GetField(ProjectInfo.TextFieldKey).GetStringValue());
            }

            return sents;
        }

        public List<string> ProcessKeywordList(List<string> keywords)
        {
            keywords.RemoveAll(x => Regex.IsMatch(x, @"[^a-zA-Z]+"));
            return keywords;
        }
    }
}
