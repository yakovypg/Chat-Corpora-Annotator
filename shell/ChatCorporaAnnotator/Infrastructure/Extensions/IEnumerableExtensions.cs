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

        public static int GetPermutationsCount<T>(this IEnumerable<T> items)
        {
            if (items.IsNullOrEmpty())
                return 0;

            int factorial = 1;
            int itemsCount = items.Count();

            for (int i = 2; i <= itemsCount; ++i)
                factorial *= i;

            return factorial;
        }

        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(this IEnumerable<T> items)
        {
            return !items.IsNullOrEmpty()
                ? GetPermutations(items, items.Count())
                : new T[0][];
        }

        private static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> items, int length)
        {
            if (length == 1)
                return items.Select(t => new T[] { t });

            var perms = GetPermutations(items, length - 1);

            return perms.SelectMany
            (
                (t) => items.Where(e => !t.Contains(e)),
                (t1, t2) => t1.Concat(new T[] { t2 })
            );
        }
    }
}
