using IndexEngine;
using IndexEngine.Paths;
using System.Collections.Generic;
using System.Xml;

namespace ChatCorporaAnnotator.Services
{
    internal class TagFileWriter : ITagFileWriter
    {
        private XmlWriter writer;

        public void OpenWriter()
        {
            writer = XmlWriter.Create(ProjectInfo.InfoPath + @"\output.xml");
            writer.WriteStartDocument();
            writer.WriteStartElement("Corpus");
        }

        public void WriteMessage(int messageId, string text, string user, string date)
        {
            writer.WriteStartElement("Message");
            writer.WriteAttributeString("id", messageId.ToString());
            writer.WriteElementString("Text", text);
            writer.WriteElementString("User", user);
            writer.WriteElementString("Date", date);
            writer.WriteEndElement();
        }

        public void CloseWriter()
        {
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }

        public void WriteSituation(List<DynamicMessage> messages, string situation, int sid)
        {
            writer.WriteStartElement("Situation", situation);
            writer.WriteAttributeString("SId", sid.ToString());

            foreach (var msg in messages)
            {
                WriteMessage(messageId: msg.Id,
                             text: msg.Contents[ProjectInfo.TextFieldKey].ToString(),
                             user: msg.Contents[ProjectInfo.SenderFieldKey].ToString(),
                             date: msg.Contents[ProjectInfo.DateFieldKey].ToString());
            }

            writer.WriteEndElement();
        }

        public void WriteSituation(List<int> messageIds, string situation, int sid)
        {
            writer.WriteStartElement("Situation", situation);
            writer.WriteAttributeString("SId", sid.ToString());

            foreach (var id in messageIds)
            {
                var msg = LuceneService.RetrieveMessageById(id);

                WriteMessage(messageId: id,
                             text: msg.Contents[ProjectInfo.TextFieldKey].ToString(),
                             user: msg.Contents[ProjectInfo.SenderFieldKey].ToString(),
                             date: msg.Contents[ProjectInfo.DateFieldKey].ToString());
            }

            writer.WriteEndElement();
        }
    }
}
