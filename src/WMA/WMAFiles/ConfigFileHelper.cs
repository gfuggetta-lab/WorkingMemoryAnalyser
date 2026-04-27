using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using WMAData;
using static WMAData.Consts;

namespace WMAFiles
{
    public static class ConfigFileHelper
    {
        // For the backwards compatibility. The parameter names
        // in the original code are passed with ":" at the end
        // Thus code simply cuts of one colon character.
        public static string GetParamName(string p)
        {
            if (p.EndsWith(":")) return p.Substring(0, p.Length - 1);
            else return p;
        }

        public static bool TryRawVal(this ConfigFile cfg, string paramName, out string val)
        {
            paramName = GetParamName(paramName);
            return cfg.strVals.TryGetValue(paramName, out val);
        }
        public static string StringLine(this ConfigFile cfg, string paramName, string def = "")
        {
            if (cfg == null) return def;
            if (!cfg.TryRawVal(paramName, out var result))
                return def;
            return result;
        }

        // Getting the first integer from the string.
        // the value can be written as:
        //    Show_S4_placeholder_when_centre: 1		[1 yes 0 no] 
        // and only the very first one value is needed
        public static int Integer(this ConfigFile cfg, string paramName, int def = 0)
        {
            if (cfg == null) return def;
            if (!cfg.TryRawVal(paramName, out var result))
                return def;

            string s = GetFirstInt(result);

            if (int.TryParse(s, out var resint))
                return resint;
            return def;
        }

        public static string GetFirstInt(string s)
        {
            int i = 0;
            while ((i < s.Length) && Char.IsWhiteSpace(s, i)) 
                i++;
            int j = i;
            while ((i < s.Length) && Char.IsDigit(s, i)) 
                i++;
            if (i > j) 
                return s.Substring(j, i - j);
            return string.Empty;
        }

        public static string String(this ConfigFile cfg, string paramName, string def = "")
        {
            if (cfg == null) return def;
            if (!cfg.TryRawVal(paramName, out var result))
                return def;

            return GetFirstStr(result);
        }

        public static string GetFirstStr(string s)
        {
            int i = 0;
            while ((i < s.Length) && Char.IsWhiteSpace(s, i))
                i++;
            int j = i;
            while ((i < s.Length) && !Char.IsWhiteSpace(s, i))
                i++;
            if (i > j)
                return s.Substring(j, i - j);
            return string.Empty;
        }

        public static double Float(this ConfigFile cfg, string paramName, double def = 0.0)
        {
            if (cfg == null) return def;
            if (!cfg.TryRawVal(paramName, out var result))
                return def;

            string vstr = GetFirstDouble(result);
            if (double.TryParse(vstr, NumberStyles.Number,  CultureInfo.InvariantCulture, out var resflt))
            {
                return resflt;
            }
            return def;
        }
        public static double FloatNonZero(this ConfigFile cfg, string paramName, double def = 0.0)
        {
            var res = cfg.Float(paramName);
            if (res <= 0) return def;
            return res;
        }

        public static string GetFirstDouble(string s)
        {
            int i = 0;
            while ((i < s.Length) && Char.IsWhiteSpace(s, i))
                i++;
            int j = i;
            while ((i < s.Length) && Char.IsDigit(s, i))
                i++;
            if ((i < s.Length) && (s[i] == '.'))
            {
                i++;
                while ((i < s.Length) && Char.IsDigit(s, i))
                    i++;
            }
            if (i > j)
                return s.Substring(j, i - j);
            return string.Empty;
        }

        public static string Unq(string s)
        {
            if (s.Length < 2) return s;

            if (s.StartsWith("\"") && s.EndsWith("\""))
                return s.Substring(1, s.Length - 2);
            return s;
        }

        public static bool Color(this ConfigFile cfg, string paramName, out ColorFloat clr)
        {
            
            var ss = cfg.String(paramName, "");
            if (string.IsNullOrEmpty(ss))
            {
                clr.r = 0;
                clr.g = 0;
                clr.b = 0;
                return false;
            }
            ss = Unq(ss);
            string[] cmp = ss.Split(new char[] { ',' });
            int r = 0;
            int g = 0;
            int b = 0;
            if (cmp.Length > 0) int.TryParse(cmp[0], out r);
            if (cmp.Length > 1) int.TryParse(cmp[1], out g);
            if (cmp.Length > 2) int.TryParse(cmp[2], out b);
            clr.r = r / 255.0;
            clr.g = g / 255.0;
            clr.b = b / 255.0;
            return true;
        }
        public static ColorFloat Color(this ConfigFile cfg, string paramName)
        {
            ColorFloat n = new ColorFloat();
            Color(cfg, paramName, out n);
            return n;
        }


