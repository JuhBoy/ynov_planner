using events_planner.Services;
using events_planner.Models;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public class CategoryServices : ICategoryServices
    {
        private PlannerContext Context { get; set; }

        public CategoryServices(PlannerContext context) {
            Context = context;    
        }

        public void bindSubCategoryFromDb(ref Category category, int subId) {
            Category sub = Context.Category.FirstOrDefault<Category>(cat => cat.Id == subId);

            // if sub is null or subId is circular, just return nullabled sub reference
            if (sub == null || subId == category.Id) {
                category.SubCategory = null;
                category.SubCategoryId = null;
                return;
            }

            category.SubCategory = sub;
        }
    }
}