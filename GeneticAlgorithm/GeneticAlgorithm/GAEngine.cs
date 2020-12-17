using GeneticAlgorithm.ExpressionTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithm
{
    public class GAEngine
    {
        private readonly double mutationProbability;
        private readonly double crossoverProbability;
        private readonly StohasticGenerator stohasticGenerator;

        public GAEngine(double mutationProbability, double crossoverProbability, StohasticGenerator stohasticGenerator)
        {
            this.mutationProbability = mutationProbability;
            this.crossoverProbability = crossoverProbability;
            this.stohasticGenerator = stohasticGenerator;
        }

        public List<MathExpressionTree> Evolve(List<MathExpressionTree> selectedIndividuals)
        {
            List<(MathExpressionTree, MathExpressionTree)> pairs = selectedIndividuals.OrderBy(x => new Guid())
                                                                                      .Take(GetNumberOfPairs(selectedIndividuals.Count))
                                                                                      .Select((individual, i) => new { index = i, individual })
                                                                                      .GroupBy(x => x.index / 2, x => x.individual)
                                                                                      .Select(g => (g.First(), g.Skip(1).FirstOrDefault()))
                                                                                      .ToList();

            List<MathExpressionTree> newPopulation = new List<MathExpressionTree>();

            foreach ((MathExpressionTree first, MathExpressionTree second) in pairs)
            {
                double randomProbability = stohasticGenerator.NextRandomDouble();
                if (randomProbability > crossoverProbability)
                {
                    MathExpressionTree firstOffspring = first.Copy();
                    MathExpressionTree secondOffspring = second.Copy();
                    Crossover(firstOffspring, secondOffspring);
                    newPopulation.Add(firstOffspring);
                    newPopulation.Add(secondOffspring);
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
                if (randomProbability < mutationProbability)
                    Mutate(expression);
            }
            return newPopulation.Where(x => x.IsValidExpression()).ToList();
        }

        public List<MathExpressionTree> DoExtinction(List<MathExpressionTree> population)
        {
            return population.OrderBy(x => new Guid()).Take(100).ToList();
        }

        private int GetNumberOfPairs(int populationSize) => populationSize % 2 == 0 ? populationSize : populationSize - 1;

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
