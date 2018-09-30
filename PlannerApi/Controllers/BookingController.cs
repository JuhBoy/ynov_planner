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
using Microsoft.Extensions.DependencyInjection;
using events_planner.Utils;

namespace events_planner.Controllers {

    [Route("api/[controller]")]
    public class BookingController : BaseController {

        public IEventServices EventServices;

        public IEmailService EmailServices;

        public IBookingServices BookingServices;

        public BookingController(PlannerContext context,
                                IEventServices eventServices,
                                IEmailService mailServices,
                                IBookingServices bookingServices) {
            Context = context;
            EventServices = eventServices;
            EmailServices = mailServices;
            BookingServices = bookingServices;
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
        ///     - Event is restricted to the current user role
        /// </remarks>
        /// <param name="eventId">The event Id</param>
        /// <returns>204 no Content</returns>
        [HttpGet("subscribe/{eventId}"), Authorize(Roles = "Student, Admin, Foreigner, Staff")]
        public async Task<IActionResult> Subscribe(int eventId) {
            Event @event = await EventServices.GetEventByIdAsync(eventId);

            if (@event == null || @event.SubscribedNumber >= @event.SubscribeNumber)
                return BadRequest("Unprocessable, Max Subscriber reach or event not found");

            // Don't force lambda to request Db for other scops.
            int userId = CurrentUser.Id;
            int roleId = CurrentUser.RoleId;
            int eventID = @event.Id;

            if (Context.Booking.Any((Booking booking) => booking.EventId == eventId
                && booking.UserId == userId)) {
                return BadRequest("User Already Booked");
            }

            if (!@event.HasSubscriptionWindow() && !@event.Forward()) {
                return BadRequest("Event Expired, can't subscribe");
            }

            if (!@event.SubscribtionOpen()) {
                return BadRequest("Subscriptions are not open");
            }

            if (@event.RestrictedEvent &&
                !Context.EventRole.Any((EventRole ev) => ev.RoleId == roleId && ev.EventId == eventID)) {
                return BadRequest("Sorry you are not allowed to subscribe");
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

                BookingTemplate template;
                if (!book.Validated.HasValue) {
                    template = BookingTemplate.AUTO_VALIDATED;
                } else {
                    template = BookingTemplate.PENDING_VALIDATION;
                }
                EmailServices.SendFor(CurrentUser, @event, template);
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return NoContent();
        }

        /// <summary>
        /// Unsubscribe from an event
        /// </summary>
        /// <returns>204 No Content</returns>
        [HttpGet("unsubscribe/{eventId}"), Authorize(Roles = "Student, Admin, Foreigner, Staff")]
        public async Task<IActionResult> UnSubscribe(int eventId) {
            Event @event = await EventServices.GetEventByIdAsync(eventId);
            Booking booking = await BookingServices.GetByIdsAsync(CurrentUser.Id, eventId);

            if (@event == null || booking == null || !@event.Forward() ||
                !@event.SubscribtionOpen()) {
                return BadRequest("Can't unsubscribe to the event");
            }

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
        /// List of all booking wich has been subscribed by the current user
        /// The list doesn't contains the past events
        /// </summary>
        /// <returns>A list of events</returns>
        [HttpGet, Authorize(Roles = "Student, Admin, Foreigner, Staff")]
        public async Task<IActionResult> GetBookedEvents() {
            Booking[] events = await BookingServices.GetBookedEventForUserAsync(CurrentUser.Id);
            Booking[] result = events.Where((arg) => arg.Event.Forward()).ToArray();
            return Ok(result);
        }

        /// <summary>
        /// Validate the presence of a user to an event.
        /// </summary>
        /// <remarks>
        /// It ensure that :
        ///     - User is subscribed to this event
        ///     - Event isn't done already
        ///     - User has been Validated if required
        ///     - Request is done by a moderator or an Administrator
        ///     - It also create the jury point associated with this event
        /// </remarks>
        /// <returns>201 No content</returns>
        [HttpPost("validate"), Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> ValidatePresence(
            [FromBody] BookingValidationDeserializer bookingValidation,
            [FromServices] IUserServices userServices) {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            if (!CurrentUser.Role.Name.Equals("Admin") &&
                !userServices.IsModeratorFor(bookingValidation.EventId,
                                            CurrentUser.Id)) {
                return BadRequest("Not Allowed to moderate this event");
            }

            var query = BookingServices.WithUserAndEvent(Context.Booking);

            Booking book =
                await BookingServices.GetByIdsAsync(bookingValidation.UserId, bookingValidation.EventId, query);

            if (book == null)
                return NotFound("Booking not found");
            else if (!book.Event.OnGoingWindow() ||
               !book.Event.Status.Equals(Status.ONGOING))
                return BadRequest("Can't validate presence outside of open window");
            else if (book.Present)
                return BadRequest("Presence Already validated");
            else if (book.Event.ValidationRequired && (bool) !book.Validated)
                return BadRequest("User hasn't been validated");

            // VALIDATE THE PRESENCE
            book.Present = true;

            // CREATE THE PointJury ASSOCIATED
            if (book.Event.JuryPoint != null) {
                BookingServices.CreateJuryPoint((float) book.Event.JuryPoint, bookingValidation.UserId);
            }

            try {
                Context.Booking.Update(book);
                await Context.SaveChangesAsync();
                EmailServices.SendFor(book.User, book.Event,
                                          BookingTemplate.PRESENT);
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return NoContent();
        }

        [HttpPatch("change-validation/{validation}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeValidation([FromBody] BookingValidationDeserializer bookingValidationDsl, 
                                              [FromRoute] bool validation) {
            Booking booking = await BookingServices.GetByIdsAsync(bookingValidationDsl.UserId,
                bookingValidationDsl.EventId, BookingServices.WithUserAndEvent(Context.Booking));
            
            if (booking == null) { return NotFound("Booking not found"); }

            booking.Present = validation;

            if (!validation && booking.Event.JuryPoint.HasValue) {
                BookingServices.RemoveJuryPoints(
                    BookingServices.GetJuryPoint(bookingValidationDsl.UserId, (float) booking.Event.JuryPoint)
                );
            } else if (booking.Event.JuryPoint.HasValue) {
                BookingServices.CreateJuryPoint((float) booking.Event.JuryPoint, bookingValidationDsl.UserId);
                EmailServices.SendFor(booking.User, booking.Event, BookingTemplate.PRESENT);
            }

            try {
                Context.Booking.Update(booking);
                await Context.SaveChangesAsync();
            } catch (DbUpdateException ex) {
                return BadRequest(ex);
            }
            
            return NoContent();
        }

        /// <summary>
        /// Confirms the user at the specified event.
        /// </summary>
        /// <returns>204</returns>
        /// <param name="userId">User identifier.</param>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="confirm">If set to <c>true</c> confirm the user,
        /// otherwise it remove the booking</param>
        [HttpPost("confirm/{userId}/{eventId}/{confirm}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmUser(int userId,
                                                     int eventId,
                                                     bool confirm) {
            Booking booking = await Context.Booking
                                           .Include(arg => arg.User)
                                           .Include(arg => arg.Event)
                                           .FirstOrDefaultAsync(arg =>
                                                                arg.UserId == userId
                                                                && arg.EventId == eventId);

            if (booking == null)
                return BadRequest("Booking Not found");
            else if (!booking.Event.Forward())
                return BadRequest("Event expired");
            else if (!booking.Event.ValidationRequired)
                return BadRequest("Event doesn't need validation");
            else if (booking.Validated.HasValue && (bool) booking.Validated == confirm) {
                return BadRequest("User already Validated this way");
            }

            booking.Validated = confirm;

            try {
                BookingTemplate template;
                if (!confirm) {
                    booking.Event.SubscribedNumber--;
                    Context.Event.Update(booking.Event);
                    Context.Booking.Remove(booking);
                    template = BookingTemplate.AWAY;
                } else {
                    booking.Event.ValidatedNumber++;
                    Context.Booking.Update(booking);
                    template = BookingTemplate.SUBSCRIPTION_VALIDATED;
                }
                await Context.SaveChangesAsync();
                EmailServices.SendFor(booking.User, booking.Event, template);
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return NoContent();
        }

        /// <summary>
        /// List of booking with user included
        /// </summary>
        /// <remarks>
        /// Response:
        ///
        ///   {
        ///     "id": 2,
        ///     "present": false,
        ///     "validated": null,
        ///      "eventId": 2
        ///     "user": {
        ///        ... User model
        ///     },
        ///   }
        ///
        /// When validate is null the event doesn't required any validations.
        /// The value will be settled to false or true otherwise.
        ///
        /// </remarks>
        /// <returns>200</returns>
        /// <param name="eventId">Event identifier.</param>
        [HttpGet("users_status/{eventId}"), Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> GetUserStatusForBooking(int eventId) {
            var books = Context.Booking
                               .Include(inc => inc.User)
                               .ThenInclude(user => user.Promotion)
                               .Where(boks => boks.EventId == eventId)
                               .ToArray();
            return Ok(books);
        }
    }
}
