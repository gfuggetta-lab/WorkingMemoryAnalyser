using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace MonitorInfo.macOS
{
    public static class macOSApi
    {
        public const uint kCFStringEncodingUTF8 = 0x08000100;

        private const string CoreGraphics =
            "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";

        private const string IOKit =
            "/System/Library/Frameworks/IOKit.framework/IOKit";

        private const string CoreFoundation =
            "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

        [DllImport(CoreGraphics)]
        public static extern int CGGetActiveDisplayList(
            uint maxDisplays,
            [Out] uint[] activeDisplays,
            out uint displayCount);

        [DllImport(CoreGraphics)]
        public static extern uint CGMainDisplayID();

        [DllImport(CoreGraphics)]
        public static extern CGRect CGDisplayBounds(uint display);

        [DllImport(CoreGraphics)]
        public static extern UIntPtr CGDisplayPixelsWide(uint display);

        [DllImport(CoreGraphics)]
        public static extern UIntPtr CGDisplayPixelsHigh(uint display);

        [DllImport(CoreGraphics)]
        public static extern CGSize CGDisplayScreenSize(uint display);

        [DllImport(CoreGraphics)]
        public static extern uint CGDisplayVendorNumber(uint display);

        [DllImport(CoreGraphics)]
        public static extern uint CGDisplayModelNumber(uint display);

        [DllImport(CoreGraphics)]
        public static extern uint CGDisplaySerialNumber(uint display);

        [DllImport(CoreGraphics)]
        public static extern uint CGDisplayIOServicePort(uint display);

        [DllImport(IOKit)]
        public static extern int IOObjectRelease(uint obj);

        [DllImport(IOKit)]
        public static extern IntPtr IORegistryEntryCreateCFProperty(
            uint entry,
            IntPtr key,
            IntPtr allocator,
            uint options);

        [DllImport(CoreFoundation)]
        public static extern IntPtr CFStringCreateWithCString(
            IntPtr allocator,
            string cStr,
            uint encoding);

        [DllImport(CoreFoundation)]
        public static extern bool CFStringGetCString(
            IntPtr theString,
            byte[] buffer,
            IntPtr bufferSize,
            uint encoding);

        [DllImport(CoreFoundation)]
        public static extern void CFRelease(IntPtr cf);

        [DllImport(CoreFoundation)]
        public static extern ulong CFGetTypeID(IntPtr cf);

        [DllImport(CoreFoundation)]
        public static extern ulong CFStringGetTypeID();

        [DllImport(CoreFoundation)]
        public static extern ulong CFDictionaryGetTypeID();

        [DllImport(CoreFoundation)]
        public static extern IntPtr CFDictionaryGetCount(IntPtr dictionary);

        [DllImport(CoreFoundation)]
        public static extern void CFDictionaryGetKeysAndValues(
            IntPtr dictionary,
            [Out] IntPtr[] keys,
            [Out] IntPtr[] values);

        [StructLayout(LayoutKind.Sequential)]
        public struct CGPoint
        {
            public double X;
            public double Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CGSize
        {
            public double Width;
            public double Height;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CGRect
        {
            public CGPoint Origin;
            public CGSize Size;
        }

    }
}
