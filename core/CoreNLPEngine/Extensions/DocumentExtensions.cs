using Edu.Stanford.Nlp.Pipeline;

namespace CoreNLPEngine.Extensions
{
    internal static class DocumentExtensions
    {
        public static bool HasQuestion(this Document document)
        {
            if (document.Sentence == null)
                return false;

            foreach (var sentence in document.Sentence)
            {
                var constituencyParse = sentence.ParseTree;

                if (constituencyParse == null || constituencyParse.Child == null)
                    continue;

                foreach (var child in constituencyParse.Child)
                {
                    string? label = child.Value;

                    if (string.IsNullOrEmpty(label))
                        continue;

                    if (label.Equals("SQ") || label.Equals("SBARQ"))
                        return true;
                }
            }

            return false;
        }
    }
}
