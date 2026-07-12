using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WMAData;

namespace WMAFiles
{
    public class TextFilesProvider : IDataProvider
    {
        public Task<bool> IsConfigureFile(string filename)
        {
            var fn = Path.GetFileName(filename);
            return Task.FromResult(string.Compare(fn, "configuration.txt", true) == 0);
        }

        public IConfigReader GetReader(string filename)
        {
            return new TextFileReader
            {
                configFileName = filename
            };
        }
    }
}
