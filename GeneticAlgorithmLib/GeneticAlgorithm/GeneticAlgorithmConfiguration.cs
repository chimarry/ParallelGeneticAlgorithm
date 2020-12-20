namespace GeneticAlgorithm
{
    public class GeneticAlgorithmConfiguration
    {
        public double MutationProbability { get; }

        public double CrossoverProbability { get; }

        public int PopulationSize { get; }

        public int EliteCount { get; }

        public int Result { get; }

        public int IterationCount { get; }

        public int[] Operands { get; set; }

        public GeneticAlgorithmConfiguration(int result, double mutationProbability = 0.05, double crossoverProbability = 0.15, int populationSize = 100, int eliteCount = 1, int iterationCount = 100)
        {
            MutationProbability = mutationProbability;
            CrossoverProbability = crossoverProbability;
            PopulationSize = populationSize;
            EliteCount = eliteCount;
            Result = result;
            IterationCount = iterationCount;
        }
    }
}
