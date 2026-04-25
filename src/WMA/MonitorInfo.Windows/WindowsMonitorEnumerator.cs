using System;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace MonitorInfo.Windows
{
    public sealed class WindowsMonitorEnumerator : IMonitorEnumerator
    {
        public IReadOnlyList<ConnectedMonitor> GetConnectedMonitors()
        {
            var result = new List<ConnectedMonitor>();

            var enumD = NativeMethods.EnumDisplayMonitors(
                IntPtr.Zero,
                IntPtr.Zero,
                delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref NativeMethods.RECT lprcMonitor, IntPtr dwData)
                {
                    ConnectedMonitor monitor = TryCreateMonitorInfo(hMonitor);
                    if (monitor != null)
                        result.Add(monitor);
                    return true;
                },
                IntPtr.Zero);

            return result;
        }

        private static ConnectedMonitor TryCreateMonitorInfo(IntPtr hMonitor)
        {
            NativeMethods.MONITORINFOEX info = new NativeMethods.MONITORINFOEX();
            info.cbSize = Marshal.SizeOf(typeof(NativeMethods.MONITORINFOEX));

            if (!NativeMethods.GetMonitorInfo(hMonitor, ref info))
                return null;

            string displayDeviceName = info.szDevice;

            DisplayDeviceInfo monitorDevice = TryGetMonitorDisplayDevice(displayDeviceName);

            //MonitorPhysicalSize physicalSize = null;

            bool haveSize = false;
            double wmm = 0.0;
            double hmm = 0.0;

            if (monitorDevice != null)
                haveSize = TryGetPhysicalSizeFromEdid(monitorDevice.DeviceID, out wmm, out hmm);

            if (!haveSize)
                TryGetPhysicalSizeFromDeviceCaps(displayDeviceName, out wmm, out hmm);

            //MonitorDpi effectiveDpi = TryGetDpiForMonitor(hMonitor, NativeMethods.MDT_EFFECTIVE_DPI);
            //MonitorDpi rawDpi = TryGetDpiForMonitor(hMonitor, NativeMethods.MDT_RAW_DPI);
            //if (!effectiveDpi.IsValid)
            double dpi = TryGetDpiFromDeviceCaps(displayDeviceName);

            ConnectedMonitor result = new ConnectedMonitor();
            result.Name = displayDeviceName;
            //result.MonitorDeviceName = monitorDevice == null ? null : monitorDevice.DeviceName;
            //result.MonitorFriendlyName = monitorDevice == null ? null : monitorDevice.DeviceString;
            //result.MonitorDeviceId = monitorDevice == null ? null : monitorDevice.DeviceID;

            result.Bounds = new Rectangle(
                info.rcMonitor.left,
                info.rcMonitor.top,
                info.rcMonitor.right,
                info.rcMonitor.bottom);

            result.WorkArea = new Rectangle(
                info.rcWork.left,
                info.rcWork.top,
                info.rcWork.right,
                info.rcWork.bottom);

            result.IsPrimary = (info.dwFlags & NativeMethods.MONITORINFOF_PRIMARY) != 0;

            result.Dpi = dpi;
            result.PhysWidthMM = wmm;
            result.PhysHeightMM = hmm;

            return result;
        }

        private static DisplayDeviceInfo TryGetMonitorDisplayDevice(string displayDeviceName)
        {
            for (uint i = 0; i < 32; i++)
            {
                NativeMethods.DISPLAY_DEVICE device = new NativeMethods.DISPLAY_DEVICE();
                device.cb = Marshal.SizeOf(typeof(NativeMethods.DISPLAY_DEVICE));

                if (!NativeMethods.EnumDisplayDevices(displayDeviceName, i, ref device, 0))
                    break;

                bool active = (device.StateFlags & NativeMethods.DISPLAY_DEVICE_ACTIVE) != 0;
                bool mirroring = (device.StateFlags & NativeMethods.DISPLAY_DEVICE_MIRRORING_DRIVER) != 0;

                if (!active || mirroring)
                    continue;

                DisplayDeviceInfo result = new DisplayDeviceInfo();
                result.DeviceName = device.DeviceName;
                result.DeviceString = device.DeviceString;
                result.DeviceID = device.DeviceID;
                result.DeviceKey = device.DeviceKey;
                result.StateFlags = device.StateFlags;

                return result;
            }

            return null;
        }

        /*
        private static double TryGetDpiForMonitor(IntPtr hMonitor, int dpiType)
        {
            try
            {
                uint dpiX;
                uint dpiY;

                int hr = NativeMethods.GetDpiForMonitor(hMonitor, dpiType, out dpiX, out dpiY);
                if (hr == 0 && dpiX > 0 && dpiY > 0)
                    return new MonitorDpi((int)dpiX, (int)dpiY);
            }
            catch (DllNotFoundException)
            {
                // Shcore.dll is not available on older Windows versions.
            }
            catch (EntryPointNotFoundException)
            {
                // GetDpiForMonitor is not available on older Windows versions.
            }

            return new MonitorDpi(0, 0);
        }
        */

        private static double TryGetDpiFromDeviceCaps(string displayDeviceName)
        {
            IntPtr hdc = NativeMethods.CreateDC(displayDeviceName, displayDeviceName, null, IntPtr.Zero);
            if (hdc == IntPtr.Zero)
                return 0.0;

            try
            {
                int dpiX = NativeMethods.GetDeviceCaps(hdc, NativeMethods.LOGPIXELSX);
                //int dpiY = NativeMethods.GetDeviceCaps(hdc, NativeMethods.LOGPIXELSY);

                if (dpiX > 0)
                    return dpiX;

                return 0.0;
            }
            finally
            {
                NativeMethods.DeleteDC(hdc);
            }
        }

        private static bool TryGetPhysicalSizeFromDeviceCaps(string displayDeviceName, out double widthMM, out double heightMM)
        {
            widthMM = 0.0;
            heightMM = 0.0;
            IntPtr hdc = NativeMethods.CreateDC(displayDeviceName, displayDeviceName, null, IntPtr.Zero);
            if (hdc == IntPtr.Zero)
                return false;

            try
            {
                widthMM = NativeMethods.GetDeviceCaps(hdc, NativeMethods.HORZSIZE);
                heightMM = NativeMethods.GetDeviceCaps(hdc, NativeMethods.VERTSIZE);
                return (widthMM > 0.0) && (heightMM > 0.0);
            }
            finally
            {
                NativeMethods.DeleteDC(hdc);
            }
        }

        private static bool TryGetPhysicalSizeFromEdid(string monitorDeviceId, out double widthMM, out double heightMM)
        {
            heightMM = 0;
            widthMM = 0;
            if (string.IsNullOrEmpty(monitorDeviceId))
                return false;

            string normalizedDeviceId = NormalizeMonitorDeviceIdForRegistry(monitorDeviceId);
            if (string.IsNullOrEmpty(normalizedDeviceId))
                return false;

            string keyPath = @"SYSTEM\CurrentControlSet\Enum\" +
                             normalizedDeviceId +
                             @"\Device Parameters";

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, false))
            {
                if (key == null)
                    return false;

                object value = key.GetValue("EDID");
                byte[] edid = value as byte[];

                if (edid == null || edid.Length < 23)
                    return false;

                return ParsePhysicalSizeFromEdid(edid, out widthMM, out heightMM);
            }
        }

        private static string NormalizeMonitorDeviceIdForRegistry(string monitorDeviceId)
        {
            string s = monitorDeviceId.Trim();

            if (s.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase))
                s = s.Substring(4);

            int hashIndex = s.IndexOf("#{", StringComparison.Ordinal);
            if (hashIndex >= 0)
                s = s.Substring(0, hashIndex);

            s = s.Replace('#', '\\');

            if (!s.StartsWith(@"MONITOR\", StringComparison.OrdinalIgnoreCase))
                return null;

            return s;
        }

        public const int EDID_WIDTH_OFS = 21;
        public const int EDID_HEIGHT_OFS = 22;

        private static bool ParsePhysicalSizeFromEdid(byte[] edid, out double widthMM, out double heightMM)
        {

            // EDID byte 21: maximum horizontal image size in centimeters.
            // EDID byte 22: maximum vertical image size in centimeters.
            int widthCm = edid[EDID_WIDTH_OFS];
            int heightCm = edid[EDID_HEIGHT_OFS];

            if (widthCm > 0 && heightCm > 0)
            {
                widthMM = widthCm * 10.0;
                heightMM = heightCm * 10.0;
                return true;
            }

            // Fallback: try detailed timing descriptors.
            // Physical size may be stored in millimeters inside descriptor blocks.
            return TryParsePhysicalSizeFromDetailedTimingDescriptor(edid, out widthMM, out heightMM);
        }

        public const int EDID_TIMING_DESCR_MIN_LENGTH = 128;

        private static bool TryParsePhysicalSizeFromDetailedTimingDescriptor(byte[] edid, out double widthMM, out double heightMM)
        {
            widthMM = 0.0;
            heightMM = 0.0;
            if (edid.Length < EDID_TIMING_DESCR_MIN_LENGTH)
                return false;

            // https://en.wikipedia.org/wiki/Extended_Display_Identification_Data

            for (int offset = 54; offset <= 108; offset += 18)
            {
                int pixelClock = edid[offset] | (edid[offset + 1] << 8);
                if (pixelClock == 0)
                    continue;

                int widthMm =
                    edid[offset + 12] |
                    ((edid[offset + 14] & 0xF0) << 4);

                int heightMm =
                    edid[offset + 13] |
                    ((edid[offset + 14] & 0x0F) << 8);

                if (widthMm > 0 && heightMm > 0)
                {
                    widthMM = widthMm;
                    heightMM = heightMm;
                    return true;
                }
            }

            return false;
        }

        private static bool IsWindows()
        {
            PlatformID platform = Environment.OSVersion.Platform;

            return platform == PlatformID.Win32NT ||
                   platform == PlatformID.Win32Windows ||
                   platform == PlatformID.Win32S ||
                   platform == PlatformID.WinCE;
        }

        private sealed class DisplayDeviceInfo
        {
            public string DeviceName;
            public string DeviceString;
            public string DeviceID;
            public string DeviceKey;
            public int StateFlags;
        }
    }
}