using events_planner.Services;
using events_planner.Deserializers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using events_planner.Models;
using System.Linq;
using System;
using System.Linq.Expressions;

namespace events_planner.Controllers {

    [Route("api/[controller]"), Authorize(Roles = "Student, Admin")]
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
                return BadRequest("Unprocessable, Max Subscriber reach or event not found");
            
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

        [HttpGet, Authorize(Roles = "Student, Admin")]
        public async Task<IActionResult> GetBookedEvents() {
            Booking[] events = await Context.Booking
                                          .AsTracking()
                                          .Include(inc => inc.Event)
                                          .Where((Booking arg) => arg.UserId == CurrentUser.Id &&
                                                !arg.Present)
                                          .ToArrayAsync();

            return new ObjectResult(events.Select((arg) => arg.Event.Forward()));
        }

        [HttpPost("validate"), Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> ValidatePresence(
            [FromBody] BookingValidationDeserializer bookingValidation,
            [FromServices] IUserServices userServices) {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            if (!userServices.IsModeratorFor(bookingValidation.EventId,
                                            CurrentUser.Id)) {
                return BadRequest("Not Allowed to moderate this event");
            }

            // RETRIEVE THE BOOKING CORRESPONDIG TO USER + EVENT
            Expression<Func<Booking, bool>> action = 
                (Booking arg) => arg.UserId == bookingValidation.UserId &&
                                 arg.EventId == bookingValidation.EventId;

            Booking book = await Context.Booking
                                        .AsTracking()
                                        .Include(inc => inc.Event)
                                        .FirstOrDefaultAsync(action);

            if (book == null)
                return NotFound("Booking not found");
            else if (book.Event.Expired() ||
               book.Event.Status != Status.ONGOING)
                return BadRequest("Can't validate presence outside of open window");
            else if (book.Present)
                return BadRequest("Presence Already validated");

            // VALIDATE THE PRESENCE
            book.Present = true;

            // CREATE THE PointJury ASSOCIATED
            if (book.Event.JuryPoint != null) {
                JuryPoint juryPoint = new JuryPoint() {
                    Points = (int)book.Event.JuryPoint,
                    UserId = bookingValidation.UserId
                };
                Context.JuryPoints.Update(juryPoint);
            }

            try {
                Context.Booking.Update(book);
                await Context.SaveChangesAsync();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return NoContent();
        }
    }
}
