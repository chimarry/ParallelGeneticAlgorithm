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
        public static int MaxLevelOfParallelismPerJob = 3;
        public static int MaxLevelOfParallelism = 2;

        public string Id { get; set; }

        public int RequestedLevelOfParallelism { get; set; }

        public Queue<JobUnit> PendingJobUnits { get; set; } = new Queue<JobUnit>();

        public List<Task> ActiveJobUnits { get; set; } = new List<Task>();

        public List<JobUnit> FinishedJobUnits { get; set; } = new List<JobUnit>();


        public bool IsFinished { get; set; }

        public bool IsPaused { get; set; }

        public bool IsStopped { get; set; }

        public SemaphoreSlim JobSemaphor { get; set; }

        private readonly SemaphoreSlim jobUnitSemaphore;

        private Job()
        {

        }

        public Job(List<JobUnit> units, string identifier, int levelOfParallelism)
        {
            foreach (JobUnit unit in units)
                PendingJobUnits.Enqueue(unit);
            Id = identifier;
            RequestedLevelOfParallelism = levelOfParallelism;
            int initialThreadCount = RequestedLevelOfParallelism < MaxLevelOfParallelismPerJob ? RequestedLevelOfParallelism : MaxLevelOfParallelismPerJob;
            jobUnitSemaphore = new SemaphoreSlim(initialThreadCount, MaxLevelOfParallelismPerJob);
        }

        public static Job InitializeFromXml(XDocument xmlDocument)
        {
            Job job = new Job();
            XElement paralelism = xmlDocument.Descendants("Parallelism").First();
            XElement id = xmlDocument.Descendants("Id").First();
            IEnumerable<XElement> jobUnits = xmlDocument.Descendants("JobUnit");
            foreach (XElement jobUnit in jobUnits)
                job.PendingJobUnits.Enqueue(new JobUnit(jobUnit.Value, jobUnit.Attribute("name").Value));
            job.Id = id.Value;
            job.RequestedLevelOfParallelism = int.Parse(paralelism.Value);
            return job;
        }


        public async Task Execute()
        {
            await JobSemaphor.WaitAsync();
            while (PendingJobUnits.NotEmpty())
            {
                if (ActiveJobUnits.Count < RequestedLevelOfParallelism)
                {
                    JobUnit jobUnit;
                    lock (PendingJobUnits)
                        jobUnit = PendingJobUnits.Dequeue();
                    Task startedTask = Task.Factory.StartNew(async () => await jobUnit.Execute());
                    lock (ActiveJobUnits)
                        ActiveJobUnits.Add(startedTask);
                }
            }
            // Treba da zapocne svoje izvrsavanje
            // Treba da pokrene svoje podzadatke, paralelno, na onoliko tredova koliko se specifikovano.
            // Kada se zavrsi jedan zadatak, poziva se drugi
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

            public async Task Execute()
            {
                /*
                * Treba da izvrsava geneticki algoritam. Treba se moci pauzirati, i treba moci nastaviti izvrsavanje.
                * Treba da obavijesti kada je zavrsio sa izvrsavanjem, i da rezultat sacuva kao sliku.
                * Treba da azurira UI shodno datim aktivnostima.
                */
                MathExpressionTree tree = geneticAlgorithmExecutor.Execute();
            }
        }
    }
}
