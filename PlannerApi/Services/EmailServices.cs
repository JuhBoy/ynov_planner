using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Microsoft.AspNetCore.Hosting;
using events_planner.Utils;
using events_planner.Models;
using System.Threading;
using NLog;

namespace Microsoft.Extensions.DependencyInjection {

    public interface IEmailService {
        void SendFor(User user, Event @event, BookingTemplate template);
        List<EmailMessage> ReceiveEmail(int maxCount = 10);
    }

    public class EmailService : IEmailService {

        private readonly IEmailConfiguration _emailConfiguration;
        private readonly IHostingEnvironment _env;
        private readonly ITemplateGenerator _generator;


        public EmailService(IEmailConfiguration emailConfiguration,
                            IHostingEnvironment env,
                            ITemplateGenerator generator) {
            _emailConfiguration = emailConfiguration;
            _env = env;
            _generator = generator;
        }

        [Obsolete("Not implemented yet, please contact the developper")]
        public List<EmailMessage> ReceiveEmail(int maxCount = 10) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Send the specified emailMessage.
        /// </summary>
        /// <param name="emailMessage">Email message.</param>
        private void SendInternal(EmailMessage emailMessage) {
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

        public void SendFor(User user, Event @event, BookingTemplate template) {
            if (!_env.IsProduction()) {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"Email are skiped out of production mode {template.ToString()}");
                Console.ResetColor();
                return;
            }

            ThreadPool.QueueUserWorkItem(new WaitCallback((a) => {
                try {
                    string mailContent = _generator.GenerateFor(template, ref user, ref @event);

                    var message = new EmailMessage();
                    message.ToAddresses.Add(new EmailAddress() { Name = user.FullName, Address = user.Email });
                    message.FromAddresses.Add(new EmailAddress() {
                        Name = _emailConfiguration.SenderName,
                        Address = _emailConfiguration.SenderEmail
                    });
                    message.Content = mailContent;
                    message.Subject = "Ynov Event Informations";
                    SendInternal(message);
                } catch (Exception ex) {
                    LogManager.GetCurrentClassLogger().Error(ex);
                }
            }));
        }
    }
}
