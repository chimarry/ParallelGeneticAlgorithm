using System;

namespace GeneticAlgorithm
{
    public class ThreadSafeRandom
    {
        private readonly Random randomGenerator = new Random();

        public int Next(int min = 0, int max = int.MaxValue)
        {
            lock (randomGenerator)
            {
                return randomGenerator.Next(min, max);
            }
        }

        public double NextDouble()
        {
            lock (randomGenerator)
            {
                return randomGenerator.NextDouble();
            }
        }
    }
}
