using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithm.Util
{
    public static class IEnumerableExtensionMethods
    {
        /// <summary>
        /// Checks if the enumerable is empty.
        /// </summary>
        /// <param name="enumerable">The enumerable to check.</param>
        /// <returns>True if not empty, fakse if empty.</returns>
        public static bool NotEmpty(this IEnumerable<object> enumerable)
            => enumerable.Count() != 0;
    }
}
