using System.Threading.Tasks;
using events_planner.Models;
using System.Linq;
using events_planner.Deserializers;

namespace events_planner.Services {
    public interface IEventServices {

        void Create(EventDeserializer eventDsl, out Event @event);

        void Update(EventUpdatableDeserializer eventDsl, Event @event);

        void UpdateBookingsValidates(EventUpdatableDeserializer eventDsl, Event @event);

        void RemoveAllEventCategoryReferencesFor(int categoryId);

        Task<Event[]> GetEVentsFromIds(int[] ids);

        Task<Event> GetEventByIdAsync(int id);

        EventRole[] GetEventRolesFrom(string[] list, int event_id);

        void RemoveAllEventRoles(int event_id);

        void AddAndRemoveEventRoles(string[] adds, string[] removes, Event @event);

        Task<Price> GetPriceForRoleAsync(int roleId, int eventId);

        Task<bool> IsEventBooked(int userId, int eventId);

        #region QUERIES

        void FromDate<T>(ref IQueryable<T> query, string date) where T : Event;

        void ToDate<T>(ref IQueryable<T> query, string date) where T : Event;

        void IncludeImages<T>(ref IQueryable<T> query) where T : Event;

        void EndAfterToday<T>(ref IQueryable<T> query) where T : Event;

        void NonObsolete<T>(ref IQueryable<T> query) where T : Event;

        void IncludeModerators<T>(ref IQueryable<T> query) where T : Event;

        void IncludeCategories<T>(ref IQueryable<T> query) where T : Event;

        void IncludePrices<T>(ref IQueryable<T> query) where T : Event;

        void LimitElements<T>(ref IQueryable<T> query, string limit) where T : Event;

        void FilterByCategories<T>(ref IQueryable<T> query, string categories) where T : Event;

        IQueryable<Event> GetParticipedEvents(int userId);

        bool IsTimeWindowValid(ref Event @event);

        #endregion
    }
}