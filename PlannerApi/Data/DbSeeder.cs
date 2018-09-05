using System;
using System.Linq;
using events_planner.Models;
using Microsoft.EntityFrameworkCore;

using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting.Internal;
using Newtonsoft.Json;

namespace events_planner.Data {
    public static class DbSeeder {

        public static Object safeThreadObject = new Object();

        public const string PROMOTIONS_PATH = "/Data/Promotions.json";
        public const string ROLE_PARH = "/Data/Roles.json";
        public const string USERS_PATH = "/Data/Users.json";

        public const string USERS_TEST_PATH = "/Fixtures/UsersTest.json";
        public const string PROMOTIONS_TEST_PATH = "/Fixtures/Promotions.json";
        public const string ROLE_TEST_PATH = "/Fixtures/Roles.json";
        public const string EVENTS_TEST_PATH = "/Fixtures/Events.json";

        public const string EVENTS_PATH = "/Data/Events.json";

        public static void Initialize(PlannerContext context, string ENV) {
            lock (safeThreadObject) {
                context.Database.Migrate();

                string currentDirectory = Directory.GetCurrentDirectory();
                List<Promotion> promotions = JsonConvert.DeserializeObject<List<Promotion>>(File.ReadAllText(currentDirectory + PROMOTIONS_PATH));
                List<Role> roles = JsonConvert.DeserializeObject<List<Role>>(File.ReadAllText(currentDirectory + ROLE_PARH));

                if (!context.Promotion.Any()) {
                    context.Promotion.AddRange(promotions);
                }

                if (!context.Role.Any()) {
                    context.Role.AddRange(roles);
                }

                if (!context.User.Any()) {
                    List<User> users = JsonConvert.DeserializeObject<List<User>>(File.ReadAllText(currentDirectory + USERS_PATH));
                    foreach (var user in users) {
                        if (user.Email.Contains("admin")) {
                            user.Role = roles.FirstOrDefault(e => e.Name.Equals("Admin"));
                            user.Promotion = promotions.FirstOrDefault(e => e.Name == "STAFF");
                        } else {
                            user.Role = roles.FirstOrDefault(e => e.Name.Equals("Student"));
                            user.Promotion = promotions.FirstOrDefault(e => e.Name == "YNOV-Student");
                        }
                    }
                    context.User.AddRange(users);
                }

                if (ENV == "Development" && !context.Event.Any()) {
                    List<Event> events = JsonConvert.DeserializeObject<List<Event>>(File.ReadAllText(currentDirectory + EVENTS_PATH));
                    context.Event.AddRange(events);
                }

                context.SaveChanges();
            }
        }

        public static void InitializeTest(PlannerContext context) {
            context.Database.Migrate();

            string currentDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            
            List<Promotion> promotions = JsonConvert.DeserializeObject<List<Promotion>>(File.ReadAllText(currentDirectory + PROMOTIONS_TEST_PATH));
            List<Role> roles = JsonConvert.DeserializeObject<List<Role>>(File.ReadAllText(currentDirectory + ROLE_TEST_PATH));
            List<User> users = JsonConvert.DeserializeObject<List<User>>(File.ReadAllText(currentDirectory + USERS_TEST_PATH));
            List<Event> events = JsonConvert.DeserializeObject<List<Event>>(File.ReadAllText(currentDirectory + EVENTS_TEST_PATH));
            
            context.Event.AddRange(events);
            
            if (!context.Role.Any())
                context.Role.AddRange(roles);
        
            if (!context.User.Any()) {
                foreach (User user in users) {
                    user.Promotion = promotions.First();
                    if (user.Email.Contains("admin")) {
                        user.Role = roles.FirstOrDefault(r => r.Name.Equals("Admin"));
                    }
                    else {
                        user.Role = roles.First();
                    }
                }
            }
            context.User.AddRange(users);
            context.SaveChanges();
        }
    }
}