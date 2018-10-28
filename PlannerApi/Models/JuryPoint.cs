using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace events_planner.Models {

    [Table("jury_point")]
    public class JuryPoint {

        [Column("id"), Key]
        public int Id { get; set; }

        [Column("points")]
        public float Points { get; set; } = 0;

        [Column("user_id"), ForeignKey("user_id"), JsonIgnore]
        public int UserId { get; set; }
        
        [JsonIgnore]
        public User User { get; set; }
        
        /// <summary>
        /// Description as been made when Jury Point is not linked with an event
        /// </summary>
        [Column("description")]
        public string Description { get; set; }
        
        /// <summary>
        /// Jury point can be linked or not with an event
        /// Optional
        /// </summary>
        [Column("event_id"), ForeignKey("event_id")]
        public int? EventId { get; set; }
        
        /// <summary>
        /// The Event linked (if exist)
        /// </summary>
        [JsonIgnore]
        public Event Event { get; set; }
        
    }
}
