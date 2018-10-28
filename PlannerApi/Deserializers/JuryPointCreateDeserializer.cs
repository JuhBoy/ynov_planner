using System.ComponentModel.DataAnnotations;

namespace events_planner.Deserializers
{
    public class JuryPointCreateDeserializer
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public float Points { get; set; }
        
        public string Description { get; set; }
    }
}