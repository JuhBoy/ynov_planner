using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using events_planner.Deserializers;
using events_planner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace events_planner.Controllers {

    [Route("api/[controller]")]
    public class CategoryController : Controller {

        public PlannerContext Context { get; set; }

        public CategoryController(PlannerContext context) {
            Context = context;    
        }

        [HttpPost, Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody] CategoryDeserializer categoryFromRequest) {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }

            Category category = new Category() { Name = categoryFromRequest.Name };

            try {
                Context.Category.Add(category);
                Context.SaveChanges();
            } catch (Exception e) {
                return BadRequest(e.InnerException.Message);
            }

            return new OkObjectResult(category);
        }

        [HttpGet, Authorize(Roles = "Student,Admin")]
        public async Task<IActionResult> ReadAll() {
            Category[] categories = await Context.Category.ToArrayAsync();

            if (categories.Length <= 0) { return NoContent(); }

            return new OkObjectResult(categories);
        }
    }
}