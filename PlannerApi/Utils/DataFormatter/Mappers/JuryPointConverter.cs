using System.Collections.Generic;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using events_planner.Models;

namespace events_planner.Utils.DataFormatter.Mappers {

    public class JuryPointConverter : DefaultTypeConverter {
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData) {
            var juryPoints = (List<JuryPoint>)value;

            foreach (var juryPoint in juryPoints) {
                row.WriteField( juryPoint.Event.Title );
                row.WriteField( juryPoint.Points );
                row.WriteField( juryPoint.Description );
            }
            
            return null;
        }
    }
    
}