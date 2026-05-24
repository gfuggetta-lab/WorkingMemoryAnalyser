using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using static WMAData.Consts;

namespace WMAData
{
    public static class PlayListHelper
    {
        // sourceNo = 0 no sound.
        public static void PlaySoundAt(this PlayList list, int soundId, double ofsTimeMs)
        {
            if (soundId == 0) return;
            list.AddSound(soundId.ToString(), ofsTimeMs);
        }
        public static PlayItem AddSound(this PlayList list, string soundId, double ofsTimeMs)
        {
            if (list == null) return null;
            var pt = list.Add(ofsTimeMs);
            pt.itemType = PlayItemType.Sound;
            pt.soundId = soundId;
            return pt;
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

        public static PlayItem AddText(this PlayList list, string text, string font, ColorFloat color, double timeOfs = 0, double durationMs = -1)
        {
            if (list == null) return null;
            var pt = list.Add(timeOfs);
            pt.itemType = PlayItemType.Text;
            pt.fontName = font;
            pt.text = text;
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
            if ((Shape >= SHAPE_CHAR_MIN) && (Shape <= SHAPE_CHAR_MAX))
            {
                var text = new string((char)((UInt16)Shape), 1);
                return AddText(list, text, "", color, timeOfs, durationMs);
            }
            return AddCircle(list, shapeSzCm / 2, color, timeOfs, durationMs);
        }

        public static PlayItem SetPos(this PlayItem itm, int dstPos, double posDistCm)
        {
            return itm.SetPos(ToPlayPos(dstPos), posDistCm);

        }

        public static PlayItem SetPos(this PlayItem itm, PlayItemPos dstPos, double posDistCm)
        {
            if (itm == null)
                return itm;
            itm.pos = dstPos;
            itm.posDistanceCm = posDistCm;
            return itm;
        }

        public static PlayItem SetFont(this PlayItem itm, string font, double fontSizePix)
        {
            if (itm == null)
                return itm;
            itm.fontName = font;
            itm.fontSizePx = (int)fontSizePix;
            return itm;
        }


        public static PlayItem SetColor(this PlayItem itm, ColorFloat clr)
        {
            if (itm == null)
                return itm;
            itm.color = clr;
            return itm;
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

        public static PlayItem StartTrial(this PlayList list, string name, double ofsMs)
        {
            var itm = list.Add(ofsMs);
            itm.itemType = PlayItemType.TrialStart;
            itm.text = name;
            itm.startMs = ofsMs;
            return itm;
        }
        public static PlayItem EndTrial(this PlayList list, string name, double ofsMs)
        {
            var itm = list.Add(ofsMs);
            itm.itemType = PlayItemType.TrialEnd;
            itm.text = name;
            itm.startMs = ofsMs;
            return itm;
        }
        public static PlayItem StartSection(this PlayList list, string name, double ofsMs, double duration)
        {
            var itm = list.Add(ofsMs);
            itm.itemType = PlayItemType.SectionStart;
            itm.text = name;
            itm.startMs = ofsMs;
            itm.durationMs = duration;
            return itm;
        }
    }
}
