using System;
using System.Collections.Generic;
using System.Text;

namespace WMAExcel
{
    public class ExcelInputEvent
    {
        public class Level
        {
            // "LN" derived from SX_LN_value
            public string LevelName = "";
            // deriver from LevelName
            public int LevelNum;

            public int VertCount;
            public decimal EccentriciyDeg;
            public int ObjCount;

        }

        public string Name = ""; // S1
        public string Type = ""; // S
        public int Num; // 1

        public string label = ""; 
        public string link_to_stimulus = "";
        public string link_to_response = "";
        public string allowed_keys_to_respond = "";
        public string number_of_layers = "";

        public Dictionary<int, Level> levels = new Dictionary<int, Level>();

    }
}
