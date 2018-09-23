using events_planner.Services;
using events_planner.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using events_planner.Deserializers;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection {

    public class CategoryServices : ICategoryServices {

        private PlannerContext Context { get; set; }

        public CategoryServices(PlannerContext context) {
            Context = context;
        }

        #region Interface ICategory

        public void LoadSubs(ref Category[] parents) {
            int[] parentIds = parents.Select(arg => arg.Id).ToArray();
            Category[] subs = Context.Category
                                     .Where((parent) => parentIds.Contains((int) parent.ParentCategory))
                                     .ToArray();
            foreach (Category parent in parents) {
                parent.SubsCategories = subs.Where((arg) => arg.ParentCategory == parent.Id).ToArray();
            }
        }

        public Category[] GetAllParents() {
            return Context.Category.AsNoTracking()
                                   .Where((arg) => arg.ParentCategory == null)
                                   .ToArray();
        }

        public void UpdateFromDeserializer(ref CategoryDeserializer deserializer,
                                           ref Category category) {
            category.Name = deserializer.Name;
            if (deserializer.ParentCategory.HasValue) {
                int parentId = (int) deserializer.ParentCategory;
                var parent = Context.Category.FirstOrDefault(arg => arg.Id == parentId);

                if (parent != null && !parent.ParentCategory.HasValue) {
                    category.ParentCategory = parentId;
                }
            }
        }

        public EventCategory[] GetCategoriesFromString(string categories) {
            if (categories == null) return null;
            string[] splited = categories.Split(',');

            return Context.EventCategory
                          .Include(inc => inc.Category)
                          .Include(inc => inc.Event)
                          .AsNoTracking()
                          .Where((arg) => splited.Contains(arg.Category.Name))
                          .ToArray();
        }

        public async Task<Category> GetByIdAsync(int id) {
            return await Context.Category
                                .FirstOrDefaultAsync(arg => arg.Id == id);
        }

        #endregion

        #region Private

        #endregion
    }
}