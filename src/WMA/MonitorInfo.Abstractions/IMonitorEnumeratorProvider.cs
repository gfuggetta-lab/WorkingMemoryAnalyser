using System;
using System.Collections.Generic;
using System.Text;

namespace MonitorInfo
{
    public interface IMonitorEnumeratorProvider
    {
        bool IsSupported();

        IMonitorEnumerator CreateEnumerator();
    }
}
