using Org.BouncyCastle.Crypto.Modes.Gcm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WMAData;
using static WMAExcel.ExcelToWMA;

namespace WMAExcel
{
    public class ExcelConfigReader
    {
        public string fileName;

        ExcelExperiment xlsmain;
        int[] inputNums = null;

        public ExcelConfigReader(string excelFile)
        {
            fileName = excelFile;
        }

        private void AssureXlsMain()
        {
            if (xlsmain != null) return;
            xlsmain = new ExcelExperiment();
            xlsmain.LoadFromFile(fileName);
        }


        public Task<bool> ReadConfig(Configuration dstCfg, CancellationToken cancel)
        {
            AssureXlsMain();
            if (xlsmain.cfg == null)
                return Task.FromResult(false);

            ExcelToCfg(xlsmain.cfg, dstCfg);

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

        public Task<bool> ReadTrials(int inputDataNum,
            List<TrialOrder> dstTrials,
            List<PauseData> dstPauses)
        {
            AssureXlsMain();
            ExcelInputData data = null;
            foreach (var inp in xlsmain.inputData)
            {
                if (inp.Index == inputDataNum)
                {
                    data = inp;
                    break;
                }
            }
            if (data == null)
                return Task.FromResult(false);
            

            foreach(var srcT in data.Trials)
            {
                TrialOrder to = new TrialOrder();
                dstTrials.Add(to);

                ExcelToTrial(data, srcT, xlsmain.cfg, to);
            }

            return Task.FromResult(true);
        }
    }
}
