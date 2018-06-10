using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;
using events_planner.Models;
using Microsoft.EntityFrameworkCore;
using events_planner.Scheduler.Extensions;
using Microsoft.Extensions.Configuration;

namespace events_planner.Scheduler {
    
    public class EventUpdateStatus : IScheduledTask {
        
        public string Schedule => "* * * * *";

        public string ConnectionString { get; }

        private Expression<Func<Event, bool>> Condition => arg =>
                    (arg.Status == Status.PENDING && arg.OpenAt <= DateTime.Now && arg.CloseAt >= DateTime.Now) ||
                    (arg.Status == Status.ONGOING && arg.CloseAt <= DateTime.Now);


        public EventUpdateStatus(string dbConectionString) {
            ConnectionString = dbConectionString;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken) {
            if (cancellationToken.IsCancellationRequested) { return; }

            this.PrettyPrint("Start fetching Events Data..", ConsoleColor.Green);

            var optionsBuilder = new DbContextOptionsBuilder<PlannerContext>();
            optionsBuilder.UseMySql(ConnectionString);

            using (var db = new PlannerContext(optionsBuilder.Options)) {
                this.PrettyPrint("Database connection established");

                bool shouldBeProc = await db.Event.AnyAsync(Condition);
                
                if (shouldBeProc) {
                    Event[] @events = await db.Event.Where(Condition).ToArrayAsync();

                    foreach (var ev in @events) {
                        this.PrettyPrint($"Event: {ev.Id} is updating..");

                        if (ev.Status == Status.PENDING) {
                            ev.Status = Status.ONGOING;
                        } else {
                            ev.Status = Status.DONE;
                        }
                    }

                    try {
                        db.Event.UpdateRange(@events);
                        await db.SaveChangesAsync();
                    } catch (DbUpdateException exception) {
                        this.PrettyPrint($"Error: {exception.Message}", ConsoleColor.Red);
                        throw exception;
                    }

                    this.PrettyPrint($"Event Updated: {@events.Count()}", ConsoleColor.Green);
                } else {
                    this.PrettyPrint("No Event available for update", ConsoleColor.Cyan);
                }
            }
        }

    }
}