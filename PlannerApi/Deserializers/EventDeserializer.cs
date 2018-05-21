using events_planner.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace events_planner.Deserializers {
    public class EventDeserializer {

        [StringLength(255, MinimumLength = 3)]
        [MaxLength(255, ErrorMessage = "Title must be under 255 characters")]
        [MinLength(3, ErrorMessage = "Title must be at least 3 characters")]
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int SubscribeNumber { get; set; }

        [Required]
        public bool ValidationRequired { get; set; }

        public Status Status { get; set; } = Status.ONGOING;
        
        public int? JuryPoint { get; set; }

        public string Location { get; set; }

        [Required]
        public DateTime StartAt { get; set; }

        [Required]
        public DateTime CloseAt { get; set; }

        [Required]
        public DateTime OpenAt { get; set; }

        [Required]
        public DateTime EndAt { get; set; }

        public ICollection<Image> Image { get; set; }

        public void BindWithEventModel<T>(out T model) where T : Event {
            Event modelEvent = new Event() {
                Title = this.Title,
                Description = this.Description,
                SubscribeNumber = this.SubscribeNumber,
                ValidationRequired = this.ValidationRequired,
                Status = this.Status,
                JuryPoint = this.JuryPoint,
                Location = this.Location,
                StartAt = this.StartAt,
                CloseAt = this.CloseAt,
                OpenAt = this.OpenAt,
                EndAt = this.EndAt,
                Images = this.Image
            };

            if (modelEvent.ValidationRequired) {
                modelEvent.ValidatedNumber = 0;
            }

            model = modelEvent as T;
        }
    }
}