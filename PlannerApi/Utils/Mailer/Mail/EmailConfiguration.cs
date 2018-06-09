namespace events_planner.Utils {

    public interface IEmailConfiguration {
        string SenderName { get; set; }
        string SenderEmail { get; set; }

        bool UseSSL { get; set; }

        // SMTP Server Options
        string SmtpServer { get; }
        int SmtpPort { get; }
        string SmtpUsername { get; set; }
        string SmtpPassword { get; set; }

        // POP Server Options
        string PopServer { get; }
        int PopPort { get; }
        string PopUsername { get; }
        string PopPassword { get; }
    }

    public class EmailConfiguration : IEmailConfiguration {
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }

        // SSL USE:
        public bool UseSSL { get; set; }

        // SMTP SERVER OPTIONS:

        public string SmtpServer { get; set; }

        public int SmtpPort { get; set; }

        public string SmtpUsername { get; set; }

        public string SmtpPassword { get; set; }

        // POP SERVER OPTIONS:

        public string PopServer { get; set; }

        public int PopPort { get; set; }

        public string PopUsername { get; set; }

        public string PopPassword { get; set; }
    }
}
