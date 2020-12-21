using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace GeneticAlgorithm
{
    public class Job
    {
        public int RequestedLevelOfParallelism { get; set; }

        public Queue<JobUnit> PendingJobUnits { get; set; } = new Queue<JobUnit>();

        public List<JobUnit> ActiveJobUnits { get; set; } = new List<JobUnit>();

        public List<JobUnit> FinishedJobUnits { get; set; } = new List<JobUnit>();


        public bool IsFinished { get; set; }

        public bool IsPaused { get; set; }

        public bool IsStopped { get; set; }

        public static Job InitializeFromXML(XDocument xmlDocument)
        {
            Job job = new Job();
            XElement paralelism = xmlDocument.Descendants("Parallelism").First();

            IEnumerable<XElement> jobUnits = xmlDocument.Descendants("JobUnit");
            foreach (XElement jobUnit in jobUnits)
                job.PendingJobUnits.Enqueue(new JobUnit(jobUnit.Value));

            job.RequestedLevelOfParallelism = int.Parse(paralelism.Value);
            return job;
        }

        public class JobUnit
        {
            public int RequestedNumber { get; set; }

            public JobUnit() { }

            public JobUnit(string number)
            {
                RequestedNumber = int.Parse(number);
            }
        }
    }
}
