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

        public EvolutionPhase(double mutationProbability, double crossoverProbability, int populationSize, StohasticGenerator stohasticGenerator, PopulationSelector populationSelector)
        {
            this.mutationProbability = mutationProbability;
            this.crossoverProbability = crossoverProbability;
            this.stohasticGenerator = stohasticGenerator;
            this.populationSize = populationSize;
            this.populationSelector = populationSelector;
        }

        public List<MathExpressionTree> Evolve(List<MathExpressionTree> selectedIndividuals)
        {
            List<(MathExpressionTree, MathExpressionTree)> pairs = selectedIndividuals.ParallelOrderBy(x => new Guid())
                                                                                      .Take(GetNumberOfPairs(selectedIndividuals.Count))
                                                                                      .Select((individual, i) => new { index = i, individual })
                                                                                      .GroupBy(x => x.index / 2, x => x.individual)
                                                                                      .Select(g => (g.First(), g.Skip(1).FirstOrDefault()))
                                                                                      .ToList();

            List<MathExpressionTree> newPopulation = new List<MathExpressionTree>();

            foreach ((MathExpressionTree firstParent, MathExpressionTree secondParent) in pairs)
            {
                double randomProbability = stohasticGenerator.NextRandomDouble();
                if (randomProbability > crossoverProbability)
                {
                    MathExpressionTree firstCopy = firstParent.Copy();
                    MathExpressionTree secondCopy = secondParent.Copy();
                    Crossover(firstCopy, secondCopy);
                    newPopulation.Add(firstCopy);
                    newPopulation.Add(secondCopy);
                }
            }
            pairs.ForEach(x =>
            {
                newPopulation.Add(x.Item1);
                newPopulation.Add(x.Item2);
            });

            foreach (MathExpressionTree expression in newPopulation)
            {
                double randomProbability = stohasticGenerator.NextRandomDouble();
                if (randomProbability < mutationProbability || !expression.IsValidExpression())
                    Mutate(expression);
            }

            List<MathExpressionTree> validExpressions = newPopulation.Where(x => x.IsValidExpression())
                                                                     .Distinct(new MathExpressionTreeEqualityComparer())
                                                                     .ToList();
            while (validExpressions.Count < populationSize)
            {
                MathExpressionTree expression;
                // Get valid expression
                do
                {
                    expression = populationSelector.GenerateIndividual();
                } while (!expression.IsValidExpression());
                validExpressions.Add(expression);
            }
            IEnumerable<MathExpressionTree> elite = validExpressions.ParallelOrderBy(x => populationSelector.CalculateFitness(x))
                                                             .Take(populationSize / 3);

            return elite.Concat(validExpressions)
                        .Take(populationSize)
                        .ToList();
        }

        private int GetNumberOfPairs(int selectedIndividualsCount) => selectedIndividualsCount % 2 == 0 ? selectedIndividualsCount : selectedIndividualsCount - 1;

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
