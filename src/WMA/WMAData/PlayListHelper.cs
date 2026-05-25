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

        public static PlayItem AddBar(this PlayList list, double lengthCm, double widthCm, double theta, ColorFloat color, double timeOfs = 0, double durationMs = -1)
        {
            if (list == null) return null;
            var pt = list.Add(timeOfs);
            pt.itemType = PlayItemType.Bar;
            pt.barLengthCm = lengthCm;
            pt.barWidthCm = widthCm;
            pt.barTheta = theta;
            pt.color = color;
            pt.durationMs = durationMs;
            pt.pos = PlayItemPos.Center;
            return pt;
        }

        public static PlayItem AddPlus(this PlayList list, double lengthCm, double widthCm, ColorFloat color, double timeOfs = 0, double durationMs = -1)
        {
            if (list == null) return null;
            var pt = list.Add(timeOfs);
            pt.itemType = PlayItemType.Plus;
            pt.barLengthCm = lengthCm;
            pt.barWidthCm = widthCm;
            pt.color = color;
            pt.durationMs = durationMs;
            pt.pos = PlayItemPos.Center;
            return pt;
        }

        public static PlayItem AddStar(this PlayList list, double outerRadiusCm, ColorFloat color, double timeOfs = 0, double durationMs = -1)
        {
            if (list == null) return null;
            var pt = list.Add(timeOfs);
            pt.itemType = PlayItemType.Star;
            pt.radiusCm = outerRadiusCm;
            pt.color = color;
            pt.durationMs = durationMs;
            pt.pos = PlayItemPos.Center;
            return pt;
        }

        public static PlayItem AddRegularShape(this PlayList list, double outerRadiusCm, double lineWidthCm, int pointCount, bool filled, double rotation, ColorFloat color, double timeOfs = 0, double durationMs = -1)
        {
            if (list == null) return null;
            var pt = list.Add(timeOfs);
            pt.itemType = PlayItemType.RegularShape;
            pt.radiusCm = outerRadiusCm;
            pt.lineWidthCm = lineWidthCm;
            pt.regularPoints = pointCount;
            pt.regularFilled = filled;
            pt.regularRotation = rotation;
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
            if (Shape == SHAPE_NONE)
                return null;
            if ((Shape >= SHAPE_BMP_MIN) && (Shape <= SHAPE_BMP_MAX))
                return AddImageById(list, Shape, ImageSzCm, color, timeOfs, durationMs);
            if ((Shape >= SHAPE_CHAR_MIN) && (Shape <= SHAPE_CHAR_MAX))
            {
                var text = new string((char)((UInt16)Shape), 1);
                return AddText(list, text, "", color, timeOfs, durationMs);
            }

            switch (Shape)
            {
                case SHAPE_DIAMOND:
                    double szD = Math.Sqrt(2.0 * Math.Pow(shapeSzCm / 2.0, 2.0));
                    return AddBar(list, szD, szD, Math.PI / 4.0, color, timeOfs, durationMs);
                
                case SHAPE_HEX:
                    double rH = shapeSzCm / 2.0;
                    return AddRegularShape(list, rH, rH * 0.01, 6, true, 0.0, color, timeOfs, durationMs);

                case SHAPE_TRIANGLE:
                    double rT = Math.Sqrt(3.0) / 3.0 * shapeSzCm;
                    return AddRegularShape(list, rT, rT * 0.01, 3, true, Math.PI, color, timeOfs, durationMs);

                case SHAPE_BOX:
                    double rB = Math.Sqrt(2.0 * Math.Pow(shapeSzCm / 2.0, 2.0));
                    return AddRegularShape(list, rB, rB * 0.66, 4, false, Math.PI / 4.0, color, timeOfs, durationMs);

                case SHAPE_RING:
                    double rG = shapeSzCm / 2.0;
                    return AddRegularShape(list, rG, rG * 0.5, 20, false, 0.0, color, timeOfs, durationMs);

                case SHAPE_PLUS:
                    return AddPlus(list, shapeSzCm, shapeSzCm * 0.33, color, timeOfs, durationMs);

                case SHAPE_HORZ:
                    return AddBar(list, shapeSzCm, shapeSzCm * 0.5, 0.0, color, timeOfs, durationMs);

                case SHAPE_VERT:
                    return AddBar(list, shapeSzCm, shapeSzCm * 0.5, Math.PI / 2.0, color, timeOfs, durationMs);

                case SHAPE_SQUARE:
                    return AddBar(list, shapeSzCm, shapeSzCm, 0.0, color, timeOfs, durationMs);

                case SHAPE_CIRCLE:
                    return AddCircle(list, shapeSzCm / 2, color, timeOfs, durationMs);

                case SHAPE_STAR:
                    // Star with radius equal to placeholder's radius. Area of 0.82627615cm2 on AOC monitor at 71cm
                    // To make this equal area to bar, replace the decimal value below with 1.145
                    //r := tan(0.69230769  *(pi/180) * SCALE_FACTOR * Shape_scale_factor)*ef.distance;
                    //r := (sqrt(5) - 1)* shapeSizeCM / 2; // using (shapeSize/2) for inscribed circle of the star
                    // r - is now a side of a pentagon
                    double PENTAGON_ANGLE = (5 - 2) * 180 / 5;
                    double PENTAGON_SIDETORADIUS = Math.Sqrt(10) * Math.Sqrt(5 + Math.Sqrt(5)) / 10;
                    double rS = (shapeSzCm / 2) / Math.Sin(DegToRad(PENTAGON_ANGLE / 2));
                    // r is now a radius of
                    rS = PENTAGON_SIDETORADIUS * rS; // sqrt(10)*sqrt(5 + sqrt(5)) / 10 * r;  
                    return AddStar(list, rS, color, timeOfs, durationMs);

                default:
                    return null;
            }
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

        public static double DegToRad(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }
    }
}
