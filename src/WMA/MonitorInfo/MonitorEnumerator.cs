using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MonitorInfo
{
    public static class MonitorEnumerator
    {
        private static readonly object SyncRoot = new object();

        private static readonly List<IMonitorEnumeratorProvider> RegisteredProviders =
            new List<IMonitorEnumeratorProvider>();

        public static void RegisterProvider(IMonitorEnumeratorProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            lock (SyncRoot)
            {
                RegisteredProviders.Add(provider);
            }
        }

        public static IReadOnlyList<ConnectedMonitor> GetConnectedMonitors()
        {
            IMonitorEnumerator enumerator = CreateEnumerator();

            if (enumerator == null)
                throw new PlatformNotSupportedException(
                    "No supported monitor enumerator backend was found for the current platform.");

            return enumerator.GetConnectedMonitors();
        }

        public static IMonitorEnumerator CreateEnumerator()
        {
            IMonitorEnumerator fromRegisteredProviders = TryCreateFromRegisteredProviders();
            if (fromRegisteredProviders != null)
                return fromRegisteredProviders;

            IMonitorEnumerator fromKnownAssemblies = TryCreateFromKnownBackendAssemblies();
            if (fromKnownAssemblies != null)
                return fromKnownAssemblies;

            return null;
        }

        private static IMonitorEnumerator TryCreateFromRegisteredProviders()
        {
            lock (SyncRoot)
            {
                for (int i = 0; i < RegisteredProviders.Count; i++)
                {
                    IMonitorEnumeratorProvider provider = RegisteredProviders[i];

                    if (provider == null)
                        continue;

                    if (!provider.IsSupported())
                        continue;

                    IMonitorEnumerator enumerator = provider.CreateEnumerator();
                    if (enumerator != null)
                        return enumerator;
                }
            }

            return null;
        }

        private static IMonitorEnumerator TryCreateFromKnownBackendAssemblies()
        {
            string assemblyName;
            string providerTypeName;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                assemblyName = "MonitorInfo.Windows";
                providerTypeName = "MonitorInfo.Windows.WindowsMonitorEnumeratorProvider";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                assemblyName = "MonitorInfo.Mac";
                providerTypeName = "MonitorInfo.Mac.MacMonitorEnumeratorProvider";
            }
            else
            {
                return null;
            }

            return TryCreateFromProviderType(assemblyName, providerTypeName);
        }

        private static IMonitorEnumerator TryCreateFromProviderType(
            string assemblyName,
            string providerTypeName)
        {
            try
            {
                Assembly assembly = Assembly.Load(new AssemblyName(assemblyName));
                if (assembly == null)
                    return null;

                Type providerType = assembly.GetType(providerTypeName, false);
                if (providerType == null)
                    return null;

                object instance = Activator.CreateInstance(providerType);
                IMonitorEnumeratorProvider provider = instance as IMonitorEnumeratorProvider;

                if (provider == null)
                    return null;

                if (!provider.IsSupported())
                    return null;

                return provider.CreateEnumerator();
            }
            catch
            {
                return null;
            }
        }
    }
}