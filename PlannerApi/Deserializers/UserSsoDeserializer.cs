using System.ComponentModel.DataAnnotations;

namespace events_planner.Deserializers
{
    public class UserSsoDeserializer
    {
        [MinLength(1, ErrorMessage = "Ticket mustn't be empty")]
        public string Ticket { get; set; }

        [MinLength(1, ErrorMessage = "Service mustn't be empty")]
        public string Service { get; set; }

        [Url]
        public string SsoUrl { get; set; }
    }
}