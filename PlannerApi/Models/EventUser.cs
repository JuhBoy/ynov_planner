using System.ComponentModel.DataAnnotations.Schema;

namespace events_planner.Models
{
    [Table("eventuser")]
    public class EventUser
    {
        [Column("event_id")]
        public int EventId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        public User User { get; set; }
        public Event Event { get; set; }
    }
}