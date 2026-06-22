using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using static MonitorInfo.macOS.macOSApi;

namespace MonitorInfo.macOS
{
    public class macOSMonitorEnumerator : IMonitorEnumerator
    {
        private const int MaxDisplays = 32;

        public IReadOnlyList<ConnectedMonitor> GetConnectedMonitors()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                throw new PlatformNotSupportedException("MacMonitorEnumerator can be used only on macOS.");

            uint[] displayIds = new uint[MaxDisplays];

            int error = CGGetActiveDisplayList(
                (uint)displayIds.Length,
                displayIds,
                out uint displayCount);

            if (error != 0)
                throw new InvalidOperationException("CGGetActiveDisplayList failed. CGError=" + error);

            uint mainDisplayId = CGMainDisplayID();

            List<ConnectedMonitor> result = new List<ConnectedMonitor>();

            for (int i = 0; i < displayCount; i++)
            {
                uint displayId = displayIds[i];

                CGRect cgBounds = CGDisplayBounds(displayId);
                CGSize screenSizeMm = CGDisplayScreenSize(displayId);

                int pixelWidth = ToInt(CGDisplayPixelsWide(displayId));
                int pixelHeight = ToInt(CGDisplayPixelsHigh(displayId));

                Rectangle bounds = ToRectangle(cgBounds);

                double physWidthMm = screenSizeMm.Width;
                double physHeightMm = screenSizeMm.Height;

                double dpi = 0.0;

                if (physWidthMm > 0.0 && physHeightMm > 0.0)
                {
                    double dpiX = pixelWidth / (physWidthMm / 25.4);
                    double dpiY = pixelHeight / (physHeightMm / 25.4);

                    // One scalar DPI because ConnectedMonitor has only one Dpi field.
                    dpi = (dpiX + dpiY) / 2.0;
                }

                string nativeDeviceId = BuildNativeDeviceId(displayId);
                string name = TryGetDisplayProductName(displayId);

                if (string.IsNullOrEmpty(name))
                    name = nativeDeviceId;

                result.Add(new ConnectedMonitor
                {
                    Id = displayId.ToString(),
                    Name = name,
                    IsPrimary = displayId == mainDisplayId,
                    Bounds = bounds,

                    // Without AppKit/NSScreen we do not know Dock/menu-bar adjusted area here.
                    WorkArea = bounds,

                    Dpi = dpi,
                    PhysWidthMM = physWidthMm,
                    PhysHeightMM = physHeightMm,

                    NativeDeviceId = nativeDeviceId
                });
            }

            return result;
        }

        private static string BuildNativeDeviceId(uint displayId)
        {
            uint vendor = CGDisplayVendorNumber(displayId);
            uint model = CGDisplayModelNumber(displayId);
            uint serial = CGDisplaySerialNumber(displayId);

            return "CGDisplayID=" + displayId +
                   ";Vendor=" + vendor +
                   ";Model=" + model +
                   ";Serial=" + serial;
        }

        private static Rectangle ToRectangle(CGRect rect)
        {
            int x = (int)Math.Round(rect.Origin.X);
            int y = (int)Math.Round(rect.Origin.Y);
            int width = (int)Math.Round(rect.Size.Width);
            int height = (int)Math.Round(rect.Size.Height);

            return new Rectangle(x, y, width, height);
        }

        private static int ToInt(UIntPtr value)
        {
            ulong raw = value.ToUInt64();

            if (raw > int.MaxValue)
                return int.MaxValue;

            return (int)raw;
        }

        private static string TryGetDisplayProductName(uint displayId)
        {
            uint service = 0;
            IntPtr key = IntPtr.Zero;
            IntPtr property = IntPtr.Zero;

            try
            {
                service = IOServicePortFromCGDisplayID(displayId);
                if (service == 0)
                    return null;

                key = CFStringCreateWithCString(
                    IntPtr.Zero,
                    "DisplayProductName",
                    kCFStringEncodingUTF8);

                if (key == IntPtr.Zero)
                    return null;

                property = IORegistryEntryCreateCFProperty(
                    service,
                    key,
                    IntPtr.Zero,
                    0);

                if (property == IntPtr.Zero)
                    return null;

                ulong propertyType = CFGetTypeID(property);

                if (propertyType == CFStringGetTypeID())
                    return CopyCFString(property);

                if (propertyType == CFDictionaryGetTypeID())
                    return ReadFirstStringFromDictionary(property);

                return null;
            }
            finally
            {
                if (property != IntPtr.Zero)
                    CFRelease(property);

                if (key != IntPtr.Zero)
                    CFRelease(key);

                if (service != 0)
                    IOObjectRelease(service);
            }
        }

        private static string ReadFirstStringFromDictionary(IntPtr dictionary)
        {
            IntPtr countPtr = CFDictionaryGetCount(dictionary);
            long count = countPtr.ToInt64();

            if (count <= 0)
                return null;

            IntPtr[] keys = new IntPtr[count];
            IntPtr[] values = new IntPtr[count];

            CFDictionaryGetKeysAndValues(dictionary, keys, values);

            ulong stringTypeId = CFStringGetTypeID();

            for (int i = 0; i < values.Length; i++)
            {
                IntPtr value = values[i];

                if (value == IntPtr.Zero)
                    continue;

                if (CFGetTypeID(value) == stringTypeId)
                    return CopyCFString(value);
            }

            return null;
        }

        private static string CopyCFString(IntPtr cfString)
        {
            if (cfString == IntPtr.Zero)
                return null;

            byte[] buffer = new byte[1024];

            bool ok = CFStringGetCString(
                cfString,
                buffer,
                new IntPtr(buffer.Length),
                kCFStringEncodingUTF8);

            if (!ok)
                return null;

            int length = 0;

            while (length < buffer.Length && buffer[length] != 0)
                length++;

            return System.Text.Encoding.UTF8.GetString(buffer, 0, length);
        }

    }
}
