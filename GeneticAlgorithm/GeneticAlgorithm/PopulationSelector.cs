using GeneticAlgorithm.ExpressionTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeneticAlgorithm
{
    public class PopulationSelector
    {
        private StohasticGenerator stohasticGenerator;
        private readonly bool elitism;
        private readonly int result;

        public PopulationSelector(bool elitism, int result, StohasticGenerator stohasticGenerator)
        {
            this.elitism = elitism;
            this.result = result;
            this.stohasticGenerator = stohasticGenerator;
        }

        public List<MathExpressionTree> SelectFittestIndividuals(List<MathExpressionTree> population)
        {
            List<MathExpressionTree> selectedIndividuals = new List<MathExpressionTree>();
            IEnumerable<(MathExpressionTree individual, int fitnessRatio)> individualsWithFitnessRatio = population.Select(x => (x, CalculateFitness(x)));

            int max = individualsWithFitnessRatio.Max(x => x.fitnessRatio);
            int min = individualsWithFitnessRatio.Min(x => x.fitnessRatio);

            individualsWithFitnessRatio = individualsWithFitnessRatio.Select(x => (x.individual, max - x.fitnessRatio))
                                                                     .OrderByDescending(x => x.Item2);

            if (elitism)
                selectedIndividuals.Add(individualsWithFitnessRatio.First().individual);
            individualsWithFitnessRatio = individualsWithFitnessRatio.Skip(1);

            double randomProbability = stohasticGenerator.NextRandomDouble();
            foreach ((MathExpressionTree individual, int fitnessRatio) in individualsWithFitnessRatio)
            {
                double probabilityToBeSelected = (fitnessRatio - min) / (double)(max - min);
                if (probabilityToBeSelected > randomProbability)
                    selectedIndividuals.Add(individual);
            }

            return selectedIndividuals;
        }

        private int CalculateFitness(MathExpressionTree individual)
            => Math.Abs(result - individual.Root.GetValue());
    }
}
