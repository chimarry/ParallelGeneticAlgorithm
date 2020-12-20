using System;

namespace GeneticAlgorithm.ExpressionTree
{
    public class Operator : MathExpressionNode
    {
        public Operation Operation { get; set; } = Operation.Add;

        public override bool IsLeaf { get => false; }

        public string OperationSign { get; private set; }

        public Func<int, int, int> CorrespondingFunction { get; private set; }

        public Operator(Operation operation)
        {
            Operation = operation;
            (string, Func<int, int, int>) initializeOperation()
            {
                switch (operation)
                {
                    case Operation.Add: return (" + ", (x, y) => x + y);
                    case Operation.Sub: return (" - ", (x, y) => x - y);
                    case Operation.Mul: return (" * ", (x, y) => x * y);
                    case Operation.Div: return (" / ", (x, y) => (int)Math.Floor((decimal)x / y));
                    default: return (" + ", (x, y) => x + y);
                }
            }
            (OperationSign, CorrespondingFunction) = initializeOperation();
        }

        public override void SubstiteValue(MathExpressionNode otherNode)
        {
            if (!(otherNode is Operator substitute))
                throw new Exception();
            CorrespondingFunction = substitute.CorrespondingFunction;
            OperationSign = substitute.OperationSign;
            Operation = substitute.Operation;
        }

        public override int GetValue()
            => CorrespondingFunction.Invoke(LeftChild.GetValue(), RightChild.GetValue());

        public override MathExpressionNode Copy(MathExpressionNode parent)
        {
            Operator newOperator = new Operator(Operation)
            {
                Parent = parent
            };
            newOperator.LeftChild = LeftChild.Copy(newOperator);
            newOperator.RightChild = RightChild.Copy(newOperator);
            return newOperator;
        }

        public override string ToString()
             => OperationSign;
    }

    public enum Operation { Add, Sub, Mul, Div }
}
