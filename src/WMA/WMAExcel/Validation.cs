using System;
using System.Collections.Generic;
using System.IO;
using WMAData;

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

            ValidationNoteWriter noteWriter = new ValidationNoteWriter(notes);
            HashSet<string> trialDataNames = new HashSet<string>(
                data.TrialDataNames,
                StringComparer.OrdinalIgnoreCase);

            foreach (var ev in data.Events.Values)
            {
                if (string.Compare(ev.Type, "S", true) != 0)
                    continue;

                string markerName = ev.Name + "_Marker";
                if (!trialDataNames.Contains(markerName))
                    noteWriter.Add($"Missing trial sequence column: {markerName}");
            }

            PopulateTrials(data);
            ValidateTrialObjects(data, noteWriter);
        }

        public static void PopulateTrials(ExcelInputData data)
        {
            if ((data == null) || (data.Trials == null))
                return;

            foreach (ExcelTrialRow trial in data.Trials)
            {
                if (trial != null)
                    trial.Populate();
            }
        }

        public static void ValidateTrialObjects(ExcelInputData data, List<string> notes)
        {
            ValidateTrialObjects(data, new ValidationNoteWriter(notes));
        }

        public static void ValidateTrialObjects(ExcelInputData data, ValidationNoteWriter notes)
        {
            if ((data == null) || (notes == null) || (data.Trials == null))
                return;

            for (int trialIndex = 0; trialIndex < data.Trials.Count; trialIndex++)
            {
                ExcelTrialRow trial = data.Trials[trialIndex];
                if (trial == null)
                    continue;

                if (trial.objects == null)
                    continue;

                foreach (ExcelTrialObject obj in trial.objects)
                {
                    if (obj == null)
                        continue;

                    string objectName = FormatTrialObjectName(obj);
                    if (string.IsNullOrWhiteSpace(obj.Type))
                        notes.AddTrialNote(data, trialIndex, $"Missing object Type: {objectName}");

                    if (string.IsNullOrWhiteSpace(obj.Object))
                        notes.AddTrialNote(data, trialIndex, $"Missing object Obj: {objectName}");
                }
            }
        }

        private static string FormatTrialObjectName(ExcelTrialObject obj)
        {
            return $"{obj.Event}_L{obj.Level}_O{obj.Index}, condition {obj.Condition}";
        }

        public static void ValidateAssets(IExperimentData data, string rootDir, List<string> notes)
        {
            if ((data == null) || (notes == null))
                return;

            string baseDir = rootDir ?? string.Empty;
            string imagesDir = Path.Combine(baseDir, "Stimulus images");
            string soundsDir = Path.Combine(baseDir, "Stimulus sounds");

            List<string> images = new List<string>();
            data.GetPreloadImages(images);

            foreach (string image in images)
            {
                if (string.IsNullOrWhiteSpace(image))
                    continue;

                string imagePath = Path.Combine(imagesDir, image);
                if (!AssetExists(imagePath, ".png", ".bmp"))
                    notes.Add($"Missing preload image file: {imagePath}");
            }

            List<string> sounds = new List<string>();
            data.GetPreloadSounds(sounds);

            foreach (string sound in sounds)
            {
                if (string.IsNullOrWhiteSpace(sound))
                    continue;

                string soundPath = Path.Combine(soundsDir, sound);
                if (!AssetExists(soundPath, ".ogg", ".mp3", ".wav"))
                    notes.Add($"Missing preload sound file: {soundPath}");
            }
        }

        private static bool AssetExists(string path, params string[] extensions)
        {
            if (File.Exists(path))
                return true;

            if (!string.IsNullOrEmpty(Path.GetExtension(path)))
                return false;

            foreach (string extension in extensions)
            {
                if (File.Exists(path + extension))
                    return true;
            }

            return false;
        }
    }
}
