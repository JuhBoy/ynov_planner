using System;

namespace events_planner.Utils.Formatters {

    public static class TimeFormatter {

        public static string GetTimeWindowFrom(DateTime d1, DateTime d2) {
            TimeSpan window = d2 - d1;
            int hours = Math.Abs(window.Hours);
            int minutes = Math.Abs(window.Minutes);

            if (hours <= 0)
                return $"{minutes} Min";

            return $"{hours} H : {minutes} Min";
        }
    }
}