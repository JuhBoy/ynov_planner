using events_planner.Deserializers;
using events_planner.Models;

namespace events_planner.Services
{
    public interface ICategoryServices
    {
        Category[] GetAllSubs();
        Category[] GetAllParents();
        void bindSubCategoryFromDb(ref Category category, int subId);
        void DeleteCircular(int categoryId);
        void UpdateFromDeserializer(ref CategoryDeserializerUpdate categoryDeserializer, ref Category category);
        EventCategory[] GetCategoriesFromString(string categories);
    }
}