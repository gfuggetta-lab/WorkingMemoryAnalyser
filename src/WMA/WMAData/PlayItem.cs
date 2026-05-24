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
        
        // the lower value is drawn first
        public int drawOrder; // the time doesn't matter

        public PlayItemPos pos = PlayItemPos.Undefined;
        public double posDistanceCm;
        public double lineWidthCm;
        public int posOther;
        public int posCount;

        public int imageId; // used only for ImageById

        public PlayItemType itemType = PlayItemType.None;
        public double radiusCm;
        
        public double sizeCm; // width and height for either image or other shapes... other than circles... why not circles?

        public ColorFloat color;

        // used by PLayItemPos.Text
        public string fontName;
        public string text;
        public int fontSizePx; // font size in godot pixels

        public string soundId; // extensions might not be needed
    }

    public enum PlayItemPos
    { 
        Undefined,
        Center,
        NE, NW, SE, SW, 
        OneOfCount, // the position is defined by N out of Count. Where N=0 is NE position.
        Other,
    }


    public enum PlayItemType
    {
        None,
        CircleFilled, // radiusCm
        CircleHollow, // radiusCm
        ImageById,
        Sound,
        Text, // draw text, centered at the pos
        TrialStart, // does nothing, only debugging
        TrialEnd, // does nothing, only debugging
        SectionStart, // does nothing, only debugging
    }
}
