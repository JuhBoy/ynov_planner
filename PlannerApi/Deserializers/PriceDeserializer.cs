using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.KeyVault.Models;

namespace events_planner.Deserializers {
    public class PriceDeserializer {
        
        [Range(0, int.MaxValue, ErrorMessage = "Price cannot be under 0 value")]
        public int Amount { get; set; }
        
        public int EventId { get; set; }
        
        public int RoleId { get; set; }
    }
}