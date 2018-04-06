using Microsoft.AspNetCore.Mvc;
using events_planner.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;

namespace events_planner.Controllers {
    public abstract class BaseController : Controller {
        protected PlannerContext Context { get; set; }

        protected User CurrentUser {
            get {
                return Context.User
                              .Include(inc => inc.Role)
                              .FirstOrDefault(user => user.Email == HttpContext.User.FindFirstValue(ClaimTypes.Email));
            }
        }
	}
}