        public static void LoadConfig(this Configuration dst, ConfigFile src)
        {
            dst.distanceCm = src.FloatNonZero("Monitor_distance_cm:", distance_DEFAULT);

            dst.Placeholder_diameter_deg = src.Float("Placeholder_diameter_deg:");
            dst.Shape_scale_factor = src.Float("Shape_scale_factor:");

            dst.Minimum_training_accuracy = src.Float("Minimum_training_accuracy:");

            dst.N_trials_before_pause_training = src.Integer("N_trials_before_pause_training:");
            dst.N_trials_before_pause_main = src.Integer("N_trials_before_pause_main:");

            
            dst.Instructions_ODD_participants = src.String("Instructions_ODD_participants:");
            dst.Instructions_EVEN_participants = src.String("Instructions_EVEN_participants:");

            dst.font_1.name = src.String("Stimulus_font_1:");
            dst.font_1.size = src.Float("Stimulus_font_1_size:");
            dst.font_1.style = src.String("Stimulus_font_1_style:");

            dst.font_2.name = src.String("Stimulus_font_2:");
            dst.font_2.size = src.Float("Stimulus_font_2_size:");
            dst.font_2.style = src.String("Stimulus_font_2_style:");

            dst.Feedback_font.name = src.String("Feedback_font:");
            dst.Feedback_font.size = src.Float("Feedback_font_size:");
            dst.Feedback_font.style = src.String("Feedback_font_style:");

            dst.Feedback_text_correct = src.StringLine("Feedback_text_correct:");
            dst.Feedback_text_incorrect = src.StringLine("Feedback_text_incorrect:");
            dst.RT_constant_error_ms = src.Integer("RT_constant_error_ms:");

            if (string.Compare(dst.Feedback_font.name, "symbola.ttf", true) == 0)
            {
                dst.Feedback_text_correct = "\u263A"; // smiley
                dst.Feedback_text_incorrect = "\u2639"; //sad
            }
            dst.Pause_background_circle_colour = src.Color("Pause_background_circle_colour:");

            src.Color("Run_background_circle_colour:", out dst.backgroundCircleColor);

            src.Color("Fixation_&_placeholders_colour:", out var _Fix_Plc_colour);

            if (!src.Color("Fixation_colour:", out dst.Fixation_colour))
                dst.Fixation_colour = _Fix_Plc_colour;
            if (!src.Color("Placeholders_colour:", out dst.Placeholders_colour))
                dst.Placeholders_colour = _Fix_Plc_colour;

            dst.Incorrect_feedback_colour = src.Color("Incorrect_feedback_colour:");
            dst.Correct_feedback_colour = src.Color("Correct_feedback_colour:");

            //  getShapeColours(configDataFilename,colours)  ;
            for (int i = 0; i < dst.ShapeColors.Length; i++)
            {
                src.Color($"Shapes_colour_{i}:", out dst.ShapeColors[i]);
            }

            // not used
            // Monitor_name:= getStringForParameter(configDataFilename, 'Monitor_name:');

            // todo:
            // ReadPhotodiode( Photodiode_S3,     configDataFilename, 'S3_photodiode_patch');
            // ReadPhotodiode( Photodiode_TMS_S3, configDataFilename, 'TMS_S3_photodiode_patch');
            // ReadPhotodiode( Photodiode_S4,     configDataFilename, 'S4_photodiode_patch');

            dst.Show_S3_peripheral_placeholders = src.Integer("Show_S3_peripheral_placeholders:") != 0;
            dst.Show_S4_peripheral_placeholders = src.Integer("Show_S4_peripheral_placeholders:") != 0;
            dst.Show_S3_placeholder_when_centre = src.Integer("Show_S3_placeholder_when_centre:") != 0;
            dst.Show_S4_placeholder_when_centre = src.Integer("Show_S4_placeholder_when_centre:") != 0;

            dst.Background_diameter_deg = src.FloatNonZero("Background_diameter_deg:", background_deg_DEFAULT);
            dst.Placeholders_diameter_deg = src.FloatNonZero("Placeholders_diameter_deg:", Placeholders_diameter_deg_DEFAULT);

            dst.S2_sample_diameter_deg = src.FloatNonZero("S2_Sample_diameter_deg:", Sample_diameter_deg_DEFAULT);
            dst.Fixation_dot_deg = src.FloatNonZero("Fixation_dot_deg:", Fixation_dot_deg_DEFAULT);
            dst.Shape_size_deg = src.FloatNonZero("Shape_size_deg:", Shape_size_deg_DEFAULT);
            dst.Image_feedback_deg = src.FloatNonZero("Image_feedback_deg:", Image_feedback_deg_DEFAULT);

            dst.Image_size_deg = src.FloatNonZero("Image_size_deg:", Image_size_deg_DEFAULT);

            dst.keyboards = src.StringLine("Keyboard_keys_used_to_respond:");
            //KeyboardSetupResponse(keyboards);                

            dst.CalculateCmFromDeg();
        }
    }
}
