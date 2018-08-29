using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace events_planner.Models {

    [Table("event_role")]
    public class EventRole {
        [Column("id"), JsonIgnore]
        public int ID { get; set; }

        [Column("event_id"), JsonIgnore]
        public int EventId { get; set; }

        [JsonIgnore]
        public Event Event { get; set; }

        [Column("role_id"), JsonIgnore]
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }

}
