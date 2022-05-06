using IndexEngine.Data.Paths;
using Newtonsoft.Json;

namespace CoreNLPEngine.Serialization
{
    internal class AnnotatedDocumentSaver : IDisposable
    {
        private readonly StreamWriter _writer;
        private readonly List<AnnotatedDocumentInfo> _documents;

        private int _flushInterval = 10000;
        public int FlushInterval
        {
            get => _flushInterval;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Value must be greater than zero.");

                _flushInterval = value;
            }
        }

        public AnnotatedDocumentSaver() : this(ProjectInfo.ExtractedDataPath)
        {
        }

        public AnnotatedDocumentSaver(string path)
        {
            _writer = new StreamWriter(path);
            _documents = new List<AnnotatedDocumentInfo>();
        }

        public void AddDocument(AnnotatedDocumentInfo document)
        {
            _documents.Add(document);

            if (_documents.Count == FlushInterval)
                FlushData();
        }

        public void FlushData()
        {
            if (_documents.Count == 0)
                return;

            foreach (var document in _documents)
            {
                string json = JsonConvert.SerializeObject(document);
                _writer.WriteLine(json);
            }
        }

        public void Close()
        {
            FlushData();
            _writer.Close();
        }

        public void Dispose()
        {
            Close();
            _writer.Dispose();
        }
    }
}
