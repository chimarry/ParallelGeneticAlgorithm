using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace GeneticAlgorithm.Controls
{
    public sealed partial class JobListElement : UserControl
    {
        public JobListElement(string name, string id)
        {
            this.InitializeComponent();
            JobName.Text = name;
            JobId.Text = id;
        }

        public override bool Equals(object obj)
        {
            return obj is string el && el == JobId.Text;
        }
    }
}
