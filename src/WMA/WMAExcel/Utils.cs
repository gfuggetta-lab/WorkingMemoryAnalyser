using NPOI.OpenXmlFormats.Dml;
using System;
using System.Collections.Generic;
using System.Text;

namespace WMAExcel
{
    public static class Utils
    {
        // Parses the line of the name
        //   "FB1_L1_No_of_vertices_of_the_virtual_circle:"
        // and returns its 
        //   stimName  = "FB1"
        //   stimType  = "FB"
        //   stimNum   = 1
        //   levelName = "L1"
        //   levelNum  = 1
        //   suffix    = "No_of_vertices_of_the_virtual_circle "
        // If no level is specified returns levelName as ""
        // if no stimuli prefix is specified returns as ""
        public static bool ParseName(string name, 
            out string stimName,
            out string stimType, out int stimNum,
            out string levelName, out int levelNum,
            out string suffix)
        {
            stimName = "";
            levelName = "";
            stimType = "";
            stimNum = 0;
            levelNum = 0;
            suffix = "";
            if (string.IsNullOrWhiteSpace(name))
                return false;
            int i = name.Length;
            while ((i >= 0) && (name[i] == ':')) 
                i--;
            if (i < 0)
                return false;
            string n = name.Substring(0, i);
            if (string.IsNullOrWhiteSpace(n))
                return false;

            string[] parts = n.Split(new char[] { '_' }, StringSplitOptions.None);
            if (parts.Length <= 1)
            {
                suffix = n;
                return true;
            }
            if (!IsSpecialNum(parts[0], out var st, out var stNum))
            {
                suffix = n;
                return true;
            }
            if (!IsValidStimuliPfx(st))
            {
                suffix = n;
                return true;
            }
            stimName = n;
            int sfxOfs = 1;
            if ((IsSpecialNum(parts[1], out var lvlPfx, out var ln)) && (IsValidLevelPfx(lvlPfx)))
            {
                levelName = parts[1];
                levelNum = ln;
                sfxOfs++;
            }
            if (parts.Length > 2)
                suffix = string.Join("_", parts, sfxOfs, parts.Length - sfxOfs);
            return true;
        }
        
        private static bool GetNum(string s, string pfx, out int num, bool okayForEmpty = true)
        {
            num = 0;
            int i = pfx.Length;
            if (i == s.Length)
            {
                return okayForEmpty;
            }
            string ns = s.Substring(i);
            return int.TryParse(ns, out num);
        }
        public static bool IsValidStimuliPfx(string s)
        {
            return (string.Compare(s, "S", true) == 0)
                || (string.Compare(s, "R", true) == 0)
                || (string.Compare(s, "FB", true) == 0)
                || (string.Compare(s, "C", true) == 0);
        }
        public static bool IsValidLevelPfx(string s)
        {
            return string.Compare(s, "L", true) == 0;
        }

        public static bool IsSpecialNum(string s, out string stimPfx, out int stimNum)
        {
            stimNum = 0;
            // C must not have a number
            if (string.Compare(s, "C", true) == 0)
            {
                stimPfx = "C";
                return true;
            }
            if (s.StartsWith("S", StringComparison.OrdinalIgnoreCase))
            {
                // S must have a number!
                stimPfx = "S";
                return GetNum(s, "S", out stimNum, false);
            }
            if (s.StartsWith("R", StringComparison.OrdinalIgnoreCase))
            {
                // R, could have a number, but not required
                stimPfx = "R";
                return GetNum(s, "R", out stimNum, true);
            }
            if (s.StartsWith("FB", StringComparison.OrdinalIgnoreCase))
            {
                // R, could have a number, but not required
                stimPfx = "FB";
                return GetNum(s, "FB", out stimNum, true);
            }
            if (s.StartsWith("L", StringComparison.OrdinalIgnoreCase))
            {
                // L, must have a number
                stimPfx = "L";
                return GetNum(s, "L", out stimNum, false);
            }
            stimPfx = "";
            return false;
        }

        public static bool IsInputDataSheet(string nm, out int idx)
        {
            idx = 0;
            if (nm == null) return false;
            bool res = (nm.IndexOf("inputdata", StringComparison.OrdinalIgnoreCase) >= 0);
            if (!res) return false;

            string[] p = nm.Split(new char[] { '_' }, StringSplitOptions.None);
            if (p.Length >= 2) 
                int.TryParse(p[1], out idx);

            return true;
        }

        public static bool IsConfigSheet(string nm)
        {
            if (nm == null) return false;
            return (nm.IndexOf("configuration", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public static bool StartsWith(this string s, string pattern)
        {
            if (s == null) return false;
            return s.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool TryGetNumber(string s, string pfx, out int num, string sep= "_")
        {
            int i = pfx.Length;
            int j = s.IndexOf(sep, i);
            string n;
            if (j > 0) n = s.Substring(i, j - i);
            else n = s.Substring(i);
            return int.TryParse(n, out num);
        }
    }
}
