using System.Linq;
using System.Threading.Tasks;
using events_planner.Constants;
using events_planner.Controllers;
using events_planner.Models;
using events_planner.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace events_planner.Services {
    
    public class BookingServices : IBookingServices {
        
        private PlannerContext Context { get; }
        private IEmailService EmailService { get; }

        public BookingServices(PlannerContext context, IEmailService emailService) {
            Context = context;
            EmailService = emailService;
        }

        public Booking GetByIds(int userId, int eventId) {
            return Context.Booking.FirstOrDefault(b => b.UserId == userId && b.EventId == eventId);
        }
        
        public async Task<Booking> GetByIdsAsync(int userId, int eventId) {
            return await Context.Booking.FirstOrDefaultAsync(b => b.UserId == userId && b.EventId == eventId);
        }
        
        public async Task<Booking> GetByIdsAsync(int userId, int eventId, IQueryable<Booking> query) {
            return await query.FirstOrDefaultAsync(b => b.UserId == userId && b.EventId == eventId);
        }

        public async Task<Booking> MakeBookingAsync(User user, Event @event) {
            Booking book = new Booking() {
                Present = false,
                User = user,
                Event = @event
            };
            @event.SubscribedNumber++;
            book.Validated &= !@event.ValidationRequired;

            Context.Booking.Add(book);
            Context.Event.Update(@event);
            await Context.SaveChangesAsync();
            
            return book;
        }

        public void SendEmailForNewBooking(Booking booking) {
            BookingTemplate template;
            if (!booking.Validated.HasValue) {
                template = BookingTemplate.AUTO_VALIDATED;
            } else {
                template = BookingTemplate.PENDING_VALIDATION;
            }
            EmailService.SendFor(booking.User, booking.Event, template);
        }

        public async Task<Booking[]> GetBookedEventForUserAsync(int userId) {
            return await Context.Booking
                .AsTracking()
                .Include(inc => inc.Event)
                .ThenInclude(tinc => tinc.Images)
                .Include(iii => iii.Event)
                .ThenInclude(tinc => tinc.EventCategory)
                .ThenInclude(evcat => evcat.Category)
                .Where(arg => arg.UserId == userId
                              && !arg.Present)
                .ToArrayAsync();
        }

        public async Task SetBookingConfirmation(bool previsousConfirm, Booking booking) {
            BookingTemplate template;
            
            if (!(bool)booking.Validated) {
                if (previsousConfirm) {
                    booking.Event.ValidatedNumber--;
                    await SetBookingPresence(booking, false);
                }
                booking.Event.SubscribedNumber--;
                Context.Event.Update(booking.Event);
                Context.Booking.Remove(booking);
                template = BookingTemplate.AWAY;
            } else {
                booking.Event.ValidatedNumber++;
                Context.Booking.Update(booking);
                template = BookingTemplate.SUBSCRIPTION_VALIDATED;
            }
            
            EmailService.SendFor(booking.User, booking.Event, template);
            await Context.SaveChangesAsync();
        }

        public async Task SetBookingPresence(Booking booking, bool presence) {
            if (booking.Present == presence) { return; }

            booking.Present = presence;
            
            if (!presence && booking.Event.JuryPoint.HasValue) {
                var points = GetJuryPoint(booking.UserId, (float) booking.Event.JuryPoint);
                RemoveJuryPoints(points);
            } else if (booking.Event.JuryPoint.HasValue) {
                CreateJuryPoint((float) booking.Event.JuryPoint, booking.UserId);
                EmailService.SendFor(booking.User, booking.Event, BookingTemplate.PRESENT);
            }
            
            Context.Booking.Update(booking);
            await Context.SaveChangesAsync();
        }
        
        public async Task SubscribeUserToEvent(Event @event, User user) {
            var book = await MakeBookingAsync(user, @event);
            book.Validated |= @event.ValidationRequired;
            await SetBookingConfirmation(false, book);
        }

        public bool IsAllowedToSubscribe(User user, Event @event) {
            int roleId = user.RoleId;
            int eventID = @event.Id;
            
            return !@event.RestrictedEvent || Context.EventRole.Any(ev => ev.RoleId == roleId && ev.EventId == eventID);
        }

        public async Task<bool> IsBookedToEvent(User user, Event @event) {
            int eventId = @event.Id;
            int userId = user.Id;
            return await Context.Booking.AnyAsync((Booking booking) => booking.EventId == eventId && booking.UserId == userId);
        }

        public async Task<BadRequestObjectResult> IsBookableAsync(Event @event, User user) {
            if (@event == null)
                return new BadRequestObjectResult(ApiErrors.EventNotFound);
            if (user == null) 
                return new BadRequestObjectResult(ApiErrors.UserNotFound);
            if (@event.SubscribedNumber >= @event.SubscribeNumber)
                return new BadRequestObjectResult(ApiErrors.SubscriptionOverFlow);
            if (await IsBookedToEvent(user, @event))
                return new BadRequestObjectResult(ApiErrors.AlreadyBooked);
            if (!@event.Forward())
                return new BadRequestObjectResult(ApiErrors.EventExpired);
            if (!@event.SubscribtionOpen())
                return new BadRequestObjectResult(ApiErrors.SubscriptionNotOpen);
            if (!IsAllowedToSubscribe(user, @event))
                return new BadRequestObjectResult(ApiErrors.SubscriptionNotPermitted);
            return null;
        }

        public async Task<BadRequestObjectResult> IsBookableWithoutDateAsync(Event @event, User user) {
            if (@event == null)
                return new BadRequestObjectResult(ApiErrors.EventNotFound);
            if (user == null) 
                return new BadRequestObjectResult(ApiErrors.UserNotFound);
            if (@event.SubscribedNumber >= @event.SubscribeNumber)
                return new BadRequestObjectResult(ApiErrors.SubscriptionOverFlow);
            if (await IsBookedToEvent(user, @event))
                return new BadRequestObjectResult(ApiErrors.AlreadyBooked);
            if (!IsAllowedToSubscribe(user, @event))
                return new BadRequestObjectResult(ApiErrors.SubscriptionNotPermitted);
            return null;
        }

        #region JuryPoints

        public JuryPoint CreateJuryPoint(float points, int userId) {
            JuryPoint juryPoint = new JuryPoint {
                Points = points,
                UserId = userId
            };
            Context.JuryPoints.Add(juryPoint);
            return juryPoint;
        }

        public JuryPoint GetJuryPoint(int userId, float points) {
            double epsilon = 0.01;
            return Context.JuryPoints.FirstOrDefault(jp => jp.UserId == userId &&
                                                           (jp.Points - points) < epsilon);
        }

        public void RemoveJuryPoints(JuryPoint juryPoint) {
            Context.Remove(juryPoint);
        }
        
        #endregion
        
        #region Queries

        public IQueryable<Booking> WithUser(IQueryable<Booking> query) {
            return query.Include(inc => inc.User);
        }

        public IQueryable<Booking> WithEvent(IQueryable<Booking> query) {
            return query.Include(inc => inc.Event);
        }

        public IQueryable<Booking> WithUserAndEvent(IQueryable<Booking> query = null) {
            return WithEvent(WithUser(query ?? Context.Booking));
        }
        
        #endregion
    }
}