using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

namespace WMAExcel
{
    // The data is populated based on the columns in Trial Orders rows
    // Using the following columns
    //  S1_Marker
    //  S1_sound
    //  S1_Duration
    //  S1_data_logging
    //  S1_ISI
    public class ExcelTrialEvent
    {
        public string Event = ""; // S1

        public string Marker = "";
        public string Sound = "";
        public string Duration = "";
        public string Data_Logging = "";
        public string ISI = "";

        // This is only used indirectly by R section
        // R section actually refers to SNNN section
        // Thus SNNN_Reponse_Time would be populated
        public string Response_time = "";

        // used by R only
        public string Response = "";
        public string Correct_response = "";

        public static readonly ExcelTrialEvent Empty = new ExcelTrialEvent();
    }
}
