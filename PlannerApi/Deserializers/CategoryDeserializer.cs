using events_planner.Models;
using System.ComponentModel.DataAnnotations;

namespace events_planner.Deserializers {
    public class CategoryDeserializer {
        [Required]
        [StringLength(20, MinimumLength = 2)]
        [MaxLength(20, ErrorMessage = "Username must be under 20 characters")]
        [MinLength(2, ErrorMessage = "Username must be at least 2 characters")]
        public string Name { get; set; }
    }
}