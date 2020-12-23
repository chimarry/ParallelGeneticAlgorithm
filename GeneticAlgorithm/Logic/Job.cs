using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        public List<JobUnit> ActiveJobUnits { get; set; } = new List<JobUnit>();

        public List<JobUnit> FinishedJobUnits { get; set; } = new List<JobUnit>();


        public bool IsFinished { get; set; }

        public bool IsPaused { get; set; }

        public bool IsStopped { get; set; }

        public Semaphore JobSemaphor { get; set; }

        private readonly Semaphore jobUnitSemaphore;

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
            jobUnitSemaphore = new Semaphore(initialThreadCount, MaxLevelOfParallelismPerJob);
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

        public class JobUnit
        {
            public int RequestedNumber { get; set; }

            public string Name { get; set; }

            public JobUnit() { }

            public JobUnit(string number, string name)
            {
                RequestedNumber = int.Parse(number);
                Name = name;
            }
        }
    }
}
