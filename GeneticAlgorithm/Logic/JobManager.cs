using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace GeneticAlgorithm.Logic
{
    public class JobManager : IJobManager
    {
        private readonly ImageMaker imageMaker = new ImageMaker();

        private readonly SemaphoreSlim jobSemaphore = new SemaphoreSlim(Job.MaxLevelOfParallelism, Job.MaxLevelOfParallelism);

        private Queue<Job> PendingJobs { get; set; } = new Queue<Job>();

        private List<Job> ExecutingJobs { get; set; } = new List<Job>();

        public void ScheduleJob(Job job)
        {
            lock (PendingJobs)
                PendingJobs.Enqueue(job);
        }

        public async Task LoadJobs()
        {
            FolderPicker openFolderPicker = new FolderPicker()
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };
            openFolderPicker.FileTypeFilter.Add(".ga");
            StorageFolder folder = await openFolderPicker.PickSingleFolderAsync();
            IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();
            foreach (StorageFile file in files)
            {
                using (Stream stream = await file.OpenStreamForReadAsync())
                {
                    XDocument jobConfiguration = XDocument.Load(stream);
                    lock (PendingJobs)
                        PendingJobs.Enqueue(Job.InitializeFromXml(jobConfiguration));
                }
            }
        }

        private async void ExecuteJob()
        {
            lock (ExecutingJobs)
            {
                if (ExecutingJobs.Count < Job.MaxLevelOfParallelism)
                {
                    Job jobToBeExecuted;
                    lock (PendingJobs)
                        jobToBeExecuted = PendingJobs.Dequeue();
                    lock (ExecutingJobs)
                        ExecutingJobs.Add(jobToBeExecuted);
                    Task startedTask = Task.Factory.StartNew(() => jobToBeExecuted.Execute());
                }
            }
        }
    }
}
