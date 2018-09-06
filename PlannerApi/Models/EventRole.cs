using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace events_planner.Models {

    [Table("event_role")]
    public class EventRole {
        [Column("id")]
        public int ID { get; set; }

        [Column("event_id")]
        public int EventId { get; set; }

        [JsonIgnore, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Event Event { get; set; }

        [Column("role_id")]
        public int RoleId { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Role Role { get; set; }
    }

}
