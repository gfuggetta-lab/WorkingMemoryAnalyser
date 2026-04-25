using System;
using System.Collections.Generic;
using System.Text;

namespace WMAData
{
    public static class PlayListHelper
    {
        // sourceNo = 0 no sound.
        public static void PlaySound(this PlayList list, double ofsTimeMs, int sourceNo)
        {
            if (list == null) return;
            if (sourceNo == 0) return;
            list.Add(ofsTimeMs);
        }

        // show an image for the duration of time
        public static void ShowImage(this PlayList list, double ofsTimeMs, double durationMs, int sourceNo)
        {
            if (list == null) return;
            if (sourceNo == 0) return;
            list.Add(ofsTimeMs);
        }

        public static PlayItem AddCircle(this PlayList list, double radiusCm, ColorFloat color, double timeOfs = 0, double durationMs = -1)
        {
            if (list == null) return null;
            var pt = list.Add(timeOfs);
            pt.itemType = PlayItemType.Circle;
            pt.radiusCm = radiusCm;
            pt.color = color;
            pt.durationMs = durationMs;
            return pt;
        }
    }
}
