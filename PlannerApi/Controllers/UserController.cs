using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using events_planner.Deserializers;
using events_planner.Services;
using events_planner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace events_planner.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        public IUserServices services;
        public PlannerContext Context;

        public UserController(IUserServices service, PlannerContext context)
        {
            Context = context;
            services = service;
        }

        /// <summary>
        /// Return a JWT token that match the current logs for a User
        /// </summary>
        /// <response code="404">User not found</response>
        /// <response code="500">if the credential given is not valid</response>
        [HttpPost("token"), AllowAnonymous]
        public async Task<IActionResult> GetToken([FromBody] UserConnectionDeserializer userCredential)
        {
            if (!ModelState.IsValid) return BadRequest();

            User m_user = await Context.User
                                       .AsNoTracking()
                                       .Include(user => user.Role)
                                       .FirstOrDefaultAsync((User user) => user.Password == userCredential.Password && user.Username == userCredential.Login);

            if (m_user == null) { return NotFound(userCredential); }

            string token = services.GenerateToken(ref m_user);

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
        public IActionResult Create([FromBody] UserCreationDeserializer userFromRequest)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); };
            User userFromModel;

            try {
              userFromModel = services.CreateUser(userFromRequest);
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
        public async Task<IActionResult> Read()
        {
            string token = Request.Headers["Authorization"];
            string email = services.ReadJwtTokenClaims(token);

            User user = await Context.User
                                     .AsNoTracking()
                                     .Include(inc => inc.Role)
                                     .FirstOrDefaultAsync((User u) => u.Email == email);

            if (user == null) { return NotFound(email); }

            return new ObjectResult(new {
                user.Email,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.Username,
                user.DateOfBirth,
                user.Role.Name
            });
        }

        /// <summary>
        /// Update a User using its credential
        /// </summary>
        /// <remarks>A user can be updated partialy</remarks>
        /// <response code="404">User not found</response>
        /// <response code="500">if the credential given is not valid</response>
        [HttpPatch("Update"), Authorize(Roles = "Student, Admin")]
        public async Task<IActionResult> Update([FromBody] UserUpdatableDeserializer userFromRequest) {
            string email = services.ReadJwtTokenClaims(Request.Headers["Authorization"]);
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
            string email = services.ReadJwtTokenClaims(Request.Headers["Authorization"]);
            User user = await Context.User.FirstOrDefaultAsync((User u) => u.Email == email);

            if (user == null) { return NotFound(); }

            Context.User.Remove(user);
            Context.SaveChanges();

            return new ObjectResult("User Removed");
        }
    }

}