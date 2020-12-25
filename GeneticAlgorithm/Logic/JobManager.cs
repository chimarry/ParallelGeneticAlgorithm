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

        private readonly SemaphoreSlim jobSemaphore = new SemaphoreSlim(Job.MaxLevelOfParallelism);

        private readonly JobCallback callback;

        private readonly JobUnitCallback jobUnitCallback;

        public bool Cancelled { get; set; }

        public JobManager(JobCallback callback, JobUnitCallback jobUnitCallback)
        {
            this.callback = callback;
            this.jobUnitCallback = jobUnitCallback;
        }


        public void ScheduleJob(Job job)
        {
            (job.Callback, job.JobUnitCallback) = (UpdateJobUI, UpdateJobUnitUI);
            lock (PendingJobs)
                PendingJobs.Enqueue(job);
        }

        // TODO: Catch exceptions
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
                        PendingJobs.Enqueue(Job.InitializeFromXml(jobConfiguration, UpdateJobUI, UpdateJobUnitUI));
                }
            }
        }

        public void StartJobs()
        {
            int numberOfJobs = PendingJobs.Count;
            Parallel.For(0, numberOfJobs, async (i) =>
                  {
                      await jobSemaphore.WaitAsync();
                      Job currentJob;
                      if (!Cancelled)
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
                  });
        }

        public Task PauseJobs()
        {
            throw new NotImplementedException();
        }

        public async Task CancelJobs()
        {
            Cancelled = true;
            foreach (Job job in ExecutingJobs)
                await job.Cancel();
            foreach (Job job in PendingJobs)
                await job.Cancel();
            ExecutingJobs.Clear();
            PendingJobs.Clear();
        }

        public Task ResumeJobs()
        {
            throw new NotImplementedException();
        }

        public Task StopJobs()
        {
            throw new NotImplementedException();
        }

        public async Task UpdateJobUI(string identifier, Job.Status status)
            => await callback(identifier, status);

        public async Task UpdateJobUnitUI(string identifier, Job.JobUnit jobUnit, Job.Status status)
            => await jobUnitCallback(identifier, jobUnit, status);
    }
}
