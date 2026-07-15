using System;
using System.Collections.Generic;
using System.Text;

namespace WMAData
{
    public interface IExperimentData
    {
        string GetOverview();

        int[] GetInputDataListSync();

        // InputData could be external, thus should it be async interface?
        bool SchedulePlaylist(int inputDataNum, TrialMonitor display, PlayList playList);
    }
}
