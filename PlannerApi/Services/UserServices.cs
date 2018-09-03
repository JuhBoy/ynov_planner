using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using events_planner.Constants.Services;
using events_planner.Deserializers;
using events_planner.Models;
using events_planner.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

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
            var promotion = PromotionServices.GetForeignPromotion();

            Role role;
            GetRole(null, out role);

            string password;
            GeneratePasswordSha256(userFromRequest.Password, out password);

            User user = Context.User.Add(new User() {
                FirstName = userFromRequest.FirstName,
                LastName = userFromRequest.LastName,
                Email = userFromRequest.Email,
                Password = password,
                PhoneNumber = phone,
                Promotion = promotion,
                DateOfBirth = userFromRequest.DateOfBirth,
                Role = role,
                Location = userFromRequest.Location
            }).Entity;
            Context.SaveChanges();
            return user;
        }

        public void MakeUser(User user) {
            GetPromotion(null, out var promotion);
            GetRole("Student", out var role);
            GeneratePasswordSha256(user.Password, out var password);

            user.Promotion = promotion;
            user.Role = role;
            user.Password = password;
        }

        private void GetRole(string roleName, out Role role) {
            role = (roleName != null) ?  RoleServices.GetRoleByName(roleName) : role = RoleServices.GetForeignerRole();
        }

        public virtual List<Claim> GetRoles(int userId, string role = null) {
            string[] roles = Context.TemporaryRoles
                                 .Include(arg => arg.Role)
                                 .Where((arg) => arg.UserId == userId)
                                 .Select((arg) => arg.Role.Name)
                                 .ToArray();
            List<Claim> claims = roles.Distinct().Select((arg) => new Claim("pk_user", arg)).ToList();
            
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

        public void GeneratePasswordSha256(string password, out string encodedPassword) {
            using (var sha = SHA1.Create()) {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                encodedPassword = Convert.ToBase64String(hash);
            }
        }

        public bool IsModeratorFor(int eventId, int userId) {
            return Context.TemporaryRoles
                          .Any((TemporaryRole arg) => arg.EventId == eventId &&
                                                      arg.UserId == userId);
        }

        public bool ShouldUpdateFromSSO(User user, ServiceResponse ssoData,
                                        out List<string> properties) {
            Type typeUser = user.GetType();
            Type typeSSO = ssoData.GetType();
            properties = new List<string>();

            foreach (var prop in typeSSO.GetProperties()) {
                var sValue = prop.GetValue(ssoData);
                var uValue = typeUser.GetProperty(prop.Name).GetValue(user);

                if (sValue != uValue) {
                    properties.Add(prop.Name);
                }
            }
            return properties.Count > 0;
        }

        public void UpdateUserFromSsoDate(User user, ServiceResponse ssoData,
                                          List<string> properties) {
            Type typeUser = user.GetType();
            Type typeSSO = ssoData.GetType();

            foreach (var prop in properties) {
                var value = typeSSO.GetProperty(prop).GetValue(ssoData);
                typeUser.GetProperty(prop).SetValue(user, value);
            }
        }

        #region Queries

        public IQueryable<User> AllForeignKeysQuery() {
            return Context.User
                .AsNoTracking()
                .Include(inc => inc.Role)
                .Include(inc => inc.Promotion)
                .Include(inc => inc.JuryPoint);
        }

        public void IncludeBookings(ref IQueryable<User> query) {
            query = query.Include(inc => inc.Bookings);
        }

        public void likeSearchQuery(ref IQueryable<User> query, string expression) {
            query = query.Where(arg => arg.FirstName.Contains(expression) || arg.LastName.Contains(expression));
        }

        public void WithouStaffMembers(ref IQueryable<User> query) {
            query = query.Where(arg => arg.Promotion.Name != "STAFF");
        }
        
        #endregion
    }
}