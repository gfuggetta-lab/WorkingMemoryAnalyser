using System;
using System.Collections.Generic;
using System.Text;

namespace WMAData
{
    public struct ColorFloat
    {
        public double r;
        public double g;
        public double b;

        public override string ToString()
        {
            return $"r:{r};g:{g}b:{b}";
        }

        public static ColorFloat White = new ColorFloat { r = 1.0, g = 1.0, b = 1.0 };
        public static ColorFloat Black = new ColorFloat { r = 0.0, g = 0.0, b = 0.0 };
    }
}
