using events_planner.Models;
using System.ComponentModel.DataAnnotations;
using System;

namespace events_planner.Deserializers {

    public class EventUpdatableDeserializer {

        [StringLength(255, MinimumLength = 3)]
        [MaxLength(255, ErrorMessage = "Title must be under 255 characters")]
        [MinLength(3, ErrorMessage = "Title must be at least 3 characters")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Range(0, 1000)]
        public int? SubscribeNumber { get; set; }

        [RegularExpression(Models.Status.ValidRegex)]
        public string Status { get; set; }

        public bool? ValidationRequired { get; set; }

        public bool? RestrictedEvent { get; set; }

        public int? JuryPoint { get; set; }

        public string Location { get; set; }

        public DateTime? StartAt { get; set; }

        public DateTime? CloseAt { get; set; }

        public DateTime? OpenAt { get; set; }

        public DateTime? EndAt { get; set; }

        public string[] RemoveRestrictedRolesList { get; set; }

        public string[] AddRestrictedRolesList { get; set; }

        public void BindWithModel(ref Event model) {
            if (Title != null)
                model.Title = Title;

            if (Description != null)
                model.Description = Description;

            if (SubscribeNumber.HasValue)
                model.SubscribeNumber = (int)SubscribeNumber;

            if (Status != null)
                model.Status = Status;

            if (Location != null)
                model.Location = Location;

            if (StartAt.HasValue)
                model.StartAt = StartAt;

            if (CloseAt.HasValue)
                model.CloseAt = CloseAt;

            if (EndAt.HasValue)
                model.EndAt = EndAt;

            if (JuryPoint.HasValue)
                model.JuryPoint = JuryPoint;

            if (ValidationRequired.HasValue && ValidationRequired != model.ValidationRequired) {
                model.ValidationRequired = (bool) ValidationRequired;
                model.ValidatedNumber = 0;
            }

            if (RestrictedEvent.HasValue)
                model.RestrictedEvent = (bool) RestrictedEvent;
        }
    }
}
