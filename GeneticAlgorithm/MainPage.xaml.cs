using GeneticAlgorithm.Controls;
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
using static GeneticAlgorithm.Logic.Job;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GeneticAlgorithm
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly JobManager jobManager;

        public MainPage()
        {
            this.InitializeComponent();
            jobManager = new JobManager(UpdateJobControl, UpdateJobUnitControl);
        }

        private async void LoadJobsButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await jobManager.LoadJobs();
            jobManager.PendingJobs
                      .Select(x => x.Id)
                      .ToList()
                      .ForEach(job => PendingJobList.Items.Add(new TextBlock()
                      {
                          Text = job
                      }));
        }

        private async void AddJobButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ContentDialog contentDialog = new CreateJob(jobManager);
            await contentDialog.ShowAsync();
        }

        private void CancelButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Otkazi sve taskove
        }

        private void PauseButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Paralelno pauziraj sve taskove koji se izvrsavaju
        }

        private void StartButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            foreach (Job job in jobManager.PendingJobs)
                JobsStackPanel.Children.Add(new JobControl(job));
            jobManager.StartJobs();
        }

        public async Task UpdateJobControl(string identifier, Status jobStatus)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
             {
                 if (jobStatus == Status.Started)
                     lock (PendingJobList)
                         PendingJobList.Items
                                       .Remove(PendingJobList.Items
                                                             .Select(x => x as TextBlock)
                                                             .First(x => x.Text == identifier));
                 await ((JobControl)JobsStackPanel.Children.First(x => x.GetHashCode() == identifier.GetHashCode())).UpdateStatus(jobStatus);
             });
        }

        public async Task UpdateJobUnitControl(string identifier, JobUnit jobUnit, Status jobStatus)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await ((JobControl)JobsStackPanel.Children.First(x => x.GetHashCode() == identifier.GetHashCode())).UpdateJobUnit(jobUnit, jobStatus);
            });
        }
    }
}
