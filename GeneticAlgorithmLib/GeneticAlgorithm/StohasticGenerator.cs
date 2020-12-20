using GeneticAlgorithm.ExpressionTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticAlgorithm
{
    public class StohasticGenerator
    {
        private const int maxNumber = 6;

        private readonly ThreadSafeRandom randomGenerator = new ThreadSafeRandom();
        private static readonly int operationCount = Enum.GetValues(typeof(Operation)).Length;
        public readonly int[] Operands;

        public StohasticGenerator(IEnumerable<int> operands)
        {
            this.Operands = operands.ToArray();
        }

        public Operator GetRandomOperator()
            => new Operator((Operation)randomGenerator.Next(max: operationCount));

        public Operator GetRandomOperator(MathExpressionNode firstOperand, MathExpressionNode secondOperand)
            => new Operator((Operation)randomGenerator.Next(max: operationCount))
            {
                LeftChild = firstOperand,
                RightChild = secondOperand
            };

        public MathExpressionNode GetRandomOperand()
            => new MathExpressionNode(Operands[randomGenerator.Next(max: Operands.Length)]);

        public int OperandsCount()
             => randomGenerator.Next(min: 2, max: maxNumber);

        public double NextRandomDouble()
            => randomGenerator.NextDouble();
    }
}