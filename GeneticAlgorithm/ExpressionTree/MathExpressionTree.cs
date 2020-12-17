using System;
using System.Collections;
using System.Collections.Generic;

namespace GeneticAlgorithm.ExpressionTree
{
    public class MathExpressionTree
    {
        public MathExpressionNode Root { get; set; }

        public MathExpressionTree(MathExpressionNode root)
        {
            this.Root = root;
        }

        public MathExpressionTree Copy()
        {
            MathExpressionNode rootNode = Root.Copy(null);
            return new MathExpressionTree(rootNode);
        }

        public void PrintInorder(MathExpressionNode node, ref string expression)
        {
            if (node == null)
                return;
            PrintInorder(node.LeftChild, ref expression);
            expression += node.ToString();
            PrintInorder(node.RightChild, ref expression);
        }

        public MathExpressionNode GetRandomSubtree()
        {
            List<MathExpressionNode> currentOperators = new List<MathExpressionNode>();
            void traverse(MathExpressionNode currentNode)
            {
                if (currentNode != null)
                {
                    if (!currentNode.IsLeaf)
                        currentOperators.Add(currentNode);
                    traverse(currentNode.LeftChild);
                    traverse(currentNode.RightChild);
                }
            }
            traverse(Root);
            return currentOperators[new Random().Next(currentOperators.Count)];
        }

        public MathExpressionNode GetRandomNode()
        {
            List<MathExpressionNode> nodes = new List<MathExpressionNode>();
            void traverse(MathExpressionNode currentNode)
            {
                if (currentNode != null)
                {
                    nodes.Add(currentNode);
                    traverse(currentNode.LeftChild);
                    traverse(currentNode.RightChild);
                }
            }
            traverse(Root);
            return nodes[new Random().Next(nodes.Count)];
        }

        public bool IsValidExpression()
        {
            try
            {
                Root.GetValue();
                return true;
            }
            catch (DivideByZeroException)
            {
                return false;
            }
        }
    }
}
