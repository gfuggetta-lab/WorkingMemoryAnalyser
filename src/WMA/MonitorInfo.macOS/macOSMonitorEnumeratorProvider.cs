using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MonitorInfo.macOS
{
    public class macOSMonitorEnumeratorProvider : IMonitorEnumeratorProvider
    {
        public bool IsSupported()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        }

        public IMonitorEnumerator CreateEnumerator()
        {
            return new macOSMonitorEnumerator();
        }
    }
}
