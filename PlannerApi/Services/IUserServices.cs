using System.Threading.Tasks;

namespace events_planner.Services
{
    public interface IUserServices
    {
         Task<object> GetToken(string login, string password);
    }
}