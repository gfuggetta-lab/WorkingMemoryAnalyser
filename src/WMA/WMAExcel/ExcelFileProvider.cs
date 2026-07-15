using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WMAData;
using System.Threading;

namespace WMAExcel
{
    public class ExcelFileProvider : IExperimentReader
    {
        public Task<bool> IsExperimentFile(string filename, CancellationToken cancel)
        {
            string p = Path.GetExtension(filename);
            bool res = (string.Compare(p, ".xlsx", true) == 0)
                || (string.Compare(p, ".xls", true) == 0);
            return Task.FromResult(res);
        }

        // allocating the configuration reader, for the specified file
        public Task<IExperimentData> ReadExperiment(string filename, CancellationToken cancel)
        {
            ExcelExperiment result = new ExcelExperiment();
            if (!result.LoadFromFile(filename))
                return Task.FromResult<IExperimentData>(null);

            return Task.FromResult<IExperimentData>(result);
        }
    }
}
