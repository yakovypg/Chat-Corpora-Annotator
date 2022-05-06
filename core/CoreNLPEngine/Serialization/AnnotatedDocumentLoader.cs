using IndexEngine.Data.Paths;
using Newtonsoft.Json;

namespace CoreNLPEngine.Serialization
{
    internal class AnnotatedDocumentLoader : IDisposable
    {
        public StreamReader _reader;

        public AnnotatedDocumentLoader() : this(ProjectInfo.ExtractedDataPath)
        {
        }

        public AnnotatedDocumentLoader(string path)
        {
            _reader = new StreamReader(path);
        }

        public void LoadNext(out AnnotatedDocumentInfo document)
        {
            string json = _reader.ReadLine() ?? string.Empty;
            document = JsonConvert.DeserializeObject<AnnotatedDocumentInfo>(json);
        }

        public bool TryLoadNext(out AnnotatedDocumentInfo document)
        {
            try
            {
                LoadNext(out document);
                return true;
            }
            catch
            {
                document = default;
                return false;
            }
        }

        public void Close()
        {
            _reader.Close();
        }

        public void Dispose()
        {
            Close();
            _reader.Dispose();
        }
    }
}
