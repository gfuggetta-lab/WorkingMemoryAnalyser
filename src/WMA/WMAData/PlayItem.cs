using System;
using System.Collections.Generic;
using System.Text;

namespace WMAData
{
    public class PlayItem
    {
        // the start time
        public double startMs;
        // the duration time
        public double durationMs;

        public int sameTimeOrder;

        public PlayItemType itemType = PlayItemType.None;
        public double radiusCm;
        public ColorFloat color;
    }

    public enum PlayItemType
    {
        None,
        Circle, // radiusCm
        Sound
    }
}
