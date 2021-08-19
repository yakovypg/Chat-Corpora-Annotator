using System;
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

        public static IEnumerable<string> ToStringEnumerable<T>(this IEnumerable<T> items)
        {
            return items?.Select(t => t.ToString());
        }

        public static IEnumerable<string> ToStringEnumerable<T>(this IEnumerable<T> items, Func<T, string> converter)
        {
            return items?.Select(t => converter(t));
        }
    }
}
