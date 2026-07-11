using System;
using System.Collections.Generic;
using System.Text;

namespace WMAData
{
    public class StimuliData
    {
        // Markers are used only for the Serial device reporting.
        // Those are not reflect on the screen in any way?
        public int Marker;

        // see SHAPE_ constants. 
        // For S2, the value isn't used, use ShapePos_XX instead
        public int Shape;

        // Shape color. No used in S2, use ShapeClr1_ instead
        public int Color;

        // distraction shape is not used in S1 or S2
        public int DistractShape;

        // used only on S3 and S4
        public int DistractColor;

        public int Sound; // 1..99 
        public int Position; // see POS_ constants
        public int Duration; // miliseconds // Range_0_10000_ms

        // transition effect in miliseconds?
        public int Next_ISI; // S1_S2_ISI, etc. Also "Response_Time_after_S4"

        // ShapePos is not used in S1
        public int ShapePos1_NW; // see SHAPE_ constants (only used for S2)
        public int ShapePos2_NE; // see SHAPE_ constants (only used for S2)
        public int ShapePos3_SE; // see SHAPE_ constants (only used for S2)
        public int ShapePos4_SW; // see SHAPE_ constants (only used for S2)
        public int ShapePos5_Center; // see SHAPE_ constants // S2_Shape_position_4(centre)
        public int ShapeClr1_NW; // see COLOR_ constants
        public int ShapeClr2_NE; // see COLOR_ constants
        public int ShapeClr3_SE; // see COLOR_ constants
        public int ShapeClr4_SW; // see COLOR_ constants
        public int ShapeClr5_Center; // see COLOR_ constants 

    }
}
