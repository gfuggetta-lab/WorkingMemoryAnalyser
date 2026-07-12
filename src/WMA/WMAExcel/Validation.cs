using System;
using System.Collections.Generic;

namespace WMAExcel
{
    public static class Validation
    {
        // Checking the InputData file structure.
        // And leaving the notes, in case something is missing
        public static void Validate(ExcelInputData data, List<string> notes)
        {
            if ((data == null) || (notes == null))
                return;

            HashSet<string> trialDataNames = new HashSet<string>(
                data.TrialDataNames,
                StringComparer.OrdinalIgnoreCase);

            foreach (var ev in data.Events.Values)
            {
                if (string.Compare(ev.Type, "S", true) != 0)
                    continue;

                string markerName = ev.Name + "_Marker";
                if (!trialDataNames.Contains(markerName))
                    notes.Add($"Missing trial sequence column: {markerName}");
            }
        }
    }
}
