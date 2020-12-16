using GeneticAlgorithm.ExpressionTree;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GeneticAlgorithm
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            PopulationGenerator populationGenerator = new PopulationGenerator();
            List<MathExpressionTree> expressions = populationGenerator.GeneratePopulation(30, new int[] { 10, 2, 13, 5, 6, 80 });
            string finalExpression = "Inicijalna populacija: ";
            foreach (var expression in expressions)
            {
                finalExpression += Environment.NewLine;
                string newExpression = "";
                expression.PrintInorder(expression.Root, ref newExpression);
                finalExpression += newExpression + " = " + expression.Root.GetValue();
            }
            ExpressionBlock.Text = finalExpression;

            List<MathExpressionTree> best = new PopulationSelector(true, 56, new StohasticGenerator(new int[] { 10, 2, 13, 5, 6, 80 })).SelectFittestIndividuals(expressions);
            finalExpression = "Najbolje jedinke: ";
            foreach (var b in best)
            {
                finalExpression += Environment.NewLine;
                string newExpression = "";
                b.PrintInorder(b.Root, ref newExpression);
                finalExpression += newExpression + " = " + b.Root.GetValue();
            }
            BestFitted.Text += finalExpression;

            GAEngine engine = new GAEngine(0.15, 0.05, new StohasticGenerator(new int[] { 10, 2, 13, 5, 6, 80 }));
            List<MathExpressionTree> crossover = engine.Evolve(best);
            finalExpression = "Nakon ukrstanja: ";
            foreach (var b in crossover)
            {
                finalExpression += Environment.NewLine;
                string newExpression = "";
                b.PrintInorder(b.Root, ref newExpression);
                finalExpression += newExpression + " = " + b.Root.GetValue();
            }
            Crossover.Text += finalExpression;

            List<MathExpressionTree> mutated = engine.Mutate(crossover);
            finalExpression = "Nakon mutacije: ";
            foreach (var b in mutated)
            {
                finalExpression += Environment.NewLine;
                string newExpression = "";
                b.PrintInorder(b.Root, ref newExpression);
                finalExpression += newExpression + " = " + b.Root.GetValue();
            }
            Mutation.Text += finalExpression;
        }
    }
}
