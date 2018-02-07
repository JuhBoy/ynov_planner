using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace events_planner.Models
{
    [Table("price")]
    public class Price
    {
        [Column("price_id")]
        [Key]
        public int Id { get; set; }

        [Column("price")]
        [Required]
        public int Amount { get; set; }

        [Column("created_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        /***********************
            RELATIONS
        ************************/

        /// <summary> relation with Event (Many to One) </summary>
        [Column("event_id")]
        [ForeignKey("event_id")]
        public int EventId { get; set; }
        public Event Event { get; set; }

        /// <summary> relation with Role (Many to Many) </summary>
        [Column("role_id")]
        [ForeignKey("role_id")]
        public int RoleId { get; set; }

        public Role Role { get; set; }
    }
}