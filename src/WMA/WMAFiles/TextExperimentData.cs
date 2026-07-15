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
            bool result = inputFiles.ContainsKey(inputNum);
            if (!result)
                inputDataNum = -1;
            else
                inputDataNum = inputNum;
            return result;
        }

        public bool SchedulePlaylist(TrialMonitor display, PlayList playList)
        {
            if (inputDataNum < 0) 
                return false;

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
