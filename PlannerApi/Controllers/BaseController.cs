using Microsoft.AspNetCore.Mvc;
using events_planner.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;

namespace events_planner.Controllers {
    public abstract class BaseController : Controller {
        
        protected PlannerContext Context { get; set; }

        private User _currentUser;

        protected User CurrentUser {
            get {
                if (_currentUser != null) {
                    return _currentUser;
                }

                return _currentUser = Context.User
                              .Include(inc => inc.Role)
                              .FirstOrDefault(user => user.Email == HttpContext.User.FindFirstValue(ClaimTypes.Email));
            }
        }
	}
}
