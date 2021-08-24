using System.Collections.Generic;

namespace QuickSharpApp
{
    static class EnumerableExtensions
    {
        public static void AddToCollection<T>(this IEnumerable<T> enumerable, ICollection<T> collection)
        {
            foreach (var i in enumerable)
            {
                collection.Add(i);
            }
        }
    }
}
