using System;
using System.Collections.Generic;
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
            return Task.FromResult(true);
        }
    }
}
