using NPOI.OpenXmlFormats.Dml.Chart;
using System;
using System.Globalization;
using System.Linq;
using WMAData;
using static WMAExcel.Utils;

namespace WMAExcel
{
    
    public static class ExcelToWMA
    {

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

        

        public static bool TryParseColor(string value, out ColorFloat result)
        {
            result = new ColorFloat();
            if (string.IsNullOrWhiteSpace(value))
                return false;

            string[] parts = value.Split(',');
            if (parts.Length < 3)
                return false;

            var r = ToDouble(parts[0]);
            var g = ToDouble(parts[1]);
            var b = ToDouble(parts[2]);
                
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

        public static PlayItemCond ToCond(ExcelTrialObject.ShowCondition cond)
        {
            switch (cond)
            {
                case ExcelTrialObject.ShowCondition.Incorrect:
                    return PlayItemCond.Incorrect;
                case ExcelTrialObject.ShowCondition.Correct:
                    return PlayItemCond.Correct;
                case ExcelTrialObject.ShowCondition.Ommission:
                    return PlayItemCond.Ommission;
                default:
                    return PlayItemCond.None;
            }

        }
    }

}
