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
using events_planner.Constants;
using Microsoft.Extensions.DependencyInjection;
using events_planner.Utils;

namespace events_planner.Controllers {

    [Route("api/[controller]")]
    public class BookingController : BaseController {

        public IEventServices EventServices;

        public IBookingServices BookingServices;

        public IUserServices UserServices;

        public BookingController(PlannerContext context,
                                IEventServices eventServices,
                                IEmailService mailServices,
                                IBookingServices bookingServices,
                                IUserServices userServices) {
            Context = context;
            EventServices = eventServices;
            BookingServices = bookingServices;
            UserServices = userServices;
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

            var requestObjectError = await BookingServices.IsBookableAsync(@event, CurrentUser);
            if (requestObjectError != null) return requestObjectError;

            try {
                var book = await BookingServices.MakeBookingAsync(CurrentUser, @event);
                BookingServices.SendEmailForNewBooking(book);
            } catch (DbUpdateException ex) {
                return BadRequest(ex.InnerException.Message);
            }

            return NoContent();
        }

        /// <summary>
        /// Unsubscribe from an event
        /// </summary>
        /// <returns>204 No Content</returns>
        [HttpGet("unsubscribe/{eventId}"), Authorize(Roles = "Student, Admin, Foreigner, Staff")]
        public async Task<IActionResult> UnSubscribe(int eventId) {
            var book = await BookingServices.GetByIdsAsync(CurrentUser.Id, eventId, BookingServices.WithUserAndEvent());

            if (book == null) {
                return BadRequest("Can't unsubscribe to the event");
            }

            try {
                await BookingServices.UnsubscribeUser(book);
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
            return Ok(events);
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
        public async Task<IActionResult> ValidatePresence([FromBody] BookingValidationDeserializer bookDsl) {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            if (UserServices.EnsureModerationCapability(CurrentUser, bookDsl.EventId)) {
                return BadRequest("User Not Allowed to moderate this event");
            }

            Booking book = await BookingServices.GetByIdsAsync(bookDsl.UserId, bookDsl.EventId,
                BookingServices.WithUserAndEvent());

            if (book == null)
                return NotFound("Booking not found");

            if (book.Event.ValidationRequired && (bool)!book.Validated)
            {
                book.Validated = true;
                await BookingServices.SetBookingConfirmation(false, book);
            }
                
            try {
                await BookingServices.SetBookingPresence(book, bookDsl.Presence);
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return NoContent();
        }

        /// <summary>
        /// Confirms the user at the specified event.
        /// </summary>
        /// <returns>204</returns>
        /// <param name="userId">User identifier.</param>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="confirm">If set to <c>true</c> validate the user,
        /// otherwise it remove the booking</param>
        [HttpPost("confirm/{userId}/{eventId}/{confirm}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmUser(int userId,
                                                     int eventId,
                                                     bool confirm) {
            Booking booking = await BookingServices.GetByIdsAsync(userId, eventId, BookingServices.WithUserAndEvent());

            if (booking == null)
                return BadRequest("Booking Not found");
            else if (!booking.Event.ValidationRequired)
                return BadRequest("Event doesn't need validation");

            var tmpValidate = (bool)booking.Validated;
            booking.Validated = confirm;

            try {
                await BookingServices.SetBookingConfirmation(tmpValidate, booking);
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
        
        /// <summary>
        /// Subscribe/unsubscribe a given user to the specified event
        /// </summary>
        /// <param name="userToEventDsl">Event to user model</param>
        /// <returns>204 or 400</returns>
        [HttpPost("user"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleSubscribeUser([FromBody] UserToEventDeserializer userToEventDsl) {
            var @event = await EventServices.GetEventByIdAsync(userToEventDsl.eventId);
            var user = await UserServices.GetUserByIdAsync(userToEventDsl.userId);

            if (user == null) return BadRequest(ApiErrors.UserNotFound);
            if (@event == null) return BadRequest(ApiErrors.EventNotFound);

            var booking = await BookingServices.GetByIdsAsync(user.Id, @event.Id, BookingServices.WithUserAndEvent());

            try {
                if (booking == null)
                    await BookingServices.SubscribeUserToEvent(@event, user);
                else
                    await BookingServices.UnsubscribeUser(booking);
            } catch (DbUpdateException ex) {
                return BadRequest(ex);
            }

            return NoContent();
        }
    }
}
