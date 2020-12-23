using GeneticAlgorithm.ExpressionTree;
using GeneticAlgorithm.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
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
        private readonly IJobManager jobManager = new JobManager();

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void LoadJobsButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await jobManager.LoadJobs();
        }

        private async void AddJobButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ContentDialog contentDialog = new CreateJob(jobManager);
            await contentDialog.ShowAsync();
        }

        private void Test_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SemaphoreSlim semaphoreSlim = new SemaphoreSlim(2);
            Parallel.For(0, 20, async i =>
            {
                await semaphoreSlim.WaitAsync();
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    lock (Block)
                        Block.Children.Add(new TextBlock() { Text = i.ToString() });
                });
                Thread.Sleep(new Random().Next(200, 5000));
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    lock (Block)
                        Block.Children.Remove(Block.Children.Select(x => x as TextBlock).First(x => x.Text == i.ToString()));
                });
                semaphoreSlim.Release();
            });
        }
    }
}
