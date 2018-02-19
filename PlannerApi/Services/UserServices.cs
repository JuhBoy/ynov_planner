using System;
using System.Linq;
using System.Threading.Tasks;
using events_planner.Services;
using events_planner.Models;
using Microsoft.EntityFrameworkCore;
using events_planner.Deserializers;

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
        public PlannerContext Context { get; set; }
        public IConfiguration Configuration { get; set; }
        public IPromotionServices PromotionServices { get; set; }
        public IRoleServices RoleServices { get; set; }

        public UserServices(PlannerContext context,
                            IConfiguration configuration,
                            IPromotionServices promotionServices,
                            IRoleServices roleServices) {
            Context = context;
            Configuration = configuration;
            PromotionServices = promotionServices;
            RoleServices = roleServices;
        }

        public string GenerateToken(ref User m_user)
        {
            var jwt = new JwtSecurityToken(
                audience: Configuration["TokenAuthentication:Audience"],
                issuer: Configuration["TokenAuthentication:Issuer"],
                claims: new Claim[] {
                                    new Claim(JwtRegisteredClaimNames.Sub, m_user.FirstName),
                                    new Claim(JwtRegisteredClaimNames.GivenName, m_user.LastName),
                                    new Claim(JwtRegisteredClaimNames.Email, m_user.Email),
                                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                },
                expires: DateTime.UtcNow.AddDays(2),
                notBefore: DateTime.UtcNow,
                signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["TokenAuthentication:Secret"])), SecurityAlgorithms.HmacSha256
                )
            );
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
        
        public User CreateUser(UserCreationDeserializer userFromRequest) {
            int phone;
            int.TryParse(userFromRequest.PhoneNumber, out phone);

            Promotion promotion;
            GetPromotion(userFromRequest.Promotion, out promotion);

            Console.WriteLine(promotion);Console.WriteLine(promotion);Console.WriteLine(promotion);Console.WriteLine(promotion);Console.WriteLine(promotion);

            Role role;
            GetRole(userFromRequest.RoleName, out role);
                                
            User user = Context.User.Add(new User() {
                FirstName = userFromRequest.FirstName,
                LastName = userFromRequest.LastName,
                Username = userFromRequest.UserName,
                Email = userFromRequest.Email,
                Password = userFromRequest.Password,
                PhoneNumber = phone,
                Promotion = promotion,
                Role = role
            }).Entity;
            Context.SaveChanges();
            return user;
        }

        private void GetRole(string roleName, out Role role) {
            role = RoleServices.GetRoleByName(roleName);
            if (role == null)
            {
                role = RoleServices.GetStudentRole();
            }
        }

        private void GetPromotion(int? promotionId, out Promotion promotion) {
            if (promotionId == null)
            {
                promotion = PromotionServices.GetStaffPromotion();
            }
            else
            {
                promotion = PromotionServices.GetPromotionById((int) promotionId);
            }
        }
    }
}