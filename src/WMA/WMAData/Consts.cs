using System;
using System.Collections.Generic;
using System.Text;

namespace WMAData
{
    public static class Consts
    {
        public const int SHAPE_NONE = 0;
        public const int SHAPE_DIAMON = 1; // diamond_area_equal_to_bar
        public const int SHAPE_HEX = 2; // hexagon_area_equal_to_bar
        public const int SHAPE_TRIANGLE = 3; // triangle_area_equal_to_bar
        public const int SHAPE_BOX = 4; // box_area_equal_to_bar
        public const int SHAPE_RING = 5; // ring_area_equal_to_bar
        public const int SHAPE_PLUS = 6; // plus_+_area_equal_to_bar
        public const int SHAPE_HORZ = 7; // horiz_bar
        public const int SHAPE_VERT = 8; // vert_bar
        public const int SHAPE_SQUARE = 9; // square_area_equal_to_bar
        public const int SHAPE_CIRCLE = 10; // circle_area_equal_to_bar
        public const int SHAPE_STAR = 11; // star
        public const int SHAPE_SKIP_TO_S2 = 12; // Go_to_S2_shape_(same/different_task)
        public const int SHAPE_SKIP_TO_S3 = 13; // Go_to_S3_shape_(identification_task)
        public const int SHAPE_SKIP_TO_S4 = 14; // Go_to_S4_shape_(identification_task)
        public const int SHAPE_CHAR_MIN = 33; // 33-255:character
        public const int SHAPE_CHAR_MAX = 255;
        public const int SHAPE_BMP_MIN = 300; // BMP_image
        public const int SHAPE_BMP_MAX = 10300;

        public const int POS_TOP_LEFT = 1;
        public const int POS_TOP_RIGHT = 2;
        public const int POS_BOT_LEFT = 3;
        public const int POS_BOT_RIGHT = 4;
        public const int POS_ALL = 5;
        public const int POS_CENTER = 6;

        public const int COLOR_WHITE0 = 0; // white_(20.97_cd/m^2)
        public const int COLOR_RED = 1; // red_(20.47_cd/m^2)
        public const int COLOR_GREEN = 1; // green_(20.57_cd/m^2)
        public const int COLOR_BLUE = 1; // blue_(20.84_cd/m^2)
        public const int COLOR_YELLOW = 4; // yellow_(20.70_cd/m^2)
        public const int COLOR_MAGENTA = 5; // magenta_(20.66_cd/m^2)
        public const int COLOR_WHITE6 = 6; // white_(20.97_cd/m^2)
        public const int COLOR_CYAN = 7; // Cyan_(20.63_cd/m^2)
        public const int COLOR_BROWN = 8; // Brown_(20.45_cd/m^2)
        public const int COLOR_WHITE9 = 9; // White_(20.97_cd/m^2)
        public const int COLOR_WHITE10 = 10; // white_(20.97_cd/m^2)
        public const int COLOR_PURPLE = 11; // Purple_(20.56_cd/m^2)
        public const int COLOR_BRIGHT_WHITE = 12; // Bright_white_(30.26_cd/m^2)
        //public const int COLOR_ // 13: Not_In_Use
        //public const int COLOR_ // 14: Not_In_Use
        //public const int COLOR_ // 15: Not_In_Use

        public const int TASK_S2_S4_SHAPE = 1;
        public const int TASK_S2_S4_COLOR = 2;
        public const int TASK_ID = 3;
        public const int TASK_BASELINE = 4; // no task

        public const int DEFAULT_REFRESH_RATE = 100;

        public const double distance_DEFAULT = 57; // centimeters
        public const double background_deg_DEFAULT = 18.3;
        public const double Fixation_dot_deg_DEFAULT = 0.3; // seems to be too much. 0.07 seems to be better

        // radius of target position in degrees
        public const double Placeholders_diameter_deg_DEFAULT = 14.2; // 10

        public const double Sample_diameter_deg_DEFAULT = 4.1;
        public const double Shape_size_deg_DEFAULT = 1.6;
        public const double Image_feedback_deg_DEFAULT = 3.2;
        public const double Image_size_deg_DEFAULT = 1.6; 

    }
}
