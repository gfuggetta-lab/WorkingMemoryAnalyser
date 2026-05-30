using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace WMAData
{
    internal static class LogHelper
    {
        public static void info(this ILogger logger, string msg)
        {
            if (logger == null) return;
            logger.LogInformation(msg);
        }
    }
}
