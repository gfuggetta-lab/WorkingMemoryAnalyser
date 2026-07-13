using System;
using System.Globalization;
using System.Linq;
using WMAData;

namespace WMAExcel
{
    public static class ExcelToWMA
    {
        public static void ExcelToCfg(ExcelConfig src, Configuration dst)
        {
            if ((src == null) || (dst == null))
                return;

            dst.Overview = ConvertOverview(src.Overview);
            dst.ExperimentName = src.Experiment;

            if (TryParseDouble(src.Minimum_training_accuracy, out var minimumTrainingAccuracy))
                dst.Minimum_training_accuracy = minimumTrainingAccuracy;

            if (TryParseInt(src.N_trials_before_pause_training, out var trialsBeforePauseTraining))
                dst.N_trials_before_pause_training = trialsBeforePauseTraining;

            dst.Instructions_ODD_participants = src.Instructions_ODD_participants;
            dst.Instructions_EVEN_participants = src.Instructions_EVEN_participants;
            dst.Audio_Instructions_ODD_participants = src.Audio_Instructions_ODD_participants;
            dst.Audio_Instructions_EVEN_participants = src.Audio_Instructions_EVEN_participants;

            if (TryParseInt(src.RT_constant_error_ms, out var rtConstantErrorMs))
                dst.RT_constant_error_ms = rtConstantErrorMs;

            if (TryParseColor(src.Run_background_shape_colour, out var runBackground))
                dst.backgroundCircleColor = runBackground;

            if (TryParseColor(src.Pause_background_shape_colour, out var pauseBackground))
                dst.Pause_background_circle_colour = pauseBackground;

            CopyMessages(src, dst);
            CopyFonts(src, dst);
            CopyShapeColors(src, dst);

            // Skipped intentionally: Reference_Number, Version, Release_date, Author,
            // Member_user_name, Age_range, Device, Study, and Study_Reference_Number
            // do not currently have direct fields in WMAData.Configuration.
            // Skipped intentionally: LUFA_USB_CDC_Interrupt is device IO metadata and
            // has no direct WMAData.Configuration target yet.
        }

        public static string ConvertOverview(string excelOver)
        {
            string[] lines = excelOver.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                string s = lines[i].Trim();
                if (s.EndsWith("@"))
                    s = s.Substring(0, s.Length - 1);
                lines[i] = s;
            }
            return string.Join("\r\n", lines);
        }

        private static void CopyMessages(ExcelConfig src, Configuration dst)
        {
            if (src.Messages.Count == 0)
                return;

            int max = src.Messages.Keys.Max();
            string[] messages = new string[max + 1];
            foreach (var kv in src.Messages)
            {
                if (kv.Key >= 0)
                    messages[kv.Key] = kv.Value;
            }
            dst.Message = messages;
        }

        private static void CopyFonts(ExcelConfig src, Configuration dst)
        {
            if (src.Fonts.TryGetValue(1, out var font1))
                dst.font_1 = font1;

            if (src.Fonts.TryGetValue(2, out var font2))
                dst.font_2 = font2;

            if (src.Fonts.TryGetValue(3, out var feedbackFont))
                dst.Feedback_font = feedbackFont;

            // Skipped intentionally: Excel Font_4 and higher have no direct named
            // targets in WMAData.Configuration at the moment.
        }

        private static void CopyShapeColors(ExcelConfig src, Configuration dst)
        {
            foreach (var kv in src.Shapes_text_colour)
            {
                if ((kv.Key < 0) || (kv.Key >= dst.ShapeColors.Length))
                    continue;

                if (TryParseColor(kv.Value, out var color))
                    dst.ShapeColors[kv.Key] = color;
            }
        }

        private static bool TryParseInt(string value, out int result)
        {
            return int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }

        private static bool TryParseDouble(string value, out double result)
        {
            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }

        private static bool TryParseColor(string value, out ColorFloat result)
        {
            result = new ColorFloat();
            if (string.IsNullOrWhiteSpace(value))
                return false;

            string[] parts = value.Split(',');
            if (parts.Length < 3)
                return false;

            if (!TryParseDouble(parts[0], out var r)
                || !TryParseDouble(parts[1], out var g)
                || !TryParseDouble(parts[2], out var b))
                return false;

            result = new ColorFloat
            {
                r = NormalizeColorChannel(r),
                g = NormalizeColorChannel(g),
                b = NormalizeColorChannel(b)
            };
            return true;
        }

        private static double NormalizeColorChannel(double value)
        {
            if (value > 1.0)
                return value / 255.0;

            return value;
        }
    }
}
