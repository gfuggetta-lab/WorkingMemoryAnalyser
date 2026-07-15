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
            dst.S1.Marker = rdr.GetInt("S1_Markers", dst.S1.Marker);
            dst.S2.Marker = rdr.GetInt("S2_Markers", dst.S2.Marker);
            dst.S3.Marker = rdr.GetInt("S3_Markers", dst.S3.Marker);
            dst.S4.Marker = rdr.GetInt("S4_Markers", dst.S4.Marker);
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

        public static bool LoadTrials(string sourceFn, List<TrialOrder> trials, List<PauseData> pauses)
        {
            string[] lines = File.ReadAllLines(sourceFn);

            InputDataReader rdr = new InputDataReader();
            foreach (var ln in lines)
            {
                var ltype = rdr.ConsumeLine(ln);
                if (ltype == InputLine.TrialData)
                {
                    var d = rdr.FillTrialData();
                    trials.Add(d);
                } 
                else if (ltype == InputLine.PauseData)
                {
                    if (rdr.lineVals.Length >= 2)
                    {
                        PauseData pd = new PauseData();
                        bool isTr = int.TryParse(rdr.lineVals[0], out pd.trial_no);
                        bool isMsg = int.TryParse(rdr.lineVals[1], out pd.message_no);
                        if (isTr&&isMsg)
                            pauses.Add(pd);
                    }
                }
            }
            return true;
        }

        public static List<TrialOrder> LoadTrials(string sourceFn)
        {
            List<TrialOrder> result = new List<TrialOrder>();
            List<PauseData> pauses = new List<PauseData>();
            LoadTrials(sourceFn, result, pauses);
            return result;
        }

        public static void FillShapes(StimuliData src, List<int> dst)
        {
            dst.Add(src.Shape);
            dst.Add(src.ShapePos1_NW);
            dst.Add(src.ShapePos2_NE);
            dst.Add(src.ShapePos3_SE);
            dst.Add(src.ShapePos4_SW);
            dst.Add(src.ShapePos5_Center);
            dst.Add(src.DistractShape);
        }
        public static List<int> GetShapes(this TrialOrder ord)
        {
            List<int> sh = new List<int>();
            FillShapes(ord.S1, sh);
            FillShapes(ord.S2, sh);
            FillShapes(ord.S3, sh);
            FillShapes(ord.S4, sh);
            sh.Add(ord.Feedback_shape);

            List<int> result = new List<int>();
            foreach (var s in sh)
                if (s != Consts.SHAPE_NONE)
                    result.Add(s);
            return result;
        }

        public static void GetPreloadImages(this Configuration cfg, List<TrialOrder> trials, List<string> images)
        {
            Dictionary<int, bool> imageCheck = new Dictionary<int, bool>();
            foreach (var vo in trials)
            {
                var shapesList = vo.GetShapes();
                foreach (var sh in shapesList)
                {
                    if ((sh >= Consts.SHAPE_BMP_MIN) && (sh <= Consts.SHAPE_BMP_MAX))
                    {
                        imageCheck[sh] = true;
                    }
                }
            }
            foreach (var nm in imageCheck.Keys)
                images.Add(nm.ToString());
        }

        public static void GetPreloadFonts(this Configuration cfg, List<string> fonts)
        {
            if (cfg == null) return;
            if (fonts == null) return;
            fonts.Add(cfg.font_1.name);
            //fonts.Add(cfg.font_2.name); // currently not used
            fonts.Add(cfg.Feedback_font.name);
        }

        public static void GetPreloadSounds(this Configuration cfg, List<TrialOrder> trials, List<string> sounds)
        {
            if (cfg == null) return;
            if (sounds == null) return;

            Dictionary<string, bool> snd = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            bool addFeedback = false;
            foreach (var o in trials)
            {
                if (o.S1.Sound != 0)
                    snd[o.S1.Sound.ToString()] = true;
                if (o.S2.Sound != 0)
                    snd[o.S2.Sound.ToString()] = true;
                if (o.S3.Sound != 0)
                    snd[o.S3.Sound.ToString()] = true;
                if (o.S4.Sound != 0)
                    snd[o.S4.Sound.ToString()] = true;
                addFeedback = addFeedback | (o.Feedback_sound != 0);
            }
            if (addFeedback)
            {
                snd["correct"] = true;
                snd["incorrect"] = true;
            }
            foreach (var k in snd.Keys)
            {
                sounds.Add($"{k}.wav");
            }
        }
    }
}