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
