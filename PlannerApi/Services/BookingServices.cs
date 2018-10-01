using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using events_planner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace events_planner.Services {
    
    public class BookingServices : IBookingServices {
        
        private PlannerContext Context { get; }

        public BookingServices(PlannerContext context) {
            Context = context;
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

        public IQueryable<Booking> WithUserAndEvent(IQueryable<Booking> query) {
            return WithEvent(WithUser(query));
        }
        
        #endregion
    }
}