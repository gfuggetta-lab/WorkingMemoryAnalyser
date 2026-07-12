using System;
using System.Collections.Generic;
using System.IO;
using WMAData;
using WMAFiles;
using System.Threading;
using System.Threading.Tasks;

namespace testFiles
{
    class Program
    {

        static IDataProvider[] provs = new IDataProvider[]
        {
            new TextFilesProvider()
        };

        static async Task<IDataProvider> GetProv(string fn)
        {
            foreach(var p in provs)
            {
                if (await p.IsConfigureFile(fn))
                    return p;
            }
            return null;
        }


        static async Task Main(string[] args)
        {
            string fn = "Configuration.txt";
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
            var rdr = prov.GetReader(fn);
            //if (args.Length == 0)
            //{
            //    Console.WriteLine("please provide the input file name");
            //    return;
            //}
            //ConfigFile cfg = ConfigFile.FromFile("Configuration.txt");
            Configuration exam = new Configuration();
            await rdr.ReadConfig(exam, CancellationToken.None);


            Console.WriteLine("---"); 
            Console.WriteLine(exam.Overview);
            Console.WriteLine("---");

            var inpNums = rdr.GetInputDataListSync();
            Console.WriteLine($"total input data: {inpNums.Length}");
            return;



            //exam.LoadConfig(cfg);
            List<TrialOrder> trials = new List<TrialOrder>();
            List<PauseData> pauses = new List<PauseData>();
            InputDataHelper.LoadTrials("InputData_1.txt", trials, pauses);


            PlayList playList = new PlayList();
            exam.Schedule(TrialMonitor.DefaultMonitor(), trials, pauses, playList);

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
