using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using events_planner.Deserializers;
using events_planner.Services;
using events_planner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.Generic;
using events_planner.PrimitiveExt;

namespace events_planner.Controllers {

    [Route("api/[controller]")]
    public class UserController : BaseController {

        public IUserServices UserServices;

        public UserController(IUserServices service, PlannerContext context) {
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

            User m_user = await Context.User
                                       .AsNoTracking()
                                       .Include(user => user.Role)
                                       .FirstOrDefaultAsync((User user) => user.Password == userCredential.Password
                                                                        && user.Email == userCredential.Login);

            if (m_user == null) { return NotFound(userCredential); }

            string token = UserServices.GenerateToken(ref m_user);

            return new ObjectResult(token);
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
        [HttpGet, Authorize(Roles = "Student, Admin")]
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

            User[] users = UserServices.AllForeignKeysQuery()
                                       .Where((arg) => userIds.Contains(arg.Id))
                                       .ToArray();
            return Ok(users);
        }

        /// <summary>
        /// Update a User using its credential
        /// </summary>?s
        /// <remarks>A user can be updated partialy</remarks>
        /// <response code="404">User not found</response>
        /// <response code="500">if the credential given is not valid</response>
        [HttpPatch("Update"), Authorize(Roles = "Student, Admin")]
        public async Task<IActionResult> Update([FromBody] UserUpdatableDeserializer userFromRequest) {
            string email = UserServices.ReadJwtTokenClaims(Request.Headers["Authorization"]);
            User user = await Context.User.FirstOrDefaultAsync((User u) => u.Email == email);

            if (user == null) { return NotFound(); }

            try {
                userFromRequest.BindWithUser(ref user);
                Context.Update(user);
                Context.SaveChanges();
            } catch (DbUpdateException update) {
                return BadRequest(update.Message);
            } catch (ValidationException e) {
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
        [HttpDelete("delete"), Authorize(Roles = "Student, Admin")]
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
            if (@event == null || @event.Expired()) 
                errors.Add("Event not found or expired");
            
            if (errors.Count > 0)
                return NotFound(errors);

            // Is already ann event ?
            if (Context.temporaryRoles.Any(tt => tt.EventId == eventId &&
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
                Context.temporaryRoles.Add(temp);
                Context.SaveChanges();
            } catch (DbUpdateException e) {
                return BadRequest(e.InnerException.Message);
            }

            return Ok();
        }

        [HttpDelete("temporary_role/{tempRoleId}"), Authorize(Roles = "Admin")]
        public IActionResult RemoveTempRole(int tempRoleId) {
            TemporaryRole temporary = Context.temporaryRoles
                                             .FirstOrDefault((arg) => arg.Id == tempRoleId);
            if (temporary == null) {
                return NotFound("Role not Found");
            }

            try {
                Context.temporaryRoles.Remove(temporary);
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
    }

}