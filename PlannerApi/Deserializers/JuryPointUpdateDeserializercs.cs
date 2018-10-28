using System.ComponentModel.DataAnnotations;
using events_planner.Models;

namespace events_planner.Deserializers {
    public class JuryPointUpdateDeserializer {
        [Required]
        public int Id { get; set; }
        
        public float? Points { get; set; }
        
        public string Description { get; set; }
    }
}