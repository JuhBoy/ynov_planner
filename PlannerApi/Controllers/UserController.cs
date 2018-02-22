using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using events_planner.Deserializers;
using events_planner.Services;
using events_planner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;
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

        [HttpPost("token")]
        public async Task<IActionResult> GetToken([FromBody] UserConnectionDeserializer userCredential)
        {
            if (!ModelState.IsValid) return BadRequest();

            User m_user = await Context.User
                                       .FirstOrDefaultAsync((User user) => user.Password == userCredential.Password && user.Username == userCredential.Login);

            if (m_user == null) { return NotFound(userCredential); }

            string token = services.GenerateToken(ref m_user);

            return new ObjectResult(token);
        }

        // ===============================
        //          User CRUD
        //===============================

        [HttpPost]
        public IActionResult Create([FromBody] UserCreationDeserializer userFromRequest)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); };
            User userFromModel;

            try {
              userFromModel = services.CreateUser(userFromRequest);
            } catch (DbUpdateException e) {
                string message = e.InnerException.Message;
                return new ObjectResult(message);
            }

            if (userFromModel == null) return BadRequest("Error");

            return new CreatedAtRouteResult(null, userFromModel);
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> Read()
        {
            string token = Request.Headers["Authorization"];
            string email = services.ReadJwtTokenClaims(token);

            User user = await Context.User.Include(inc => inc.Role).FirstOrDefaultAsync((User u) => u.Email == email);

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

        [HttpPatch("Update"), Authorize]
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

        [HttpDelete]
        public async Task<IActionResult> Delete() {
            return new ObjectResult(new { token = "No Delete Implemented" });
        }
    }

}