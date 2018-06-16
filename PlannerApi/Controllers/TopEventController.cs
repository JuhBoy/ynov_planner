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

        [HttpPost("add/{eventId}/{name}")]
        public async Task<IActionResult> AddTopEvent(int eventId, string name) {
            Event @event = await EventServices.GetEventByIdAsync(eventId);

            if (@event == null)
                return NotFound("Event not found");
            else if (Context.Tops.Any<TopEvents>((TopEvents arg) => arg.EventId == eventId))
                return BadRequest("Event has been already add to the top list");

            TopEvents newTopEvent = new TopEvents() {
                Event = @event,
                EventId = @event.Id,
                Index = Context.Tops.Count() + 1,
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

        [HttpGet("list"), AllowAnonymous]
        public IActionResult GetAllTopEvents() {
            TopEvents[] events = Context.Tops
                                        .AsNoTracking()
                                        .Include(args => args.Event)
                                            .ThenInclude(ev => ev.Images)
                                        .OrderBy(arg => arg.Index)
                                        .ToArray();
            return Ok(events);
        }

        [HttpDelete("{eventId}")]
        public IActionResult RemoveEventFromTop(int eventId) {
            TopEvents @event = Context.Tops
                                      .FirstOrDefault((arg) => arg.EventId == eventId);
            if (@event == null)
                return NotFound("Event Not In Top");

            var tops = Context.Tops.Where((arg) => arg.EventId != eventId)
                              .OrderBy(arg => arg.Index)
                              .ToArray();
            int cursor = 1;
            foreach(var top in tops) {
                top.Index = cursor;
                cursor++;
            }

            try {
                Context.Tops.Remove(@event);
                Context.Tops.UpdateRange(tops);
                Context.SaveChanges();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return NoContent();
        }

        [HttpPatch("order/{eventId}/{newIndex}"), Authorize(Roles = "Admin")]
        public IActionResult Order(int eventId, int newIndex) {
            TopEvents[] topEvents = Context.Tops
                                           .Include(ccc => ccc.Event)
                                           .AsTracking()
                                           .OrderBy((arg) => arg.Index)
                                           .ToArray();
            
            TopEvents current = topEvents.FirstOrDefault((arg) => arg.EventId == eventId);
            int diff = newIndex - current.Index;
            current.Index = newIndex;

            if (current == null)
                return NotFound("Event not found");
            if (newIndex > topEvents.Length) 
                return BadRequest("Index out of range");

            if (diff == 0) {
                /* things ordered */ 
            } else if (Math.Abs(diff) == 1) {
                topEvents[newIndex-1].Index -= Math.Sign(diff);
            } else {
                int cursor = 1;
                for (int i = 0; i < topEvents.Length; i++) {
                    if (topEvents[i] == current) continue;

                    if (i == newIndex-1) {
                        cursor = newIndex - Math.Sign(diff);
                    }

                    topEvents[i].Index = cursor;
                    cursor++;
                }
            }

            try {
                Context.Tops.UpdateRange(topEvents);
                Context.SaveChanges();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return Ok(topEvents.OrderBy((arg) => arg.Index));
        }
    }
}
