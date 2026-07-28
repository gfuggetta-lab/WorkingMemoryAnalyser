using System;
using System.Collections.Generic;

namespace WMAExcel
{
    public sealed class ValidationNoteWriter
    {
        private readonly List<string> notes;
        private readonly Dictionary<string, TrialNoteGroup> trialGroups = new Dictionary<string, TrialNoteGroup>(StringComparer.Ordinal);
        private readonly List<TrialNoteGroup> trialGroupsOrder = new List<TrialNoteGroup>();

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

            string groupKey = data.Index.ToString() + "\u001f" + note;
            if (!trialGroups.TryGetValue(groupKey, out TrialNoteGroup group))
            {
                group = new TrialNoteGroup(data.Index, note);
                trialGroups[groupKey] = group;
                trialGroupsOrder.Add(group);
            }

            group.TrialNumbers.Add(trialIndex + 1);
        }

        public void FlushTrialNotes()
        {
            if (notes == null)
                return;

            foreach (TrialNoteGroup group in trialGroupsOrder)
            {
                string trialLabel = group.TrialNumbers.Count == 1 ? "trial" : "trials";
                notes.Add($"InputData_{group.InputDataIndex}, {trialLabel} {FormatTrialRanges(group.TrialNumbers)}:");
                notes.Add("  " + group.Note);
            }

            trialGroups.Clear();
            trialGroupsOrder.Clear();
        }

        private static string FormatTrialRanges(SortedSet<int> trialNumbers)
        {
            List<string> ranges = new List<string>();
            int rangeStart = 0;
            int rangeEnd = 0;

            foreach (int trialNumber in trialNumbers)
            {
                if (rangeStart == 0)
                {
                    rangeStart = trialNumber;
                    rangeEnd = trialNumber;
                    continue;
                }

                if (trialNumber == rangeEnd + 1)
                {
                    rangeEnd = trialNumber;
                    continue;
                }

                ranges.Add(FormatRange(rangeStart, rangeEnd));
                rangeStart = trialNumber;
                rangeEnd = trialNumber;
            }

            if (rangeStart != 0)
                ranges.Add(FormatRange(rangeStart, rangeEnd));

            return string.Join(",", ranges.ToArray());
        }

        private static string FormatRange(int rangeStart, int rangeEnd)
        {
            if (rangeStart == rangeEnd)
                return rangeStart.ToString();

            return rangeStart + "-" + rangeEnd;
        }

        private sealed class TrialNoteGroup
        {
            public readonly int InputDataIndex;
            public readonly string Note;
            public readonly SortedSet<int> TrialNumbers = new SortedSet<int>();

            public TrialNoteGroup(int inputDataIndex, string note)
            {
                InputDataIndex = inputDataIndex;
                Note = note;
            }
        }
    }
}
