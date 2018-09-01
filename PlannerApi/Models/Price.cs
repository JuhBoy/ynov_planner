using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace events_planner.Models
{
    [Table("price")]
    public class Price
    {
        [Column("price_id"), Key]
        public int Id { get; set; }

        [Column("price"), Required, Range(0, float.MaxValue)]
        public float Amount { get; set; }

        [Column("created_at"),
         DatabaseGenerated(DatabaseGeneratedOption.Identity),
         JsonIgnore]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed), JsonIgnore]
        public DateTime UpdatedAt { get; set; }

        /***********************
            RELATIONS
        ************************/

        /// <summary> relation with Event (Many to One) </summary>
        [Column("event_id"), ForeignKey("event_id"), JsonIgnore]
        public int EventId { get; set; }

        [JsonIgnore]
        public Event Event { get; set; }

        /// <summary> relation with Role (Many to Many) </summary>
        [ForeignKey("role_id"), Column("role_id")]
        public int RoleId { get; set; }

        [JsonIgnore]
        public Role Role { get; set; }
        
        [NotMapped, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string roleName {
            get { return Role?.Name;  }
        }
    }
}