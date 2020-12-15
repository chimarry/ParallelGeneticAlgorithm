using System;
using System.Collections.Generic;

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

        public static void SwitchSubtrees(MathExpressionNode first, MathExpressionNode second)
        {
            MathExpressionNode firstParent = first.Parent;
            MathExpressionNode secondParent = second.Parent;
            if (firstParent.LeftChild.Id == first.Id)
                firstParent.LeftChild = second;
            else firstParent.RightChild = second;
            if (secondParent.LeftChild.Id == second.Id)
                secondParent.LeftChild = first;
            else secondParent.RightChild = first;

            first.Parent = secondParent;
            second.Parent = firstParent;
        }
    }
}
