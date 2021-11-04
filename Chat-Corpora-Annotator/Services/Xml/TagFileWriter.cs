using IndexEngine;
using IndexEngine.Data.Serialization;
using IndexEngine.Paths;
using System.Collections.Generic;
using System.Xml;

namespace ChatCorporaAnnotator.Services.Xml
{
    internal class TagFileWriter : ITagFileWriter
    {
        private XmlWriter _writer;

        public void OpenWriter()
        {
            _writer = XmlWriter.Create(ProjectInfo.OutputXmlFilePath);
            _writer.WriteStartDocument();
            _writer.WriteStartElement(TagFileRules.START_ELEMENT);
        }

        public void CloseWriter()
        {
            _writer.WriteEndElement();
            _writer.WriteEndDocument();
            _writer.Close();
        }

        public void WriteMessage(int messageId, string text, string user, string date)
        {
            _writer.WriteStartElement(TagFileRules.MESSAGE_ELEMENT_NAME);
            _writer.WriteAttributeString(TagFileRules.MESSAGE_ID_ATTRIBUTE, messageId.ToString());
            _writer.WriteElementString(TagFileRules.MESSAGE_TEXT_ELEMENT_NAME, text);
            _writer.WriteElementString(TagFileRules.MESSAGE_USER_ELEMENT_NAME, user);
            _writer.WriteElementString(TagFileRules.MESSAGE_DATE_ELEMENT_NAME, date);
            _writer.WriteEndElement();
        }

        public void WriteSituation(List<DynamicMessage> messages, string situation, int sid)
        {
            _writer.WriteStartElement(TagFileRules.SITUATION_ELEMENT_NAME, situation);
            _writer.WriteAttributeString(TagFileRules.SITUATION_ID_ATTRIBUTE, sid.ToString());

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
            _writer.WriteStartElement(TagFileRules.SITUATION_ELEMENT_NAME, situation);
            _writer.WriteAttributeString(TagFileRules.SITUATION_ID_ATTRIBUTE, sid.ToString());

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
