using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using events_planner.Deserializers;
using events_planner.Services;
using events_planner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using events_planner.PrimitiveExt;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using System.Xml.Serialization;
using events_planner.Utils;
using NLog;

namespace events_planner.Controllers {

    [Route("api/[controller]")]
    public class UserController : BaseController {

        public static readonly HttpClient HttpClient;

        public IUserServices UserServices;
        public IHostingEnvironment Env;

        static UserController() {
            HttpClient = new HttpClient();
            HttpClient.Timeout = new TimeSpan(hours:0, minutes:1, seconds:0);
        }

        public UserController(IUserServices service,
                                PlannerContext context,
                                IHostingEnvironment env) {
            Env = env;
            Context = context;
            UserServices = service;
        }

        /// <summary>
        /// Return a JWT token that match the current logs for a User
        /// </summary>
        /// <response code="404">User not found</response>
        /// <response code="500">if the credential given is not valid</response>
        [HttpPost("token"), AllowAnonymous]
        public async Task<IActionResult> GetToken([FromBody] UserConnectionDeserializer userCredential) {
            if (!ModelState.IsValid) return BadRequest();
            UserServices.GeneratePasswordSha256(userCredential.Password, out var passwordEncoded);

            User m_user = await Context.User
                                       .AsNoTracking()
                                       .Include(user => user.Role)
                                       .FirstOrDefaultAsync((User user) => user.Password == passwordEncoded
                                                                        && user.Email == userCredential.Login);

            if (m_user == null) { return NotFound(); }

            string token = UserServices.GenerateToken(ref m_user);

            return new ObjectResult(token);
        }


