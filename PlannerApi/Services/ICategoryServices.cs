using System.Threading.Tasks;
using events_planner.Deserializers;
using events_planner.Models;

namespace events_planner.Services
{
    public interface ICategoryServices
    {
        void LoadSubs(ref Category[] parents);

        void UpdateFromDeserializer(ref CategoryDeserializer categoryDeserializer,
                                    ref Category category);

        Category[] GetAllParents();
        
        EventCategory[] GetCategoriesFromString(string categories);

        Task<Category> GetByIdAsync(int id);
    }
}