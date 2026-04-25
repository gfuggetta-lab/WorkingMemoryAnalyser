using System.Runtime.InteropServices;

namespace MonitorInfo.Windows
{
    public sealed class WindowsMonitorEnumeratorProvider : IMonitorEnumeratorProvider
    {
        public bool IsSupported()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        public IMonitorEnumerator CreateEnumerator()
        {
            return new WindowsMonitorEnumerator();
        }
    }
}