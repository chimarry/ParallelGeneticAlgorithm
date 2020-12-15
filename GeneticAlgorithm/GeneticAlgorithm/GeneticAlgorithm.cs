using GeneticAlgorithm.ExpressionTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm
{
    public class GeneticAlgorithm
    {
        private readonly double mutationProbability;
        private readonly double crossoverProbability;
        private readonly StohasticGenerator stohasticGenerator;

        public GeneticAlgorithm(double mutationProbability, double crossoverProbability, StohasticGenerator stohasticGenerator)
        {
            this.mutationProbability = mutationProbability;
            this.crossoverProbability = crossoverProbability;
            this.stohasticGenerator = stohasticGenerator;
        }

        public List<MathExpressionTree> Evolve(List<MathExpressionTree> selectedIndividuals)
        {
            List<(MathExpressionTree, MathExpressionTree)> pairs = selectedIndividuals.OrderBy(x => new Guid())
                                                                                      .Select((individual, i) => new { index = i, individual })
                                                                                      .GroupBy(x => x.index / 2, x => x.individual)
                                                                                      .Select(g => (g.First(), g.Skip(1).FirstOrDefault()))
                                                                                      .ToList();

            foreach ((MathExpressionTree first, MathExpressionTree second) in pairs)
            {
                double randomProbability = stohasticGenerator.NextRandomDouble();
                if (randomProbability > crossoverProbability)
                {
                    MathExpressionNode crossoverPointOfFirst = first.GetRandomSubtree();
                    MathExpressionNode crossoverPointOfSecond = second.GetRandomSubtree();
                    MathExpressionTree.SwitchSubtrees(crossoverPointOfFirst, crossoverPointOfSecond);
                }
            }

            // Preform crossover for choosen pairs
            return null;
            // Get pair for crossover
            // Mutate
        }
    }
}
