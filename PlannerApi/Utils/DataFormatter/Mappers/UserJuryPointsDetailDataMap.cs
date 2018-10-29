using CsvHelper;
using CsvHelper.Configuration;
using events_planner.Models;

namespace events_planner.Utils.DataFormatter.Mappers {
    public class UserJuryPointsDetailDataMap : UserDataMap {
        public UserJuryPointsDetailDataMap() {
            Map(m => m.JuryPoint).TypeConverter<JuryPointConverter>();
        }
    }
}