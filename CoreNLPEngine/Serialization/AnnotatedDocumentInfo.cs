using Edu.Stanford.Nlp.Pipeline;

namespace CoreNLPEngine.Serialization
{
    [Serializable]
    public struct AnnotatedDocumentInfo
    {
        public int MessageId { get; set; }
        public Document? Document { get; set; }

        public AnnotatedDocumentInfo(int messsageId, Document document)
        {
            MessageId = messsageId;
            Document = document;
        }
    }
}
