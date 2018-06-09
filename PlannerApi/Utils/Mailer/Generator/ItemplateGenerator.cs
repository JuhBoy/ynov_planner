using events_planner.Models;

namespace events_planner.Utils {

    public interface ITemplateGenerator {

        string GenerateFor(BookingTemplate template, ref User user, ref Event @event);

        string[] GenerateFor(BookingTemplate templateName,
                                        ref User[] users, ref Event @event);
    }

}
