namespace events_planner.Deserializers
{
    public class UserSsoDeserializer
    {
        public string Ticket { get; set; }
        public string Service { get; set; }
        public string SsoUrl { get; set; }
    }
}