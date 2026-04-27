using System;
using System.Collections.Generic;
using System.Text;

namespace WMAData
{
    public class TrialMonitor
    {
        public int widthPx;
        public int heightPx;
        public double widthCm;
        public double heightCm;

        public static TrialMonitor DefaultMonitor()
        {
            return new TrialMonitor
            {
                widthPx = 1920,
                heightPx = 1080,
                widthCm = 52.4,
                heightCm = 29.6
            };
        }
    }
}
