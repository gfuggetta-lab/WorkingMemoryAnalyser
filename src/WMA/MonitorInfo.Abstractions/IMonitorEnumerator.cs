using System;
using System.Collections.Generic;

namespace MonitorInfo
{
    public interface IMonitorEnumerator
    {
        IReadOnlyList<ConnectedMonitor> GetConnectedMonitors();
    }
}
