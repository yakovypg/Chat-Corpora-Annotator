using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ChatCorporaAnnotator.Models.Chat.Core
{
    internal interface IChatCache
    {
        int CurrentPackageCapacity { get; }
        IList<ChatMessage> CurrentPackage { get; }
        ObservableCollection<ChatMessage> CurrentMessages { get; }

        bool MoveBack(int currOffset, int retainedItems);
        bool MoveForward(int currOffset, int retainedItems);

        void Reset();
    }
}
