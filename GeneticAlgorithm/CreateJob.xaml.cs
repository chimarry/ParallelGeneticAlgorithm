using GeneticAlgorithm.Logic;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using static GeneticAlgorithm.Logic.Job;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace GeneticAlgorithm
{
    public sealed partial class CreateJob : ContentDialog
    {
        private const string jobUnitSeparator = ";";
        private const string attributeSeparator = "-";
        private const int requestedNumberIndex = 0;
        private const int nameIndex = 1;

        private readonly Job.JobUnitUICallback jobUnitControlCallback;
        private readonly Job.JobUICallback jobControlCallback;
        private readonly Func<Job, Task> addJob;

        public CreateJob(Job.JobUnitUICallback jobUnitControlCallback, Job.JobUICallback jobControlCallback, Func<Job, Task> addJob)
        {
            this.InitializeComponent();
            for (int i = 0; i < 20; ++i)
                Parallelism.Items.Add(i);
            this.jobControlCallback = jobControlCallback;
            this.jobUnitControlCallback = jobUnitControlCallback;
            this.addJob = addJob;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                Regex createJobUnits = new Regex(@"^((\d)+-(\w)+\;)+$");
                Match match = createJobUnits.Match(JobUnits.Text);
                if (!match.Success)
                    throw new Exception(ExceptionContentDialog.InvalidDataForJob);
                string[] jobUnitStrings = JobUnits.Text.Split(jobUnitSeparator);
                List<JobUnit> jobUnits = new List<JobUnit>();
                foreach (string unit in jobUnitStrings)
                    jobUnits.Add(new JobUnit(unit.Split(attributeSeparator)[requestedNumberIndex], unit.Split(attributeSeparator)[nameIndex]));
                string identifier = Identifier.Text;
                int parallelism = (int)Parallelism.SelectedValue;
                Job job = new Job(jobUnits, identifier, parallelism, jobControlCallback, jobUnitControlCallback);
                await addJob.Invoke(job);
            }
            catch (Exception e)
            {
                await new ExceptionContentDialog(e.Message).ShowAsync();
            }
        }
    }
}
