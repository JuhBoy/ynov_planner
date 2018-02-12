using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace events_planner.Models
{
    [Table("eventcategory")]
    public class EventCategory
    {
        [Column("event_id")]
        public int EventId { get; set; }

        [Column("category_id")]
        public int CategoryId { get; set; }

        public Category Category { get; set; }
        public Event Event { get; set; }
    }
}