using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;
using events_planner.Models;
using Microsoft.EntityFrameworkCore;
using events_planner.Scheduler.Extensions;

namespace events_planner.Scheduler {
    
    public class EventUpdateStatus : IScheduledTask {
        
        public string Schedule => "* * * * *";

        public string ConnectionString { get; }

        private Expression<Func<Event, bool>> Condition => arg =>
            arg.Status != Status.DRAFT && (
                (arg.Status != Status.SUBSCRIPTION && arg.HasSubscriptionWindow() && arg.SubscribtionOpen()) || // From any to SUBSCRIPTION
                ((arg.Status == Status.SUBSCRIPTION || arg.Status == Status.PENDING) && arg.EndAt <= DateTime.Now) || // From SUBSCRIPTION to INCOMING
                (arg.Status == Status.PENDING && arg.StartAt == null && arg.EndAt == null) || // From PENDING to INCOMING
                (arg.Status != Status.ONGOING && arg.OpenAt <= DateTime.Now && arg.CloseAt >= DateTime.Now) || // From any to ONGOING
                (arg.Status != Status.DONE && arg.CloseAt <= DateTime.Now) // From any to DONE
            );


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

                bool shouldBeProc = await db.Event.AnyAsync(Condition, cancellationToken);
                
                if (shouldBeProc) {
                    Event[] @events = await db.Event.Where(Condition).ToArrayAsync(cancellationToken);
                    foreach (var @event in @events) {
                        
                        var from = @event.Status;
                        
                        if (@event.HasSubscriptionWindow() && @event.SubscribtionOpen()) {
                            @event.Status = Status.SUBSCRIPTION;
                        } else if (@event.OnGoingWindow()) {
                            @event.Status = Status.ONGOING;
                        } else if (@event.CloseAt >= DateTime.Now && 
                                   (!@event.HasSubscriptionWindow() || @event.EndAt <= DateTime.Now)) {
                            @event.Status = Status.INCOMING;
                        } else if (@event.CloseAt <= DateTime.Now) {
                            @event.Status = Status.DONE;
                        } else {
                            @event.Status = Status.PENDING;
                        }
                        
                        this.PrettyPrint($"Event: {@event.Id} is updating from: [{from}] to: [{@event.Status}] ");
                    }

                    try {
                        db.Event.UpdateRange(@events);
                        await db.SaveChangesAsync(cancellationToken);
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