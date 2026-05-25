using System;
using System.Collections.Generic;
using System.Text;

namespace WMAData
{
    public class ResultReport : IResultReport
    {
        public StringBuilder text = new StringBuilder();

        private int constant_error_time;

        public string experimentName;
        public string currentDate;
        public string currentTime;
        public int trialOrderFileNo;
        public string participantID;
        public string age;
        public string sex;
        public string handedness;
        public string displayType;
        // Added for WriteTrial: Pascal writes observerNo from a global variable.
        public int observerNo;

        public void SetConfig(Configuration cfg)
        {
            if (cfg == null)
                return;

            constant_error_time = cfg.RT_constant_error_ms;

            WriteLine("Experiment:", "");
            WriteLine("N_trials_before_pause_training:", cfg.N_trials_before_pause_training);
            WriteLine("N_trials_before_pause_main:", cfg.N_trials_before_pause_main);
            WriteLine("Instructions_ODD_participants:", cfg.Instructions_ODD_participants);
            WriteLine("Instructions_EVEN_participants:", cfg.Instructions_EVEN_participants);
            WriteLine("Pause_background_circle_colour:\t", FormatColor(cfg.Pause_background_circle_colour));
            WriteLine("Run_background_circle_colour:\t", FormatColor(cfg.backgroundCircleColor));
            WriteLine("Fixation_colour:\t", FormatColor(cfg.Fixation_colour));
            WriteLine("Placeholders_colour:\t", FormatColor(cfg.Placeholders_colour));
            WriteLine("Incorrect_feedback_colour:\t", FormatColor(cfg.Incorrect_feedback_colour));
            WriteLine("Correct_feedback_colour:\t", FormatColor(cfg.Correct_feedback_colour));

            for (int i = 0; i < 16; i++)
            {
                ColorFloat color = i < cfg.ShapeColors.Length
                    ? cfg.ShapeColors[i]
                    : new ColorFloat();
                WriteLine($"Shapes_colour_{i}:", FormatColor(color));
            }
            text.AppendLine();

            text.Append("Experiment\t");
            text.Append("Date\t");
            text.Append("Start_Time\t");
            text.Append("Trial_Order_File_No\t");
            text.Append("ParticipantID\t");
            text.Append("Age\t");
            text.Append("sex\t");
            text.Append("Handedness\t");
            text.Append("Display_Type\t");
            text.Append("Observer_number\t");
            text.Append("session_no\t");
            text.Append("s1_marker\t");
            text.Append("s2_marker\t");
            text.Append("s3_marker\t");
            text.Append("s4_marker\t");
            text.Append("s1_shape\t");
            text.Append("s1_sound\t");
            text.Append("s1_quad\t");
            text.Append("s1_duration\t");
            text.Append("s1_s2_isi\t");
            text.Append("s2_shape_position_1(NW)\t");
            text.Append("s2_shape_position_2(NE)\t");
            text.Append("s2_shape_position_3(SE)\t");
            text.Append("s2_shape_position_4(SW)\t");
            text.Append("s2_shape_position_5(centre)\t");
            text.Append("s2_sound\t");
            text.Append("s2_duration\t");
            text.Append("s2_s3_isi\t");
            text.Append("s3_shape\t");
            text.Append("s3_distractor_shape\t");
            text.Append("s3_sound\t");
            text.Append("s3_quad\t");
            text.Append("s3_duration\t");
            text.Append("s3_s4_isi\t");
            text.Append("s4_shape\t");
            text.Append("s4_distractor_shape\t");
            text.Append("s4_sound\t");
            text.Append("s4_quad\t");
            text.Append("s4_duration\t");
            text.Append("Response_Time_after_S4\t");
            text.Append("Feedback_shape\t");
            text.Append("Feedback_sound\t");
            text.Append("Feedback_duration_after_response_time\t");
            text.Append("ITI_after_feedback\t");
            text.Append("s1_colour\t");
            text.Append("s2_colour_position_1(NW)\t");
            text.Append("s2_colour_position_2(NE)\t");
            text.Append("s2_colour_position_3(SE)\t");
            text.Append("s2_colour_position_4(SW)\t");
            text.Append("s2_colour_position_5(centre)\t");
            text.Append("s3_colour\t");
            text.Append("s3_distractor_colour\t");
            text.Append("s4_colour\t");
            text.Append("S4_distractor_colour\t");
            text.Append("keyMapping\t");
            text.Append("taskType\t");
            text.Append("TMS_s3_SOA\t");
            text.Append("Experimental_Condition\t");
            text.Append("response\t");
            text.Append("Accuracy\t");
            text.Append("RT_ms\t");
            text.Append("RT_ms_minus_constant_error\t");
            text.Append("s1_onsetTime_ms\t");
            text.Append("s2_onsetTime_ms\t");
            text.Append("s3_onsetTime_ms\t");
            text.Append("TMS_onsetTime_ms\t");
            text.Append("s4_onsetTime_ms\t");
            text.Append("response_onsetTime_ms\t");
            text.Append("feedback_onsetTime_ms\t");
            text.Append("blank_onsetTime_ms\t");
            text.Append("user_paused_the_trial");

            text.Append('\t');
            text.Append("Factor_1\t");
            text.Append("Factor_2\t");
            text.Append("Factor_3\t");
            text.Append("Factor_4\t");
            text.Append("Factor_5\t");
            text.Append("Factor_6\t");
            text.Append("Factor_7\t");
            text.Append("Factor_8\t");
            text.Append("Factor_9");

            text.AppendLine();
        }

