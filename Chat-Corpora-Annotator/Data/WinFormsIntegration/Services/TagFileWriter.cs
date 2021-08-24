using IndexEngine;
using IndexEngine.Paths;
using System.Collections.Generic;
using System.Xml;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Services
{
    public interface ITagFileWriter
    {
        void WriteMessage(int messageId, string text, string user, string date);
        void WriteSituation(List<DynamicMessage> messages, string situation, int sid);
        void WriteSituation(List<int> messageIds, string situation, int sid);
        void CloseWriter();
        void OpenWriter();
    }

    public class TagFileWriter : ITagFileWriter
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
                WriteMessage(msg.Id, msg.Contents[ProjectInfo.TextFieldKey].ToString(), msg.Contents[ProjectInfo.SenderFieldKey].ToString(), msg.Contents[ProjectInfo.DateFieldKey].ToString());
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
                WriteMessage(id, msg.Contents[ProjectInfo.TextFieldKey].ToString(), msg.Contents[ProjectInfo.SenderFieldKey].ToString(), msg.Contents[ProjectInfo.DateFieldKey].ToString());
            }

            writer.WriteEndElement();
        }
    }
}
