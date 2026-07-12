using System;
using System.Collections.Generic;
using System.Text;

namespace WMAExcel
{
    public class ExcelInputEvent
    {
        public class Level
        {
            // derived from SX_LN_
            public string LevelName;
            // deriver from LevelName
            public int LevelNum;

            public int VertCount;
            public decimal EccentriciyDeg;
            public int ObjCount;

        }

    }
}
