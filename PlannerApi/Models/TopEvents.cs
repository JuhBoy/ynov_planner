using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace events_planner.Models {
    [Table("top_events")]
    public class TopEvents {

        [Key, Column("top_events_id"), JsonIgnore]
        public int TopEventsId { get; set; }

        [Required]
        public int Index { get; set; }

        public string Name { get; set; }

        [Required, Column("event_id"), ForeignKey("event_id"), JsonIgnore]
        public int EventId { get; set; }

        public Event Event { get; set; }
    }
}
