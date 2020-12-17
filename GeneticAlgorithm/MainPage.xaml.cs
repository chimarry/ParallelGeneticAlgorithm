using GeneticAlgorithm.ExpressionTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
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
        }


        private void Print(string finalExpression, List<MathExpressionTree> expressions, TextBlock textBlock)
        {
            textBlock.Text = "";
            foreach (MathExpressionTree expression in expressions)
            {
                finalExpression += Environment.NewLine;
                string newExpression = "";
                expression.PrintInorder(expression.Root, ref newExpression);
                finalExpression += newExpression + " = " + expression.Root.GetValue();
            }
            textBlock.Text += finalExpression;
        }

        private async void StartButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            StohasticGenerator stohasticGenerator = new StohasticGenerator(new int[] { 10, 2, 13, 5, 6, 80 });
            PopulationGenerator populationGenerator = new PopulationGenerator(stohasticGenerator);
            PopulationSelector populationSelector = new PopulationSelector(true, 56, stohasticGenerator);
            GAEngine gAEngine = new GAEngine(0.05, 0.15, stohasticGenerator);
            List<MathExpressionTree> expressions = populationGenerator.GeneratePopulation(100);
            Print("Inicijalna populacija: ", expressions, ExpressionBlock);
            MathExpressionTree newMathTree = expressions[0].Copy();
            /* await Task.Run(async () =>
                     {
                         for (int i = 0; i < 100; ++i)
                         {
                             List<MathExpressionTree> best = populationSelector.SelectFittestIndividuals(expressions);
                             await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Print("Najbolje jedinke: ", best, BestFitted));

                             List<MathExpressionTree> crossover = gAEngine.Evolve(best);
                             await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Print("Nakon ukrstanja i mutacije: ", crossover, Crossover));

                             if (crossover.Any(x => x.Root.GetValue() == 56))
                                 break;
                             await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => NumberOfIteration.Text = "Broj iteracije: " + i);
                             Thread.Sleep(500);
                         }
                     });*/
        }
    }
}
