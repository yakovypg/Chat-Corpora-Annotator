namespace ChatCorporaAnnotator.Infrastructure.Extensions
{
    internal static class ArrayExtensions
    {
        public static void Swap<T>(this T[] array, int firstItemIndex, int secondItemIndex)
        {
            (array[secondItemIndex], array[firstItemIndex]) = (array[firstItemIndex], array[secondItemIndex]);
        }
    }
}
