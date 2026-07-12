using System;
using System.Collections.Generic;
using System.Text;

namespace WMAData
{


    public class TrialOrder
    {
        public int session_number;
        public Dictionary<string, StimuliData> slk = new Dictionary<string, StimuliData>(StringComparer.OrdinalIgnoreCase);

        public StimuliData ForceStimuli(string nm)
        {
            return GetStimuli(nm, true);
        }

        public StimuliData S1 => ForceStimuli("S1");
        public StimuliData S2 => ForceStimuli("S2");
        public StimuliData S3 => ForceStimuli("S3");
        public StimuliData S4 => ForceStimuli("S4");

        public int Feedback_shape; // see SHAPE_ constants

        public int Feedback_sound; // 0 - none, 1:correct_incorrect_auditory_feedback

        public int Feedback_duration_after_response_time; // milisecond. Range_200_10000_ms

        // Range_from_>=_response_time_plus_feedback to_10000_milliseconds
        public int ITI_after_feedback; // Inter_trial_interval

        public int key_mapping;
        public int taskType;

        public int TMS_S3_SOA = -100000; // milisecond

        public string ExpCondition;

        public List<string> Factors = new List<string>();
        public Dictionary<string, string> FactorLk = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        public StimuliData GetStimuli(string nm, bool forced = false)
        {
            if (!slk.TryGetValue(nm, out var result))
            {
                if (!forced)
                    return null;
                result = new StimuliData();
                slk[nm] = result;
            }
            return result;
        }
    }
}
 