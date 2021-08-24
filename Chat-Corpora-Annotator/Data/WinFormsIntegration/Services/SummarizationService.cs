using IndexEngine;
using System;
using System.Collections.Generic;
using OpenTextSummarizer;
using IndexEngine.Paths;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Services
{
    public class SummarizationService : ISummarizationService
    {
        public Dictionary<int, List<string>> Concepts { get; set; }
        public List<string> Sentences { get; set; }

        public void SummarizeList(List<DynamicMessage> list)
        {
            throw new NotImplementedException();
        }

        public void SummarizeMessage(DynamicMessage msg)
        {
            var prov = new DirectTextContentProvider(msg.Contents[ProjectInfo.TextFieldKey].ToString());
            SummarizedDocument doc = Summarizer.Summarize(prov, new SummarizerArguments() { Language = "en", MaxSummarySentences = 1 });

            Concepts.Add(msg.Id, doc.Concepts);
            Sentences.Add(doc.Sentences[0]);
        }
    }

    public interface ISummarizationService
    {
        void SummarizeMessage(DynamicMessage msg);
        void SummarizeList(List<DynamicMessage> list);

        Dictionary<int,List<string>> Concepts { get; set; }
        List<string> Sentences { get; set; }
    }
}
