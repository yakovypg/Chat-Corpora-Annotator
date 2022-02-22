using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ChatCorporaAnnotator.Models.Chat.Core
{
    internal interface IMessageCollection
    {
        ObservableCollection<ChatMessage> CurrentMessages { get; }

        void SetMessages(IEnumerable<ChatMessage> messages);
    }
}