        [HttpPost("sso"), AllowAnonymous]
        public async Task<IActionResult> GetSso([FromBody] UserSsoDeserializer userSso) {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            string xml;

            try {
                xml = await HttpClient.GetStringAsync(
                    $"{userSso.SsoUrl}/serviceValidate?service={userSso.Service}&ticket={userSso.Ticket}");
                LogManager.GetCurrentClassLogger().Warn($"sso: {xml}");
            } catch (HttpRequestException e) {
                LogManager.GetCurrentClassLogger().Error($"Sso invalid {e.Message}");
                return BadRequest("Connexion Failed");
            }

            XmlSerializer parser = new XmlSerializer(typeof(ServiceResponse));
            ServiceResponse serviceResponse;

            using (var reader = new StringReader(xml)) {
                try {
                    serviceResponse = parser.Deserialize(reader) as ServiceResponse;
                } catch (InvalidOperationException e) {
                    LogManager.GetCurrentClassLogger().Error($"Sso invalid {e.Message}");
                    return BadRequest("Connexion Failed" + e.InnerException.Message);
                }
            }

            if (serviceResponse.AuthenticationFailure != null) {
                LogManager.GetCurrentClassLogger().Error($"Sso invalid: {xml}");
                return BadRequest("Connexion Failed");
            }

            User user = Context.User.Include(u => u.Role)
                                    .Include(u => u.Promotion)
                                    .FirstOrDefault(u => u.Email == serviceResponse.AuthenticationSuccess.Attributes.Email);
            try {
                if (user == null) {
                    UserServices.MakeUser(serviceResponse.AuthenticationSuccess.Attributes, out var nUser);
                    user = nUser;
                    Context.User.Add(user);
                } else if (UserServices.ShouldUpdateFromSSO(user, serviceResponse, out var properties)) {
                    UserServices.UpdateUserFromSsoData(user, serviceResponse, properties);
                    Context.User.Update(user);
                }

                await Context.SaveChangesAsync();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            string token = UserServices.GenerateToken(ref user);
            return Ok(token);
        }

        // ===============================
        //          User CRUD
        //================================

        /// <summary>
        /// Create a User
        /// </summary>
        /// <remarks>Create a new user using a given credential</remarks>
        /// <response code="500">if the credential given is not valid or the database is unreachable</response>
        [HttpPost, AllowAnonymous]
        public IActionResult Create([FromBody] UserCreationDeserializer userFromRequest) {
            if (!ModelState.IsValid) { return BadRequest(ModelState); };
            User userFromModel;

            try {
                userFromModel = UserServices.CreateUser(userFromRequest);
            } catch (DbUpdateException e) {
                string message = e.InnerException.Message;
                return BadRequest(message);
            }

            if (userFromModel == null) return BadRequest("Error");

            return new CreatedAtRouteResult(null, userFromModel);
        }

        /// <summary>
        /// Return the User model
        /// </summary>
        /// <remarks>return a set of informations about the user</remarks>
        /// <response code="404">User not found</response>
        [HttpGet, Authorize(Roles = "Student, Admin, Foreigner, Staff")]
        public async Task<IActionResult> Read() {
            string token = Request.Headers["Authorization"];
            string email = UserServices.ReadJwtTokenClaims(token);

            User user = await UserServices.AllForeignKeysQuery()
                                          .FirstOrDefaultAsync((User u) => u.Email == email);

            if (user == null) { return NotFound(email); }

            return Ok(user);
        }

        /// <summary>
        /// Return Users as a list
        /// </summary>
        /// <remarks>
        /// GET Params:
        ///     - search ([a-zA-Z]+ for Firstname and Lastname)
        ///     - range  (Two Numbers separated by "-")
        /// For the range param, the First number is the number of elements to skip,
        /// the second is how many elements you take.
        /// <br/> example : to get the elements from 7 to 13 ask for : 7-6 (skip the 7 first element then take 6)
        /// </remarks>
        [HttpGet("all"), Authorize(Roles = "Admin")]
        public IActionResult ReadAll() {
            string search = HttpContext.Request.Query["search"];
            string range = HttpContext.Request.Query["range"];
            IQueryable<User> query = UserServices.AllForeignKeysQuery();

            if (search != null) {
                UserServices.likeSearchQuery(ref query, search);
            }

            if (range != null) {
                int[] ranges = new int[2];
                try {
                    ranges = range.SplitRange("-");
                }
                catch (InvalidOperationException e) {
                    ranges[0] = 0;
                    ranges[1] = int.MaxValue;
                } finally {
                    query = query.Skip(ranges[0])
                                 .Take(ranges[1]);
                }
            }

            UserServices.WithouStaffMembers(ref query);
            UserServices.IncludeBookings(ref query);

            User[] users = query.AsNoTracking().ToArray();
            return Ok(users);
        }

        /// <summary>
        /// List all users for an event
        /// </summary>
        /// <returns>The list.</returns>
        /// <param name="eventId">Event identifier.</param>
        [HttpGet("event_list/{eventId}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserList(int eventId) {
            if (!Context.Event.Any(ev => ev.Id == eventId)) {
                return BadRequest("Event not found");
            }

            int[] userIds = Context.Booking
                                   .Where((arg) => arg.EventId == eventId)
                                   .Select((arg) => arg.UserId).ToArray();

            User[] users = await UserServices.AllForeignKeysQuery()
                                       .Where((arg) => userIds.Contains(arg.Id))
                                       .ToArrayAsync();
            return Ok(users);
        }

        /// <summary>
        /// Update a User using its credential
        /// </summary>?s
        /// <remarks>A user can be updated partialy</remarks>
        /// <response code="404">User not found</response>
        /// <response code="500">if the credential given is not valid</response>
        [HttpPatch("Update"), Authorize(Roles = "Student, Admin, Foreigner, Staff")]
        public async Task<IActionResult> Update([FromBody] UserUpdatableDeserializer userDsl) {
            string email = UserServices.ReadJwtTokenClaims(Request.Headers["Authorization"]);
            User user = await Context.User
                                     .Include(i => i.Role)
                                     .FirstOrDefaultAsync((User u) => u.Email == email);

            if (user == null) { return NotFound(); }

            if (!string.IsNullOrEmpty(userDsl.Password) && !UserServices.IsStudent(user)) {
                UserServices.GeneratePasswordSha256(userDsl.Password, out var encodedPassword);
                UserServices.GeneratePasswordSha256(userDsl.PasswordConfirmation, out var confirmedPassword);
                userDsl.Password = encodedPassword;
                userDsl.PasswordConfirmation = confirmedPassword;
            }

            try {
                UserServices.Update(userDsl, user);
                Context.Update(user);
                Context.SaveChanges();
            } catch (DbUpdateException update) {
                return BadRequest(update.Message);
            } catch (PasswordConfirmationException e) {
                return BadRequest(e.Message);
            }

            return Ok();
        }

        /// <summary>
        /// Delete User
        /// </summary>
        /// <remarks>Only User himself can delete it's account</remarks>
        /// <response code="200">User removed</response>
        /// <response code="404">User not found</response>
        /// <response code="500">Db update exception, database is down</response>
        [HttpDelete("delete"), Authorize(Roles = "Student, Admin, Foreigner, Staff")]
        public async Task<IActionResult> Delete() {
            string email = UserServices.ReadJwtTokenClaims(Request.Headers["Authorization"]);
            User user = await Context.User.FirstOrDefaultAsync((User u) => u.Email == email);

            if (user == null) { return NotFound(); }

            Context.User.Remove(user);
            Context.SaveChanges();

            return new ObjectResult("User Removed");
        }

        [HttpPost("temporary_role/{userId}/{roleId}/{eventId}"), Authorize(Roles = "Admin")]
        public IActionResult GiveTemporaryRole(int userId,
                                               int roleId,
                                               int eventId) {
            List<string> errors = new List<string>();
            User user = Context.User.FirstOrDefault(f => f.Id == userId);
            Role role = Context.Role.FirstOrDefault(f => f.Id == roleId);
            Event @event = Context.Event.FirstOrDefault(ff => ff.Id == eventId);

            if (role == null)
                errors.Add("Role not found");
            if (user == null)
                errors.Add("User not found");
            if (@event == null || !@event.Forward())
                errors.Add("Event not found or expired");

            if (errors.Count > 0)
                return NotFound(errors);

            // Is already ann event ?
            if (Context.TemporaryRoles.Any(tt => tt.EventId == eventId &&
                                           tt.UserId == userId &&
                                           tt.RoleId == roleId)) {
                return BadRequest("User Already has this role");
            }

            TemporaryRole temp = new TemporaryRole() {
                RoleId = roleId,
                EventId = eventId,
                UserId = userId
            };

            try {
                Context.TemporaryRoles.Add(temp);
                Context.SaveChanges();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return Ok();
        }

        [HttpDelete("temporary_role/{tempRoleId}"), Authorize(Roles = "Admin")]
        public IActionResult RemoveTempRole(int tempRoleId) {
            TemporaryRole temporary = Context.TemporaryRoles
                                             .FirstOrDefault((arg) => arg.Id == tempRoleId);
            if (temporary == null) {
                return NotFound("Role not Found");
            }

            try {
                Context.TemporaryRoles.Remove(temporary);
                Context.SaveChanges();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return Ok();
        }

        [HttpGet("{userId}/detail"), Authorize(Roles = "Admin")]
        public IActionResult GetUserDetail(int userId,
                                            [FromServices] IEventServices eventServices) {
            User user = UserServices.AllForeignKeysQuery()
                                    .FirstOrDefault(u => u.Id == userId);

            if (user == null) return NotFound("User Not Found");

            Event[] events = eventServices.GetParticipedEvents(userId).ToArray();

            return Ok(new {
                User = user,
                Events = events
            });
        }

        [HttpPost("export/{ids}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> CsvExport(string ids) {
            int[] iIds = ids.Split(',').Select(arg => int.Parse(arg)).ToArray();
            string fileName;
            if (iIds.Length <= 0) {
                return BadRequest("No Ids provided, unable to write the CSV file");
            }

            try {
                IEnumerable<User> users = Context.User.Include(a => a.JuryPoint)
                                                      .Include(a => a.Promotion)
                                                      .Where(arg => iIds.Contains(arg.Id));
                fileName = "export_" + DateTime.Now.ToString("HH_mm_ss") + ".csv";
                string path = Path.Combine(Env.WebRootPath, "csv", fileName);

                using (var stream = new StreamWriter(path, false)) {
                    var csvWriter = new CsvWriter(stream);
                    csvWriter.Configuration.RegisterClassMap<UserDataMap>();
                    csvWriter.WriteRecords(users);
                }
            } catch (ParserException e) {
                return BadRequest(e);
            } catch (IOException e) {
                return BadRequest(e);
            }

            return Ok(fileName);
        }
    }

    // =========================================
    // Used to provide a mapping for CSV export
    // =========================================
    public class UserDataMap : ClassMap<User> {
        public UserDataMap() {
            Map(m => m.LastName);
            Map(m => m.FirstName);
            Map(m => m.Email);
            Map(m => m.PhoneNumber);
            Map(m => m.TotalJuryPoints);
            Map(m => m.Promotion.Name).Name("promotion_name");
        }
    }

}