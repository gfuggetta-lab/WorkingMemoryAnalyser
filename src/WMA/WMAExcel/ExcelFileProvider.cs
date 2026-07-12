using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WMAData;

namespace WMAExcel
{
    public class ExcelFileProvider : IDataProvider
    {
        public Task<bool> IsConfigureFile(string filename)
        {
            string p = Path.GetExtension(filename);
            bool res = (string.Compare(p, ".xlsx", true) == 0)
                || (string.Compare(p, ".xls", true) == 0);
            return Task.FromResult(res);
        }

        // allocating the configuration reader, for the specified file
        public IConfigReader GetReader(string filename)
        {
            return new ExcelConfigReader(filename);
        }
    }
}
