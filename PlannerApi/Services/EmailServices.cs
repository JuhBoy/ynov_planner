using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace events_planner.App_Start {
    
    public interface IEmailService {
        void Send(EmailMessage emailMessage);
        List<EmailMessage> ReceiveEmail(int maxCount = 10);
    }

    public class EmailService : IEmailService {
        
        private readonly IEmailConfiguration _emailConfiguration;

        public EmailService(IEmailConfiguration emailConfiguration) {
            _emailConfiguration = emailConfiguration;
        }

        [Obsolete("Not implemented yet, please contact the developper")]
        public List<EmailMessage> ReceiveEmail(int maxCount = 10) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Send the specified emailMessage.
        /// </summary>
        /// <param name="emailMessage">Email message.</param>
        public void Send(EmailMessage emailMessage) {
            var message = new MimeMessage();
            message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));

            message.Subject = emailMessage.Subject;
            message.Body = new TextPart(TextFormat.Html) {
                Text = emailMessage.Content
            };

            using (var emailClient = new SmtpClient()) {
                emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, _emailConfiguration.UseSSL);

                //Remove any OAuth functionality
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                emailClient.Authenticate(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);
                emailClient.Send(message);
                emailClient.Disconnect(true);
            }
        }
    }
}
