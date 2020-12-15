using GeneticAlgorithm.ExpressionTree;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeneticAlgorithm
{
    /// <summary>
    /// Needs to generate initial population of mathematical expressions.
    /// </summary>
    public class PopulationGenerator
    {
        private int[] operands;

        public List<MathExpressionTree> GeneratePopulation(int initialPopulationSize, int[] operands)
        {
            this.operands = operands;
            BlockingCollection<MathExpressionTree> expressions = new BlockingCollection<MathExpressionTree>(initialPopulationSize);
            Parallel.For(0, initialPopulationSize, (i) =>
            {
                MathExpressionTree expression = GenerateIndividual();
                expressions.Add(expression);
            });
            return expressions.ToList();
        }

        private MathExpressionTree GenerateIndividual()
        {
            StohasticGenerator stohasticGenerator = new StohasticGenerator(operands);
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
    }
}
