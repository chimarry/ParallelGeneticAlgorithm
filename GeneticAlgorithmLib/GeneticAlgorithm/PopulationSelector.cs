using GeneticAlgorithm.ExpressionTree;
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
            this.stohasticGenerator = stohasticGenerator;
        }

        public List<MathExpressionTree> GeneratePopulation(int initialPopulationSize)
        {
            BlockingCollection<MathExpressionTree> expressions = new BlockingCollection<MathExpressionTree>(initialPopulationSize);
            Parallel.For(0, initialPopulationSize, i =>
            {
                MathExpressionTree expression;
                // Get valid expression
                do
                {
                    expression = GenerateIndividual();
                } while (!expression.IsValidExpression());

                expressions.Add(expression);
            });
            return expressions.ToList();
        }

        public MathExpressionTree GenerateIndividual()
        {
            int operandsCount = stohasticGenerator.NextRandom();
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

            return new MathExpressionTree(getGenome(operatorsAndOperands, null, root));
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

            foreach ((MathExpressionTree individual, int fitnessRatio) in individualsWithFitnessRatio)
            {
                double randomProbability = stohasticGenerator.NextRandomDouble();
                double probabilityToBeSelected = (fitnessRatio - min) / (double)(max - min);
                if (probabilityToBeSelected > randomProbability && !selectedIndividuals.Contains(individual, new MathExpressionTreeEqualityComparer()))
                    selectedIndividuals.Add(individual);
            }

            return selectedIndividuals;
        }

        public int CalculateFitness(MathExpressionTree individual)
           => Math.Abs(result - individual.Root.GetValue());
    }
}
