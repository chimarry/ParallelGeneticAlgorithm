using System.Collections.Generic;

namespace GeneticAlgorithm.ExpressionTree
{
    public class MathExpressionTreeEqualityComparer : IEqualityComparer<MathExpressionTree>
    {
        public bool Equals(MathExpressionTree x, MathExpressionTree y)
           => x.ToString() == y.ToString();

        public int GetHashCode(MathExpressionTree obj)
           => obj.ToString().GetHashCode();
    }
}
