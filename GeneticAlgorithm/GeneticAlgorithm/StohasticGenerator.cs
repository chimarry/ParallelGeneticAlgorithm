using GeneticAlgorithm.ExpressionTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm
{
    public class StohasticGenerator
    {
        private readonly Random randomGenerator = new Random();
        private static readonly int operationCount = Enum.GetValues(typeof(Operation)).Length;
        private readonly int[] operands;

        public StohasticGenerator(IEnumerable<int> operands)
        {
            this.operands = operands.ToArray();
        }

        public Operator GetRandomOperator()
        {
            lock (randomGenerator)
            {
                return new Operator((Operation)randomGenerator.Next(operationCount));
            }
        }

        public Operator GetRandomOperator(MathExpressionNode firstOperand, MathExpressionNode secondOperand)
        {
            lock (randomGenerator)
            {
                return new Operator((Operation)randomGenerator.Next(operationCount))
                {
                    LeftChild = firstOperand,
                    RightChild = secondOperand
                };
            }
        }

        public MathExpressionNode GetRandomOperand()
        {
            lock (randomGenerator)
            {
                return new MathExpressionNode(operands[randomGenerator.Next(operands.Length)]);
            }
        }

        public int GetRandomNumber()
        {
            lock (randomGenerator)
            {
                return randomGenerator.Next(2, operands.Length);
            }
        }
    }
}
