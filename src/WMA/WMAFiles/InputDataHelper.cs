using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using WMAData;

namespace WMAFiles
{
    public static class InputDataHelper
    {
        public static int GetInt(this InputDataReader rdr, string name, int defVal = 0)
        {
            if (!rdr.TryGetVal(name, out var v))
                return defVal;

            if (!int.TryParse(v, out var result))
                return defVal;

            return result;
        }

        public static string GetStr(this InputDataReader rdr, string name, string defVal = "")
        {
            if (!rdr.TryGetVal(name, out var v))
                return defVal;
            return v;
        }

        public static bool FillTrialData(this InputDataReader rdr, TrialOrder dst)
        {
            if (rdr == null) return false;
            if (dst == null) return false;

            dst.session_number = rdr.GetInt("session_number", dst.session_number);
            dst.S1.Markers = rdr.GetInt("S1_Markers", dst.S1.Markers);
            dst.S2.Markers = rdr.GetInt("S2_Markers", dst.S2.Markers);
            dst.S3.Markers = rdr.GetInt("S3_Markers", dst.S3.Markers);
            dst.S4.Markers = rdr.GetInt("S4_Markers", dst.S4.Markers);
            dst.S1.Shape = rdr.GetInt("S1_Shape", dst.S1.Shape);
            dst.S1.Sound = rdr.GetInt("S1_Sound", dst.S1.Sound);
            dst.S1.Position = rdr.GetInt("S1_Position", dst.S1.Position);
            dst.S1.Duration = rdr.GetInt("S1_Duration", dst.S1.Duration);
            dst.S1.Next_ISI = rdr.GetInt("S1_S2_ISI", dst.S1.Next_ISI);

            // S2 doesn't have "shape"
            dst.S2.ShapePos1_NW = rdr.GetInt("S2_Shape_position_1(NW)", dst.S2.ShapePos1_NW);
            dst.S2.ShapePos2_NE = rdr.GetInt("S2_Shape_position_2(NE)", dst.S2.ShapePos2_NE);
            dst.S2.ShapePos3_SE = rdr.GetInt("S2_Shape_position_3(SE)", dst.S2.ShapePos3_SE);
            dst.S2.ShapePos4_SW = rdr.GetInt("S2_Shape_position_4(SW)", dst.S2.ShapePos4_SW);
            dst.S2.ShapePos5_Center = rdr.GetInt("S2_Shape_position_4(centre)", dst.S2.ShapePos5_Center);
            dst.S2.Sound = rdr.GetInt("S2_Sound", dst.S2.Sound);
            dst.S2.Duration = rdr.GetInt("S2_Duration", dst.S2.Duration);
            dst.S2.Next_ISI = rdr.GetInt("S2_3_ISI", dst.S2.Next_ISI);

            dst.S3.Shape = rdr.GetInt("S3_Shape", dst.S3.Shape);
            dst.S3.DistractShape = rdr.GetInt("S3_distractor_shape", dst.S3.DistractShape);
            dst.S3.Sound = rdr.GetInt("S3_Sound", dst.S3.Sound);
            dst.S3.Position = rdr.GetInt("S3_Position", dst.S3.Position);
            dst.S3.Duration = rdr.GetInt("S3_Duration", dst.S3.Duration);
            dst.S3.Next_ISI = rdr.GetInt("S3_S4_ISI", dst.S3.Next_ISI);

            dst.S4.Shape = rdr.GetInt("S4_Shape", dst.S4.Shape);
            dst.S4.DistractShape = rdr.GetInt("S4_distractor_shape", dst.S4.DistractShape);
            dst.S4.Sound = rdr.GetInt("S4_Sound", dst.S4.Sound);
            dst.S4.Position = rdr.GetInt("S4_Position", dst.S4.Position);
            dst.S4.Duration = rdr.GetInt("S4_Duration", dst.S4.Duration);
            dst.S4.Next_ISI = rdr.GetInt("Response_Time_after_S4", dst.S4.Next_ISI);

            dst.Feedback_shape = rdr.GetInt("Feedback_shape", dst.Feedback_shape);
            dst.Feedback_sound = rdr.GetInt("Feedback_Sound", dst.Feedback_sound);
            dst.Feedback_duration_after_response_time = rdr.GetInt("Feedback_duration_after_response_time", dst.Feedback_duration_after_response_time);
            dst.ITI_after_feedback = rdr.GetInt("ITI_after_feedback", dst.ITI_after_feedback);

            // colors
            dst.S1.Color = rdr.GetInt("s1_colour");
            dst.S2.ShapeClr1_NW = rdr.GetInt("s2_colour_position_1(NW)");
            dst.S2.ShapeClr2_NE = rdr.GetInt("s2_colour_position_2(NE)");
            dst.S2.ShapeClr3_SE = rdr.GetInt("s2_colour_position_3(SE)");
            dst.S2.ShapeClr4_SW = rdr.GetInt("s2_colour_position_4(SW)");
            dst.S2.ShapeClr5_Center = rdr.GetInt("s2_colour_position_5(centre)");
            dst.S3.Color = rdr.GetInt("s3_colour", dst.S3.Color);
            dst.S3.DistractColor = rdr.GetInt("S3_distractor_colour", dst.S3.DistractColor);
            dst.S4.Color = rdr.GetInt("s4_colour", dst.S4.Color);
            dst.S4.DistractColor = rdr.GetInt("S4_distractor_colour", dst.S4.DistractColor);

            dst.key_mapping = rdr.GetInt("key_mapping", dst.key_mapping);
            dst.taskType = rdr.GetInt("Task", dst.taskType);
            dst.TMS_S3_SOA = rdr.GetInt("TMS_s3_SOA", dst.TMS_S3_SOA);
            dst.ExpCondition = rdr.GetStr("Experimental_Condition");


            // Gathering factors
            int f = 1;
            while (true)
            {
                string colname = $"Factor_{f}";
                int cidx = rdr.GetNameIdx(colname);
                if (cidx < 0) break;

                if (rdr.TryGetVal(cidx, out var fav))
                {
                    dst.Factors.Add(fav);
                    dst.FactorLk[colname] = fav;
                }
                f++;
            }

            return true;
        }

        public static TrialOrder FillTrialData(this InputDataReader rdr)
        {
            if (rdr == null)
                return null;
            TrialOrder result = new TrialOrder();
            rdr.FillTrialData(result);
            return result;
        }

        public static List<TrialOrder> LoadTrials(string sourceFn)
        {
            string[] lines = File.ReadAllLines(sourceFn);
            InputDataReader rdr = new InputDataReader();
            List<TrialOrder> result = new List<TrialOrder>();
            foreach (var ln in lines)
            {
                var ltype = rdr.ConsumeLine(ln);
                if (ltype == InputLine.TrialData)
                {
                    var d = rdr.FillTrialData();
                    result.Add(d);
                }
            }
            return result;
        }
    }
}