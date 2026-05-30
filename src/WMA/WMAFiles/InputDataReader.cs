using System;
using System.Collections.Generic;
using System.Text;

namespace WMAFiles
{
    public enum InputLine
    {
        Other,
        Experiment,
        End,
        PauseStart,
        PauseData,
        TrialStart,
        TrialData
    }
    public class InputDataReader
    {
        public string[] lineVals;
        public string[] lastNotEmpty;
        public Dictionary<string, int> names = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

        public readonly char[] TabSplit = new char[] { '\t' };

        private InputLine inData = InputLine.Other;

        public static bool IsEmpty(string[] vv)
        {
            if (vv == null) return true;
            foreach(var v in vv)
            {
                if (!string.IsNullOrWhiteSpace(v))
                    return false;
            }
            return true;
        }

        private void FillNames(string[] vals)
        {
            if (vals == null) return;
            names.Clear();

            // This is a special workaround for some older files that had "Feedback_duration_after_response_time"
            // declared twice
            for (int i = 0; i < vals.Length; i++)
            {
                if (string.Compare(vals[i], "Feedback_duration_after_response_time", true)==0)
                {
                    if ((i + 1 < vals.Length)&& (string.Compare(vals[i+1], "Feedback_duration_after_response_time", true) == 0))
                    {
                        vals[i] = "Feedback_shape";
                    }
                }
            }


            for (int i = 0; i < vals.Length; i++)
            {
                string n = vals[i];
                if (string.IsNullOrWhiteSpace(n))
                    continue;
                names[n] = i;
            }
        }

        public InputLine ConsumeLine(string l)
        {
            if (!IsEmpty(lineVals))
                lastNotEmpty = lineVals;

            lineVals = l.Split(TabSplit);
            string st;
            if (lineVals.Length > 0)
                st = lineVals[0];
            else
                st = string.Empty;

            InputLine resultType = InputLine.Other;
            if (st == "#PAUSE_TRIALS")
            {
                FillNames(lastNotEmpty);
                resultType = InputLine.PauseStart;
                inData = InputLine.PauseData;
            }
            else if (st == "#TRIAL_DATA")
            {
                FillNames(lastNotEmpty);
                resultType = InputLine.TrialStart;
                inData = InputLine.TrialData;
            }
            else if (st == "#END")
            {
                resultType = InputLine.End;
                inData = InputLine.Other;
            }
            else
                resultType = inData;

            return resultType;
        }
        
        public int GetNameIdx(string nm)
        {
            if (!names.TryGetValue(nm, out var result))
                return -1;
            return result;
        }
        public bool TryGetVal(int idx, out string val)
        {
            val = null;
            if ((idx < 0) || (idx >= lineVals.Length))
                return false;
            val = lineVals[idx];
            return true;
        }
        public bool TryGetVal(string name, out string val)
        {
            var idx = GetNameIdx(name);
            return TryGetVal(idx, out val);
        }
    }
}
