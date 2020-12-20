using GeneticAlgorithm.ExpressionTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithm.Util
{
    public static class EnumerableExtensionMethods
    {
        public static OrderedParallelQuery<MathExpressionTree> ParallelOrderBy<TKey>
            (this IEnumerable<MathExpressionTree> enumerable, Func<MathExpressionTree, TKey> keySelector)
            => enumerable.AsParallel()
                         .WithDegreeOfParallelism(Environment.ProcessorCount)
                         .OrderBy(keySelector);

        public static OrderedParallelQuery<MathExpressionTree> ParallelDescendingOrderBy<TKey>
           (this IEnumerable<MathExpressionTree> enumerable, Func<MathExpressionTree, TKey> keySelector)
           => enumerable.AsParallel()
                        .WithDegreeOfParallelism(Environment.ProcessorCount)
                        .OrderByDescending(keySelector);
    }
}
