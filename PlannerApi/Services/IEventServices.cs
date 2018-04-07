using System.Threading.Tasks;
using System.Collections.Generic;
using events_planner.Models;

namespace events_planner.Services {
    public interface IEventServices {
        void RemoveAllEventCategoryReferencesFor(int categoryId);
        Task<Event[]> GetEVentsFromIds(int[] ids);
    }
}