using IndexEngine;
using System.Collections.Generic;
using System.Windows.Media;

namespace ChatCorporaAnnotator.Models.Chat
{
    internal interface IChatMessage
    {
        string Text { get; }

        DynamicMessage Source { get; }
        Brush SenderColor { get; }
        Brush BackgroundBrush { get; set; }

        void AddSituation(ISituation situation, IEnumerable<Tag> tagset = null);
    }
}
