using IndexEngine;
using IndexEngine.Paths;
using System.Collections.Generic;
using System.Xml;

namespace ChatCorporaAnnotator.Services
{
    internal class TagFileWriter : ITagFileWriter
    {
        private XmlWriter _writer;

        public void OpenWriter()
        {
            _writer = XmlWriter.Create(ProjectInfo.InfoPath + @"\output.xml");
            _writer.WriteStartDocument();
            _writer.WriteStartElement("Corpus");
        }

        public void WriteMessage(int messageId, string text, string user, string date)
        {
            _writer.WriteStartElement("Message");
            _writer.WriteAttributeString("id", messageId.ToString());
            _writer.WriteElementString("Text", text);
            _writer.WriteElementString("User", user);
            _writer.WriteElementString("Date", date);
            _writer.WriteEndElement();
        }

        public void CloseWriter()
        {
            _writer.WriteEndElement();
            _writer.WriteEndDocument();
            _writer.Close();
        }

        public void WriteSituation(List<DynamicMessage> messages, string situation, int sid)
        {
            _writer.WriteStartElement("Situation", situation);
            _writer.WriteAttributeString("SId", sid.ToString());

            foreach (var msg in messages)
            {
                WriteMessage(messageId: msg.Id,
                             text: msg.Contents[ProjectInfo.TextFieldKey].ToString(),
                             user: msg.Contents[ProjectInfo.SenderFieldKey].ToString(),
                             date: msg.Contents[ProjectInfo.DateFieldKey].ToString());
            }

            _writer.WriteEndElement();
        }

        public void WriteSituation(List<int> messageIds, string situation, int sid)
        {
            _writer.WriteStartElement("Situation", situation);
            _writer.WriteAttributeString("SId", sid.ToString());

            foreach (var id in messageIds)
            {
                var msg = LuceneService.RetrieveMessageById(id);

                WriteMessage(messageId: id,
                             text: msg.Contents[ProjectInfo.TextFieldKey].ToString(),
                             user: msg.Contents[ProjectInfo.SenderFieldKey].ToString(),
                             date: msg.Contents[ProjectInfo.DateFieldKey].ToString());
            }

            _writer.WriteEndElement();
        }
    }
}
