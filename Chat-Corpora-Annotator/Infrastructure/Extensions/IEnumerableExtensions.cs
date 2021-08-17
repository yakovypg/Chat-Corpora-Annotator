using System.Collections.Generic;
using System.Linq;

namespace ChatCorporaAnnotator.Infrastructure.Extensions
{
    internal static class IEnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> items)
        {
            return items is null || items.Count() == 0;
        }
    }
}
