using System;
using System.Collections.Generic;
using System.Text;

namespace WMAData
{
    public class ExperimentData
    {
        public string ExperimentName;
        public string ParticipantId;
        public string Age;
        public string Sex;
        public DateTime TimeStamp;
        public string Handedness;
        public string DisplayType;

        // the X in InputData_X file name that was used to run the experiment
        public int TrialOrderNum;

        // the session number. Ususally it's 1
        public int SessionNum;

        public ExperimentData()
        {

        }

        public static ExperimentData Start()
        {
            return new ExperimentData
            {
                TimeStamp = DateTime.Now
            };
        }
    }
}
