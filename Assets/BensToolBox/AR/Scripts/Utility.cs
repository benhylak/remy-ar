using System;

namespace BensToolBox.AR.Scripts
{
    public static class Utility
    {
        public static double Map(double x, double in_min, double in_max, double out_min, double out_max, bool clamp = false)
        {
            if (clamp) x = Math.Max(in_min, Math.Min(x, in_max));
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }
    }
}