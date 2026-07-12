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
        public Task<bool> ReadConfig(Configuration cfg, CancellationToken cancel)
        {
            ConfigFile file = ConfigFile.FromFile(configFileName);
            cfg.LoadConfig(file);

            // overfiew file must reside at the same directory as config file
            string ov = Path.Combine(Path.GetDirectoryName(configFileName), "Overview.txt");
            if (File.Exists(ov))
                cfg.Overview = File.ReadAllText(ov);
            else
                cfg.Overview = "";

            return Task.FromResult(true);
        }
    }
}
