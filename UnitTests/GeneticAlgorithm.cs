using GeneticAlgorithm;
using GeneticAlgorithm.ExpressionTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

[assembly: Parallelize(Workers = 3, Scope = ExecutionScope.MethodLevel)]

namespace UnitTests
{

    [TestClass]
    public class GeneticAlgorithm
    {

        [DataRow(100, 56, new int[] { 10, 2, 13, 5, 6, 80 }, 100, 1, 0.05, 0.15)]
        [DataRow(50, 14, new int[] { 10, 2, 13, 5, 6, 80 }, 100, 5, 0.05, 0.15)]
        [DataRow(50, 4, new int[] { 10, 2, 13, 5, 6, 80 }, 50, 5, 0.05, 0.15)]
        [DataRow(100, 789, new int[] { 10, 2, 13, 5, 6, 80 }, 100, 10, 0.1, 0.15)]
        [DataRow(100, 8068, new int[] { 10, 2, 13, 5, 6, 80 }, 100, 10, 0.05, 0.15)]
        [DataRow(100, 1060, new int[] { 10, 2, 13, 5, 6, 80 }, 100, 10, 0.05, 0.15)]
        [DataTestMethod]
        public async Task Execute(int populationSize, int lookup, int[] operands, int iterationCount = 100, int eliteCount = 1, double mutationProbability = 0.05, double crossover = 0.15)
        {
            GeneticAlgorithmConfiguration configuration = new GeneticAlgorithmConfiguration(lookup, mutationProbability, crossover, populationSize, eliteCount, iterationCount)
            {
                Operands = operands
            };
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            GeneticAlgorithmExecutor geneticAlgorithm = new GeneticAlgorithmExecutor(configuration, cancellationTokenSource.Token, new SemaphoreSlim(1));
            MathExpressionTree result = await geneticAlgorithm.Execute();
            Assert.IsNotNull(result, "Result is not found");
        }
    }
}
