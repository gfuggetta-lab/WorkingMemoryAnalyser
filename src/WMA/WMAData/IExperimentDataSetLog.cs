using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace WMAData
{
    public interface IExperimentDataSetLog
    {
        void SetLog(ILogger log);
    }
}
