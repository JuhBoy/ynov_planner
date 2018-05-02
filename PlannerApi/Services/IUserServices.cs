using System.Linq;
using System.Threading.Tasks;
using events_planner.Deserializers;
using events_planner.Models;
using events_planner.Constants.Services;

namespace events_planner.Services
{
    public interface IUserServices
    {
        string GenerateToken(ref User m_user);
        User CreateUser(UserCreationDeserializer userFromRequest);
        string ReadJwtTokenClaims(string bearerToken, JwtSelector extractor = JwtSelector.EMAIL);
        bool IsModeratorFor(int eventId, int userId);

        #region QUERIES
        
        /// <summary>
        /// Return a query that handle the inclusion of:
        /// - Role
        /// - Promotion
        /// - Jury Point
        /// </summary>
        /// <returns>IQueryable object</returns>
        IQueryable<User> AllForeignKeysQuery();
        
        #endregion
    }
}