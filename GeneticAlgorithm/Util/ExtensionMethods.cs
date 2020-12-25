using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithm.Util
{
    public static class IEnumerableExtensionMethods
    {
        public static bool NotEmpty(this IEnumerable<object> list)
            => list.Count() != 0;
    }
}
