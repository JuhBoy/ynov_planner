using events_planner.Models;
using System.ComponentModel.DataAnnotations;
using System;

namespace events_planner.Deserializers
{
    public class EventUpdatableDeserializer
    {

        [StringLength(255, MinimumLength = 3)]
        [MaxLength(255, ErrorMessage = "Username must be under 255 characters")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Range(0, 1000)]
        public int? SubscribeNumber { get; set; }

        [Range(1, 3)]
        public int? Status { get; set; }

        public string Location { get; set; }

        public DateTime? StartAt { get; set; }

        public DateTime? CloseAt { get; set; }

        public DateTime? OpenAt { get; set; }

        public DateTime? EndAt { get; set; }

        public string Image { get; set; }

        public void BindWithModel(ref Event model) {
            if (Title != null)
                model.Title = Title;

            if (Description != null)
                model.Description = Description;
            
            if (Image != null)
                model.Image = Image;
            
            if (SubscribeNumber.HasValue)
                model.SubscribeNumber = (int) SubscribeNumber;

            if (Status.HasValue)
                model.Status = (Status)Status;
            
            if (Location != null)
                model.Location = Location;

            if (StartAt.HasValue)
                model.StartAt = StartAt;

            if (CloseAt.HasValue)
                model.CloseAt = CloseAt;

            if (EndAt.HasValue)
                model.EndAt = EndAt;
        }
    }
}