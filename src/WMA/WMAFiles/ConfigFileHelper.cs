using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;

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
    }
}
