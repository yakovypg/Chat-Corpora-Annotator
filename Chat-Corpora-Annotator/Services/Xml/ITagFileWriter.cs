using IndexEngine;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Services.Xml
{
    internal interface ITagFileWriter
    {
        void OpenWriter();
        void CloseWriter();

        void WriteMessage(int messageId, string text, string user, string date);
        void WriteSituation(List<DynamicMessage> messages, string situation, int sid);
        void WriteSituation(List<int> messageIds, string situation, int sid);
    }
}
