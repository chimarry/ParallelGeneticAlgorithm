using GeneticAlgorithm.Controls;
using GeneticAlgorithm.Logic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
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
        private readonly ImageMaker imageMaker;

        private bool isPaused = false;
        private bool isStarted = false;

        public MainPage()
        {
            this.InitializeComponent();
            imageMaker = new ImageMaker();
            jobManager = new JobManager(UpdateJobControl, UpdateJobUnitControl)
            {
                ImageMaker = imageMaker
            };
            StartButton.IsEnabled = PauseButton.IsEnabled = CancelButton.IsEnabled = false;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            FileActivatedEventArgs args = (FileActivatedEventArgs)e.Parameter;
            StorageFile file = args.Files[0] as StorageFile;
            Job job = await jobManager.ParseJobFromFile(file);
            await AddJob(job);
        }

        public async Task AddJob(Job job)
        {
            AddToList(job);
            jobManager.AddJob(job);

            // If start button is already pressed
            if (isStarted)
            {
                JobsStackPanel.Children.Add(new JobControl(job));
                await jobManager.StartJob();
            }
            else
                StartButton.IsEnabled = true;
        }

        private void AddToList(Job job)
        {
            if (!PendingJobList.Items.Select(x => x as TextBlock).Any(x => x.Text == job.Id))
                PendingJobList.Items.Add(new TextBlock()
                {
                    Text = job.Id
                });
        }

        private async void LoadJobsButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await jobManager.LoadJobs();
            jobManager.PendingJobs
                      .ToList()
                      .ForEach(job => AddToList(job));
            StartButton.IsEnabled = true;
        }

        private async void AddJobButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ContentDialog contentDialog = new CreateJob(UpdateJobUnitControl, UpdateJobControl, AddJob);
            await contentDialog.ShowAsync();
        }

        private async void CancelButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            PauseButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
            await jobManager.CancelJobs();
        }

        private async void PauseButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            CancelButton.IsEnabled = false;
            PauseButton.IsEnabled = false;
            isPaused = true;
            await jobManager.PauseJobs();
            StartButton.IsEnabled = true;
        }

        private async void StartButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            if (isPaused)
            {
                isPaused = false;
                await jobManager.ResumeJobs();
            }
            else
            {
                isStarted = true;
                await imageMaker.LoadFolder();
                foreach (Job job in jobManager.PendingJobs)
                    JobsStackPanel.Children.Add(new JobControl(job));
                jobManager.StartJobs();
            }
            PauseButton.IsEnabled = true;
            CancelButton.IsEnabled = true;
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
