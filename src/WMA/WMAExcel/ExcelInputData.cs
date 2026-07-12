using System;
using System.Collections.Generic;
using System.Text;

namespace WMAExcel
{
    public class ExcelInputData
    {
        public int Index;

        public string Experiment;
        public string Reference_Number;
        public string Version;
        public string Release_date;
        public string Author;
        public string Member_user_name;
        public string Age_range;
        public string Device;
        public string Study;
        public string Study_Reference_Number;
        public string Overview;

        public string Background_Type;
        public string Background_Object;
        public string Background_diameter_deg;
        public string Background_sound;

        public string Number_of_events_on_a_trial;
        // Events  can be either
        // Stimulus [S] range from 1 to 64,
        // Response [R] range from 1 to 64,
        // Feedback [FB] range from 1 to 64
        // and a continuously displayed superimposed stimulus [C]. 
        public string Sequence_of_Events_of_a_trial;

        public Dictionary<string, ExcelInputEvent> Events = new Dictionary<string, ExcelInputEvent>(StringComparer.OrdinalIgnoreCase);

        public List<ExcelTrialRow> Trials = new List<ExcelTrialRow>();

    }
}
