using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using events_planner.Models;
using Microsoft.EntityFrameworkCore.Migrations;

namespace events_planner.Utils.DataFormatter.Mappers {
    
    public class BookingDataMap : ClassMap<Booking> {
        public BookingDataMap() {
            Map(b => b.Present);
            References<UserDataMap>(b => b.User);
        }    
    }
}