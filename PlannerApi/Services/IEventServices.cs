using System.Threading.Tasks;
using System.Collections.Generic;
using events_planner.Models;
using System.Linq;

namespace events_planner.Services {
    public interface IEventServices {
        
        void RemoveAllEventCategoryReferencesFor(int categoryId);

        Task<Event[]> GetEVentsFromIds(int[] ids);

        Task<Event> GetEventByIdAsync(int id);

        Task<Price> GetPriceForRoleAsync(int roleId, int eventId);

        Task<bool> IsEventBooked(int userId, int eventId);

        #region QUERIES

        void FromDate<T>(ref IQueryable<T> query, string date) where T : Event;

        void ToDate<T>(ref IQueryable<T> query, string date) where T : Event;

        void IncludeImages<T>(ref IQueryable<T> query) where T : Event;

        void EndAfterToday<T>(ref IQueryable<T> query) where T : Event;

        void IncludeModerators<T>(ref IQueryable<T> query) where T : Event;

        void IncludeCategories<T>(ref IQueryable<T> query) where T : Event;

        void LimitElements<T>(ref IQueryable<T> query, string limit) where T : Event;
        
        IQueryable<Event> GetParticipedEvents(int userId);

        #endregion
    }
}