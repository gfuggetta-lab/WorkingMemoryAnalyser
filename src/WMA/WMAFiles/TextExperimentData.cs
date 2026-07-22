using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WMAData;

namespace WMAFiles
{
    public class TextExperimentData : IExperimentData
    {
        public Configuration cfg;
        public string Overview;

        public int[] inputNums;
        public Dictionary<int, string> inputFiles;

        private int inputDataNum = -1;
        private List<TrialOrder> trials = null;
        private List<PauseData> pauses = null;

        public string GetOverview()
        {
            return this.Overview;
        }

        public int[] GetInputDataListSync()
        {
            return inputNums;
        }

        public bool SelectInputdata(int inputNum)
        {
            bool result = inputFiles.TryGetValue(inputNum, out var inputFn);
            if ((inputNum < 0) || (!result))
            {
                inputDataNum = -1;
                trials = null;
                pauses = null;
                // we return true, if -1 is explicitly requested
                return inputNum == -1;
            }

            inputDataNum = inputNum;

            trials = new List<TrialOrder>();
            pauses = new List<PauseData>();
            InputDataHelper.LoadTrials(inputFn, trials, pauses);
            return result;
        }

        public bool SchedulePlaylist(TrialMonitor display, PlayList playList, out int trialsCount)
        {
            trialsCount = 0;
            if (inputDataNum < 0) 
                return false;

            if (!inputFiles.TryGetValue(inputDataNum, out var fn))
                return false;

            cfg.Schedule(display, trials, pauses, playList);
            trialsCount = trials.Count;
            return true;
        }

        public string GetKeyboardCsv()
        {
            return cfg.keyboards;
        }
        public void GetPreloadImages(List<string> names)
        {
            if (inputDataNum < 0) return;
            cfg.GetPreloadImages(trials, names);
        }
        public void GetPreloadFonts(List<string> names)
        {
            if (inputDataNum < 0) return;
            cfg.GetPreloadFonts(names);
        }

        public void GetPreloadSounds(List<string> names)
        {
            if (inputDataNum < 0) return;
            cfg.GetPreloadImages(trials, names);
        }
    }
}
