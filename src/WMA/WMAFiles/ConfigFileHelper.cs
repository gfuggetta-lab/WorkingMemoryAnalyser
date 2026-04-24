using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using WMAData;

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

        public static string GetFirstDouble(string s)
        {
            int i = 0;
            while ((i < s.Length) && Char.IsWhiteSpace(s, i))
                i++;
            int j = i;
            while ((i < s.Length) && Char.IsDigit(s, i))
                i++;
            if ((i < s.Length) && (s[i] == '.'));
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

        public static ColorFloat Color(this ConfigFile cfg, string paramName)
        {
            ColorFloat n = new ColorFloat();
            var ss = cfg.String(paramName, "");
            if (string.IsNullOrEmpty(ss)) return n;
            ss = Unq(ss);
            string[] cmp = ss.Split(new char[] { ',' });
            int r = 0;
            int g = 0;
            int b = 0;
            if (cmp.Length > 0) int.TryParse(cmp[0], out r);
            if (cmp.Length > 1) int.TryParse(cmp[1], out g);
            if (cmp.Length > 2) int.TryParse(cmp[1], out b);
            n.r = r / 255.0;
            n.g = g / 255.0;
            n.b = b / 255.0;
            return n;

        }

    }
}
