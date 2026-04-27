using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using static System.Math;
using static WMAData.Consts;

namespace WMAData
{
    public class Configuration
    {
        private int width_px;
        private int height_px;
        private double width_cm;
        private double height_cm;


        // the expacted distance from the monitor
        public double distanceCm;

        // background circle
        public double Background_diameter_deg;
        public double backgroundRadiusCM;
        public ColorFloat backgroundCircleColor;


        public ColorFloat Pause_background_circle_colour;

        // fixation point (the small in the center of background circle)
        public double Fixation_dot_deg;
        public double fixSpotSizeCM;
        public ColorFloat Fixation_colour;
        
        // yay. there are different
        public double Placeholder_diameter_deg;
        public double Placeholders_diameter_deg;

        public ColorFloat Placeholders_colour;
        public double targetRadiusCM;
        public double cueRadiusCM;

        public double Shape_scale_factor;

        public double Image_size_deg;
        public double Image_size_CM;


        public double Minimum_training_accuracy;

        public int N_trials_before_pause_training;
        public int N_trials_before_pause_main;

        public string Instructions_ODD_participants;
        public string Instructions_EVEN_participants;

        // Stimulus font_1 and Stimulus_font_2
        public FontData font_1 = new FontData();
        public FontData font_2 = new FontData();
        public FontData Feedback_font = new FontData();

        public string Feedback_text_correct;
        public string Feedback_text_incorrect;
        public int RT_constant_error_ms;

        public ColorFloat Incorrect_feedback_colour;
        public ColorFloat Correct_feedback_colour;

        public ColorFloat[] ShapeColors = new ColorFloat[15];

        public bool Show_S3_peripheral_placeholders = true;
        public bool Show_S4_peripheral_placeholders = true;
        public bool Show_S3_placeholder_when_centre = true;
        public bool Show_S4_placeholder_when_centre = true;

        public double S2_sample_diameter_deg;
        public double s2_sample_diameter_cm;

        public double Shape_size_deg;
        public double Shape_size_CM;

        public double Image_feedback_deg;
        public double Image_feedback_CM;

        public string keyboards;

        // Monitor Refresh rate
        public int RefreshRate = 100;


        // above all others!
        public const int FIXATION_ORDER = 1000;

        // drawFixationWithPlaceholders

        private double DegToCm(double deg)
        {
            return (((deg / 2) * (PI / 180)) * distanceCm) * 2;
        }

        private void ScheduleDefaults(PlayList dst)
        {
            dst.AddCircle(backgroundRadiusCM, backgroundCircleColor);

            var placeHolderRad = DegToCm(Placeholder_diameter_deg/2);
            var lineWidthCm = DegToCm(0.05);
            PlayItemPos[] placeHolders = new PlayItemPos[] 
            {
                PlayItemPos.NW, 
                PlayItemPos.NE, 
                PlayItemPos.SW, 
                PlayItemPos.SE
            };
            foreach (var p in placeHolders)
            {
                var ph = dst.AddCircleHollow(placeHolderRad, Placeholders_colour);
                ph.pos = p;
                ph.posDistanceCm = targetRadiusCM;
                ph.drawOrder = FIXATION_ORDER;
                ph.lineWidthCm = lineWidthCm;
            }

            var fx = dst.AddCircle(fixSpotSizeCM, Fixation_colour);
            fx.drawOrder = FIXATION_ORDER;
        }

        public void CalculateCmFromDeg()
        {
            // requires distanceCm to be populated

            backgroundRadiusCM = Tan(Background_diameter_deg / 2.0 * (PI / 180.0)) * distanceCm;
            Shape_size_CM = Tan(Shape_size_deg * (PI / 180)) * distanceCm;
            Image_feedback_CM = Tan(Image_feedback_deg * (PI / 180)) * distanceCm;
            fixSpotSizeCM = (Fixation_dot_deg * (PI / 180.0))* distanceCm;
            Image_size_CM = Tan(Image_size_deg * (PI / 180)) * distanceCm;
            s2_sample_diameter_cm = Tan(S2_sample_diameter_deg * (PI / 180)) * distanceCm;
            targetRadiusCM = Tan(Placeholders_diameter_deg / 2 * (PI / 180)) * distanceCm;
            cueRadiusCM = Tan(Placeholders_diameter_deg / 2 * (PI / 180)) * distanceCm;

        }

