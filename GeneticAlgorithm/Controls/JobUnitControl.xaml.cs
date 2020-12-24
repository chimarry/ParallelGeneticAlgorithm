using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using static GeneticAlgorithm.Logic.Job;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace GeneticAlgorithm.Controls
{
    public sealed partial class JobUnitControl : UserControl
    {
        public JobUnit JobUnit { get; set; }

        public JobUnitControl(JobUnit jobUnit)
        {
            this.InitializeComponent();
            JobUnit = jobUnit;
        }

        public async Task UpdateStatus(Status status)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                JobUnitStatus.Text = status.ToString();
                switch (status)
                {
                    case Status.Started:
                    case Status.Resumed:
                        JobUnitProgressRing.IsActive = true; break;
                    default: JobUnitProgressRing.IsActive = false; break;
                }
            });
        }
    }
}
