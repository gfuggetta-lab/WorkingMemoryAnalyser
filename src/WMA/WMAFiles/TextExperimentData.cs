using System;
using System.Collections.Generic;
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

        public string GetOverview()
        {
            return this.Overview;
        }

        public int[] GetInputDataListSync()
        {
            return inputNums;
        }

        public bool SchedulePlaylist(int inputDataNum, TrialMonitor display, PlayList playList)
        {
            if (!inputFiles.TryGetValue(inputDataNum, out var fn))
                return false;

            List<TrialOrder> trials = new List<TrialOrder>();
            List<PauseData> pauses = new List<PauseData>();
            var result = InputDataHelper.LoadTrials(fn/*$"InputData_{inputDataNum}.txt"*/, trials, pauses);
            cfg.Schedule(display, trials, pauses, playList);
            return true;
        }
    }
}
