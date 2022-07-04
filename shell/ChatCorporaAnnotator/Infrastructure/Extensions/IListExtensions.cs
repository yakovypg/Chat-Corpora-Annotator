using System.Collections.Generic;

namespace ChatCorporaAnnotator.Infrastructure.Extensions
{
    internal static class IListExtensions
    {
        public static void Swap<T>(this IList<T> list, T item1, T item2)
        {
            if (!list.Contains(item1) || !list.Contains(item2))
                return;

            int index1 = list.IndexOf(item1);
            int index2 = list.IndexOf(item2);

            if (index1 == index2)
                return;

            if (index1 > index2)
            {
                (index2, index1) = (index1, index2);
                (item2, item1) = (item1, item2);
            }

            list.RemoveAt(index2);
            list.RemoveAt(index1);

            list.Insert(index1, item2);
            list.Insert(index2, item1);
        }

        /// <summary>
        /// Moves the first element of the collection to the second (inserts to the left of the second).
        /// </summary>
        /// <typeparam name="T">The type of collection items.</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="itemToMove">The item to move.</param>
        /// <param name="itemDest">The element to which the 'itemToMove' will be moved.</param>
        public static void MoveItem<T>(this IList<T> list, T itemToMove, T itemDest)
        {
            if (!list.Contains(itemToMove) || !list.Contains(itemDest))
                return;

            int itemToMoveIndex = list.IndexOf(itemToMove);
            int itemDestIndex = list.IndexOf(itemDest);

            if (itemToMoveIndex == itemDestIndex)
                return;

            list.RemoveAt(itemToMoveIndex);
            list.Insert(itemDestIndex, itemToMove);
        }
    }
}
