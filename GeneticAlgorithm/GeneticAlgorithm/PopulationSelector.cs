using GeneticAlgorithm.ExpressionTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithm
{
    public class PopulationSelector
    {
        private const int eliteIndividualCount = 1;

        private readonly StohasticGenerator stohasticGenerator;
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
            IEnumerable<(MathExpressionTree individual, int fitnessRatio)> individualsWithFitnessRatio = population.Select(expression => (expression, CalculateFitness(expression)));

            int max = individualsWithFitnessRatio.Max(x => x.fitnessRatio);
            int min = individualsWithFitnessRatio.Min(x => x.fitnessRatio);

            individualsWithFitnessRatio = individualsWithFitnessRatio.Select(x => (x.individual, max - x.fitnessRatio))
                                                                     .OrderByDescending(x => x.Item2);

            if (elitism)
                selectedIndividuals.Add(individualsWithFitnessRatio.First().individual);
            individualsWithFitnessRatio = individualsWithFitnessRatio.Skip(eliteIndividualCount);

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
