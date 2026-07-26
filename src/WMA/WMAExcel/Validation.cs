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

        public static void ValidateAssets(IExperimentData data, string rootDir, List<string> notes)
        {
            if ((data == null) || (notes == null))
                return;

            string baseDir = rootDir ?? string.Empty;
            string imagesDir = Path.Combine(baseDir, "Stimulis images");
            string soundsDir = Path.Combine(baseDir, "Stimulus sounds");

            List<string> images = new List<string>();
            data.GetPreloadImages(images);

            foreach (string image in images)
            {
                if (string.IsNullOrWhiteSpace(image))
                    continue;

                string imagePath = Path.Combine(imagesDir, image);
                if (!File.Exists(imagePath))
                    notes.Add($"Missing preload image file: {imagePath}");
            }

            List<string> sounds = new List<string>();
            data.GetPreloadSounds(sounds);

            foreach (string sound in sounds)
            {
                if (string.IsNullOrWhiteSpace(sound))
                    continue;

                string soundPath = Path.Combine(soundsDir, sound);
                if (!File.Exists(soundPath))
                    notes.Add($"Missing preload sound file: {soundPath}");
            }
        }
    }
}
