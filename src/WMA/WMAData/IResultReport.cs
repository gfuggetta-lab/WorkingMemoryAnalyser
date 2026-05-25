using System;
using System.Collections.Generic;
using System.Text;

namespace WMAData
{
    public interface IResultReport
    {
        // Write Date
        // Write Time
        // Write Particiap ID
        // Write Age
        // Write Gender

        void SetConfig(Configuration cfg);
        void WriteTrial(TrialOrder trial, TrialResults res);
    }

    public class TrialResults
    {
        // -1: not given; 0: different; 1: same;
        public int observedDataResponseRecord;
        // -1: not given; 0: incorrect; 1: correct
        public int observedDataCorrectResponseRecord;

        public int responseTimeMs;

        public int s1_onsetTime;
        public int s2_onsetTime;
        public int s3_onsetTime;
        public int s4_onsetTime;
        public int response_onsetTime;
        public int feedback_onsetTime;
        public int blank_onsetTime;    // ( 1000ms after auditory feedback)
        public int TMS_onsetTime;

        public int isRuinedTrial;
    }

}