        private void ScheduleTrials(List<TrialOrder> trials, PlayList dst)
        {
            double ofsTime = 0;

            for (int i = 0; i < trials.Count; i++)
            {
                bool show_s1 = true;
                bool show_s2 = true;
                bool show_s3 = true;
                var tr = trials[i];

                switch (tr.S1.Shape)
                {
                    case SHAPE_SKIP_TO_S2:
                        show_s1 = false;
                        break;
                    case SHAPE_SKIP_TO_S3:
                        show_s1 = false;
                        show_s2 = false;
                        break;
                    case SHAPE_SKIP_TO_S4:
                        show_s1 = false;
                        show_s2 = false;
                        show_s3 = false;
                        break;
                }

                // Determine the frame numbers of s3 . The TMS onset frame is specified relative to this
                // If s3 is not shown then there is no TMS photodiode patch.
                // int s3_onset_frameNo = 0;
                // if (show_s1) 
                //     s3_onset_frameNo = s3_onset_frameNo + round(s1_duration / 1000 / (1 / REFRESH_RATE)) + round(s1_s2_isi / 1000 / (1 / REFRESH_RATE));
                // if (show_s2)
                //     s3_onset_frameNo:= s3_onset_frameNo + round(s2_duration / 1000 / (1 / REFRESH_RATE)) + round(s2_s3_isi / 1000 / (1 / REFRESH_RATE));

                bool isBaselineCondition = (tr.S2.ShapePos1_NW == SHAPE_STAR)
                    && (tr.S3.Shape == SHAPE_STAR)
                    && (tr.S4.Shape == SHAPE_STAR);

                // check if the current trial is a pause trial.
                // NB pause trials start at 1, but the trialNo counter starts at 0.

                // -- PAUSE processing --

                //=======================================================================================
                //     S1
                //=======================================================================================
                // skip S1 if show_s1=false
                int s1_onsetTime = -1;
                double duration;
                if (show_s1)
                {

                    duration = tr.S1.Duration;
                    var clr = GetShapeColor(tr.S1.Color);
                    //var c = dst.AddCircle(Image_size_CM, Fixation_colour, ofsTime, duration);
                    double fntsz = Math.Round(font_1.size * (width_px / width_cm) * (distanceCm / 57));

                    if (tr.S1.Position != POS_ALL)
                    {
                        dst.AddByShape(tr.S1.Shape, Image_size_CM, Shape_size_CM, clr, ofsTime, duration)
                            .SetFont(font_1.name, fntsz)
                            .SetPos(tr.S1.Position, cueRadiusCM)
                            ;

                    }
                    else
                    {
                        dst.AddByShape(tr.S1.Shape, Image_size_CM, Shape_size_CM, clr, ofsTime, duration)
                            .SetFont(font_1.name, fntsz)
                            .SetPos(PlayItemPos.NW, cueRadiusCM);
                        dst.AddByShape(tr.S1.Shape, Image_size_CM, Shape_size_CM, clr, ofsTime, duration)
                            .SetFont(font_1.name, fntsz)
                            .SetPos(PlayItemPos.NE, cueRadiusCM);
                        dst.AddByShape(tr.S1.Shape, Image_size_CM, Shape_size_CM, clr, ofsTime, duration)
                            .SetFont(font_1.name, fntsz)
                            .SetPos(PlayItemPos.SW, cueRadiusCM);
                        dst.AddByShape(tr.S1.Shape, Image_size_CM, Shape_size_CM, clr, ofsTime, duration)
                            .SetFont(font_1.name, fntsz)
                            .SetPos(PlayItemPos.SE, cueRadiusCM);

                    }
                    dst.PlaySoundAt(tr.S1.Sound, ofsTime);


                    ofsTime += tr.S1.Duration;

                    ofsTime += tr.S1.Next_ISI;
                }


                //=======================================================================================
                //     S2 informative cue ---------------------------------------------------
                //=======================================================================================
                // skip S2 if show_s2=false
                if (show_s2)
                {

                    var half = s2_sample_diameter_cm / 2;

                    duration = tr.S2.Duration;
                    var v1 = dst.AddByShape(tr.S2.ShapePos1_NW, Image_size_CM, Shape_size_CM, Fixation_colour, ofsTime, duration);
                    v1.pos = PlayItemPos.NW;
                    v1.posDistanceCm = half;

                    var v2 = dst.AddByShape(tr.S2.ShapePos2_NE, Image_size_CM, Shape_size_CM, Fixation_colour, ofsTime, duration);
                    v2.pos = PlayItemPos.NE;
                    v2.posDistanceCm = half;

                    var v3 = dst.AddByShape(tr.S2.ShapePos4_SW, Image_size_CM, Shape_size_CM, Fixation_colour, ofsTime, duration);
                    v3.pos = PlayItemPos.SW;
                    v3.posDistanceCm = half;

                    var v4 = dst.AddByShape(tr.S2.ShapePos3_SE, Image_size_CM, Shape_size_CM, Fixation_colour, ofsTime, duration);
                    v4.pos = PlayItemPos.SE;
                    v4.posDistanceCm = half;

                    ofsTime += tr.S2.Duration;
                    ofsTime += tr.S2.Next_ISI;
                }


                //=======================================================================================
                //=======================================================================================
                //=======================================================================================
                //     S3 spatial cue onset with optional TMS photodiode patch
                //=======================================================================================
                // skip S3 if show_s3=false   
                if (show_s3)
                {
                    duration = tr.S3.Duration;

                    if (tr.S3.Shape != SHAPE_SKIP_TO_S2)
                    {

                    }

                    var c3 = dst.AddCircle(Image_size_CM, Fixation_colour, ofsTime, duration);
                    c3.pos = PlayListHelper.ToPlayPos(tr.S3.Position);
                    c3.posDistanceCm = targetRadiusCM;

                    ofsTime += tr.S3.Duration;
                    ofsTime += tr.S3.Next_ISI;
                }

                duration = tr.S4.Duration;
                var c4 = dst.AddCircle(Image_size_CM, Fixation_colour, ofsTime, duration);
                c4.pos = PlayListHelper.ToPlayPos(tr.S4.Position);
                c4.posDistanceCm = targetRadiusCM;

                ofsTime += tr.S4.Duration;
                ofsTime += tr.S4.Next_ISI;
            }
        }

        public ColorFloat GetShapeColor(int i )
        {
            if (i < 0 || i >= ShapeColors.Length)
                return new ColorFloat();
            return ShapeColors[i];
        }


        public void Schedule(TrialMonitor tm, List<TrialOrder> trials, PlayList dst)
        {
            width_px = tm.widthPx;
            height_px = tm.widthPx;
            width_cm = tm.widthCm;
            height_cm = tm.heightCm;

            ScheduleDefaults(dst);
            if (trials != null)
                ScheduleTrials(trials, dst);
            dst.Sort();
        }
    }
}
