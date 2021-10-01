using IndexEngine;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace ChatCorporaAnnotator.Models.Chat
{
    internal interface IChatMessage
    {
        string Text { get; }
        string TagsPresenter { get; }

        DynamicMessage Source { get; }
        Brush SenderColor { get; }
        Brush BackgroundBrush { get; set; }

        void AddSituation(ISituation situation, IEnumerable<Tag> tagset = null);
        bool RemoveSituation(string situationKey, IEnumerable<Tag> tagset = null);

        bool TryGetSender(out string sender);
        bool TryGetSentDate(out DateTime sentDate);

        void UpdateBackgroundBrush(IEnumerable<Tag> tagset = null);
    }
}
