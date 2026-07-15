using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using NPOI;
using NPOI.OOXML;
using NPOI.SS.UserModel;
using WMAData;
using static WMAExcel.Utils;

namespace WMAExcel
{
    public class ExcelExperiment : IExperimentData
    {
        public ILogger log;
        public ExcelConfig cfg = null;
        public List<ExcelInputData> inputData = new List<ExcelInputData>();
        public int[] inputNums;
        int inputDataNum;
        public bool LoadFromFile(string fn)
        {
            IWorkbook workbook = WorkbookFactory.Create(fn);
            var cnt = workbook.NumberOfSheets;
            log.debug($"{workbook.GetType().Name}");

            cfg = null;
            for (int i = 0; i<cnt; i++)
            {
                var sh = workbook.GetSheetAt(i);
                
                log.debug($"{sh.SheetName} -> {IsConfigSheet(sh.SheetName)}");
                

                if ((cfg == null) &&(IsConfigSheet(sh.SheetName)))
                {
                    //log.debug("parsing config");
                    cfg = new ExcelConfig();
                    ParseConfig(sh, cfg);
                } 
                else if (IsInputDataSheet(sh.SheetName, out var inpIdx))
                {
                    //log.debug("parsing input data");
                    var data = new ExcelInputData();
                    data.Index = inpIdx;
                    ParseInputData(sh, data);
                    inputData.Add(data);
                }
            }

            List<int> indicies = new List<int>();
            foreach (var data in inputData)
                indicies.Add(data.Index);
            inputNums = indicies.ToArray();

            return ((cfg != null) || (inputData.Count > 0));
        }

        public string GetOverview()
        {
            return cfg.Overview;
        }

        public int[] GetInputDataListSync()
        {
            return inputNums;
        }

        public bool SelectInputdata(int inputNum)
        {
            inputDataNum = inputNum;
            return true;
        }
        public bool SchedulePlaylist(TrialMonitor display, PlayList playList)
        {
            if (inputDataNum <= 0) return false;
            return false;
        }
    }
}
