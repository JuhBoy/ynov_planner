using events_planner.Models;
using System.ComponentModel.DataAnnotations;
using System;
using Microsoft.Data.OData.Query.SemanticAst;

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

        public float? JuryPoint { get; set; }

        public string Location { get; set; }

        public DateTime? StartAt { get; set; }

        public DateTime? CloseAt { get; set; }

        public DateTime? OpenAt { get; set; }

        public DateTime? EndAt { get; set; }

        public int[] RestrictedRolesList { get; set; } = null;
    }
}
