using System.Collections.Generic;

namespace ChatCorporaAnnotator.Models.Chat.Core
{
    internal interface IChatCache
    {
        int CurrentPackageCapacity { get; }
        IList<ChatMessage> CurrentMessages { get; }

        IList<ChatMessage> MoveBack(int currOffset, int retainedItems);
        IList<ChatMessage> MoveForward(int currOffset, int retainedItems);

        void Reset(IEnumerable<ChatMessage> currentPackage, int messageReadIndex = 0);
    }
}
