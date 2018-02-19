using System.Threading.Tasks;
using events_planner.Deserializers;
using events_planner.Models;

namespace events_planner.Services
{
    public interface IUserServices
    {
        string GenerateToken(ref User m_user);
        User CreateUser(UserCreationDeserializer userFromRequest);
    }
}