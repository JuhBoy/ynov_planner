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
    public class CategoryController : BaseController {
        
        private ICategoryServices Services { get; set; }

        public CategoryController(PlannerContext context, ICategoryServices categoryServices) {
            Context = context;
            Services = categoryServices;
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        /// <response code="401">Admin token is not permitted</response>
        /// <response code="500">if the credential given is not valid or DB update failed</response>
        /// <response code="200">If category has been Added to the database</response>
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

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <param name="type">0 = All, 1 = SUBS, 2 = PARENTS</param>
        /// <remarks>All = All categories are returned, SUBS return only sub cateogir and parent so .. </remarks>
        /// <response code="401">Admin token is not permitted</response>
        /// <response code="500">if the credential given is not valid or DB update failed</response>
        /// <response code="200">If category has been Added to the database</response>
        [HttpGet("all/{type}"), AllowAnonymous]
        public async Task<IActionResult> ReadAll(int type) {
            Category[] categories = new Category[0];

            switch ((CategoryListType) type) {
                case (CategoryListType.ALL):
                    categories = await Context.Category.AsNoTracking().ToArrayAsync();
                    break;
                case (CategoryListType.SUBS):
                    categories = Services.GetAllSubs();
                    break;
                case (CategoryListType.PARENTS):
                    categories = Services.GetAllParents();
                    break;
                default:
                    categories = await Context.Category.AsNoTracking().ToArrayAsync();
                    break;
            }

            if (categories.Length <= 0) { return NoContent(); }

            return new OkObjectResult(categories);
        }

        /// <summary>
        /// Update a category
        /// </summary>
        /// <response code="401">Admin token is not permitted</response>
        /// <response code="500">if the credential given is not valid or DB update failed</response>
        /// <response code="200">If category has been Updated to the database</response>
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

        /// <summary>
        /// Delete a category
        /// </summary>
        /// <response code="401">Admin token is not permitted</response>
        /// <response code="500">if the credential given is not valid or DB update failed</response>
        /// <response code="201">If category has been Removed from the database</response>
        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public IActionResult Delete(int id) {
            Category category = Context.Category.FirstOrDefault<Category>(cat => cat.Id == id);

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