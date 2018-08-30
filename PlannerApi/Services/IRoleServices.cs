using events_planner.Models;

namespace events_planner.Services {
    public interface IRoleServices {
        Role GetStudentRole();
        Role GetForeignerRole();
        Role GetRoleByName(string name);
    }
}