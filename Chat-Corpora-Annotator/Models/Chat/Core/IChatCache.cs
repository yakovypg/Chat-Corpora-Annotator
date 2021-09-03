using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ChatCorporaAnnotator.Models.Chat.Core
{
    internal interface IChatCache
    {
        int RetainedItemsCount { get; set; }
        int CurrentPackageItemsCount { get; }

        ObservableCollection<ChatMessage> CurrentMessages { get; }

        IList<ChatMessage> MoveBack(out int pageStartIndex);
        IList<ChatMessage> MoveForward(out int pageStartIndex);

        void Reset();
    }
}
