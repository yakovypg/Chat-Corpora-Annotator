using IndexEngine;
using System;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Models.Chat
{
    internal class ChatMessage : IChatMessage
    {
        public DynamicMessage Source { get; }

        public int ID => Source.Id;
        public Dictionary<string, object> Contents => Source.Contents;
        public Dictionary<string, int> Situations => Source.Situations;

        public ChatMessage(DynamicMessage source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public void AddSituation(ISituation situation)
        {
            if (situation == null)
                return;

            Source.AddSituation(situation.Header, situation.ID);
        }
    }
}
