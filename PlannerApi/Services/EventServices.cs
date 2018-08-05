using System;
using events_planner.Services;
using events_planner.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Extensions.DependencyInjection {

    public partial class EventServices : IEventServices {

        private PlannerContext Context { get; set; }

        private ICategoryServices CategoryServices { get; set; }

        public EventServices(PlannerContext context,
                             ICategoryServices categoryServices) {
            Context = context;
            CategoryServices = categoryServices;
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

        public async Task<Price> GetPriceForRoleAsync(int roleId, int eventId) {
            Price price;
            price = await Context.Price
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(p => p.EventId == eventId &&
                                                      p.RoleId == roleId);
            return price;
        }

        public async Task<bool> IsEventBooked(int userId, int eventId) {
            return await Context.Booking
                                .AnyAsync(prop => prop.EventId == eventId &&
                                          prop.UserId == userId);
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
            query = query.Where(cc => cc.CloseAt <= DateTime.Parse(date));
        }

        /// <summary>
        /// Events that end after the current date
        /// </summary>
        /// <param name="query">Query Event / Childs.</param>
        /// <typeparam name="T">The 1st type parameter as event.</typeparam>
        public void EndAfterToday<T>(ref IQueryable<T> query) where T : Event {
            query = query.Where((arg) => arg.CloseAt >= DateTime.Now);
        }

        /// <summary>
        /// Events which arn't obsolete.
        /// </summary>
        /// <param name="query">Query Event / Childs.</param>
        /// <typeparam name="T">The 1st type parameter as event.</typeparam>
        public void NonObsolete<T>(ref IQueryable<T> query) where T : Event {
            string[] status = { Status.PENDING, Status.SUBSCRIPTION, Status.INCOMING, Status.ONGOING };
            query = query.Where((arg) => status.Contains(arg.Status));
        }

        /// <summary>
        /// Includes the images.
        /// </summary>
        /// <param name="query">Query Event / childs.</param>
        /// <typeparam name="T">The 1st type parameter as event.</typeparam>
        public void IncludeImages<T>(ref IQueryable<T> query) where T : Event {
            query = query.Include(ev => ev.Images);
        }

        public void IncludeModerators<T>(ref IQueryable<T> query) where T : Event {
            query = query.Include(arg => arg.Moderators)
                         .ThenInclude(arg => arg.User);
        }

        public void IncludeCategories<T>(ref IQueryable<T> query) where T : Event {
            query = query.Include(arg => arg.EventCategory)
                         .ThenInclude(arg => arg.Category);
        }

        public void LimitElements<T>(ref IQueryable<T> query,
                                     string limit) where T : Event {
            Match match = (new Regex("[0-9]+")).Match(limit);

            if (!String.IsNullOrEmpty(match.Value)) {
                query = query.Take(int.Parse(match.Value));
            }
        }

        public void IncludePrices<T>(ref IQueryable<T> query) where T : Event {
            query = query.Include(arg => arg.Prices);
        }

        public void FilterByCategories<T>(ref IQueryable<T> query,
                                                string categories) where T : Event {
            int[] eventIds = CategoryServices.GetCategoriesFromString(categories)
                                           .Select((arg) => arg.EventId)
                                           .ToArray();

            query = query.Where((arg) => eventIds.Contains(arg.Id))
                         .Include(arg => arg.EventCategory)
                         .ThenInclude(arg => arg.Category);
        }

        public IQueryable<Event> GetParticipedEvents(int userId) {
            return Context.Booking
                .Include(inc => inc.Event)
                .Where(arg => arg.UserId == userId && arg.Present == true)
                .Select(arg => arg.Event);
        }

        public bool IsTimeWindowValid(ref Event @event) {
            bool valid = true;

            if (@event.StartAt != null)
                valid &= (@event.CloseAt >= @event.StartAt);
            if (@event.EndAt != null)
                valid &= (@event.CloseAt >= @event.EndAt);
            if (@event.StartAt != null && @event.EndAt != null)
                valid &= (@event.StartAt < @event.EndAt);

            valid &= (@event.CloseAt >= @event.OpenAt);

            return valid;
        }

    }

    #endregion
}