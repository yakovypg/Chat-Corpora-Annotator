using System;
using System.Collections;
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

        public static List<T> ToGenericList<T>(this IEnumerable items)
        {
            List<T> outputList = new List<T>();

            if (items == null)
                return outputList;

            foreach (var item in items)
                outputList.Add((T)item);

            return outputList;
        }

        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(this IEnumerable<T> list)
        {
            return !list.IsNullOrEmpty()
                ? GetPermutations(list, list.Count())
                : new T[0][];
        }

        private static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1)
                return list.Select(t => new T[] { t });

            var perms = GetPermutations(list, length - 1);

            return perms.SelectMany
            (
                (t) => list.Where(e => !t.Contains(e)),
                (t1, t2) => t1.Concat(new T[] { t2 })
            );
        }
    }
}
