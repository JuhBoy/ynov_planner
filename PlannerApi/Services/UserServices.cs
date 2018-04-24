using System;
using System.Linq;
using System.Threading.Tasks;
using events_planner.Services;
using events_planner.Models;
using Microsoft.EntityFrameworkCore;
using events_planner.Deserializers;
using events_planner.Constants.Services;

// JWT GENERATING ENGINE
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.Extensions.DependencyInjection {
    public class UserServices : IUserServices {
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

        public string GenerateToken(ref User m_user) {
            List<Claim> claims = GetRoles(m_user.Id, m_user.Role.Name);
            claims.AddRange(new Claim[] {
                                    new Claim(JwtRegisteredClaimNames.Sub, m_user.FirstName),
                                    new Claim(JwtRegisteredClaimNames.GivenName, m_user.LastName),
                                    new Claim(JwtRegisteredClaimNames.Email, m_user.Email),
                                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())});

            var jwt = new JwtSecurityToken(
                audience: Configuration["TokenAuthentication:Audience"],
                issuer: Configuration["TokenAuthentication:Issuer"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(2),
                notBefore: DateTime.UtcNow,
                signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["TokenAuthentication:Secret"])), SecurityAlgorithms.HmacSha256
                )
            );
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public string ReadJwtTokenClaims(string bearerToken, JwtSelector extractor = JwtSelector.EMAIL) {
            string pattern = @"([A-Za-z0-9-_]+)";
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(bearerToken);
            string payload;

            if (matches.Count != 4) { return null; }

            payload = matches[2].Value;
            JwtHeader jwt = JwtHeader.Base64UrlDeserialize(payload);
            object valueObject;

            switch (extractor) {
                case (JwtSelector.EMAIL):
                    jwt.TryGetValue("email", out valueObject);
                    break;
                case (JwtSelector.ALG):
                    jwt.TryGetValue("alg", out valueObject);
                    break;
                case (JwtSelector.AUD):
                    jwt.TryGetValue("aud", out valueObject);
                    break;
                case (JwtSelector.EXP):
                    jwt.TryGetValue("exp", out valueObject);
                    break;
                case (JwtSelector.GIVEN_NAME):
                    jwt.TryGetValue("given_name", out valueObject);
                    break;
                case (JwtSelector.ISS):
                    jwt.TryGetValue("iss", out valueObject);
                    break;
                case (JwtSelector.JTI):
                    jwt.TryGetValue("jti", out valueObject);
                    break;
                case (JwtSelector.NBF):
                    jwt.TryGetValue("nbf", out valueObject);
                    break;
                case (JwtSelector.SUB):
                    jwt.TryGetValue("sub", out valueObject);
                    break;
                case (JwtSelector.TYP):
                    jwt.TryGetValue("typ", out valueObject);
                    break;
                default:
                    jwt.TryGetValue("email", out valueObject);
                    break;
            }
            return (string)valueObject;
        }

        public User CreateUser(UserCreationDeserializer userFromRequest) {
            int phone;
            int.TryParse(userFromRequest.PhoneNumber, out phone);

            Promotion promotion;
            GetPromotion(userFromRequest.Promotion, out promotion);

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
                DateOfBirth = userFromRequest.DateOfBirth,
                Role = role
            }).Entity;
            Context.SaveChanges();
            return user;
        }

        private void GetRole(string roleName, out Role role) {
            role = RoleServices.GetRoleByName(roleName);
            if (role == null) {
                role = RoleServices.GetStudentRole();
            }
        }

        private List<Claim> GetRoles(int userId, string role = null) {
            List<Claim> claims = Context.temporaryRoles
                                 .Include(arg => arg.Role)
                                 .Where((arg) => arg.UserId == userId)
                                 .Select((arg) => new Claim("pk_user", arg.Role.Name))
                                 .ToList();
            
            if (role != null)
                claims.Add(new Claim("pk_user", role));
            
            return claims;
        }

        private void GetPromotion(int? promotionId, out Promotion promotion) {
            if (promotionId == null) {
                promotion = PromotionServices.GetForeignPromotion();
            } else {
                promotion = PromotionServices.GetPromotionById((int)promotionId);
            }
        }

        public bool IsModeratorFor(int eventId, int userId) {
            return Context.temporaryRoles
                          .Any((TemporaryRole arg) => arg.EventId == eventId &&
                                                      arg.UserId == userId);
        }
    }
}