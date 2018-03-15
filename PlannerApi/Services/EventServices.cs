using System;
using events_planner.Services;
using events_planner.Models;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public class EventServices : IEventServices
    {
        private PlannerContext Context { get; set; }

        public EventServices(PlannerContext context) {
            Context = context;
        }

        public void RemoveAllEventCategoryReferencesFor(int categoryId) {
            EventCategory[] categories = Context.EventCategory
                                              .Where((EventCategory ec) => ec.CategoryId == categoryId)
                                              .ToArray();

            if (categories == null) { return; }

            Context.EventCategory.RemoveRange(categories);
            Context.SaveChanges();
        }
    }
}