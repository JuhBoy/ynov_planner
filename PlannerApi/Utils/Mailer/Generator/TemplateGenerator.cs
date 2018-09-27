using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using events_planner.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using events_planner.Utils.Formatters;

namespace events_planner.Utils {

    public class TemplateGenerator : ITemplateGenerator {

        public TemplateMailerConfiguration Configuration { get; set; }

        private IHostingEnvironment Environment { get; set; }

        private readonly string FILE_EXT = ".cshtml";

        public TemplateGenerator(IHostingEnvironment hostingEnvironment) {
            Environment = hostingEnvironment;
        }

        public string GenerateFor(BookingTemplate template, ref User user, ref Event @event) {
            var users = new User[] { user };
            return GenerateFor(template, ref users, ref @event)[0];
        }

        /// <summary>
        /// Generate a string[] from a template file located in templates path
        /// </summary>
        /// <returns>The from razor file as string.</returns>
        /// <param name="template">Template name without extension.</param>
        public string[] GenerateFor(BookingTemplate template, ref User[] users, ref Event @event) {
            string templaseAsUtf8String = null;
            string path = Path.Combine(Environment.ContentRootPath,
                                       Configuration.MailerPath,
                                       template.ToString().ToLower() + FILE_EXT);
            string[] templates = new string[users.Length];

            using (var stream = new StreamReader(File.Open(path, FileMode.Open))) {
                string fileContent = stream.ReadToEnd();
                if (string.IsNullOrEmpty(fileContent)) {
                    throw new TemplateEmptyException("File content is null or empty");
                }
                templaseAsUtf8String = fileContent;
            }

            for (int i = 0; i < users.Length; i++) {
                templates[i] = Format(fileContent: templaseAsUtf8String,
                                      template: template, user: ref users[i], @event: ref @event);
            }

            return templates;
        }

        private string Format(BookingTemplate template, string fileContent, ref User user,
                              ref Event @event) {
            var content = fileContent;
            var root =  Configuration.ServerDomain;
            string mainIconPath = null;

            content = content.Replace("{{facebook_url}}", Configuration.FacebookUrl);
            content = content.Replace("{{facebook_icon}}", Path.Combine(root, "images/facebook.svg"));

            content = content.Replace("{{instagram_url}}", Configuration.InstagramUrl);
            content = content.Replace("{{instagram_icon}}", Path.Combine(root, "images/instagram.svg"));

            content = content.Replace("{{twitter_url}}", Configuration.TwitterUrl);
            content = content.Replace("{{twitter_icon}}", Path.Combine(root, "images/twitter.svg"));

            content = content.Replace("{{ynov_url}}", Configuration.YnovUrl);

            content = content.Replace("{{logo}}", Path.Combine(root, "images/logo.png"));

            content = content.Replace("{{user_firstname}}", user.FirstName);
            content = content.Replace("{{event_name}}", @event.Title);

            mainIconPath = "images/" + template.ToString().ToLower() + ".png";

            switch (template) {
                case BookingTemplate.AWAY:
                    break;
                case BookingTemplate.PRESENT:
                    content = content.Replace("{{jurypoint_number}}", (@event.JuryPoint ?? 0).ToString());
                    break;
                case BookingTemplate.RECALL:
                    string remain = TimeFormatter.GetTimeWindowFrom(DateTime.Now, (DateTime)@event.OpenAt);
                    content = content.Replace("{{remaining_hours}}", remain);
                    AddEventInfos(ref content, ref @event);
                    break;
                case BookingTemplate.AUTO_VALIDATED:
                    mainIconPath = "images/validate.png";
                    AddEventInfos(ref content, ref @event);
                    break;
                case BookingTemplate.SUBSCRIPTION_VALIDATED:
                    mainIconPath = "images/validate.png";
                    AddEventInfos(ref content, ref @event);
                    break;
                case BookingTemplate.PENDING_VALIDATION:
                    content = content.Replace("{{subscribed_at}}", DateTime.Now.ToString());
                    break;
            }

            content = content.Replace("{{main_icon}}", Path.Combine(root, mainIconPath));

            content = content.Replace(System.Environment.NewLine, "");

            return content;
        }

        private void AddEventInfos(ref string content, ref Event @event) {
            content = content.Replace("{{event_date}}", @event.OpenAt?.ToString());
            content = content.Replace("{{event_open_at_hour}}", @event.OpenAt?.ToShortTimeString());
            content = content.Replace("{{event_close_at_hour}}", @event.CloseAt?.ToShortTimeString());

            if (!string.IsNullOrEmpty(@event.Location)) {
                int start = @event.Location.IndexOf("?q=") + 3;
                String location = @event.Location.Substring(start, @event.Location.Length - start);
                content = content.Replace("{{event_address}}", location);
            }
        }
    }
}
