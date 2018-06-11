using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using events_planner.Deserializers;
using events_planner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using events_planner.Services;
using events_planner.Services.Constants;

namespace events_planner.Controllers {

    [Route("api/[controller]")]
    public class CategoryController : BaseController {

        private ICategoryServices Services { get; set; }

        public CategoryController(PlannerContext context,
                                  ICategoryServices categoryServices) {
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

            Category category = new Category() {
                Name = categoryFromRequest.Name,
                ParentCategory = categoryFromRequest.ParentCategory
            };

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
        /// <param name="type">all, subs, parents</param>
        /// <remarks>
        ///     - all All categories are returned
        ///     - subs return all parents categories with their sub included,
        ///     - parents return only the parent without subs included  
        /// </remarks>
        /// <response code="401">Admin token is not permitted</response>
        /// <response code="500">if the credential given is not valid or DB update failed</response>
        /// <response code="200">If category has been Added to the database</response>
        [HttpGet("all/{type}"), AllowAnonymous]
        public async Task<IActionResult> ReadAll(string type) {
            Category[] categories;

            switch (type) {
                case (CategoryListType.SUBS):
                    categories = Services.GetAllParents();
                    Services.LoadSubs(ref categories);
                    break;
                case (CategoryListType.PARENTS):
                    categories = Services.GetAllParents();
                    break;
                case (CategoryListType.ALL):
                default:
                    categories = await Context.Category
                                              .AsNoTracking()
                                              .ToArrayAsync();
                    break;
            }

            return Ok(categories);
        }

        /// <summary>
        /// Update a category
        /// </summary>
        /// <response code="401">Admin token is not permitted</response>
        /// <response code="500">if the credential given is not valid or DB update failed</response>
        /// <response code="200">If category has been Updated to the database</response>
        [HttpPut("{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id,
                                                [FromBody] CategoryDeserializer categoryFromRequest) {
            Category category = await Context.FindAsync<Category>(id);

            if (category == null) { return NotFound(); }

            try {
                Services.UpdateFromDeserializer(ref categoryFromRequest, ref category);
            } catch (DbUpdateException db) {
                return BadRequest(db.InnerException.Message);
            }

            return Ok(category);
        }

        /// <summary>
        /// Delete a category and its sub categories
        /// </summary>
        /// <response code="401">Admin token is not permitted</response>
        /// <response code="500">if the credential given is not valid or DB update failed</response>
        /// <response code="201">If category has been Removed from the database</response>
        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public IActionResult Delete(int id) {
            Category category = Context.Find<Category>(id);

            if (category == null) { return NotFound(); }

            var catReft = new Category[] { category };
            Services.LoadSubs(ref catReft);

            try {
                Context.Category.Remove(category);
                Context.RemoveRange(category.SubsCategories);
                Context.SaveChanges();
            } catch (Exception e) {
                return BadRequest(e.InnerException.Message);
            }

            return NoContent();
        }
    }
}