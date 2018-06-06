using events_planner.Models;

namespace events_planner.Utils {

    public interface ITemplateGenerator {
        string[] GenerateFor(BookingTemplate templateName,
                                        ref User[] users, ref Event @event);
    }

}
