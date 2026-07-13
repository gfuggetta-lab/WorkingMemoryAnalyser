using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WMAData;

namespace WMAFiles
{
    // This is the legacy .txt format to read the configuration
    public class TextFileReader : IConfigReader
    {
        public string configFileName;
        private string dir;
        private int[] inpData = null;

        public TextFileReader(string configFileName)
        {
            this.configFileName = configFileName;

            dir = Path.GetDirectoryName(configFileName);
            if (!Path.IsPathRooted(dir))
            {
                if (string.IsNullOrWhiteSpace(dir))
                    dir = Environment.CurrentDirectory;
                else
                    dir = Path.GetFullPath(dir);
            }
        }

        public Task<bool> ReadConfig(Configuration cfg, CancellationToken cancel)
        {
            ConfigFile file = ConfigFile.FromFile(configFileName);
            cfg.LoadConfig(file);

            // overfiew file must reside at the same directory as config file
            string ov = Path.Combine(dir, "Overview.txt");
            if (File.Exists(ov))
                cfg.Overview = File.ReadAllText(ov);
            else
                cfg.Overview = "";
            cfg.isLegacySchedule = true;

            return Task.FromResult(true);
        }

        public int[] GetInputDataListSync()
        {
            const string pfx = "inputdata_";
            if (inpData == null)
            {
                var files = Directory.EnumerateFiles(dir, "InputData_*.txt", SearchOption.TopDirectoryOnly);
                List<int> nums = new List<int>();
                foreach (var fn in files)
                {
                    string n = Path.GetFileNameWithoutExtension(fn);
                    n = n.Substring(pfx.Length);
                    if (int.TryParse(n, out var num))
                        nums.Add(num);
                }
                inpData = nums.ToArray();
            }
            return inpData;
        }

        public Task<bool> ReadTrials(int inputDataNum,
            List<TrialOrder> dstTrials,
            List<PauseData> dstPauses)
        {
            var result = InputDataHelper.LoadTrials($"InputData_{inputDataNum}.txt", dstTrials, dstPauses);
            return Task.FromResult(result);
        }
    }
}
