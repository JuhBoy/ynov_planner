namespace events_planner.Utils {

	public class TemplateMailerConfiguration {

        public string ServerDomain { get; set; }

        public string MailerPath { get; set; }

        public string FacebookUrl { get; set; }

        public string InstagramUrl { get; set; }

        public string TwitterUrl { get; set; }

        public string YnovUrl { get; set; }

    }

    public enum BookingTemplate {
        AWAY,
        PRESENT,
        RECALL,
        AUTO_VALIDATED,
        SUBSCRIPTION_VALIDATED,
        PENDING_VALIDATION
    }
}
