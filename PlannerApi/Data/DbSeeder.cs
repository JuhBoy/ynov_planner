using System;
using System.Linq;
using events_planner.Models;
using Microsoft.EntityFrameworkCore;

using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace events_planner.Data {
    public static class DbSeeder {

        public static Object safeThreadObject = new Object();

        public const string PROMOTIONS_PATH = "Data/Promotions.json";
        public const string ROLE_PARH = "Data/Roles.json";

        public const string USERS_TEST_PARTH = "../../../../PlannerApi/Data/UsersTest.json";
        public const string PROMOTIONS_TEST_PATH = "../../../../PlannerApi/Data/Promotions.json";
        public const string ROLE_TEST_PARH = "../../../../PlannerApi/Data/Roles.json";

        public static void Initialize(PlannerContext context) {
            lock (safeThreadObject) {
            context.Database.Migrate();

                if (!context.Promotion.Any())
                {
                    List<Promotion> promotions = JsonConvert.DeserializeObject<List<Promotion>>(File.ReadAllText(PROMOTIONS_PATH));
                    context.Promotion.AddRange(promotions);
                }

                if (!context.Role.Any()) {
                    List<Role> roles = JsonConvert.DeserializeObject<List<Role>>(File.ReadAllText(ROLE_PARH));
                    context.Role.AddRange(roles);
                }

                context.SaveChanges();
            }
        }

        public static void InitializeTest(PlannerContext context) {
            context.Database.Migrate();
            RemoveAdded(context);

            if (!context.User.Any()) {
                List<Promotion> promotions = JsonConvert.DeserializeObject<List<Promotion>>(File.ReadAllText(PROMOTIONS_TEST_PATH));
                List<Role> role = JsonConvert.DeserializeObject<List<Role>>(File.ReadAllText(ROLE_TEST_PARH));
                List<User> users = JsonConvert.DeserializeObject<List<User>>(File.ReadAllText(USERS_TEST_PARTH));

                foreach (User user in users) {
                    user.Promotion = promotions.First();
                    user.Role = role.First();
                }
                context.User.AddRange(users);
                context.SaveChanges();
            }
        }

        private static void RemoveAdded(PlannerContext context) {
            if (context.User.Count() > 2) {
                context.User.RemoveRange(context.User.ToList());
                context.SaveChanges();
            }
        }
    }
}