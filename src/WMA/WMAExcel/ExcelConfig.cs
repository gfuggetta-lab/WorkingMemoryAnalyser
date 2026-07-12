using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using WMAData;

namespace WMAExcel
{
    public class ExcelConfig
    {
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

        // todo: an array?
        public Dictionary<int, ExcelLufa> LUFA_USB_CDC_Interrupt = new Dictionary<int, ExcelLufa>();

        public string Minimum_training_accuracy;
        public string N_trials_before_pause_training;

        public string Instructions_ODD_participants;
        public string Instructions_EVEN_participants;

        public string Audio_Instructions_ODD_participants;
        public string Audio_Instructions_EVEN_participants;

        public string RT_constant_error_ms = "49";

        public string Pause_background_shape_colour;
        public string Run_background_shape_colour;
        public Dictionary<int, string> Shapes_text_colour = new Dictionary<int, string>();


        public Dictionary<int, string> Messages = new Dictionary<int, string>();
        public Dictionary<int, FontData> Fonts = new Dictionary<int, FontData>();
    }
}
