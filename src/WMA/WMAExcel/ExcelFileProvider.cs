using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WMAData;

namespace WMAExcel
{
    public class ExcelFileProvider : IExperimentReader, IExperimentDataSetLog
    {
        ILogger log;
        public void SetLog(ILogger log)
        {
            this.log = log;
        }
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
            result.SetLog(log);
            if (!result.LoadFromFile(filename))
                return Task.FromResult<IExperimentData>(null);

            return Task.FromResult<IExperimentData>(result);
        }
    }
}
