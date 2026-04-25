using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace MonitorInfo
{
    public class ConnectedMonitor
    {
        //public MonitorPlatform Platform { get; internal set; }

        /// <summary>
        /// Stable platform-specific identifier where possible.
        /// Windows: display device name or monitor device path.
        /// macOS: CGDirectDisplayID.
        /// </summary>
        public string Id { get; set; }

        public string Name { get; set; }

        public bool IsPrimary { get; set; }

        /// <summary>
        /// Desktop/global coordinate space.
        /// On Windows: virtual desktop coordinates.
        /// On macOS: global display coordinate space.
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// Work area, if the platform backend can provide it.
        /// On macOS this may be equal to Bounds unless additional AppKit integration is added.
        /// </summary>
        public Rectangle WorkArea { get; set; }

        /// <summary>
        /// Logical/effective DPI, mainly relevant on Windows.
        /// </summary>
        //public MonitorDpi EffectiveDpi { get; internal set; }

        public double Dpi { get; set; }

        /// Physical/raw DPI calculated from pixels and physical dimensions where possible.
        /// </summary>
        //public MonitorDpi PhysicalDpi { get; internal set; }

        public int PixelWidth { get; set; }

        public int PixelHeight { get; set; }

        public double ScaleX { get; set; }

        public double ScaleY { get; set; }

        public double PhysWidthMM { get; set; }
        public double PhysHeightMM { get; set; }

        /// <summary>
        /// Optional platform-specific data useful for diagnostics.
        /// </summary>
        public string NativeDeviceId { get; set; }

        /*
        public override string ToString()
        {
            return Platform +
                    ", Id=" + Id +
                    ", Name=" + Name +
                    ", Primary=" + IsPrimary +
                    ", Bounds=" + Bounds +
                    ", Pixels=" + PixelWidth + "x" + PixelHeight +
                    ", EffectiveDpi=" + (EffectiveDpi == null ? "unknown" : EffectiveDpi.ToString()) +
                    ", PhysicalDpi=" + (PhysicalDpi == null ? "unknown" : PhysicalDpi.ToString()) +
                    ", PhysicalSize=" + (PhysicalSize == null ? "unknown" : PhysicalSize.ToString());
        }
        */
    }
}
