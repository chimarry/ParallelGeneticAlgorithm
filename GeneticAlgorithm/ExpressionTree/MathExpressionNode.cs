using System;

namespace GeneticAlgorithm.ExpressionTree
{
    public class MathExpressionNode
    {
        public long Id { get; }

        public MathExpressionNode LeftChild { get; set; }

        public MathExpressionNode RightChild { get; set; }

        public MathExpressionNode Parent { get; set; }

        public virtual bool IsLeaf { get => LeftChild == null && RightChild == null; }

        protected int value;

        public MathExpressionNode(int value) : this()
        {
            this.value = value;
            LeftChild = null;
            RightChild = null;
        }

        public MathExpressionNode()
        {
            Id = new Random().Next();
        }

        public override string ToString()
        {
            if (Parent.LeftChild.Id == Id)
                return " (" + value;
            else return value + ") ";
        }

        public virtual int GetValue() => value;

        public virtual void Substite(MathExpressionNode otherNode)
        {
            value = otherNode.GetValue();
        }

        public override bool Equals(object obj)
        {
            return obj is MathExpressionNode node &&
                   node.Id == Id;
        }

        public override int GetHashCode()
        {
            int hashCode = 2141646327;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + IsLeaf.GetHashCode();
            hashCode = hashCode * -1521134295 + value.GetHashCode();
            return hashCode;
        }
    }
}
