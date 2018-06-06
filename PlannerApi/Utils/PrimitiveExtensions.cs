using System;

namespace events_planner.PrimitiveExt {
    public static class PrimitiveExtensions {
        public static int[] SplitRange(this string fsm, string spliter) {
            string[] s = fsm.Split(spliter);
            
            if (s.Length != 2) {
                throw new InvalidOperationException("Range must be at least two numbers separated by < - >");
            }
            
            int[] r = new int[2];

            try {
                r[0] = int.Parse(s[0]);
                r[1] = int.Parse(s[1]);
            }
            catch (FormatException e) {
                throw new InvalidOperationException("Elements From range arn't in the good format [0-9]+");
            }

            return r;
        }
    }
}