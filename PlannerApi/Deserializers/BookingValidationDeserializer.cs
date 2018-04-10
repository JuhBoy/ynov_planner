using System;
using System.ComponentModel.DataAnnotations;

namespace events_planner.Deserializers {

    public class BookingValidationDeserializer {

        [Required]
        public int EventId { get; set; }

        [Required]
        public int UserId { get; set; }

    }
}
