using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using events_planner.Deserializers;
using events_planner.Services;
using events_planner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

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
            return new ObjectResult("authorized");
        }

        [HttpPatch]
        public async Task<IActionResult> PartialUpdate() {
            return new ObjectResult(new { token = "lala" });
        }

        [HttpPatch]
        public async Task<IActionResult> FullUpdate()
        {
            return new ObjectResult(new { token = "lala" });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            return new ObjectResult(new { token = "lala" });
        }

    }

}