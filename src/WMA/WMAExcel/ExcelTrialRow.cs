using System;
using System.Collections.Generic;
using System.Text;

namespace WMAExcel
{
    public class ExcelTrialRow
    {
        public Dictionary<string, string> data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public List<ExcelTrialObject> objects = null;
        public List<ExcelTrialEvent> events = null; 

        // Experimental_Condition is recorder as conditions[0]
        // Condition_1 is recorded as conditions[1]
        // Condition_2 is recorded as conditions[2], etc
        public Dictionary<int, string> conditions = null;

        public void Populate()
        {

        }

        public string St(string fullName, string def = "")
        {
            if (!data.TryGetValue(fullName, out var result))
                return def;
            return result;
        }

    }
}
