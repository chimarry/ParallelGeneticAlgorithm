using GeneticAlgorithm.Util;
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
    public class JobManager
    {
        public delegate Task JobCallback(string identifier, Job.Status status);
        public delegate Task JobUnitCallback(string identifier, Job.JobUnit jobUnit, Job.Status status);

        public Queue<Job> PendingJobs { get; set; } = new Queue<Job>();

        public List<Job> ExecutingJobs { get; set; } = new List<Job>();

        public ImageMaker ImageMaker { get; set; }

        public bool Cancelled { get; set; }

        private readonly SemaphoreSlim jobSemaphore = new SemaphoreSlim(Job.MaxLevelOfParallelism);

        private JobCallback callback;

        private JobUnitCallback jobUnitCallback;

        public JobManager(JobCallback callback, JobUnitCallback jobUnitCallback)
        {
            this.callback = callback;
            this.jobUnitCallback = jobUnitCallback;
        }

        public void AddJob(Job job)
        {
            if (!PendingJobs.Contains(job))
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
                Job job = await ParseJobFromFile(file);
                AddJob(job);
            }
        }

        public async Task<Job> ParseJobFromFile(StorageFile file)
        {
            using (Stream stream = await file.OpenStreamForReadAsync())
            {
                XDocument jobConfiguration = XDocument.Load(stream);
                Job job = Job.InitializeFromXml(jobConfiguration, UpdateJobUI, UpdateJobUnitUI);
                return job;
            }
        }

        public async Task StartJob()
        {
            await jobSemaphore.WaitAsync();
            Job currentJob;
            if (PendingJobs.NotEmpty() && !Cancelled)
            {
                lock (PendingJobs)
                    currentJob = PendingJobs.Dequeue();
                lock (ExecutingJobs)
                    ExecutingJobs.Add(currentJob);
                await currentJob.Execute(ImageMaker);
                if (!Cancelled)
                    lock (ExecutingJobs)
                        ExecutingJobs.Remove(currentJob);
                jobSemaphore.Release();
            }
        }

        public void StartJobs()
        {
            int numberOfJobs = PendingJobs.Count;
            Parallel.For(0, numberOfJobs, async (i) => await StartJob());
        }

        public async Task PauseJobs()
        {
            foreach (Job job in ExecutingJobs)
                await job.Pause();
        }

        public async Task CancelJobs()
        {
            Cancelled = true;
            foreach (Job job in ExecutingJobs)
                await job.Cancel();
            foreach (Job job in PendingJobs)
                await job.Cancel();
        }

        public async Task ResumeJobs()
        {
            foreach (Job job in ExecutingJobs)
                await job.Resume();
        }

        public async Task UpdateJobUI(string identifier, Job.Status status)
            => await callback(identifier, status);

        public async Task UpdateJobUnitUI(string identifier, Job.JobUnit jobUnit, Job.Status status)
            => await jobUnitCallback(identifier, jobUnit, status);
    }
}
