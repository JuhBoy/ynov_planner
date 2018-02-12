using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using events_planner.Deserializers;
using events_planner.Services;
using Microsoft.AspNetCore.Authorization;

namespace events_planner.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        public IUserServices services;

        public UserController(IUserServices service) {
            services = service;
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> GetToken([FromBody] UserConnectionDeserializer userCredential) {
            if (!ModelState.IsValid) return BadRequest();
            
            // Access to the user's token
            object token = await services.GetToken(userCredential.login, userCredential.password);

            return new ObjectResult(token);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
