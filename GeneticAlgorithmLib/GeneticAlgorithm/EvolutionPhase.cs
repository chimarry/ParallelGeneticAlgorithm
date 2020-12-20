using GeneticAlgorithm.ExpressionTree;
using GeneticAlgorithm.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithm
{
    public class EvolutionPhase
    {
        private readonly double mutationProbability;
        private readonly double crossoverProbability;
        private readonly StohasticGenerator stohasticGenerator;
        private readonly PopulationSelector populationSelector;
        private readonly int populationSize;
        private readonly int eliteCount;

        public EvolutionPhase(int result, int eliteCount, double mutationProbability, double crossoverProbability, int populationSize, StohasticGenerator stohasticGenerator)
        {
            this.mutationProbability = mutationProbability;
            this.crossoverProbability = crossoverProbability;
            this.stohasticGenerator = stohasticGenerator;
            this.populationSize = populationSize;
            this.eliteCount = eliteCount;
            this.populationSelector = new PopulationSelector(result, eliteCount, stohasticGenerator);
        }

        public List<MathExpressionTree> Evolve(List<MathExpressionTree> selectedIndividuals)
        {
            List<(MathExpressionTree, MathExpressionTree)> pairs = selectedIndividuals.ParallelOrderBy(x => new Guid())
                                                                                      .Take(selectedIndividuals.Count)
                                                                                      .Select((individual, i) => new { index = i, individual })
                                                                                      .GroupBy(x => x.index / 2, x => x.individual)
                                                                                      .Select(g => (g.First(), g.Skip(1).FirstOrNew(populationSelector)))
                                                                                      .ToList();
            List<MathExpressionTree> newPopulation = new List<MathExpressionTree>();

            ThreadSafeRandom crossoverRandom = new ThreadSafeRandom();
            ThreadSafeRandom mutationRandom = new ThreadSafeRandom();

            foreach ((MathExpressionTree firstParent, MathExpressionTree secondParent) in pairs)
            {
                double randomProbability = crossoverRandom.NextDouble();
                if (randomProbability > crossoverProbability)
                {
                    MathExpressionTree firstCopy = firstParent.Copy();
                    MathExpressionTree secondCopy = secondParent.Copy();
                    Crossover(firstCopy, secondCopy);
                    newPopulation.AddTwo(firstCopy, secondCopy);
                }
            }
            pairs.ForEach(x =>
            {
                newPopulation.AddTwo(x.Item1, x.Item2);
            });

            foreach (MathExpressionTree expression in newPopulation)
            {
                double randomProbability = mutationRandom.NextDouble();
                if (randomProbability < mutationProbability || !expression.IsValidExpression())
                    Mutate(expression);
            }

            List<MathExpressionTree> validExpressions = newPopulation.Where(x => x.IsValidExpression())
                                                                     .Distinct(new MathExpressionTreeEqualityComparer())
                                                                     .ToList();
            while (validExpressions.Count < populationSize)
                validExpressions.Add(populationSelector.GenerateIndividual());

            IEnumerable<MathExpressionTree> elite = validExpressions.ParallelOrderBy(x => populationSelector.CalculateFitness(x))
                                                             .Take(eliteCount);

            return elite.Concat(validExpressions)
                        .Take(populationSize)
                        .ToList();
        }

        private void Crossover(MathExpressionTree first, MathExpressionTree second)
        {
            MathExpressionNode crossoverPointOfFirst = first.GetRandomSubtree();
            MathExpressionNode crossoverPointOfSecond = second.GetRandomSubtree();
            MathExpressionNode firstParent = crossoverPointOfFirst.Parent;
            MathExpressionNode secondParent = crossoverPointOfSecond.Parent;
            if (firstParent != null)
            {
                if (firstParent.LeftChild.Id == crossoverPointOfFirst.Id)
                    firstParent.LeftChild = crossoverPointOfSecond;
                else firstParent.RightChild = crossoverPointOfSecond;
            }
            else first.Root = crossoverPointOfSecond;
            if (secondParent != null)
            {
                if (secondParent.LeftChild.Id == crossoverPointOfSecond.Id)
                    secondParent.LeftChild = crossoverPointOfFirst;
                else secondParent.RightChild = crossoverPointOfFirst;
            }
            else second.Root = crossoverPointOfFirst;

            crossoverPointOfFirst.Parent = secondParent;
            crossoverPointOfSecond.Parent = firstParent;
        }

        private void Mutate(MathExpressionTree expression)
        {
            MathExpressionNode randomNode = expression.GetRandomNode();
            randomNode.SubstiteValue(randomNode.IsLeaf ?
                                           stohasticGenerator.GetRandomOperand()
                                           : stohasticGenerator.GetRandomOperator(randomNode.LeftChild, randomNode.RightChild));
        }
    }
}