        private void WriteLine(string name, object value)
        {
            text.Append(name);
            text.Append('\t');
            text.Append(value);
            text.AppendLine();
        }

        private static string FormatColor(ColorFloat color)
        {
            return $"{ToByte(color.r)},{ToByte(color.g)},{ToByte(color.b)}";
        }

        private static int ToByte(double value)
        {
            return (int)Math.Round(value * 255.0);
        }

        public void WriteTrial(TrialOrder trial, TrialResults res)
        {
            if (trial == null)
                return;

            AppendField(experimentName);
            AppendField(currentDate);
            AppendField(currentTime);
            AppendField(trialOrderFileNo);
            AppendField(participantID);
            AppendField(age);
            AppendField(sex);
            AppendField(handedness);
            AppendField(displayType);
            AppendField(observerNo);
            AppendField(trial.session_number);
            AppendField(trial.S1.Markers);
            AppendField(trial.S2.Markers);
            AppendField(trial.S3.Markers);
            AppendField(trial.S4.Markers);
            AppendField(trial.S1.Shape);
            AppendField(trial.S1.Sound);
            AppendField(trial.S1.Position);
            AppendField(trial.S1.Duration);
            AppendField(trial.S1.Next_ISI);
            AppendField(trial.S2.ShapePos1_NW);
            AppendField(trial.S2.ShapePos2_NE);
            AppendField(trial.S2.ShapePos3_SE);
            AppendField(trial.S2.ShapePos4_SW);
            AppendField(trial.S2.ShapePos5_Center);
            AppendField(trial.S2.Sound);
            AppendField(trial.S2.Duration);
            AppendField(trial.S2.Next_ISI);
            AppendField(trial.S3.Shape);
            AppendField(trial.S3.DistractShape);
            AppendField(trial.S3.Sound);
            AppendField(trial.S3.Position);
            AppendField(trial.S3.Duration);
            AppendField(trial.S3.Next_ISI);
            AppendField(trial.S4.Shape);
            AppendField(trial.S4.DistractShape);
            AppendField(trial.S4.Sound);
            AppendField(trial.S4.Position);
            AppendField(trial.S4.Duration);
            AppendField(trial.S4.Next_ISI);
            AppendField(trial.Feedback_shape);
            AppendField(trial.Feedback_sound);
            AppendField(trial.Feedback_duration_after_response_time);
            AppendField(trial.ITI_after_feedback);
            AppendField(trial.S1.Color);
            AppendField(trial.S2.ShapeClr1_NW);
            AppendField(trial.S2.ShapeClr2_NE);
            AppendField(trial.S2.ShapeClr3_SE);
            AppendField(trial.S2.ShapeClr4_SW);
            AppendField(trial.S2.ShapeClr5_Center);
            AppendField(trial.S3.Color);
            AppendField(trial.S3.DistractColor);
            AppendField(trial.S4.Color);
            AppendField(trial.S4.DistractColor);
            AppendField(trial.key_mapping);
            AppendField(trial.taskType);
            AppendField(trial.TMS_S3_SOA);
            AppendField(trial.ExpCondition);

            AppendField(res.observedDataResponseRecord);
            AppendField(res.observedDataCorrectResponseRecord);
            AppendField(res.responseTimeMs);

            // RT_ms_minus_constant_error
            if (res.observedDataResponseRecord < 0)
                AppendField(-1);
            else
                AppendField(res.responseTimeMs - constant_error_time);

            AppendField(res.s1_onsetTime);
            AppendField(res.s2_onsetTime);
            AppendField(res.s3_onsetTime);
            AppendField(res.TMS_onsetTime);
            AppendField(res.s4_onsetTime);
            AppendField(res.response_onsetTime);
            AppendField(res.feedback_onsetTime);
            AppendField(res.blank_onsetTime);
            text.Append(res.isRuinedTrial);

            text.Append('\t');
            for (int i = 0; i < 9; i++)
            {
                string factor = "";
                if ((trial.Factors != null) && (i < trial.Factors.Count))
                    factor = trial.Factors[i]?.Trim() ?? "";
                AppendField(factor);
            }

            text.AppendLine();
        }

        private void AppendField(object value)
        {
            text.Append(value);
            text.Append('\t');

        }
    }
}
