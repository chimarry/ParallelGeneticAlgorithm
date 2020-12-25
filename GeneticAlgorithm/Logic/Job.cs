using GeneticAlgorithm.ExpressionTree;
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

        public bool IsFinished { get; set; }

        public bool IsPaused { get; set; }

        public bool IsStopped { get; set; }

        public SemaphoreSlim JobSemaphor { get; set; }

        private SemaphoreSlim jobUnitSemaphore;

        public Job(List<JobUnit> units, string identifier, int levelOfParallelism, JobUICallback jobUICallback, JobUnitUICallback jobUnitUICallback)
        {
            foreach (JobUnit unit in units)
                PendingJobUnits.Enqueue(unit);
            Id = identifier;
            Callback = jobUICallback;
            JobUnitCallback = jobUnitUICallback;
            RequestedLevelOfParallelism = levelOfParallelism < MaxLevelOfParallelismPerJob ? levelOfParallelism : MaxLevelOfParallelismPerJob;
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
            ImageMaker = imageMaker;
            await Callback(Id, Status.Started);
            int numberOfJobUnits = PendingJobUnits.Count;
            jobUnitSemaphore = new SemaphoreSlim(RequestedLevelOfParallelism);
            List<(string unitName, string expression)> results = new List<(string, string)>();
            List<Task> executionTasks = Enumerable.Range(0, numberOfJobUnits)
                                                  .Select(x => Task.Run(async () =>
                                                                     {
                                                                         await jobUnitSemaphore.WaitAsync();
                                                                         JobUnit jobUnit;
                                                                         lock (PendingJobUnits)
                                                                             jobUnit = PendingJobUnits.Dequeue();
                                                                         lock (ActiveJobUnits)
                                                                             ActiveJobUnits.Add(jobUnit);
                                                                         await JobUnitCallback(Id, jobUnit, Status.Started);
                                                                         (string, string) result = jobUnit.Execute();
                                                                         lock (results)
                                                                             results.Add(result);
                                                                         lock (ActiveJobUnits)
                                                                             ActiveJobUnits.Remove(jobUnit);
                                                                         await JobUnitCallback(Id, jobUnit, Status.Finished);
                                                                         jobUnitSemaphore.Release();
                                                                     })).ToList();
            await Task.WhenAll(executionTasks);
            results.AsParallel().ForAll(async x => await ImageMaker.SaveResultAsImage(Id, x.unitName, x.expression));
            // Save as images
            await Callback(Id, Status.Finished);
        }

        public async Task Pause()
        {

        }

        public async Task Resume()
        {

        }

        public async Task Stop()
        {

        }

        public async Task Finish()
        {

        }
        public class JobUnit
        {
            private readonly GeneticAlgorithmExecutor geneticAlgorithmExecutor;

            public int RequestedNumber { get; set; }

            public string Name { get; set; }

            public JobUnit(string number, string name)
            {
                RequestedNumber = int.Parse(number);
                Name = name;
                ThreadSafeRandom threadSafeRandom = new ThreadSafeRandom();
                GeneticAlgorithmConfiguration geneticAlgorithmConfiguration = new GeneticAlgorithmConfiguration(RequestedNumber)
                {
                    Operands = new int[] { 10, 1, 28, 3, 14, 80 }
                };
                geneticAlgorithmExecutor = new GeneticAlgorithmExecutor(geneticAlgorithmConfiguration);
            }

            public (string, string) Execute()
            {
                /*
                * Treba da izvrsava geneticki algoritam. Treba se moci pauzirati, i treba moci nastaviti izvrsavanje.
                * Treba da obavijesti kada je zavrsio sa izvrsavanjem, i da rezultat sacuva kao sliku.
                * Treba da azurira UI shodno datim aktivnostima.
                */
                MathExpressionTree tree = geneticAlgorithmExecutor.Execute();

                return (Name, tree?.ToString() ?? $"Result {RequestedNumber} was not found");
            }
        }
    }
}
