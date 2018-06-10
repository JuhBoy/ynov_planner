using System.Threading;
using System.Threading.Tasks;

namespace events_planner.Scheduler
{
    public interface IScheduledTask
    {
        /// <summary>
        /// Gets the schedule with Cron Format * * * * *.
        /// </summary>
        /// <value>Return The schedule.</value>
        string Schedule { get; }

        /// <summary>
        /// Execute the core operation, Execute Async is derived
        /// from the abstract class <see cref="HostedService"/>
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}