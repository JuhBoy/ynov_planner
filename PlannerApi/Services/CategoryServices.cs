using events_planner.Services;
using events_planner.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using events_planner.Deserializers;

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

        public void DeleteCircular(int categoryId) {
            string sql = string.Format("DELETE FROM category WHERE category_id = {0}", categoryId);
            Context.Database.ExecuteSqlCommand(sql);
        }

        public void UpdateFromDeserializer(ref CategoryDeserializerUpdate categoryDeserializer, ref Category category) {
            Category sub;

            if (categoryDeserializer.subCategoryId.HasValue && (sub = Context.Category.Find(categoryDeserializer.subCategoryId)) != null)
                category.SubCategory = sub;
            else if (categoryDeserializer.subCategoryId.HasValue)
                throw new DbUpdateException("", new System.Exception("Sub Category Not found"));

            if (categoryDeserializer.Name != null)
                category.Name = categoryDeserializer.Name;

            Context.Category.Update(category);
            Context.SaveChanges();
        }
    }
}