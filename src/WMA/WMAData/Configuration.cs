using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
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

        // the constant error time, which is subtracted for the response time
        // used in the reporting only
        public int RT_constant_error_ms;

        public ColorFloat Incorrect_feedback_colour;
        public ColorFloat Correct_feedback_colour;

        public ColorFloat[] ShapeColors = new ColorFloat[15];

        // should S3 show additional placeholder (not just default 4)
        // the placeholders would be filled with the distractionShape (unless it's zero)
        // The default S3 is actually 16
        // When drawing a distraction, drawing the shape inside of the target is discouraged
        public bool Show_S3_peripheral_placeholders = true;

        // The default S4 is actually 16, but it's rarely used?
        // When drawing a distraction, drawing the shape inside of the target is discouraged
        public bool Show_S4_peripheral_placeholders = true;
        
        // should S3 show the placeholder AROUND the target image
        // however, it's enforced to be drawn anyway, when the position is other than ALL
        // it might be a deprecated option.
        // this is redundant because placeholders are forced to be drawn anyway
        public bool Show_S3_placeholder_when_centre = true;

        // should S4 show the placeholder AROUND the target image 
        // however, it's enforced to be drawn anyway, when the position is other than ALL
        // it might be a deprecated option
        // this is redundant because placeholders are forced to be drawn anyway
        public bool Show_S4_placeholder_when_centre = true;

        public double S2_sample_diameter_deg;
        public double s2_sample_diameter_cm;

        public int s2_set_size = 4;
        public int s3_set_size = 16;
        public int s4_set_size = 4;

        public double Shape_size_deg;
        public double Shape_size_CM;

        // the size of the feedback image in degrees
        public double Image_feedback_deg;
        // the size of the feedback image if used
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

        private void ScheduleFixationDot(PlayList dst, double ofs, double duration)
        {
            var fx = dst.AddCircle(fixSpotSizeCM / 2.0, Fixation_colour);
            fx.drawOrder = FIXATION_ORDER;
            fx.startMs = ofs;
            fx.durationMs = duration;
        }

        private PlayItem StartPlaceholder(PlayList dst, double ofsTime, double duration)
        {
            var placeHolderRad = DegToCm(Placeholder_diameter_deg / 2);
            var lineWidthCm = DegToCm(0.05);
            var ph = dst.AddCircleHollow(placeHolderRad, Placeholders_colour, ofsTime, duration);
            ph.drawOrder = FIXATION_ORDER;
            ph.lineWidthCm = lineWidthCm;
            return ph;
        }

        private void SchedulePlaceholders4(PlayList dst, double ofs, double duration)
        {
            var placeHolderRad = DegToCm(Placeholder_diameter_deg / 2);
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
                var ph = StartPlaceholder(dst, ofs, duration);
                ph.SetPos(p, targetRadiusCM);
            }
        }

        private void ScheduleShapesN(PlayList dst, double ofs, double duration, int count, int shape, int color, int skipPos)
        {
            var placeHolderRad = DegToCm(Placeholder_diameter_deg / 2);
            var lineWidthCm = DegToCm(0.05);

            if ((skipPos >= POS_TOP_LEFT) && (skipPos <= POS_BOT_RIGHT))
            {
                if (count % 4 == 0)
                {
                    // only if count if a mulitply of 4
                    skipPos = (skipPos - 1) * count / 4;
                }
                else
                    skipPos = -1;
            }
            else
                skipPos = -1;

            for (int i = 0; i < count; i++)
            {
                if (skipPos == i)
                    continue;
                var ph = dst.AddByShape(shape, Image_size_CM, Shape_size_CM, GetShapeColor(color), ofs, duration);
                ph.pos = PlayItemPos.OneOfCount;
                ph.posOther = i;
                ph.posCount = count;
                ph.posDistanceCm = targetRadiusCM;
                ph.drawOrder = FIXATION_ORDER;
                ph.lineWidthCm = lineWidthCm;
            }
        }

        private void SchedulePlaceholdersN(PlayList dst, double ofs, double duration, int count)
        {
            var placeHolderRad = DegToCm(Placeholder_diameter_deg / 2);
            var lineWidthCm = DegToCm(0.05);
            for(int i = 0; i < count; i++)
            {
                var ph = StartPlaceholder(dst, ofs, duration);
                ph.pos = PlayItemPos.OneOfCount;
                ph.posOther = i;
                ph.posCount = count;
                ph.posDistanceCm = targetRadiusCM;
            }
        }

        private void ScheduleBackground(PlayList dst)
        {
            dst.AddCircle(backgroundRadiusCM, backgroundCircleColor);
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

        private void ScheduleS1_Focus(TrialOrder tr, PlayList dst, ref double ofsTime)
        {
            double duration = tr.S1.Duration;
            dst.StartSection("S1", ofsTime, duration);

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


            ScheduleFixationDot(dst, ofsTime, duration);
            SchedulePlaceholders4(dst, ofsTime, duration);

            ofsTime += duration;
        }

        private void ScheduleS1_S2(TrialOrder tr, PlayList dst, ref double ofsTime)
        {
            double duration = tr.S1.Next_ISI;
            dst.StartSection("S1>S2", ofsTime, duration);
            ScheduleFixationDot(dst, ofsTime, duration);
            SchedulePlaceholders4(dst, ofsTime, duration);
            ofsTime += duration;
        }

        private void ScheduleS2_Info(TrialOrder tr, PlayList dst, ref double ofsTime)
        {
            double duration = tr.S2.Duration;
            dst.StartSection("S2", ofsTime, duration);
            var half = s2_sample_diameter_cm / 2;

            // draw S2 cues
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

            dst.PlaySoundAt(tr.S2.Sound, ofsTime);
            ScheduleFixationDot(dst, ofsTime, duration);
            SchedulePlaceholdersN(dst, ofsTime, duration, s2_set_size);

            ofsTime += duration;
        }

        private void ScheduleS2_S3(TrialOrder tr, PlayList dst, ref double ofsTime)
        {
            double duration = tr.S2.Next_ISI;

            dst.StartSection("S2>S3", ofsTime, duration);
            ScheduleFixationDot(dst, ofsTime, duration);
            SchedulePlaceholders4(dst, ofsTime, duration);

            ofsTime += duration;
        }

        private void ScheduleS3_Distract(TrialOrder tr, PlayList dst, double trialOfs, ref double ofsTime)
        {
            double duration = tr.S3.Duration;

            dst.StartSection("S3", ofsTime, duration);

            // The TMS onset frame is specified relative to S3
            // If s3 is not shown then there is no TMS photodiode patch.
            double tmsOfs = ofsTime + tr.TMS_S3_SOA;
            if (tmsOfs >= trialOfs)
            {
                // the event is not scheduled, unless S3 is used
                // the TMS event should not be called Prior to the current trial
                // and it's not uncommon for TMS offset to be negative -100000
                dst.CustomEvent(tmsOfs, "S3TMS");
            }

            if (tr.S3.Shape != SHAPE_S3_HIDE)
            {
                if (Show_S3_peripheral_placeholders)
                {
                    ScheduleShapesN(dst, ofsTime, duration, s3_set_size, tr.S3.DistractShape, tr.S3.DistractColor, tr.S3.Position);
                }

                // target Image
                dst.AddByShape(tr.S3.Shape, Image_size_CM, Shape_size_CM, GetShapeColor(tr.S3.Color), ofsTime, duration)
                    .SetPos(tr.S3.Position, targetRadiusCM);
                if ((Show_S3_placeholder_when_centre) || (tr.S3.Position != POS_ALL))
                {
                    StartPlaceholder(dst, ofsTime, duration)
                        .SetPos(tr.S3.Position, targetRadiusCM);
                }
            }




            dst.PlaySoundAt(tr.S3.Sound, ofsTime);


            if (tr.S3.Position != POS_ALL)
                ScheduleFixationDot(dst, ofsTime, duration);

            SchedulePlaceholdersN(dst, ofsTime, duration, s3_set_size);

            ofsTime += duration;
        }
        private void ScheduleS3_S4(TrialOrder tr, PlayList dst, ref double ofsTime)
        {
            double duration = tr.S3.Next_ISI;
            dst.StartSection("S3>S4", ofsTime, duration);
            ScheduleFixationDot(dst, ofsTime, duration);
            SchedulePlaceholders4(dst, ofsTime, duration);
            ofsTime += duration;
        }

        private void ScheduleS4_Target(TrialOrder tr, PlayList dst, ref double ofsTime)
        {
            double duration = tr.S4.Duration;
            dst.StartSection("S4", ofsTime, duration);
            
            // the response can be given as soon as S4 is shown
            dst.ReadResponse(ofsTime, duration + tr.S4.Next_ISI);

            if (tr.S4.Shape != SHAPE_S4_HIDE)
            {
                if ((Show_S4_peripheral_placeholders)&&(tr.S4.DistractShape != 0))
                {
                    ScheduleShapesN(dst, ofsTime, duration, s4_set_size, 
                        tr.S4.DistractShape, tr.S4.DistractColor, tr.S4.Position);
                }

                // set target image
                dst.AddByShape(tr.S4.Shape, Image_size_CM, Shape_size_CM, GetShapeColor(tr.S4.Color), ofsTime, duration)
                    .SetPos(tr.S4.Position, targetRadiusCM);
                if ((Show_S4_placeholder_when_centre) || (tr.S4.Position != POS_ALL))
                {
                    StartPlaceholder(dst, ofsTime, duration)
                        .SetPos(tr.S4.Position, targetRadiusCM);
                }
            }

            //if (Show_S4_peripheral_placeholders)
            //    SchedulePlaceholdersN(dst, ofsTime, duration, s4_set_size);

            dst.PlaySoundAt(tr.S4.Sound, ofsTime);

            // originally there would be a condition, not to draw the fixation
            // when "ALL" position is selected (similar to one at S3),
            // but it was removed for some reason
            ScheduleFixationDot(dst, ofsTime, duration);

            SchedulePlaceholdersN(dst, ofsTime, duration, s4_set_size);


            ofsTime += duration;
        }

        private void Schedule_Response(TrialOrder tr, PlayList dst, ref double ofsTime)
        {
            double duration = tr.S4.Next_ISI; // Response_Time_after_S4
            dst.StartSection("RSP", ofsTime, duration);

            ScheduleFixationDot(dst, ofsTime, duration);
            SchedulePlaceholders4(dst, ofsTime, duration);
            ofsTime += duration;
        }

        private void Schedule_AfterResponse(TrialOrder tr, PlayList dst, bool isBaseLineCond, ref double ofsTime)
        {
            double duration = tr.ITI_after_feedback; // Response_Time_after_S4
            dst.StartSection("Aft", ofsTime, duration);
            dst.CheckResponse(ofsTime);

            ScheduleFixationDot(dst, ofsTime, duration);
            SchedulePlaceholders4(dst, ofsTime, duration);


            // With the baseline condition, no audio or visual feedback is given.
            if (!isBaseLineCond)
            {
                // The correct feedback
                PlayItem res = null;
                if ((tr.Feedback_shape >= SHAPE_DIAMOND) && (tr.Feedback_shape <= SHAPE_STAR))
                    res = dst.AddByShape(tr.Feedback_shape, Image_size_CM, Shape_size_CM, Correct_feedback_colour, ofsTime, duration);
                else if (tr.Feedback_shape == SHAPE_RESPONSE_TEXT)
                    res = dst.AddText(Feedback_text_correct, "Arial", GetShapeColor(COLOR_WHITE0));
                else if (tr.Feedback_shape == SHAPE_RESPONSE_IMAGE)
                    res = dst.AddImageById(IMAGEID_CORRECT, Image_feedback_CM, GetShapeColor(COLOR_WHITE0), ofsTime, duration);
                if (res != null)
                    res.cond = PlayItemCond.Correct;

                res = dst.AddSound("correct.wav", ofsTime);
                res.cond = PlayItemCond.Correct;

                // The incorrect feedback
                res = null;
                if ((tr.Feedback_shape >= SHAPE_DIAMOND) && (tr.Feedback_shape <= SHAPE_STAR))
                    res = dst.AddByShape(tr.Feedback_shape, Image_size_CM, Shape_size_CM, Incorrect_feedback_colour, ofsTime, duration);
                else if (tr.Feedback_shape == SHAPE_RESPONSE_TEXT)
                    res = dst.AddText(Feedback_text_incorrect, "Arial", GetShapeColor(COLOR_WHITE0));
                else if (tr.Feedback_shape == SHAPE_RESPONSE_IMAGE)
                    res = dst.AddImageById(IMAGEID_INCORRECT, Image_feedback_CM, GetShapeColor(COLOR_WHITE0), ofsTime, duration);
                if (res != null)
                    res.cond = PlayItemCond.Incorrect;

                res = dst.AddSound("incorrect.wav", ofsTime);
                res.cond = PlayItemCond.Incorrect;
            }

            ofsTime += duration;
        }

        private void ScheduleTrial(TrialOrder tr, PlayList dst, ref double ofsTime)
        {
            bool show_s1 = true;
            bool show_s2 = true;
            bool show_s3 = true;

            double trialOfs = ofsTime;
            dst.StartTrial($"trial {tr.taskType}", ofsTime);

            switch (tr.S1.Shape)
            {
                case SHAPE_S1_SKIP_TO_S2:
                    show_s1 = false;
                    break;
                case SHAPE_S1_SKIP_TO_S3:
                    show_s1 = false;
                    show_s2 = false;
                    break;
                case SHAPE_S1_SKIP_TO_S4:
                    show_s1 = false;
                    show_s2 = false;
                    show_s3 = false;
                    break;
            }

            // if none of the stimuli s2-s4 are being shown (shape=11)
            // then this is a baseline condition, and we want no audio 'feedback'
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
            if (show_s1)
            {
                ScheduleS1_Focus(tr, dst, ref ofsTime);
                ScheduleS1_S2(tr, dst, ref ofsTime);
            }


            //=======================================================================================
            //     S2 informative cue ---------------------------------------------------
            //=======================================================================================
            // skip S2 if show_s2=false
            if (show_s2)
            {
                ScheduleS2_Info(tr, dst, ref ofsTime);
                ScheduleS2_S3(tr, dst, ref ofsTime);
            }


            //=======================================================================================
            //=======================================================================================
            //=======================================================================================
            //     S3 spatial cue onset with optional TMS photodiode patch
            //=======================================================================================
            // skip S3 if show_s3=false   
            if (show_s3)
            {
                ScheduleS3_Distract(tr, dst, trialOfs, ref ofsTime);
                ScheduleS3_S4(tr, dst, ref ofsTime);
            }

            ScheduleS4_Target(tr, dst, ref ofsTime);
            Schedule_Response(tr, dst, ref ofsTime);
            Schedule_AfterResponse(tr, dst, isBaselineCondition, ref ofsTime);

            dst.EndTrial($"trial {tr.taskType}", ofsTime);
        }
        private void ScheduleTrials(List<TrialOrder> trials, PlayList dst, double startOffset = 0.0)
        {
            double ofsTime = startOffset;

            for (int i = 0; i < trials.Count; i++)
            {
                var tr = trials[i];
                Schedule2SecDelay(dst, ref ofsTime);
                ScheduleTrial(tr, dst, ref ofsTime);
            }
        }

        public ColorFloat GetShapeColor(int i )
        {
            if (i < 0 || i >= ShapeColors.Length)
                return new ColorFloat();
            return ShapeColors[i];
        }

        private void Schedule2SecDelay(PlayList dst, ref double ofsTime)
        {
            double duration = 2000.0; // 2 seconds
            dst.StartSection("del2", ofsTime, duration);
            ScheduleFixationDot(dst, ofsTime, duration);
            SchedulePlaceholders4(dst, ofsTime, duration);
            ofsTime += duration;
        }

        private double ScheduleWaitClick(PlayList dst)
        {
            double duration = 0.001; // this is 0.001 of ms
            ScheduleFixationDot(dst, 0.0, duration);
            SchedulePlaceholders4(dst, 0.0, duration);
            var txt = dst.AddText("hello world", "Arial.ttf", GetShapeColor(COLOR_WHITE0), 0, duration)
                .SetPos(PlayItemPos.Center, 0);
            txt.fontSizePx = (int)Math.Round(2.0 * (width_px / width_cm) * (distanceCm / 57));

            dst.WaitMouse(0.0);
            return duration;
        }


        public void Schedule(TrialMonitor tm, List<TrialOrder> trials, PlayList dst)
        {
            width_px = tm.widthPx;
            height_px = tm.widthPx;
            width_cm = tm.widthCm;
            height_cm = tm.heightCm;

            ScheduleBackground(dst);
            double startOffset = ScheduleWaitClick(dst);
            if (trials != null)
            {
                ScheduleTrials(trials, dst, startOffset);
            }
            dst.Sort();
        }

        public void ProcessResponse(
            ResponseButton btn, 
            TrialOrder trial,
            out int isSameDiff, 
            out bool isCorrect)
        {
            if (btn == ResponseButton.NotGiven)
            {
                isSameDiff = RESPONSE_NOTGIVEN;
                isCorrect = false;
                return;
            }

            if (trial.key_mapping == 0)
            {
                if (btn == ResponseButton.LeftButton)
                    isSameDiff = RESPONSE_DIFF; 
                else
                    isSameDiff = RESPONSE_SAME;
            }
            else
            {
                if (btn == ResponseButton.LeftButton)
                    isSameDiff = RESPONSE_SAME;
                else
                    isSameDiff = RESPONSE_DIFF;
            }

            // determine whether cue and target match on the task dimension in same/different tasks (taskType 1 and 2)
            bool isTargetMatchesCue = false;
            switch (trial.taskType)
            {
                //shape matching task   
                case TASKTYPE_SHAPE:
                    isTargetMatchesCue = 
                        ((trial.S2.ShapePos1_NW == trial.S4.Shape)
                        || (trial.S2.ShapePos2_NE == trial.S4.Shape)
                        || (trial.S2.ShapePos3_SE == trial.S4.Shape)
                        || (trial.S2.ShapePos4_SW == trial.S4.Shape)
                        || (trial.S2.ShapePos5_Center == trial.S4.Shape)
                        );
                    break;

                // colour matching task   
                case TASKTYPE_COLOR:
                    isTargetMatchesCue =
                        (((trial.S2.ShapePos1_NW != 0) && (trial.S2.ShapeClr1_NW == trial.S4.Color)) 
                        || ((trial.S2.ShapePos2_NE != 0) && (trial.S2.ShapeClr2_NE == trial.S4.Color)) 
                        || ((trial.S2.ShapePos3_SE != 0) && (trial.S2.ShapeClr3_SE == trial.S4.Color)) 
                        || ((trial.S2.ShapePos4_SW != 0) && (trial.S2.ShapeClr4_SW == trial.S4.Color)) 
                        || ((trial.S2.ShapePos5_Center != 0) && (trial.S2.ShapeClr5_Center == trial.S4.Color)));
                    break;

                default:
                    isTargetMatchesCue = false;
                    break;
            }


            if (trial.taskType != TASKTYPE_IDENTIFY)
            {
                // check for correct/incorrect response.  
                // same/different tasks (taskType 1 and 2)
                isCorrect = ((isSameDiff == RESPONSE_SAME) == isTargetMatchesCue);
            }
            else
            {
                // Task Type 3 identification task  
                isCorrect = isSameDiff == RESPONSE_SAME;
            }
        }
    }
}
