using System;
using System.IO;
using MonitorInfo;

namespace testMonitors
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var en = MonitorEnumerator.CreateEnumerator();
            if (en == null)
            {
                Console.WriteLine("no enumarators available");
                return;
            }
            foreach(var m in en.GetConnectedMonitors())
            {
                Console.WriteLine($"{m.Id} {m.Name} {m.NativeDeviceId}");
            }
        }
    }
}
