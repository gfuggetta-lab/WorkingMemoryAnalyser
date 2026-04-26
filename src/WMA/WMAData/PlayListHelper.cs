using System;
using System.Collections.Generic;
using System.Text;
using static WMAData.Consts;

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

        public static PlayItem AddCircleHollow(this PlayList list, double radiusCm, ColorFloat color, double timeOfs = 0, double durationMs = -1)
        {
            var res = AddCircle(list, radiusCm, color, timeOfs, durationMs);
            res.itemType = PlayItemType.CircleHollow;
            return res;
        }

        public static PlayItem AddCircle(this PlayList list, double radiusCm, ColorFloat color, double timeOfs = 0, double durationMs = -1)
        {
            if (list == null) return null;
            var pt = list.Add(timeOfs);
            pt.itemType = PlayItemType.CircleFilled;
            pt.radiusCm = radiusCm;
            pt.color = color;
            pt.durationMs = durationMs;
            pt.pos = PlayItemPos.Center;
            return pt;
        }


        public static PlayItem AddImageById(this PlayList list, int imageId, double ImageSize, ColorFloat color, double timeOfs = 0, double durationMs = -1)
        {
            if (list == null) return null;
            var pt = list.Add(timeOfs);
            pt.itemType = PlayItemType.ImageById;
            pt.imageId = imageId;
            pt.sizeCm = ImageSize;
            pt.color = color;
            pt.durationMs = durationMs;
            pt.pos = PlayItemPos.Center;
            return pt;
        }

        public static PlayItem AddByShape(this PlayList list, int Shape, 
            double ImageSzCm, 
            double shapeSzCm, ColorFloat color, double timeOfs = 0, double durationMs = -1)
        {
            if ((Shape >= SHAPE_BMP_MIN) && (Shape <= SHAPE_BMP_MAX))
                return AddImageById(list, Shape, ImageSzCm, color, timeOfs, durationMs);
            return AddCircle(list, shapeSzCm / 2, color, timeOfs, durationMs);
        }

        public static PlayItemPos ToPlayPos(int pos)
        {
            switch (pos)
            {
                case POS_TOP_LEFT: return PlayItemPos.NW;
                case POS_TOP_RIGHT: return PlayItemPos.NE;
                case POS_BOT_LEFT: return PlayItemPos.SW;
                case POS_BOT_RIGHT: return PlayItemPos.SE;
                case POS_CENTER: return PlayItemPos.Center;
                default: 
                    return PlayItemPos.Undefined;
            }
        }
    }
}
