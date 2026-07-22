using System;
using System.Collections.Generic;
using System.IO;
using WMAData;
using WMAFiles;
using WMAExcel;
using System.Threading;
using System.Threading.Tasks;

namespace testFiles
{
    class Program
    {

        static IExperimentReader[] provs = new IExperimentReader[]
        {
            new TextExperimentReader(),
            new ExcelFileProvider()
        };

        static async Task<IExperimentReader> GetProv(string fn)
        {
            foreach(var p in provs)
            {
                if (await p.IsExperimentFile(fn, CancellationToken.None))
                    return p;
            }
            return null;
        }


        static async Task Main(string[] args)
        {
            string fn = "GODOT_Input_Data_1.xlsx";
            if (args.Length  > 0)
            {
                fn = args[0]; 
            }
            var prov = await GetProv(fn);
            if (prov == null)
            {
                Console.WriteLine($"The file {fn} is not supported.");
                return;
            }
            Console.WriteLine($"config file: {fn}");
            var exam = await prov.ReadExperiment(fn, CancellationToken.None);
            if (exam == null)
            {
                Console.WriteLine("failed to read the experiment");
                return;
            }
            //ConfigFile cfg = ConfigFile.FromFile("Configuration.txt");

            Console.WriteLine("---"); 
            Console.WriteLine(exam.GetOverview());
            Console.WriteLine("---");

            var inpNums = exam.GetInputDataListSync();
            Console.WriteLine($"total input data: {inpNums.Length}");
            if (inpNums.Length == 0)
                return;
            int n = inpNums[0];
            Console.WriteLine($"Reading: {n}");

            PlayList playList = new PlayList();
            exam.SelectInputdata(n);
            exam.SchedulePlaylist(TrialMonitor.DefaultMonitor(), playList, out var cnt);
            Console.WriteLine($"count: {cnt}");

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
                    foreach (var itm in fd)
                    {
                        switch (itm.itemType)
                        {
                            case PlayItemType.SectionStart:
                            case PlayItemType.TrialStart:
                                Console.WriteLine($"{itm.itemType}:{itm.text}");
                                break;
                        }
                    }
                    foreach (var itm in nw)
                    {
                        switch (itm.itemType)
                        {
                            case PlayItemType.SectionStart:
                            case PlayItemType.TrialStart:
                                Console.WriteLine($"{itm.itemType}:{itm.text}");
                                break;
                        }
                    }
                    Console.WriteLine($"{trck.lastMs}: triggered: {c}; eff: {pl.Count}; on: {nw.Count}; off: {fd.Count}");
                }
                else
                {
                    if (pl.Count == 0)
                    {
                        Console.WriteLine("no more items in effect");
                        break;
                    }
                }
            }
        }
    }
}
