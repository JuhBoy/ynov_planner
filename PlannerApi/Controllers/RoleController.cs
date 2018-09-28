using System.Threading.Tasks;
using events_planner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace events_planner.Controllers {
    
    [Route("api/[controller]")]
    public class RoleController : BaseController {

        public RoleController(PlannerContext context) {
            Context = context;
        }
        
        /// <summary>
        /// Get The list of all roles
        /// </summary>
        /// <returns>Array[Role]</returns>
        [HttpGet("list"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRoles() {
            return Ok(await Context.Role.ToArrayAsync());
        }
        
    }
}