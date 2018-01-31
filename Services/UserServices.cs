using System.Threading.Tasks;
using events_planner.Services;
using Microsoft.EntityFrameworkCore;
using events_planner.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    public class UserServices : IUserServices
    {
        public PlannerContext _context { get; set; }

        public UserServices(PlannerContext context) {
            _context = context;

        }

        public async Task<object> GetToken(string login, string password) {
            string userName = "George";
            string lastName = "Boulet";

            return new {
                Token = "123456789" + userName + lastName
            };
        }
    }
}