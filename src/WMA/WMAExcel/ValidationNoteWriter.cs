using System;
using System.Collections.Generic;

namespace WMAExcel
{
    public sealed class ValidationNoteWriter
    {
        private readonly List<string> notes;
        private readonly HashSet<string> groups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public ValidationNoteWriter(List<string> notes)
        {
            this.notes = notes;
        }

        public void Add(string note)
        {
            if ((notes == null) || string.IsNullOrWhiteSpace(note))
                return;

            notes.Add(note);
        }

        public void AddTrialNote(ExcelInputData data, int trialIndex, string note)
        {
            if ((notes == null) || (data == null) || string.IsNullOrWhiteSpace(note))
                return;

            string groupKey = $"InputData_{data.Index}_Trial_{trialIndex + 1}";
            if (!groups.Contains(groupKey))
            {
                groups.Add(groupKey);
                notes.Add($"InputData_{data.Index}, trial {trialIndex + 1}:");
            }

            notes.Add("  " + note);
        }
    }
}
