using events_planner.Services;
using events_planner.Models;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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

        public Category[] GetAllSubs() {
            string subDistinctIds = "SELECT sub.sub_category_id FROM (SELECT DISTINCT sub_category_id FROM category WHERE sub_category_id IS NOT NULL) as sub";
            string sqlForSubs = string.Format("SELECT * FROM category WHERE category_id IN ({0})", subDistinctIds);

            Category[] category = Context.Category.FromSql(sqlForSubs).ToArray();

            return category;
        }

        public Category[] GetAllParents() {
            return Context.Category.Where(s => s.SubCategoryId != null).ToArray();
        }
    }
}