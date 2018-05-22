using System.Collections.Generic;

namespace events_planner.App_Start {
    
    public class EmailMessage {

        /// <summary>
        /// Gets or sets to users addresses.
        /// </summary>
        /// <value>To addresses.</value>
        public List<EmailAddress> ToAddresses { get; set; }

        /// <summary>
        /// Gets or sets from addresses.
        /// </summary>
        /// <value>From addresses.</value>
        public List<EmailAddress> FromAddresses { get; set; }

        /// <summary>
        /// Gets or sets the Email's subject.
        /// </summary>
        /// <value>The subject.</value>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the Email's content.
        /// </summary>
        /// <value>The content.</value>
        public string Content { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:events_planner.App_Start.EmailMessage"/> class.
        /// </summary>
        public EmailMessage() {
            ToAddresses = new List<EmailAddress>();
            FromAddresses = new List<EmailAddress>();
        }
    }
}
