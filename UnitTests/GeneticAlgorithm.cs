using GeneticAlgorithm;
using GeneticAlgorithm.ExpressionTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]

namespace UnitTests
{

    [TestClass]
    [DoNotParallelize]
    public class GeneticAlgorithm
    {
        [DataRow(100, 56, new int[] { 10, 2, 13, 5, 6, 80 })]
        [DataRow(50, 1026, new int[] { 10, 2, 13, 5, 6, 80 })]
        [DataRow(50, 4, new int[] { 10, 2, 13, 5, 6, 80 })]
        [DataRow(10, 4, new int[] { 10, 2, 13, 5, 6, 80 })]
        [DataRow(100, 17, new int[] { 11, 2, 4, 34, 6, 80 })]
        [DataRow(100, 8068, new int[] { 11, 2, 4, 34, 6, 80 })]
        [DataTestMethod]
        public void ExecuteOld(int populationSize, int lookup, int[] operands)
        {
            StohasticGenerator stohasticGenerator = new StohasticGenerator(operands);
            PopulationSelector populationSelector = new PopulationSelector(lookup, 1, stohasticGenerator);
            EvolutionPhase gAEngine = new EvolutionPhase(0.1, 0.15, populationSize, stohasticGenerator, populationSelector);
            List<MathExpressionTree> expressions = populationSelector.GeneratePopulation(populationSize);
            string result = null;
            for (int i = 0; i < 100; ++i)
            {
                List<MathExpressionTree> best = populationSelector.SelectFittestIndividuals(expressions);

                List<MathExpressionTree> crossover = gAEngine.Evolve(best);

                if (crossover.Any(x => x.Root.GetValue() == lookup))
                {
                    result = crossover.FirstOrDefault(x => x.Root.GetValue() == lookup).ToString();
                    break;
                }
                Assert.AreEqual(populationSize, crossover.Count);
                expressions = crossover;
            }
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Split("=")[1].Trim(), lookup.ToString(), $"Is false: {result}");
        }


        [DataRow(100, 56, new int[] { 10, 2, 13, 5, 6, 80 }, 100, 1, 0.05, 0.15)]
        [DataRow(50, 14, new int[] { 10, 2, 13, 5, 6, 80 }, 100, 5, 0.05, 0.15)]
        [DataRow(50, 4, new int[] { 10, 2, 13, 5, 6, 80 }, 50, 5, 0.05, 0.15)]
        [DataRow(100, 789, new int[] { 10, 2, 13, 5, 6, 80 }, 100, 10, 0.1, 0.15)]
        [DataRow(100, 8068, new int[] { 10, 2, 13, 5, 6, 80 }, 100, 10, 0.05, 0.15)]
        [DataRow(100, 1060, new int[] { 10, 2, 13, 5, 6, 80 }, 100, 10, 0.05, 0.15)]
        [DataTestMethod]
        public void Execute(int populationSize, int lookup, int[] operands, int iterationCount = 100, int eliteCount = 1, double mutationProbability = 0.05, double crossover = 0.15)
        {
            GeneticAlgorithmConfiguration configuration = new GeneticAlgorithmConfiguration(lookup, mutationProbability, crossover, populationSize, eliteCount, iterationCount)
            {
                Operands = operands
            };
            GeneticAlgorithmExecutor geneticAlgorithm = new GeneticAlgorithmExecutor(configuration);
            MathExpressionTree result = geneticAlgorithm.Execute();
            Assert.IsNotNull(result, "Result is not found");
        }
    }
}
