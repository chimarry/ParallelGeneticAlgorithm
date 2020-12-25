using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace GeneticAlgorithm
{
    public sealed partial class ExceptionContentDialog : ContentDialog
    {
        public const string InvalidFormatForJob = "Invalid job format.";
        public const string InvalidDataForJob = "Invalid data for a job.";
        public const string UnknownError = "Unknown error";

        public ExceptionContentDialog(string error = UnknownError)
        {
            this.InitializeComponent();
            ErrorDetailsTextBlock.Text = error;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
