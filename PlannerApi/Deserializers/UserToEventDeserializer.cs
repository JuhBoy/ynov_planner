using System.ComponentModel.DataAnnotations;

namespace events_planner.Deserializers {
    public class UserToEventDeserializer {
        
        [Required]
        public int userId { get; set; }
        
        [Required]
        public int eventId { get; set; }
    }
}