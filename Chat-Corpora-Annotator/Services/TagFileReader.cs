using ChatCorporaAnnotator.Models.Serialization;
using IndexEngine.Data.Serialization;
using IndexEngine.Paths;
using System.Collections.Generic;
using System.Xml;

namespace ChatCorporaAnnotator.Services
{
    internal class TagFileReader : ITagFileReader
    {
        private XmlReader _reader;

        public void OpenReader(string path)
        {
            _reader = XmlReader.Create(path);
            _reader.ReadStartElement(TagFileRules.START_ELEMENT);
        }

        public void OpenReader()
        {
            OpenReader(ProjectInfo.OutputXmlFilePath);
        }

        public void CloseReader()
        {
            _reader.Close();
        }

        public List<SituationData> ReadAllSituations()
        {
            var situations = new List<SituationData>();

            while (true)
            {
                var situation = ReadSituation();

                if (situation == null)
                    break;

                situations.Add(situation);
            }

            return situations;
        }

        public SituationData ReadSituation()
        {
            while (_reader.NodeType == XmlNodeType.EndElement)
            {
                _reader.Read();
            }

            if (_reader.EOF)
                return null;

            string sitIdStr = _reader.GetAttribute(TagFileRules.SITUATION_ID_ATTRIBUTE);

            int sitId = int.Parse(sitIdStr);
            string sitHeader = _reader.NamespaceURI;

            var messages = new List<int>();

            _reader.Read();

            while (_reader.LocalName != TagFileRules.SITUATION_ELEMENT_NAME)
            {
                if (_reader.NodeType == XmlNodeType.EndElement || _reader.LocalName != TagFileRules.MESSAGE_ELEMENT_NAME)
                {
                    _reader.Read();
                    continue;
                }

                string msgIdStr = _reader.GetAttribute(TagFileRules.MESSAGE_ID_ATTRIBUTE);
                int msgId = int.Parse(msgIdStr);

                messages.Add(msgId);
                _reader.Read();

                while (_reader.NodeType == XmlNodeType.EndElement)
                {
                    _reader.Read();
                }
            }

            return new SituationData(sitId, sitHeader, messages);
        }
    }
}
