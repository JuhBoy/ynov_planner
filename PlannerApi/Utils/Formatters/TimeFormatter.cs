using System;

namespace events_planner.Utils.Formatters {

    public static class TimeFormatter {

        public static string GetTimeWindowFrom(DateTime d1, DateTime d2) {
            TimeSpan window = d2 - d1;
            int hours = Math.Abs(window.Hours);
            string minutes = Math.Abs(window.Minutes).ToString();

            if (hours <= 0)
                return $"{minutes} Min";

            return $"{hours.ToString()} H : {minutes} Min";
        }
    }
}