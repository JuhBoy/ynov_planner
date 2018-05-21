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

        /// <summary>
        /// Subscribe current user to an event by its ID
        /// </summary>
        /// <remarks>
        /// Subscribe only if:
        ///     - Event is found
        ///     - Event hasn't reached its max subscribed number
        ///     - Event has not expired
        ///     - User doesn't already subsribed to this event
        /// </remarks>
        /// <param name="eventId">The event Id</param>
        /// <returns>204 no Content</returns>
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
            book.Validated &= !@event.ValidationRequired;

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

            if (@event.ValidationRequired && (bool) booking.Validated)
                @event.ValidatedNumber--;

            try {
                Context.Update(@event);
                Context.Remove(booking);
                await Context.SaveChangesAsync();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return NoContent();
        }

        /// <summary>
        /// List of all events wich has been subscribed by the current user
        /// The list doesn't contains the past events
        /// </summary>
        /// <returns>A list of events</returns>
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

        /// <summary>
        /// Validate the presence of a user to an event.
        /// It ensure that :
        ///     - User is subscribed to this event
        ///     - Event isn't done already
        ///     - User has been Validated if required
        ///     - Request is done by a moderator or an Administrator
        ///     - It also create the jury point associated with this event
        /// </summary>
        /// <returns>201 No content</returns>
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
            else if (book.Event.ValidationRequired && (bool) !book.Validated)
                return BadRequest("User hasn't been validated");

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

        [HttpPost("confirm/{userId}/{eventId}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmUser(int userId, int eventId) {
            Booking booking = await Context.Booking
                                           .Include(arg => arg.Event)
                                           .FirstOrDefaultAsync(arg => 
                                                                arg.UserId == userId 
                                                                && arg.EventId == eventId);

            if (booking == null) 
                return BadRequest("Booking Not found");
            else if (booking.Event.Expired()) 
                return BadRequest("Event expired");
            else if (!booking.Event.ValidationRequired)
                return BadRequest("Event doesn't need validation");
            else if (booking.Validated.HasValue && (bool) booking.Validated) {
                return BadRequest("User already Validated");
            }

            booking.Event.ValidatedNumber++;
            booking.Validated = true;

            try {
                Context.Booking.Update(booking);
                await Context.SaveChangesAsync();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return NoContent();
        }
    }
}
