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

        public bool RestrictedEvent { get; set; }

        [RegularExpression(Models.Status.ValidRegex)]
        public string Status { get; set; } = Models.Status.PENDING;

        [Range(0, int.MaxValue, ErrorMessage = "The Jury Points must be superior to 0")]
        public int? JuryPoint { get; set; }

        public string Location { get; set; }

        public DateTime? StartAt { get; set; }

        public DateTime? EndAt { get; set; }

        [Required]
        public DateTime CloseAt { get; set; }

        [Required]
        public DateTime OpenAt { get; set; }

        public ICollection<Image> Images { get; set; }

        public Price[] Prices { get; set; }

        public string[] AddRestrictedRolesList { get; set; }
    }
}
