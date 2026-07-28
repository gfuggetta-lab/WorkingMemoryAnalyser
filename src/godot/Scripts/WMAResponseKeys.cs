using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace godot.Scripts
{
    public class WMAResponseKeys
    {
        public HashSet<string> resp = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> corr = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public void SetResponse(string[] responseKeys, string[] correctKeys)
        {
            foreach (var t in responseKeys)
            {
                if (t == null) continue;
                resp.Add(t);
            }
            foreach (var t in correctKeys)
            {
                if (t == null) continue;
                corr.Add(t);
            }
        }
        public bool IsResponse(string s)
        {
            if (s == null) return false;
            return resp.Contains(s);
        }
        public bool IsCorrect(string s)
        {
            if (s == null) return false;
            return corr.Contains(s);
        }
    }

    public class WMAResponseResults
    {
        public string name;
        public bool isCorrect;
    }
}
