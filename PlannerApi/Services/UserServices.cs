using System;
using System.Threading.Tasks;
using events_planner.Services;
using Microsoft.EntityFrameworkCore;
using events_planner.Models;

// JWT GENERATING ENGINE
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public class UserServices : IUserServices
    {
        public PlannerContext _context { get; set; }
        public IConfiguration Configuration { get; set; }

        public UserServices(PlannerContext context, IConfiguration configuration) {
            _context = context;
            Configuration = configuration;

        }

        public async Task<object> GetToken(string login, string password) {
            string userName = "George";
            string lastName = "Boulet";

            var token = new JwtSecurityToken(
                audience: Configuration["TokenAuthentication:Audience"],
                issuer: Configuration["TokenAuthentication:Issuer"],
                claims: new Claim[] {
                    new Claim(JwtRegisteredClaimNames.Sub, userName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                },
                expires: DateTime.UtcNow.AddDays(2),
                notBefore: DateTime.UtcNow,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["TokenAuthentication:Secret"])), SecurityAlgorithms.HmacSha256)
            );

            return new {
                Token = new JwtSecurityTokenHandler().WriteToken(token) 
            };
        }
    }
}