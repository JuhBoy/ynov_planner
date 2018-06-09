using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace events_planner.Models {

    public static class Status {
        public const string ValidRegex = @"ongoing|done|draft";
        public const string ONGOING = "ongoing";
        public const string DONE = "done";
        public const string DRAFT = "draft";
    }

    [Table("event")]
    public class Event {

        [Column("event_id"), Key]
        public int Id { get; set; }

        [Column("title")]
        [StringLength(255, MinimumLength = 3)]
        [MaxLength(255, ErrorMessage = "Title must be under 255 characters")]
        [MinLength(3, ErrorMessage = "Title must be at least 3 characters")]
        public string Title { get; set; }

        [Column("description"), Required]
        public string Description { get; set; }

        [Column("subscribe_number"), Required]
        public int SubscribeNumber { get; set; }

        [Column("subscribed_number"),
        JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int SubscribedNumber { get; set; } = 0;

        [Column("validation_number"),
         JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ValidatedNumber { get; set; } = null;

        [Column("validation_required")]
        public bool ValidationRequired { get; set; } = false;

        [Column("jury_point"),
         JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? JuryPoint { get; set; } = null;

        [Column("status"), Required]
        public string Status { get; set; }

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

        /// <summary>
        /// Gets or sets the start at.
        /// </summary>
        /// <value>When the subscriptions start.</value>
        [Column("start_at")]
        public DateTime? StartAt { get; set; }

        /// <summary>
        /// Gets or sets the end at.
        /// </summary>
        /// <value>When the subscription ends</value>
        [Column("end_at")]
        public DateTime? EndAt { get; set; }

        /// <summary>
        /// Gets or sets the open at.
        /// </summary>
        /// <value>When the event start</value>
        [Column("open_at"), Required]
        public DateTime? OpenAt { get; set; }

        /// <summary>
        /// Gets or sets the close at.
        /// </summary>
        /// <value>When the event ends</value>
        [Column("close_at"), Required]
        public DateTime? CloseAt { get; set; }

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
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<EventCategory> EventCategory { get; set; }

        /// <summary> relation with Promotion (Many to Many) </summary>
        [JsonIgnore] public IList<EventPromotion> EventPromotion { get; set; }

        /// <summary> relation with User (Many to Many) </summary>
        [JsonIgnore] public IList<EventUser> EventUser { get; set; }

        /// <summary> relation with moderators </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore,
                      DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IList<TemporaryRole> Moderators { get; set; }

        public bool HasSubscriptionWindow() {
            return (StartAt.HasValue || EndAt.HasValue);
        }

        /// <summary> Is Event expired ? </summary>
        public bool Expired() {
            int from = DateTime.Compare(DateTime.UtcNow, (DateTime)OpenAt);
            int to = DateTime.Compare(DateTime.UtcNow, (DateTime)CloseAt);

            return !(CloseAt == null || (from >= 0 && to < 0));
        }

        public bool Forward() {
            return DateTime.Compare(DateTime.UtcNow, (DateTime)CloseAt) <= 0;
        }

        public bool SubscribtionOpen() {
            var now = DateTime.UtcNow;
            bool isOpen = true;

            if (StartAt != null)
                isOpen &= DateTime.Compare(now, (DateTime)StartAt) >= 0;

            if (EndAt != null)
                isOpen &= DateTime.Compare(now, (DateTime)EndAt) <= 0;

            return isOpen;
        }
    }
}
