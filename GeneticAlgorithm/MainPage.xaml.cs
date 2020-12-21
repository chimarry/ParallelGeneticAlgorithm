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
        ImageMaker imageMaker = new ImageMaker();

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
            //int lookup = 8068;
            //int populationSize = 100;
            //StohasticGenerator stohasticGenerator = new StohasticGenerator(new int[] { 10, 2, 110, 5, 6, 80 });
            //PopulationSelector populationSelector = new PopulationSelector(lookup, 10, stohasticGenerator);
            //EvolutionPhase gAEngine = new EvolutionPhase(lookup, 10, 0.10, 0.15, populationSize, stohasticGenerator);

            //List<MathExpressionTree> expressions = populationSelector.GeneratePopulation(populationSize);
            //Print("Inicijalna populacija: ", expressions, ExpressionBlock);

            //await Task.Run(async () =>
            //        {
            //            for (int i = 0; i < 100; ++i)
            //            {
            //                List<MathExpressionTree> best = populationSelector.SelectFittestIndividuals(expressions);
            //                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Print("Najbolje jedinke: ", best, BestFitted));

            //                List<MathExpressionTree> crossover = gAEngine.Evolve(best);
            //                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Print("Nakon ukrstanja i mutacije: ", crossover, Crossover));

            //                if (crossover.Any(x => x.Root.GetValue() == lookup))
            //                {
            //                    string result = crossover.FirstOrDefault(x => x.Root.GetValue() == lookup).ToString();
            //                    await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => FoundExpression.Text = "Rezultat: " + result);
            //                    break;
            //                }
            //                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => PopulationCount.Text = "Broj jedinki nakon mutacije: " + crossover.Count + "------- ");
            //                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => NumberOfIteration.Text = "Broj iteracije: " + i + "-------");
            //                expressions = crossover;
            //                Thread.Sleep(500);
            //            }
            //        });
        }

        private async void File_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await imageMaker.SaveResultAsImage(12, "(2+3)*7=56");
        }
    }
}
