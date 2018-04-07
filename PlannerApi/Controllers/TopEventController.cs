using System;
using Microsoft.EntityFrameworkCore;
using events_planner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using events_planner.Services;
using System.Threading.Tasks;

namespace events_planner.Controllers {

    [Route("api/[controller]"), Authorize(Roles = "Admin")]
    public class TopEventController : BaseController {

        private IEventServices EventServices;

        public TopEventController(PlannerContext plannerContext,
                                  IEventServices eventServices) {
            Context = plannerContext;
            EventServices = eventServices;
        }

        [HttpGet("add/{eventId}/{index}/{name?}")]
        public async Task<IActionResult> AddTopEvent(int eventId, int index, string name) {
            Event @event = await EventServices.GetEventByIdAsync(eventId);

            if (@event == null) 
                return NotFound("Event not found");
            else if (Context.Tops.Any<TopEvents>((TopEvents arg) => arg.EventId == eventId))
                return BadRequest("Event has been already add to the top list");

            TopEvents newTopEvent = new TopEvents() {
                Event = @event,
                EventId = @event.Id,
                Index = index,
                Name = name
            };
            @event.TopEvents = newTopEvent;

            try {
                Context.Tops.Add(newTopEvent);
                Context.Event.Update(@event);
                Context.SaveChanges();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }
            return NoContent();
        }

        [HttpGet("list")]
        public IActionResult GetAllTopEvents() {
            TopEvents[] events = Context.Tops
                                        .Include(args => args.Event)
                                        .ToArray();
            return new ObjectResult(events);
        }
    }
}
