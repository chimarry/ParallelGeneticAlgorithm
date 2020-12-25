using GeneticAlgorithm.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static GeneticAlgorithm.Logic.Job;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace GeneticAlgorithm.Controls
{
    public sealed partial class JobControl : UserControl
    {
        public Job Job { get; set; }

        public JobControl(Job job)
        {
            this.InitializeComponent();
            Job = job;
            Status.Text = job.CurrentStatus.ToString();
            JobName.Text = Job.Id;
            AddJobUnits();
        }

        private void AddJobUnits()
        {
            foreach (JobUnit jobUnit in Job.PendingJobUnits)
                JobUnitStackPanel.Children.Add(new JobUnitControl(jobUnit));
        }

        public override bool Equals(object obj)
             => obj is JobControl control && control.Job.Id == Job.Id;

        public override int GetHashCode()
            => Job.Id.GetHashCode();


        public async Task UpdateStatus(Status newStatus)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                Status.Text = newStatus.ToString();
            });
        }

        public async Task UpdateJobUnit(JobUnit jobUnit, Status status)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
             {
                 await JobUnitStackPanel.Children
                                        .Select(x => x as JobUnitControl)
                                        .First(x => x.JobUnit.Name == jobUnit.Name)
                                        .UpdateStatus(status);
             });
        }
        // Method for update
    }
}
