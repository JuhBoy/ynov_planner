using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace events_planner.Models
{
    [Table("booking")]
    public class Booking
    {
        [Column("booking_id"), Key]
        public int Id { get; set; }

        [Column("present")]
        public bool? Present { get; set; }

        [Column("validated")]
        public bool? Validated { get; set; } = null;

        [Column("created_at"), JsonIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at"), JsonIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        /***********************
            RELATIONS
        ************************/

        /// <summary> relation with user (Many to One) </summary>
        [Column("user_id"), ForeignKey("user_id"), JsonIgnore]
        public int UserId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public User User { get; set; }

        /// <summary> relation with Event  (Many To One) </summary>
        [Column("event_id"), ForeignKey("event_id")]
        public int EventId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Event Event { get; set; }
    }
}
