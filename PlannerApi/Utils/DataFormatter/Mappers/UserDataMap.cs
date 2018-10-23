using CsvHelper.Configuration;
using events_planner.Models;

namespace events_planner.Utils.DataFormatter.Mappers
{
    public class UserDataMap : ClassMap<User> {
        public UserDataMap() {
            Map(m => m.LastName);
            Map(m => m.FirstName);
            Map(m => m.Email);
            Map(m => m.PhoneNumber);
            Map(m => m.TotalJuryPoints);
            Map(m => m.Promotion.Name).Name("promotion_name");
        }
    }
}