using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WMAData;

namespace WMAFiles
{
    public class TextExperimentReader : IExperimentReader
    {
        public Task<bool> IsExperimentFile(string filename, CancellationToken cancel)
        {
            var fn = Path.GetFileName(filename);
            return Task.FromResult(string.Compare(fn, "configuration.txt", true) == 0);
        }

        public async Task<IExperimentData> ReadExperiment(string filename, CancellationToken cancel)
        {
            string dir = Path.GetDirectoryName(filename);
            if (string.IsNullOrWhiteSpace(dir))
                dir = Environment.CurrentDirectory;

            TextExperimentData te = new TextExperimentData();
            te.cfg = new Configuration();
            ConfigFile src = ConfigFile.FromFile(filename);
            ConfigFileHelper.LoadConfig(te.cfg, src);

            string ofn = Path.Combine(dir, "Overview.txt");
            if (File.Exists(ofn))
                te.Overview = File.ReadAllText(ofn);
            else
                te.Overview = "";


            te.inputFiles = new Dictionary<int, string>();
            const string pfx = "inputdata_";
            
            var files = Directory.EnumerateFiles(dir, "InputData_*.txt", SearchOption.TopDirectoryOnly);
            List<int> nums = new List<int>();
            foreach (var fn in files)
            {
                string n = Path.GetFileNameWithoutExtension(fn);
                n = n.Substring(pfx.Length);
                if (int.TryParse(n, out var num))
                {
                    nums.Add(num);
                    te.inputFiles[num] = fn;
                }
            }
            nums.Sort();

            te.inputNums = nums.ToArray();
            return await Task.FromResult(te);
        }
    }
}
