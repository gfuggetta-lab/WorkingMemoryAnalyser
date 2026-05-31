using System;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
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
            string edidName = "";

            if (monitorDevice != null)
            {
                if (TryGetEdid(monitorDevice.DeviceID, out byte[] edid))
                {
                    haveSize = ParsePhysicalSizeFromEdid(edid, out wmm, out hmm);
                    if (!TryParseMonitorNameFromEdid(edid, out edidName))
                    {
                        edidName = "";
                    }
                }
            }
                //haveSize = TryGetPhysicalSizeFromEdid(monitorDevice.DeviceID, out wmm, out hmm);

            if (!haveSize)
                TryGetPhysicalSizeFromDeviceCaps(displayDeviceName, out wmm, out hmm);

            //MonitorDpi effectiveDpi = TryGetDpiForMonitor(hMonitor, NativeMethods.MDT_EFFECTIVE_DPI);
            //MonitorDpi rawDpi = TryGetDpiForMonitor(hMonitor, NativeMethods.MDT_RAW_DPI);
            //if (!effectiveDpi.IsValid)
            double dpi = TryGetDpiFromDeviceCaps(displayDeviceName);

            ConnectedMonitor result = new ConnectedMonitor();
            result.Id = displayDeviceName;
            if (!string.IsNullOrEmpty(edidName))
                result.Name = edidName;
            else 
                result.Name = GetHumanReadableMonitorName(displayDeviceName, monitorDevice);
            //result.MonitorDeviceName = monitorDevice == null ? null : monitorDevice.DeviceName;
            //result.MonitorFriendlyName = monitorDevice == null ? null : monitorDevice.DeviceString;
            //result.MonitorDeviceId = monitorDevice == null ? null : monitorDevice.DeviceID;

            result.Bounds = new Rectangle(
                info.rcMonitor.left,
                info.rcMonitor.top,
                info.rcMonitor.right - info.rcMonitor.left,
                info.rcMonitor.bottom - info.rcMonitor.top);

            result.WorkArea = new Rectangle(
                info.rcWork.left,
                info.rcWork.top,
                info.rcWork.right - info.rcWork.left,
                info.rcWork.bottom - info.rcWork.top);

            result.IsPrimary = (info.dwFlags & NativeMethods.MONITORINFOF_PRIMARY) != 0;

            result.Dpi = dpi;
            result.PhysWidthMM = wmm;
            result.PhysHeightMM = hmm;
            result.NativeDeviceId = monitorDevice == null ? null : monitorDevice.DeviceID;

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

        private static string GetHumanReadableMonitorName(string displayDeviceName, DisplayDeviceInfo monitorDevice)
        {
            if (monitorDevice != null)
            {

                if (!string.IsNullOrWhiteSpace(monitorDevice.DeviceString))
                    return monitorDevice.DeviceString.Trim();
            }

            return displayDeviceName;
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

            if (!TryGetEdid(monitorDeviceId, out byte[] edid))
                return false;

            return ParsePhysicalSizeFromEdid(edid, out widthMM, out heightMM);
        }

        private static bool TryGetMonitorNameFromEdid(string monitorDeviceId, out string monitorName)
        {
            monitorName = null;

            if (!TryGetEdid(monitorDeviceId, out byte[] edid))
                return false;

            return TryParseMonitorNameFromEdid(edid, out monitorName);
        }

        private static bool TryGetEdid(string monitorDeviceId, out byte[] edid)
        {
            edid = null;

            if (string.IsNullOrEmpty(monitorDeviceId))
                return false;
            if (!NormalizeMonitorDeviceIdForRegistry(monitorDeviceId, out var dispName, out var clsid))
                return false;

            string keyPath = @"SYSTEM\CurrentControlSet\Enum\DISPLAY\" + dispName;
                             //@"\Device Parameters";

            using (RegistryKey dispKey = Registry.LocalMachine.OpenSubKey(keyPath, false))
            {
                if (dispKey == null)
                    return false;

                string[] subDisp = dispKey.GetSubKeyNames();

                if (subDisp == null || subDisp.Length == 0)
                    return false;

                foreach (var sub in subDisp)
                {
                    using (RegistryKey inKey = dispKey.OpenSubKey(sub))
                    {
                        if (inKey == null)
                            continue;

                        string cls = inKey.GetValue("ClassGUID") as string;
                        if (string.Compare(cls, clsid, true) != 0)
                            continue;

                        using (RegistryKey edidKey = inKey.OpenSubKey("Device Parameters"))
                        {
                            if (edidKey == null)
                                return false;

                            object value = edidKey.GetValue("EDID");
                            byte[] edidBytes = value as byte[];

                            if (edidBytes == null || edidBytes.Length < 23)
                                return false;

                            edid = edidBytes;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool NormalizeMonitorDeviceIdForRegistry(string monitorDeviceId, out string dispName, out string classId)
        {
            const string monitorPrefix = @"MONITOR\";
            dispName = "";
            classId = "";
            string s = monitorDeviceId.Trim();

            if (s.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase))
                s = s.Substring(4);

            int hashIndex = s.IndexOf("#{", StringComparison.Ordinal);
            if (hashIndex >= 0)
                s = s.Substring(0, hashIndex);

            s = s.Replace('#', '\\');

            if (!s.StartsWith(monitorPrefix, StringComparison.OrdinalIgnoreCase))
                return false;

            string[] parts = s.Split(new string[] { "\\" }, StringSplitOptions.None);
            if (parts.Length < 3) return false;

            // MONITOR\NAME\ClassID
            dispName = parts[1];
            classId = parts[2];

            return true;
        }

        public const int EDID_WIDTH_OFS = 21;
        public const int EDID_HEIGHT_OFS = 22;
        public const int EDID_TIMING_DESCR_MIN_LENGTH = 128;
        public const int EDID_DESCRIPTOR_START = 54;
        public const int EDID_DESCRIPTOR_END = 108;
        public const int EDID_DESCRIPTOR_SIZE = 18;
        public const byte EDID_MONITOR_NAME_TAG = 0xFC;

        public static bool ParsePhysicalSizeFromEdid(byte[] edid, out double widthMM, out double heightMM)
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

        public static bool TryParseMonitorNameFromEdid(byte[] edid, out string monitorName)
        {
            monitorName = null;

            if (edid == null || edid.Length < EDID_TIMING_DESCR_MIN_LENGTH)
                return false;

            for (int offset = EDID_DESCRIPTOR_START; offset <= EDID_DESCRIPTOR_END; offset += EDID_DESCRIPTOR_SIZE)
            {
                if (edid[offset] != 0x00 ||
                    edid[offset + 1] != 0x00 ||
                    edid[offset + 2] != 0x00 ||
                    edid[offset + 3] != EDID_MONITOR_NAME_TAG ||
                    edid[offset + 4] != 0x00)
                {
                    continue;
                }

                string name = Encoding.ASCII.GetString(edid, offset + 5, 13)
                    .Trim('\0', '\r', '\n', ' ');

                if (!string.IsNullOrWhiteSpace(name))
                {
                    monitorName = name;
                    return true;
                }
            }

            return false;
        }

        private static bool TryParsePhysicalSizeFromDetailedTimingDescriptor(byte[] edid, out double widthMM, out double heightMM)
        {
            widthMM = 0.0;
            heightMM = 0.0;
            if (edid.Length < EDID_TIMING_DESCR_MIN_LENGTH)
                return false;

            // https://en.wikipedia.org/wiki/Extended_Display_Identification_Data

            for (int offset = EDID_DESCRIPTOR_START; offset <= EDID_DESCRIPTOR_END; offset += EDID_DESCRIPTOR_SIZE)
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
