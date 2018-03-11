using System.Threading.Tasks;
using events_planner.Models;

namespace events_planner.Services
{
    public interface ICategoryServices
    {
        // Bind a sub category if it's not the same as the category given
        void bindSubCategoryFromDb(ref Category category, int subId);
    }
}