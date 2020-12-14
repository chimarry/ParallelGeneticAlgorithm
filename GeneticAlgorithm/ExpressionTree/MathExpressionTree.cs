using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticAlgorithm.ExpressionTree
{
    public class MathExpressionTree
    {
        public MathExpressionNode Root { get; }

        public MathExpressionTree(MathExpressionNode root)
        {
            this.Root = root;
        }

        public void PrintInorder(MathExpressionNode node, ref string expression)
        {
            if (node == null)
                return;
            PrintInorder(node.LeftChild, ref expression);
            expression += node.ToString();
            PrintInorder(node.RightChild, ref expression);
        }
    }
}
