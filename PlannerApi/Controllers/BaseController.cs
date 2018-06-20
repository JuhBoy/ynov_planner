using Microsoft.AspNetCore.Mvc;
using events_planner.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;
using events_planner.Utils;

namespace events_planner.Controllers {

    public abstract class BaseController : Controller {

        protected PlannerContext Context { get; set; }

        private User _currentUser;

        protected User CurrentUser {
            get {
                if (_currentUser != null) {
                    return _currentUser;
                }
                var user = _currentUser = Context.User
                              .Include(inc => inc.Role)
                              .FirstOrDefault(uu => uu.Email == HttpContext.User.FindFirstValue(ClaimTypes.Email));
                if (user == null) {
                    throw new NotFoundUserException("User Invalid");
                }
                return user;
            }
        }
	}
}
