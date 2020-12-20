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

        public static void AddTwo(this List<MathExpressionTree> list, MathExpressionTree first, MathExpressionTree second)
        {
            list.Add(first);
            list.Add(second);
        }

        public static MathExpressionTree FirstOrNew(this IEnumerable<MathExpressionTree> selected, PopulationSelector populationSelector)
             => selected.FirstOrDefault() ?? populationSelector.GenerateIndividual();
    }

    public static class StohasticGeneratorExtensionMethods
    {
        public static StohasticGenerator Copy(this StohasticGenerator stohasticGenerator)
            => new StohasticGenerator(stohasticGenerator.Operands);
    }
}
