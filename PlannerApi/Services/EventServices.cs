using System;
using events_planner.Services;
using events_planner.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    public class EventServices : IEventServices
    {
        private PlannerContext Context { get; set; }

        public EventServices(PlannerContext context) {
            Context = context;
        }
    }
}