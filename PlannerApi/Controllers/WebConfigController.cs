using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using events_planner.Models;


namespace events_planner.Controllers {

    [Route("api/[controller]")]
    public class WebConfigController : BaseController {

        public PlannerContext Context { get; }

        public WebConfigController(PlannerContext context) {
            Context = context;
        }

        /// <summary>
        /// Retrun the webconfig if it exist
        /// </summary>
        /// <returns>The Web Config as a single hash object</returns>
        [HttpGet, AllowAnonymous]
        public IActionResult GetWebConfig() {
            var conf = Context.WebConfig.FirstOrDefault(arg => arg.Name != null);

            try {
                conf.SessionCount += 1;
                Context.WebConfig.Update(conf);
                Context.SaveChanges();
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }

            return Ok(conf);
        }

        /// <summary>
        /// Remove past web config and add new
        /// </summary>
        /// <returns>The Web Config as a single hash object</returns>
        [HttpPut, AllowAnonymous]
        public IActionResult PutWebConfig([FromBody] WebConfig WebConfig) {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }

            try {
                var conf = Context.WebConfig.FirstOrDefault(arg => arg.Name != null);
                if (conf != null) {
                    Context.WebConfig.Remove(conf);
                }
                Context.WebConfig.Add(WebConfig);
                Context.SaveChanges();
            } catch (DbUpdateException e) {
                return BadRequest(e.Message);
            }

            return Ok(WebConfig);
        }
    }
}
