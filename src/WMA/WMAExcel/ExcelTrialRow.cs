using System;
using System.Collections.Generic;
using System.Text;

namespace WMAExcel
{
    public class ExcelTrialRow
    {
        public Dictionary<string, string> data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string St(string fullName, string def = "")
        {
            if (!data.TryGetValue(fullName, out var result))
                return def;
            return result;
        }

    }
}
