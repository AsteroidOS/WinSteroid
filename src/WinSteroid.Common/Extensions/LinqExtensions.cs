using System.Collections.Generic;

namespace System.Linq
{
    public static class LinqExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || collection.Count() == 0;
        }
    }
}
