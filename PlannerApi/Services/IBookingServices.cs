using System;
using System.Linq;
using System.Threading.Tasks;
using events_planner.Models;
using events_planner.Utils;
using Microsoft.AspNetCore.Mvc;

namespace events_planner.Services
{
    public interface IBookingServices
    {
        Booking GetByIds(int userId, int eventId);
        Task<Booking> GetByIdsAsync(int userId, int eventId);
        Task<Booking> GetByIdsAsync(int userId, int eventId, IQueryable<Booking> query);
        Task<Booking[]> GetBookedEventForUserAsync(int userId);

        Task SetBookingPresence(Booking booking, bool presence);
        bool EnsureModerationCapability(User user, int eventId);

        /// <summary>
        /// Process to Booking confirmation with the booking already updated (in memory)
        /// </summary>
        /// <param name="previsousConfirm">Previous Value of confirm</param>
        /// <param name="booking">The booking targeted</param>
        /// <exception cref="InvalidCastException">If booking.Validate is null</exception>
        Task SetBookingConfirmation(bool previsousConfirm, Booking booking);
        
        #region JuryPoints
        JuryPoint CreateJuryPoint(float points, int userId);
        JuryPoint GetJuryPoint(int userId, float points);
        void RemoveJuryPoints(JuryPoint juryPoint);
        #endregion

        #region Queries
        IQueryable<Booking> WithUser(IQueryable<Booking> query);
        IQueryable<Booking> WithEvent(IQueryable<Booking> query);
        IQueryable<Booking> WithUserAndEvent(IQueryable<Booking> query = null);
        #endregion
    }
}