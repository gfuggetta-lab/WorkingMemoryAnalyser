using System;
using System.Collections.Generic;
using System.Text;

namespace WMAData
{
    public class StageData
    {
        public int Markers;
        
        public int Shape; // see SHAPE_ constants. No used in S2.
                            // S2 is using ShapePos_XX instead
        public int Color; // Shape color. No used in S2.

        // distraction shape is not used in S1 or S2
        public int DistractShape; // used only on S3 and S4
        public int DistractColor; 

        public int Sound; // 1..99 
        public int Position; // see POS_ constants
        public int Duration; // miliseconds // Range_0_10000_ms

        // transition effect in miliseconds?
        public int Next_ISI; // S1_S2_ISI, etc. Also "Response_Time_after_S4"

        // ShapePos is not used in S1
        public int ShapePos_NW; // see SHAPE_ constants (only used for S2)
        public int ShapePos_NE; // see SHAPE_ constants (only used for S2)
        public int ShapePos_SE; // see SHAPE_ constants (only used for S2)
        public int ShapePos_SW; // see SHAPE_ constants (only used for S2)
        public int ShapePos_Center; // see SHAPE_ constants // S2_Shape_position_4(centre)
        public int ShapeClr_NW; // see COLOR_ constants
        public int ShapeClr_NE; // see COLOR_ constants
        public int ShapeClr_SE; // see COLOR_ constants
        public int ShapeClr_SW; // see COLOR_ constants
        public int ShapeClr_Center; // see COLOR_ constants 

    }

    public class TrialOrder
    {
        public int session_number;
        public StageData S1 = new StageData();
        public StageData S2 = new StageData();
        public StageData S3 = new StageData();
        public StageData S4 = new StageData();

        public int Feedback_shape; // see SHAPE_ constants

        public int Feedback_sound; // 0 - none, 1:correct_incorrect_auditory_feedback

        public int Feedback_duration_after_response_time; // milisecond. Range_200_10000_ms

        // Range_from_>=_response_time_plus_feedback to_10000_milliseconds
        public int ITI_after_feedback; // Inter_trial_interval

        public int key_mapping;
        public int task;

        public int TMS_S3_SOA = -100000; // milisecond

        public string ExpCondition;

        public List<string> Factors = new List<string>();
        public Dictionary<string, string> FactorLk = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
    }
}
 