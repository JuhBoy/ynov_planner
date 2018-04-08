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

        [HttpDelete("{eventId}")]
        public IActionResult RemoveEventFromTop(int eventId) {
            TopEvents @event = Context.Tops
                                      .FirstOrDefault((arg) => arg.EventId == eventId);
            if (@event == null)
                return NotFound("Event Not In Top");

            try {
                Context.Tops.Remove(@event);
                Context.SaveChanges();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return NoContent();
        }

        [HttpGet("order/{eventId}/{newOrderIndex}")]
        public IActionResult Order(int newOrderIndex, int eventId) {
            TopEvents[] topEvents = Context.Tops
                                           .Include(ccc => ccc.Event)
                                           .AsTracking().ToArray();
            
            TopEvents currentEvent = topEvents.FirstOrDefault((arg) => arg.EventId == eventId);

            if (currentEvent == null) return NotFound("Event not found");

            if (MathF.Abs(newOrderIndex - currentEvent.Index) > 1) {
                int sign = (newOrderIndex > currentEvent.Index) ? -1 : 1;
                int totalIteration = (newOrderIndex > currentEvent.Index) ? newOrderIndex - 1 : topEvents.Length - newOrderIndex - 1;
                int start = newOrderIndex - 1;

                currentEvent.Index = newOrderIndex;
                topEvents.OrderBy((arg) => arg.Index);

                for (int i = start; totalIteration > 0; i += sign) {
                    totalIteration--;
                    if (topEvents[i] == currentEvent ||
                       topEvents[i].Index + sign == 0) continue;
                    topEvents[i].Index += sign;
                }
            } else {
                topEvents.First((arg) => arg.Index == newOrderIndex).Index = (int) currentEvent.Index;
                currentEvent.Index = newOrderIndex;
            }

            try {
                Context.Tops.UpdateRange(topEvents);
                Context.SaveChanges();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return new ObjectResult(topEvents.OrderBy((arg) => arg.Index));
        }
    }
}
