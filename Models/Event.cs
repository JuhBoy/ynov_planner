using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace events_planner.Models
{
    [Table("event")]
    public class Event
    {
        [Column("event_id")]
        [Key]
        public int Id { get; set; }

        [Column("title")]
        [StringLength(255, MinimumLength = 3)]
        [MaxLength(255, ErrorMessage = "Username must be under 255 characters")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        public string Title { get; set; }

        [Column("description")]
        [Required]
        public string Description { get; set; }

        [Column("subscribe_number")]
        [Required]
        public int SubscribeNumber { get; set; }

        [Column("status")]
        [StringLength(30, MinimumLength = 3)]
        [ConcurrencyCheck]
        [Required]
        public string Status { get; set; }

        [Column("location")]
        public JSON DateOfBirth { get; set; }

        [Column("created_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; }

        [Column("start_at")]
        [Required]
        public DateTime StartAt { get; set; }

        [Column("open_at")]
        public DateTime OpenAt { get; set; }

        [Column("end_at")]
        public DateTime EndAt { get; set; }
    }
}