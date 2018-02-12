using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace events_planner.Models
{
    [Table("eventpromotion")]
    public class EventPromotion
    {
        [Column("event_id")]
        public int EventId { get; set; }

        [Column("promotion_id")]
        public int PromotionId { get; set; }

        public Promotion Promotion { get; set; }
        public Event Event { get; set; }
    }
}