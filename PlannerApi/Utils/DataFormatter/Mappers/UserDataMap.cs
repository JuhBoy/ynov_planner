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
            Map(m => m.LastName).Name("Nom");
            Map(m => m.FirstName).Name("Prénom");
            Map(m => m.Email);
            Map(m => m.PhoneNumber).Name("Téléphone");
            Map(m => m.Promotion.Name).Name("Promotion");
            Map(m => m.Participations).Name("Participations");
            Map(m => m.TotalJuryPoints).Name("Points Jury");
        }
    }
}