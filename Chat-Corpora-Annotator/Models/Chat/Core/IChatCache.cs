using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ChatCorporaAnnotator.Models.Chat.Core
{
    internal interface IChatCache
    {
        int RetainedItemsCount { get; set; }
        int CurrentPackageItemsCount { get; }
        bool IsPaused { get; }

        ObservableCollection<ChatMessage> CurrentMessages { get; }

        IList<ChatMessage> MoveBack();
        IList<ChatMessage> MoveForward();

        /// <summary>
        /// Saves current package and adds temporary messages to displaying collection.
        /// </summary>
        /// <param name="tempMessages">The temporary messages that will be displayed on the screen.</param>
        void Pause(IEnumerable<ChatMessage> tempMessages);

        void Resume();
        void Reset();
    }
}
