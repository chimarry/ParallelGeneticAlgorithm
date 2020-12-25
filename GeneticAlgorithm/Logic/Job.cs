using GeneticAlgorithm.ExpressionTree;
using GeneticAlgorithm.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GeneticAlgorithm.Logic
{
    public class Job
    {
        public delegate Task JobUICallback(string identifier, Status status);
        public delegate Task JobUnitUICallback(string identifier, JobUnit jobUnit, Status status);

        public enum Status { Pending, Started, Paused, Resumed, Cancelled, Finished }

        public const int MaxLevelOfParallelismPerJob = 3;
        public const int MaxLevelOfParallelism = 2;

        public ImageMaker ImageMaker { get; set; }

        public string Id { get; set; }

        public Status CurrentStatus { get; set; }

        public int RequestedLevelOfParallelism { get; set; }

        public Queue<JobUnit> PendingJobUnits { get; set; } = new Queue<JobUnit>();

        public List<JobUnit> ActiveJobUnits { get; set; } = new List<JobUnit>();

        public JobUICallback Callback { get; set; }

        public JobUnitUICallback JobUnitCallback { get; set; }

        public bool IsFinished { get => CurrentStatus == Status.Finished; }

        public bool IsPaused { get; set; }

        public bool IsCancelled { get => CurrentStatus == Status.Cancelled; }

        public SemaphoreSlim JobSemaphor { get; set; }

        private CancellationTokenSource cancellationTokenSource;

        private SemaphoreSlim jobUnitSemaphore;

        public Job(List<JobUnit> units, string identifier, int levelOfParallelism, JobUICallback jobUICallback, JobUnitUICallback jobUnitUICallback)
        {
            foreach (JobUnit unit in units)
                PendingJobUnits.Enqueue(unit);
            Id = identifier;
            Callback = jobUICallback;
            JobUnitCallback = jobUnitUICallback;
            RequestedLevelOfParallelism = levelOfParallelism < MaxLevelOfParallelismPerJob ? levelOfParallelism : MaxLevelOfParallelismPerJob;
            jobUnitSemaphore = new SemaphoreSlim(RequestedLevelOfParallelism);
            cancellationTokenSource = new CancellationTokenSource();
            CurrentStatus = Status.Pending;
        }

        public static Job InitializeFromXml(XDocument xmlDocument, JobUICallback callback, JobUnitUICallback jobUICallback)
        {
            XElement paralelism = xmlDocument.Descendants("Parallelism").First();
            XElement id = xmlDocument.Descendants("Id").First();
            IEnumerable<XElement> jobUnits = xmlDocument.Descendants("JobUnit");
            List<JobUnit> units = new List<JobUnit>();

            foreach (XElement jobUnit in jobUnits)
                units.Add(new JobUnit(jobUnit.Value, jobUnit.Attribute("name").Value));
            return new Job(units, id.Value, int.Parse(paralelism.Value), callback, jobUICallback);
        }


        public async Task Execute(ImageMaker imageMaker)
        {
            if (IsCancelled)
                return;
            CurrentStatus = Status.Started;
            await Callback(Id, CurrentStatus);

            ImageMaker = imageMaker;

            int numberOfJobUnits = PendingJobUnits.Count;
            List<(string unitName, string expression)> results = new List<(string, string)>();
            List<Task> executionTasks = Enumerable.Range(0, numberOfJobUnits)
                                                  .Select(x => Task.Run(async () =>
                                                                     {
                                                                         await jobUnitSemaphore.WaitAsync();
                                                                         JobUnit jobUnit;
                                                                         if (PendingJobUnits.NotEmpty())
                                                                         {
                                                                             lock (PendingJobUnits)
                                                                                 jobUnit = PendingJobUnits.Dequeue();
                                                                             lock (ActiveJobUnits)
                                                                                 ActiveJobUnits.Add(jobUnit);
                                                                             await JobUnitCallback(Id, jobUnit, CurrentStatus);
                                                                             (string, string) result = await jobUnit.Execute(cancellationTokenSource);
                                                                             if (!IsCancelled)
                                                                             {
                                                                                 lock (ActiveJobUnits)
                                                                                     ActiveJobUnits.Remove(jobUnit);
                                                                                 lock (results)
                                                                                     results.Add(result);
                                                                                 await JobUnitCallback(Id, jobUnit, Status.Finished);
                                                                             }
                                                                             jobUnitSemaphore.Release();
                                                                         }
                                                                     })).ToList();
            await Task.WhenAll(executionTasks);
            // Save finished results as images
            results.AsParallel()
                   .ForAll(async x => await ImageMaker.SaveResultAsImage(Id, x.unitName, x.expression));

            if (!IsCancelled)
            {
                CurrentStatus = Status.Finished;
                await Callback(Id, CurrentStatus);
            }
        }

        public async Task Pause()
        {
            CurrentStatus = Status.Paused;

            // Notify UI about the change
            await Callback(Id, Status.Paused);
            ActiveJobUnits.AsParallel().ForAll(async jobUnit =>
            {
                await jobUnit.Pause();
                await JobUnitCallback(Id, jobUnit, Status.Paused);
            });
            PendingJobUnits.AsParallel().ForAll(async jobUnit => await JobUnitCallback(Id, jobUnit, Status.Paused));
        }

        public async Task Resume()
        {
            CurrentStatus = Status.Resumed;
            await Callback(Id, Status.Resumed);
            ActiveJobUnits.AsParallel().ForAll(async jobUnit =>
            {
                jobUnit.Resume();
                await JobUnitCallback(Id, jobUnit, Status.Resumed);
            });
            PendingJobUnits.AsParallel().ForAll(async jobUnit => await JobUnitCallback(Id, jobUnit, Status.Resumed));
        }

        public async Task Cancel()
        {
            CurrentStatus = Status.Cancelled;
            cancellationTokenSource.Cancel();

            // Notify UI about the change
            await Callback(Id, Status.Cancelled);
            ActiveJobUnits.AsParallel().ForAll(async jobUnit => await JobUnitCallback(Id, jobUnit, Status.Cancelled));
            PendingJobUnits.AsParallel().ForAll(async jobUnit => await JobUnitCallback(Id, jobUnit, Status.Cancelled));

            ActiveJobUnits.Clear();
            PendingJobUnits.Clear();
        }

        public class JobUnit
        {
            private GeneticAlgorithmExecutor geneticAlgorithmExecutor;
            public CancellationTokenSource CancellationTokenSource { get; set; }

            public int RequestedNumber { get; set; }

            public string Name { get; set; }

            private SemaphoreSlim pauseSemaphore = new SemaphoreSlim(1);

            public JobUnit(string number, string name)
            {
                RequestedNumber = int.Parse(number);
                Name = name;
            }

            public async Task<(string, string)> Execute(CancellationTokenSource cancellationTokenSource)
            {
                this.CancellationTokenSource = cancellationTokenSource;
                ThreadSafeRandom threadSafeRandom = new ThreadSafeRandom();
                GeneticAlgorithmConfiguration geneticAlgorithmConfiguration = new GeneticAlgorithmConfiguration(RequestedNumber)
                {
                    Operands = new int[] { 10, 1, 28, 3, 14, 80 }
                };
                geneticAlgorithmExecutor = new GeneticAlgorithmExecutor(geneticAlgorithmConfiguration, this.CancellationTokenSource.Token, pauseSemaphore);
                MathExpressionTree tree = await geneticAlgorithmExecutor.Execute();
                return (Name, tree?.ToString() ?? $"Result {RequestedNumber} was not found");
            }

            public async Task Pause()
            {
                await pauseSemaphore.WaitAsync();
            }

            public void Resume()
            {
                pauseSemaphore.Release();
            }
        }
    }
}
