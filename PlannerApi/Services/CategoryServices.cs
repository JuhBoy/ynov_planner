using events_planner.Services;
using events_planner.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using events_planner.Deserializers;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection {
    public class CategoryServices : ICategoryServices {
        private PlannerContext Context { get; set; }
        private IEventServices EventServices { get; set; }

        public CategoryServices(PlannerContext context, IEventServices eventServices) {
            Context = context;
            EventServices = eventServices;
        }

        #region Interface ICategory

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
            using (var DbTransaction = Context.Database.BeginTransaction()) {
                DropSubReferencesFromParents(categoryId);
                EventServices.RemoveAllEventCategoryReferencesFor(categoryId);
                DeleteCategory(categoryId);

                DbTransaction.Commit();
            }
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

        #endregion

        #region Private

        private void DropSubReferencesFromParents(int id) {
            Category[] categories = GetParentsForSubId(id);

            if (categories == null) { return; }

            foreach (Category category in categories) {
                category.SubCategoryId = null;
                category.SubCategory = null;
            }

            Context.Category.UpdateRange(categories);
            Context.SaveChanges();
        }

        private Category[] GetParentsForSubId(int subId) {
            return Context.Category.Where((Category cat) => cat.SubCategoryId == subId).ToArray();
        }

        private void DeleteCategory(int id) {
            string sql = string.Format("DELETE FROM category WHERE category_id = {0}", id);
            Context.Database.ExecuteSqlCommand(sql);
        }

        #endregion
    }
}