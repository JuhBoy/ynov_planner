using System.Linq;
using System.Threading.Tasks;
using events_planner.Models;

namespace events_planner.Services
{
    public interface IBookingServices
    {
        Booking GetByIds(int userId, int eventId);
        Task<Booking> GetByIdsAsync(int userId, int eventId);
        Task<Booking> GetByIdsAsync(int userId, int eventId, IQueryable<Booking> query);
        Task<Booking[]> GetBookedEventForUserAsync(int userId);
        
        #region JuryPoints
        JuryPoint CreateJuryPoint(float points, int userId);
        JuryPoint GetJuryPoint(int userId, float points);
        void RemoveJuryPoints(JuryPoint juryPoint);
        #endregion

        #region Queries
        IQueryable<Booking> WithUser(IQueryable<Booking> query);
        IQueryable<Booking> WithEvent(IQueryable<Booking> query);
        IQueryable<Booking> WithUserAndEvent(IQueryable<Booking> query);
        #endregion
    }
}