using System.ComponentModel;

namespace ChatCorporaAnnotator.Models.Chat.Core
{
    internal interface IMobileMessageCollection : IMessageCollection
    {
        ICollectionView MessageCollection { get; }
        string MessageFilterText { get; set; }

        void SetSortDescriptions(SortDescription sortDescription);
        void SetGroupDescriptions(GroupDescription groupDescription);
    }
}
