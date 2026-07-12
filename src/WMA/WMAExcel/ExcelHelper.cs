using NPOI.SS.Formula;
using NPOI.SS.UserModel;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Crypto.Prng;

namespace WMAExcel
{
    internal static class ExcelHelper
    {


        public static string ValueAsStr(this ICell cl)
        {
            if (cl == null) return "";
            switch (cl.CellType)
            {
                case CellType.Numeric:
                    var d = cl.NumericCellValue;
                    var dc = Convert.ToDecimal(d);
                    return dc.ToString(CultureInfo.InvariantCulture);
                        //Numeric = 0,
                case CellType.String:
                    return cl.StringCellValue;
                

                case CellType.Boolean:
                    var b = cl.BooleanCellValue;
                    if (b) return "1";
                    else return "0";
                default:
                    //case CellType.Blank:
                    //case CellType.Error:
                    //case CellType.Formula:
                    return "";
            }
        }
    }
}
