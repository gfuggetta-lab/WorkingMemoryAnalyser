using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace WMAExcel
{
    public static class LogHelper
    {
        public static void warn(this ILogger log, string msg)
        {
            if (log == null) return;
            log.LogWarning(msg);
        }

        public static void debug(this ILogger log, string msg)
        {
            if (log == null) return;
            log.LogDebug(msg);
        }
    }
}
