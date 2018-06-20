using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace events_planner.Models
{
    [Table("eventcategory")]
    public class EventCategory
    {
        [Column("event_id"), JsonIgnore]
        public int EventId { get; set; }

        [Column("category_id"), JsonIgnore]
        public int CategoryId { get; set; }

        public Category Category { get; set; }
        
        [JsonIgnore]
        public Event Event { get; set; }
    }
}