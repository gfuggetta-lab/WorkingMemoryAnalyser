using System;
using System.IO;
using System.Collections.Generic;

namespace WMAFiles
{
    public class ConfigFile
    {
        public Dictionary<string, string> strVals = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        public void FeedRawLine(string ln)
        {
            if (string.IsNullOrEmpty(ln)) return;
            int i = ln.IndexOf(":");
            if (i < 0) return;
            string k = ln.Substring(0, i).Trim();
            string v = ln.Substring(i + 1);
            strVals[k] = v;
        }

        public static ConfigFile FromStream(Stream src)
        {
            ConfigFile result = new ConfigFile();
            using (StreamReader rdr = new StreamReader(src))
            {
                while (true)
                {
                    var s = rdr.ReadLine();
                    if (s == null) break;
                    if (string.IsNullOrWhiteSpace(s)) continue;
                    result.FeedRawLine(s);
                }
                return result;
            }

        }
        public static ConfigFile FromFile(string fileName)
        {
            using(FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return FromStream(fs);
            }
        }
    }
}
