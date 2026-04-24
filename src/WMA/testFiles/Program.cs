using System;
using WMAFiles;

namespace testFiles
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("please provide the input file name");
                return;
            }
            ConfigFile cfg = ConfigFile.FromFile(args[0]);
            string v = cfg.StringLine("Show_S4_placeholder_when_centre:");
            Console.WriteLine($"{v}");
            int vi = cfg.Integer("Show_S4_placeholder_when_centre:", -1);
            Console.WriteLine($"{vi}");
            var vf = cfg.Float("S4_photodiode_patch_horz_distance_deg:", -1);
            Console.WriteLine($"{vf}");
            
        }
    }
}
