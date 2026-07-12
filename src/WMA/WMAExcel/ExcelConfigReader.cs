using Org.BouncyCastle.Crypto.Modes.Gcm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WMAData;

namespace WMAExcel
{
    public class ExcelConfigReader : IConfigReader
    {
        public string fileName;

        ExcelMain xlsmain;
        int[] inputNums = null;

        public ExcelConfigReader(string excelFile)
        {
            fileName = excelFile;
        }

        private void AssureXlsMain()
        {
            if (xlsmain != null) return;
            xlsmain = new ExcelMain();
            xlsmain.LoadFromFile(fileName);
        }


        public Task<bool> ReadConfig(Configuration cfg, CancellationToken cancel)
        {
            AssureXlsMain();
            if (xlsmain.cfg == null)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }
        public int[] GetInputDataListSync()
        {
            AssureXlsMain();
            if (inputNums == null)
            {
                List<int> indicies = new List<int>();
                foreach (var data in xlsmain.inputData)
                    indicies.Add(data.Index);
                inputNums = indicies.ToArray();
            }
            return inputNums;
        }
    }
}
