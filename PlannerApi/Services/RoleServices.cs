using System.Linq;
using events_planner.Models;
using events_planner.Services;

namespace Microsoft.Extensions.DependencyInjection {
    public class RoleServices : IRoleServices {
        public PlannerContext Context { get; set; }

        public RoleServices(PlannerContext context) {
            Context = context;
        }

        Role IRoleServices.GetRoleByName(string name) {
            return Context.Role.FirstOrDefault((Role arg) => arg.Name == name);
        }

        Role IRoleServices.GetStudentRole() {
            return Context.Role.FirstOrDefault((Role arg) => arg.Name == "Student");
        }
    }
}