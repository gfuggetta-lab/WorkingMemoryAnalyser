using System;
using System.Collections.Generic;
using System.Text;
using WMAData;

namespace WMAFiles
{


    public class TrialOrder
    {
        public int session_number;
        public Dictionary<string, StimuliData> slk = new Dictionary<string, StimuliData>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, List<StimuliData>> grpLk = new Dictionary<string, List<StimuliData>>(StringComparer.OrdinalIgnoreCase);

        public StimuliData S1 => GetStimuli("S1", true);
        public StimuliData S2 => GetStimuli("S2", true);
        public StimuliData S3 => GetStimuli("S3", true);
        public StimuliData S4 => GetStimuli("S4", true);

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

        public StimuliData AddStimuli(string nm, string grp)
        {
            return GetStimuli(nm, grp, true);
        }

        public StimuliData GetStimuli(string nm, bool forced = false)
        {
            return GetStimuli(nm, nm, forced);
        }
        public StimuliData GetStimuli(string nm, string grp, bool forced = false)
        {
            if (!slk.TryGetValue(nm, out var result))
            {
                if (!forced)
                    return null;
                result = new StimuliData();
                slk[nm] = result;
            }
            if (!grpLk.TryGetValue(grp, out var grpList))
            {
                grpList = new List<StimuliData>();
                grpLk[nm] = grpList;
            }
            grpList.Add(result);
            return result;
        }
    
        public List<StimuliData> GetStimuliGroup(string grp)
        {
            grpLk.TryGetValue(grp, out var result);
            return result;
        }
    }
}
 