using System.Collections.Generic;

namespace ChatCorporaAnnotator.Infrastructure.Extensions
{
    internal static class ListExtensions
    {
        public static void Reset<T>(this List<T> list, IEnumerable<T> newItems)
        {
            list.Clear();

            if (newItems != null)
                list.AddRange(newItems);
        }
    }
}
