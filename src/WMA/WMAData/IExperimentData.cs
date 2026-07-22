using System;
using System.Collections.Generic;
using System.Text;

namespace WMAData
{
    public interface IExperimentData
    {
        string GetOverview();

        // todo: this might need to be async
        int[] GetInputDataListSync();


        // inputDataNum must be part of the InputDataList array
        bool SelectInputdata(int inputDataNum);


        // SelectInputdata() must be called prior to calling SchedulePlayList
        // InputData could be external, thus should it be async interface?
        bool SchedulePlaylist(TrialMonitor display, PlayList playList, out int trialCount);
        string GetKeyboardCsv();
        void GetPreloadImages(List<string> names);
        void GetPreloadFonts(List<string> names);
        void GetPreloadSounds(List<string> names);
    }
}
