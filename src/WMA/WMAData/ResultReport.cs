using System;
using System.Collections.Generic;
using System.Text;

namespace WMAData
{
    public class ResultReport : IResultReport
    {
        public StringBuilder text = new StringBuilder();

        public void WriteConfig(Configuration cfg)
        {

        }
    }
}
