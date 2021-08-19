using IndexEngine;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Models.Chat
{
    internal interface IChatMessage
    {
        DynamicMessage Source { get; }

        int ID { get; }
        Dictionary<string, object> Contents { get; }
        Dictionary<string, int> Situations { get; }

        void AddSituation(ISituation situation);
    }
}
