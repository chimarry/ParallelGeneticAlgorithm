using GeneticAlgorithm.ExpressionTree;
using GeneticAlgorithm.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeneticAlgorithm
{
    public class PopulationSelector
    {
        private readonly int eliteCount;
        private readonly int result;
        private readonly StohasticGenerator stohasticGenerator;

        public PopulationSelector(int result, int eliteCount, StohasticGenerator stohasticGenerator)
        {
            this.eliteCount = eliteCount;
            this.result = result;
            this.stohasticGenerator = stohasticGenerator.Copy();
        }

        public List<MathExpressionTree> GeneratePopulation(int initialPopulationSize)
        {
            BlockingCollection<MathExpressionTree> expressions = new BlockingCollection<MathExpressionTree>(initialPopulationSize);
            Parallel.For(0, initialPopulationSize, i =>
            {
                MathExpressionTree expression = GenerateIndividual();
                expressions.Add(expression);
            });
            return expressions.ToList();
        }

        public MathExpressionTree GenerateIndividual()
        {
            MathExpressionTree validExpression;
            do
            {
                int operandsCount = stohasticGenerator.OperandsCount();
                List<MathExpressionNode> operatorsAndOperands = Enumerable.Range(0, 2 * operandsCount - 1)
                                                                          .Select(x => x % 2 == 0 ? stohasticGenerator.GetRandomOperand() : stohasticGenerator.GetRandomOperator())
                                                                          .ToList();
                MathExpressionNode root = operatorsAndOperands.Where(x => !x.IsLeaf)
                                                              .OrderBy(x => Guid.NewGuid())
                                                              .First();

                MathExpressionNode getGenome(List<MathExpressionNode> elements, MathExpressionNode parent, MathExpressionNode rootNode = null)
                {
                    if (elements.Count == 0)
                        return null;
                    MathExpressionNode currentNode = (rootNode ?? elements.FirstOrDefault(x => !x.IsLeaf)) ?? elements.First();
                    currentNode.Parent = parent;
                    if (!currentNode.IsLeaf)
                    {
                        int currentNodeIndex = elements.IndexOf(currentNode);
                        currentNode.LeftChild = getGenome(elements.Take(currentNodeIndex).ToList(), currentNode);
                        currentNode.RightChild = getGenome(elements.Skip(currentNodeIndex + 1).ToList(), currentNode);
                    }
                    return currentNode;
                }

                validExpression = new MathExpressionTree(getGenome(operatorsAndOperands, null, root));
            } while (!validExpression.IsValidExpression());
            return validExpression;
        }

        public List<MathExpressionTree> SelectFittestIndividuals(List<MathExpressionTree> population)
        {
            List<MathExpressionTree> selectedIndividuals = new List<MathExpressionTree>();
            IEnumerable<(MathExpressionTree individual, int fitnessRatio)> individualsWithFitnessRatio =
                population.Select(expression => (expression, CalculateFitness(expression)));

            int max = individualsWithFitnessRatio.Max(x => x.fitnessRatio);
            int min = individualsWithFitnessRatio.Min(x => x.fitnessRatio);

            individualsWithFitnessRatio = individualsWithFitnessRatio.Select(x => (x.individual, max - x.fitnessRatio))
                                                                     .OrderByDescending(x => x.Item2);

            selectedIndividuals.AddRange(individualsWithFitnessRatio.Take(eliteCount).Select(x => x.individual));

            individualsWithFitnessRatio = individualsWithFitnessRatio.Skip(eliteCount);

            ThreadSafeRandom random = new ThreadSafeRandom();
            individualsWithFitnessRatio.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount - 1).ForAll(x =>
            {
                double randomProbability = random.NextDouble();
                double probabilityToBeSelected = (x.fitnessRatio - min) / (double)(max - min);
                if (probabilityToBeSelected > randomProbability)
                    lock (selectedIndividuals)
                        selectedIndividuals.Add(x.individual);
            });
            return selectedIndividuals.AsParallel()
                                      .Distinct(new MathExpressionTreeEqualityComparer())
                                      .ToList();
        }

        public int CalculateFitness(MathExpressionTree individual)
           => Math.Abs(result - individual.Root.GetValue());
    }
}
