using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using events_planner.Constants.Services;
using events_planner.Deserializers;
using events_planner.Models;
using events_planner.Services;
using events_planner.Utils;
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
                expires: DateTime.UtcNow.AddHours(int.Parse(Configuration["TokenAuthentication:TokenLifeInHours"])),
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

        public User CreateUser(UserCreationDeserializer userDsl) {
            int phone;
            int.TryParse(userDsl.PhoneNumber, out phone);
            var promotion = PromotionServices.GetForeignPromotion();

            Role role;
            GetRole(null, out role);

            string password;
            GeneratePasswordSha256(userDsl.Password, out password);

            User user = Context.User.Add(new User() {
                FirstName = userDsl.FirstName,
                LastName = userDsl.LastName,
                Email = userDsl.Email,
                Password = password,
                PhoneNumber = phone,
                Promotion = promotion,
                DateOfBirth = userDsl.DateOfBirth,
                Role = role,
                Location = userDsl.Location
            }).Entity;

            Context.SaveChanges();
            return user;
        }

        public void Update(UserUpdatableDeserializer userDsl, User user) {
            if (userDsl.Password != null && !userDsl.Password.Equals(userDsl.PasswordConfirmation)) {
                throw new PasswordConfirmationException("Confirmed password is not valid");
            }

            if (userDsl.LastName != null)
                user.LastName = userDsl.LastName;
            if (userDsl.FirstName != null)
                user.FirstName = userDsl.FirstName;
            if (userDsl.Email != null)
                user.Email = userDsl.Email;
            if (userDsl.Password != null)
                user.Password = userDsl.Password;
            if (userDsl.PhoneNumber != null)
                user.PhoneNumber = int.Parse(userDsl.PhoneNumber);
            if (userDsl.ImageUrl != null)
                user.ImageUrl = userDsl.ImageUrl;
            if (userDsl.Location != null)
                user.Location = userDsl.Location;
            if (userDsl.DateOfBirth.HasValue)
                user.DateOfBirth = userDsl.DateOfBirth;
        }

        public void MakeUser(Attributes attributes, out User user) {
            user = new User() {
                        Email = attributes.Email,
                        SSOID = attributes.SSOID,
                        FirstName = attributes.FirstName,
                        LastName = attributes.LastName,
                        ImageUrl = attributes.ImageUrl,
                        Password = Guid.NewGuid().ToString(),
                        PhoneNumber = 0
                    };
            string roleName = GetRoleForCategories(attributes.Categories);
            
            GetPromotion(attributes.Name ?? roleName, out var promotion);
            GetRole(roleName, out var role);
            GeneratePasswordSha256(user.Password, out var password);
            
            if (promotion == null) {
                promotion = new Promotion() {
                    Name = attributes.Name,
                    Description = attributes.Description,
                    Year = attributes.Year
                };
            }

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

        private void GetPromotion(string promotionName, out Promotion promotion) {
            promotion = PromotionServices.GetPromotionByName(promotionName);
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

        public bool IsStudent(User user) {
            return (user.Role.Name == "Student");
        }

        public bool ShouldUpdateFromSSO(User user, ServiceResponse serviceResponse,
                                        out List<string> properties) {
            if (user.Promotion == null) { throw new MissingMemberException("Promotion has not been loaded or initialized"); }

            var ssoData = serviceResponse.AuthenticationSuccess.Attributes;
            Type typeUser = user.GetType();
            Type typeSSO = ssoData.GetType();
            Type typePromotion = user.Promotion.GetType();

            properties = new List<string>();

            foreach (var prop in typeSSO.GetProperties()) {
                 object sValue = prop.GetValue(ssoData);;
                 object uValue;

                if (typeUser.GetProperty(prop.Name) != null) {
                   uValue = typeUser.GetProperty(prop.Name).GetValue(user);
                } else {
                   uValue = typePromotion.GetProperty(prop.Name).GetValue(user.Promotion);
                }

                if (sValue != uValue) {
                    properties.Add(prop.Name);
                }
            }

            return properties.Count > 0;
        }

        public void UpdateUserFromSsoData(User user, ServiceResponse serviceResponse,
                                          List<string> properties) {
            var ssoData = serviceResponse.AuthenticationSuccess.Attributes;
            Type typeUser = user.GetType();
            Type typeSSO = ssoData.GetType();
            Type typePromotion = user.Promotion.GetType();

            foreach (var prop in properties) {
                var value = typeSSO.GetProperty(prop).GetValue(ssoData);
                if (typeUser.GetProperty(prop) != null) {
                    typeUser.GetProperty(prop).SetValue(user, value);
                    continue;
                }

                
                typePromotion.GetProperty(prop).SetValue(user.Promotion, value);
            }
        }

        private string GetRoleForCategories(string categories) {
            string roleName = "Student";
            string[] staff = Configuration["UserAsStaffMember:Staff"].Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (string staffed in staff) {
                if (!categories.Contains(staffed)) { continue; }
                roleName = "Staff";
                break;
            }
            return roleName;
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