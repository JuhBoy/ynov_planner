using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using events_planner.Models;

namespace events_planner.Utils.DataFormatter.Mappers
{
    public class UserDataMap : ClassMap<User> {
        public UserDataMap() {
            Map(m => m.LastName);
            Map(m => m.FirstName);
            Map(m => m.Email);
            Map(m => m.PhoneNumber);
            Map(m => m.Promotion.Name).Name("promotion_name");
            Map(m => m.Participations);
            Map(m => m.TotalJuryPoints);
        }
    }
}