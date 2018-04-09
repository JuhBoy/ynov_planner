
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using events_planner.Models;
using System.Linq;

namespace events_planner.Controllers {

    [Route("api/[controller]"), Authorize(Roles = "Student,Admin")]
    public class BookingController : BaseController {

      public BookingController(PlannerContext context) {
          Context = context;
      }

      [HttpGet("subscribe/{eventId}")]
      public async Task<IActionResult> Subscribe(int eventId) {
          Event @event = await Context.Event
                                      .AsTracking()
                                      .FirstOrDefaultAsync((Event ev) => ev.Id == eventId);
                                      
          if (@event == null || @event.SubscribedNumber >= @event.SubscribeNumber) 
              return BadRequest("Unprocessable Operation");

          if (Context.Booking.Any((Booking booking) => booking.EventId == eventId 
              && booking.UserId == CurrentUser.Id)
              || @event.Expired()) {
                  return BadRequest("User already subsribed");
              }

          Booking book = new Booking() {
            Present = false,
            User = CurrentUser,
            Event = @event
          };
          @event.SubscribedNumber++;

          try {
            Context.Booking.Add(book);
            Context.Event.Update(@event);
            await Context.SaveChangesAsync();
          } catch (DbUpdateException e) {
            return BadRequest(e.InnerException.Message);
          }

          return Ok();
      }
    }
}
