using System.Threading.Tasks;
using events_planner.Models;

namespace events_planner.Services
{
    public interface ICategoryServices
    {
        void bindSubCategoryFromDb(ref Category category, int subId);
        Category[] GetAllSubs();
        Category[] GetAllParents();
    }
}