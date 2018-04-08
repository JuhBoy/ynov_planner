using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace events_planner.Models
{
    public enum Status {
        DRAFT = 1,
        ONGOING = 2,
        DONE = 3
    }

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
        [Required]
        public Status Status { get; set; }

        [Column("location")]
        public string Location { get; set; }

        [Column("created_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [JsonIgnore]
        public DateTime UpdatedAt { get; set; }

        [Column("start_at")]
        [Required]
        public DateTime? StartAt { get; set; }

        [Column("close_at")]
        [Required]
        public DateTime? CloseAt { get; set; }

        [Column("open_at")]
        public DateTime? OpenAt { get; set; }

        [Column("end_at")]
        public DateTime? EndAt { get; set; }

        /***********************
            RELATIONS
        ************************/

        [ForeignKey("top_events_id"), JsonIgnore, Column("top_events_id")]
        public int? TopEventsId { get; set; }

        [JsonIgnore]
        public TopEvents TopEvents { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<Image> Images { get; set; }

        /// <summary> relation with Bookings (One to Many) </summary>
        [JsonIgnore] public ICollection<Booking> Bookings { get; set; }

        /// <summary> relation with Prices (One to Many) </summary>
        [JsonIgnore] public ICollection<Price> Prices { get; set; }

        /// <summary> relation with Category (Many to Many) </summary>
        [JsonIgnore] public IList<EventCategory> EventCategory { get; set; }

        /// <summary> relation with Promotion (Many to Many) </summary>
        [JsonIgnore] public IList<EventPromotion> EventPromotion { get; set; }

        /// <summary> relation with User (Many to Many) </summary>
        [JsonIgnore] public IList<EventUser> EventUser { get; set; }
    }
}