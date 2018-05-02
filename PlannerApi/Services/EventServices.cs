using System;
using events_planner.Services;
using events_planner.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection {

    public partial class EventServices : IEventServices {

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

        public async Task<Event[]> GetEVentsFromIds(int[] ids) {
            return await Context.Event.Where((arg) => ids.Contains(arg.Id)).ToArrayAsync();
        }

        public async Task<Event> GetEventByIdAsync(int id) {
            return await Context.Event.FirstOrDefaultAsync<Event>((Event @event) => @event.Id == id);
        }
    }

    #region QUERIES

    public partial class EventServices {

        /// <summary>
        /// Get the events froms the date in string.
        /// </summary>
        /// <param name="query">Query Event / childs.</param>
        /// <param name="date">From Date.</param>
        /// <typeparam name="T">The 1st type parameter as Event.</typeparam>
        public void FromDate<T>(ref IQueryable<T> query,
                                         string date) where T : Event {
            query = query.Where(cc => cc.OpenAt >= DateTime.Parse(date));
        }

        /// <summary>
        /// Get th events before then end date specified
        /// </summary>
        /// <param name="query">Query Event / childs.</param>
        /// <param name="date">Date To.</param>
        /// <typeparam name="T">The 1st type parameter as Event.</typeparam>
        public void ToDate<T>(ref IQueryable<T> query,
                              string date) where T : Event {
            query = query.Where(cc => cc.EndAt <= DateTime.Parse(date));
        }

        /// <summary>
        /// Events that end after the current date
        /// </summary>
        /// <param name="query">Query Event / Childs.</param>
        /// <typeparam name="T">The 1st type parameter as event.</typeparam>
        public void EndAfterToday<T>(ref IQueryable<T> query) where T : Event {
            query = query.Where((arg) => arg.EndAt >= DateTime.Now);
        }

        /// <summary>
        /// Includes the images.
        /// </summary>
        /// <param name="query">Query Event / childs.</param>
        /// <typeparam name="T">The 1st type parameter as event.</typeparam>
        public void IncludeImages<T>(ref IQueryable<T> query) where T : Event {
            query = query.Include(ev => ev.Images);
        }

        public IQueryable<Event> GetParticipedEvents(int userId) {
            return Context.Booking
                .Include(inc => inc.Event)
                .Where(arg => arg.UserId == userId && arg.Present == true)
                .Select(arg => arg.Event);
        }
    }

    #endregion
}