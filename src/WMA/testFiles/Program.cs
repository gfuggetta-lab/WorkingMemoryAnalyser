using System;
using System.Collections.Generic;
using System.IO;
using WMAData;
using WMAFiles;

namespace testFiles
{
    class Program
    {
        static void Main(string[] args)
        {
            //if (args.Length == 0)
            //{
            //    Console.WriteLine("please provide the input file name");
            //    return;
            //}
            ConfigFile cfg = ConfigFile.FromFile("Configuration.txt");
            Configuration exam = new Configuration();
            exam.LoadConfig(cfg);
            var trials = InputDataHelper.LoadTrials("InputData_1.txt");


            PlayList playList = new PlayList();
            exam.Schedule(trials, playList);

            var tick = 1000.0 / 60.0;
            PlayListTracker trck = new PlayListTracker(playList);
            trck.Track(0, null, null, null, null);

            List<PlayItem> pl = new List<PlayItem>();
            List<PlayItem> nw = new List<PlayItem>();
            List<PlayItem> fd = new List<PlayItem>();
            while (true)
            {
                pl.Clear();
                nw.Clear();
                fd.Clear();
                int c = trck.Track(tick, pl, nw, fd, null);
                if (c > 0)
                {
                    Console.WriteLine($"{trck.lastMs}: triggered: {c}; eff: {pl.Count}; on: {nw.Count}; off: {fd.Count}");
                }
            }
        }
    }
}
