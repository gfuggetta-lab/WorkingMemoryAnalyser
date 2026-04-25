using System;
using System.Collections.Generic;
using System.Text;
using static WMAData.Consts;

namespace WMAData
{
    public class Configuration
    {
        // the expacted distance from the monitor
        public double distanceCm;

        // background circle
        public double Background_diameter_deg;
        public double backgroundRadiusCM;
        public ColorFloat backgroundCircleColor;

        // fixation point (the small in the center of background circle)
        public double Fixation_dot_deg;
        public double fixSpotSizeCM;
        public ColorFloat Fixation_colour;

        public double Image_size_deg;
        public double Image_size_CM;

 
        // Monitor Refresh rate
        public int RefreshRate = 100;


        private void ScheduleDefaults(PlayList dst)
        {
            dst.AddCircle(backgroundRadiusCM, backgroundCircleColor);
            dst.AddCircle(fixSpotSizeCM, Fixation_colour);
        }

        public void Schedule(List<TrialOrder> trials, PlayList dst)
        {

            ScheduleDefaults(dst);
            if (trials == null)
                return;

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
                    dst.AddCircle(Image_size_CM, Fixation_colour, ofsTime, duration);
                    if (tr.S1.Position == POS_CENTER)
                    {
                        dst.ShowImage(ofsTime, duration, 0);
                    } 
                    else if (tr.S1.Position != POS_ALL)
                    {
                        dst.ShowImage(ofsTime, duration, 0);
                    } 
                    else
                    {
                        dst.ShowImage(ofsTime, duration, 0);
                        dst.ShowImage(ofsTime, duration, 0);
                        dst.ShowImage(ofsTime, duration, 0);
                        dst.ShowImage(ofsTime, duration, 0);
                    }
                    dst.PlaySound(ofsTime, tr.S1.Sound);


                    ofsTime += tr.S1.Duration;

                    ofsTime += tr.S1.Next_ISI;
                }


                //=======================================================================================
                /*
                if (show_s2)
                {

                    duration = tr.S2.Duration;
                    dst.AddCircle(Image_size_CM, Fixation_colour, ofsTime, duration);
                    ofsTime += tr.S2.Duration;
                    ofsTime += tr.S2.Next_ISI;
                }

                if (show_s3)
                {

                    duration = tr.S3.Duration;
                    dst.AddCircle(Image_size_CM, Fixation_colour, ofsTime, duration);
                    ofsTime += tr.S3.Duration;
                    ofsTime += tr.S3.Next_ISI;
                }

                duration = tr.S4.Duration;
                dst.AddCircle(Image_size_CM, Fixation_colour, ofsTime, duration);
                ofsTime += tr.S4.Duration;
                ofsTime += tr.S4.Next_ISI;
                */
            }
        }
    }
}
