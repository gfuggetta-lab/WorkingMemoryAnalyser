using System;
using System.Collections.Generic;
using System.Text;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;

namespace WMAExcel
{
    public class SheetReader
    {
        public ISheet sheet;

        public int row;


        public string Name;
        public string Value;

        public SheetReader(ISheet sheet)
        {
            this.sheet = sheet;
            row = 0;
        }
        private IRow GetNextRow()
        {
            IRow r = null;
            r = sheet.GetRow(row);
            while ((r == null) && (row < sheet.LastRowNum))
            {
                row++;
                r = sheet.GetRow(row);
            }
            row++;
            return r;
        }


        private bool GetNameValue(IRow row, out string name, out string val)
        {
            name = "";
            val = "";
            var x = row.FirstCellNum;
            if (x < 0)
                return false;
            while (x < row.LastCellNum)
            {
                name = row.GetCell(x).ValueAsStr();
                if (!string.IsNullOrWhiteSpace(name))
                    break;
                x++;
            }
            name = name.Trim();
            if (string.IsNullOrWhiteSpace(name))
                return false;
            
            int i = name.Length -1;
            while ((i >= 0) && (name[i] == ':')) i--;
            if (i < 0)
                return false;
            name = name.Substring(0, i+1);

            if (row.LastCellNum == x)
                return true;
            x++;
            while (x <= row.LastCellNum)
            {
                var cl = row.GetCell(x);
                if (cl != null)
                {
                    val = cl.ValueAsStr();
                    val = val.Trim();
                    break;
                }
                x++;
            }
            return true;
        }

        public bool ReadNext()
        {
            if (row > sheet.LastRowNum)
                return false;
            while (true)
            {
                IRow r = GetNextRow();
                if (r == null)
                    return false;
                if (!GetNameValue(r, out Name, out Value))
                    continue;

                break;
            }
            return true;
        }

    }
}
