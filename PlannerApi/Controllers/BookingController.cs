using events_planner.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using events_planner.Models;
using System.Linq;

namespace events_planner.Controllers {

    [Route("api/[controller]"), Authorize(Roles = "Student,Admin")]
    public class BookingController : BaseController {

        public IEventServices EventServices;

        public BookingController(PlannerContext context,
                                IEventServices eventServices) {
            Context = context;
            EventServices = eventServices;
        }

        [HttpGet("subscribe/{eventId}")]
        public async Task<IActionResult> Subscribe(int eventId) {
            Event @event = await EventServices.GetEventByIdAsync(eventId);

            if (@event == null || @event.SubscribedNumber >= @event.SubscribeNumber)
                return BadRequest("Unprocessable Operation");

            if (Context.Booking.Any((Booking booking) => booking.EventId == eventId
                && booking.UserId == CurrentUser.Id) || @event.Expired()) {
                return BadRequest("User Already Booked or Event Expired");
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

            return NoContent();
        }

        [HttpGet("unsubscribe/{eventId}")]
        public async Task<IActionResult> UnSubscribe(int eventId) {
            Event @event = await EventServices.GetEventByIdAsync(eventId);
            Booking booking = await Context.Booking.FirstOrDefaultAsync(
                (Booking book) => book.UserId == CurrentUser.Id && 
                                  book.EventId == eventId
            );

            if (@event == null || booking == null || @event.Expired())
                return BadRequest("Event not found or Expired");

            @event.SubscribedNumber--;

            try {
                Context.Update(@event);
                Context.Remove(booking);
                await Context.SaveChangesAsync();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return NoContent();
        }
    }
}
