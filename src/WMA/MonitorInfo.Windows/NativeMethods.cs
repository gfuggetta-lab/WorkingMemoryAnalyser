using System;
using System.Runtime.InteropServices;

namespace MonitorInfo.Windows
{
    internal static class NativeMethods
    {
        public const int MONITORINFOF_PRIMARY = 0x00000001;

        public const int DISPLAY_DEVICE_ACTIVE = 0x00000001;
        public const int DISPLAY_DEVICE_MIRRORING_DRIVER = 0x00000008;

        public const int MDT_EFFECTIVE_DPI = 0;
        public const int MDT_ANGULAR_DPI = 1;
        public const int MDT_RAW_DPI = 2;

        public const int HORZSIZE = 4;
        public const int VERTSIZE = 6;
        public const int LOGPIXELSX = 88;
        public const int LOGPIXELSY = 90;

        public delegate bool MonitorEnumProc(
            IntPtr hMonitor,
            IntPtr hdcMonitor,
            ref RECT lprcMonitor,
            IntPtr dwData);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(
            IntPtr hdc,
            IntPtr lprcClip,
            MonitorEnumProc lpfnEnum,
            IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetMonitorInfo(
            IntPtr hMonitor,
            ref MONITORINFOEX lpmi);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool EnumDisplayDevices(
            string lpDevice,
            uint iDevNum,
            ref DISPLAY_DEVICE lpDisplayDevice,
            uint dwFlags);

        /*
        [DllImport("shcore.dll")]
        public static extern int GetDpiForMonitor(
            IntPtr hmonitor,
            int dpiType,
            out uint dpiX,
            out uint dpiY);
        */

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CreateDC(
            string lpszDriver,
            string lpszDevice,
            string lpszOutput,
            IntPtr lpInitData);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, int index);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DISPLAY_DEVICE
        {
            public int cb;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;

            public int StateFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }
    }
}
