using events_planner.Models;

namespace events_planner.Services {
    public interface IRoleServices {
        Role GetStudentRole();
        Role GetRoleByName(string name);
    }
}