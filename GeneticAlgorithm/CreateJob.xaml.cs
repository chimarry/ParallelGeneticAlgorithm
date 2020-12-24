using GeneticAlgorithm.Logic;
using System.Collections.Generic;
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

        private readonly JobManager jobManager;

        public CreateJob(JobManager jobManager)
        {
            this.InitializeComponent();
            for (int i = 0; i < 20; ++i)
                Parallelism.Items.Add(i);
            this.jobManager = jobManager;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string[] jobUnitStrings = JobUnits.Text.Split(jobUnitSeparator);
            List<JobUnit> jobUnits = new List<JobUnit>();
            foreach (string unit in jobUnitStrings)
                jobUnits.Add(new JobUnit(unit.Split(attributeSeparator)[requestedNumberIndex], unit.Split(attributeSeparator)[nameIndex]));
            string identifier = Identifier.Text;
            int parallelism = (int)Parallelism.SelectedValue;
           // jobManager.ScheduleJob(new Job(jobUnits, identifier, parallelism));
        }
    }
}
