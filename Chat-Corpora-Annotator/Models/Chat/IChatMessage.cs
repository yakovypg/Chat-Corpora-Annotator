using IndexEngine;
using System.Windows.Media;

namespace ChatCorporaAnnotator.Models.Chat
{
    internal interface IChatMessage
    {
        string Text { get; }

        DynamicMessage Source { get; }
        Brush SenderColor { get; }

        void AddSituation(ISituation situation);
    }
}
