using System.Threading.Tasks;

namespace GeneticAlgorithm.Logic
{
    public interface IJobManager
    {
        void ScheduleJob(Job job);

        Task LoadJobs();
    }
}
