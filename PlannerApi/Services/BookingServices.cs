using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using events_planner.Models;
using events_planner.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace events_planner.Services {
    
    public class BookingServices : IBookingServices {
        
        private PlannerContext Context { get; }
        private IUserServices UserServices { get; }
        private IEmailService EmailService { get; }

        public BookingServices(PlannerContext context, IUserServices userServices, IEmailService emailService) {
            Context = context;
            UserServices = userServices;
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

        public bool EnsureModerationCapability(User user, int eventId) {
            return !user.Role.Name.Equals("Admin") && !UserServices.IsModeratorFor(eventId, user.Id);
        }

        public async Task SetBookingConfirmation(bool previsousConfirm, Booking booking) {
            BookingTemplate template;
            
            if (!(bool)booking.Validated) {
                if (previsousConfirm) { booking.Event.ValidatedNumber--; }
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