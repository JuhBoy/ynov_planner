using Microsoft.AspNetCore.Mvc;
using events_planner.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using events_planner.Services;
using Microsoft.AspNetCore.Authorization;
using events_planner.Deserializers;
using events_planner.Constants.Services;
using System.Linq;

namespace events_planner.Controllers
{
    [Route("api/[controller]")]
    public class EventController : Controller
    {
        private PlannerContext Context { get; set; }
        private IEventServices Services { get; set; }

        public EventController(PlannerContext context, IEventServices services) {
            Context = context;
            Services = services;
        }

        //=========================
        //         CRUD
        //=========================

        [HttpPost, Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody] EventDeserializer eventFromRequest) {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }

            Event eventModel;
            try {
                eventFromRequest.BindWithEventModel<Event>(out eventModel); 
                Context.Event.Add(eventModel);
                Context.SaveChanges();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return CreatedAtAction(nameof(Read), new { id = eventModel.Id }, eventModel);
        }

        [HttpGet("{id}"), Authorize(Roles = "Admin, Student")]
        public async Task<IActionResult> Read(int id) {
            Event eventModel = await Context.Event.FirstOrDefaultAsync(e => e.Id == id);

            if (eventModel == null) { return NotFound(id); }

            return new ObjectResult(eventModel);
        }

        [HttpPatch("{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] EventUpdatableDeserializer eventFromRequest, int id) {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            Event eventModel = await Context.Event.FirstOrDefaultAsync((obj) => obj.Id == id);

            try {
                eventFromRequest.BindWithModel(ref eventModel);
                Context.Event.Update(eventModel);
                int i = await Context.SaveChangesAsync();
                if (i < 1)
                    throw new DbUpdateException("Nothing has been updated", innerException: new System.Exception());
            } catch(DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return NoContent();
        }

        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id) {
            Event eventModel = await Context.Event.FirstOrDefaultAsync((obj) => obj.Id == id);
            Context.Event.Remove(eventModel);
            await Context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("list/{order}"), Authorize(Roles = "Student, Admin")]
        public async Task<IActionResult> GetList(int order) {
            IOrderedQueryable<Event> query;

            switch((OrderBy)order) {
                case (OrderBy.ASC):
                    query = Context.Event.OrderBy((Event arg) => arg.StartAt);
                    break;
                case (OrderBy.DESC):
                default:
                    query = Context.Event.OrderByDescending((Event arg) => arg.StartAt);
                    break;
            }

            Event[] events = await query.ToArrayAsync();

            return new ObjectResult(events);
        }

        [HttpGet("{eventId}/category/{categoryId}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddCategory(int eventId, int categoryId) {
            Category category = await Context.Category
                                             .FirstOrDefaultAsync((Category arg) => arg.Id == categoryId);
            
            Event eventModel = await Context.Event
                                            .Include(o => o.EventCategory)
                                            .FirstOrDefaultAsync((Event arg) => arg.Id == eventId);

            if (eventModel == null || category == null) { return NotFound(); }

            EventCategory eventCategory = new EventCategory() { Category = category, Event = eventModel };
            eventModel.EventCategory.Add(eventCategory);

            try {
                Context.Update(eventModel);
                await Context.SaveChangesAsync();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return NoContent();
         }
    }
}