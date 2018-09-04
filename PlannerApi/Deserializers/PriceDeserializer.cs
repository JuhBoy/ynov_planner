using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.KeyVault.Models;

namespace events_planner.Deserializers {
    public class PriceDeserializer {
        
        [Range(0, float.MaxValue, ErrorMessage = "Price cannot be under 0 value")]
        public float Amount { get; set; }
        
        public int EventId { get; set; }
        
        public int RoleId { get; set; }
    }
}