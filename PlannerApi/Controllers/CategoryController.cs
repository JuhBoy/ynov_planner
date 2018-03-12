using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using events_planner.Deserializers;
using events_planner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using events_planner.Services;
using System.Linq;
using events_planner.Services.Constants;

namespace events_planner.Controllers {

    [Route("api/[controller]")]
    public class CategoryController : Controller {

        private PlannerContext Context { get; set; }
        private ICategoryServices Services { get; set; }

        public CategoryController(PlannerContext context, ICategoryServices categoryServices) {
            Context = context;
            Services = categoryServices;
        }

        [HttpPost, Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody] CategoryDeserializer categoryFromRequest) {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }

            Category category = new Category() { Name = categoryFromRequest.Name };

            if (categoryFromRequest.subCategoryId.HasValue)
                Services.bindSubCategoryFromDb(ref category, (int) categoryFromRequest.subCategoryId);

            try {
                Context.Category.Add(category);
                Context.SaveChanges();
            } catch (Exception e) {
                return BadRequest(e.InnerException.Message);
            }

            return new OkObjectResult(category);
        }

        [HttpGet("all/{type}"), Authorize(Roles = "Student,Admin")]
        public async Task<IActionResult> ReadAll(int type) {
            Category[] categories = new Category[0];

            switch ((CategoryListType) type) {
                case (CategoryListType.ALL):
                    categories = await Context.Category.ToArrayAsync();
                    break;
                case (CategoryListType.SUBS):
                    categories = Services.GetAllSubs();
                    break;
                case (CategoryListType.PARENTS):
                    categories = Services.GetAllParents();
                    break;
                default:
                    categories = await Context.Category.ToArrayAsync();
                    break;
            }

            if (categories.Length <= 0) { return NoContent(); }

            return new OkObjectResult(categories);
        }

        [HttpPut("{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryDeserializerUpdate categoryFromRequest) {
            Category category = await Context.FindAsync<Category>(id);

            if (category == null) { return NotFound(); }

            try {
                Services.UpdateFromDeserializer(ref categoryFromRequest, ref category);
            } catch (DbUpdateException db) {
                return BadRequest(db.InnerException.Message);
            }

            return new OkObjectResult(category);
        }

        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public IActionResult Delete(int id) {
            Category category = Context.Category.Include(inc => inc.SubCategory).FirstOrDefault<Category>(cat => cat.Id == id);

            if (category == null) { return NotFound(); }

            try {
                Services.DeleteCircular(category.Id);
            } catch (Exception e) {
                return BadRequest(e.InnerException.Message);
            }

            return NoContent();
        }
    }
}