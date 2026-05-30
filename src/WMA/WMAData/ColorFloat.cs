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
    }
}
