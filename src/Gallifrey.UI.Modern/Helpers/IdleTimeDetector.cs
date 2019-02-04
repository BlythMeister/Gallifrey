using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Gallifrey.UI.Modern.Helpers
{
    public static class IdleTimeDetector
    {
        [DllImport("user32.dll")]
        private static bool GetLastInputInfo(ref LASTINPUTINFO plii);

        public static IdleTimeInfo GetIdleTimeInfo()
        {
            var systemUptime = Environment.TickCount;
            var idleTicks = 0;

            var lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            if (GetLastInputInfo(ref lastInputInfo))
            {
                var lastInputTicks = (int)lastInputInfo.dwTime;

                idleTicks = systemUptime - lastInputTicks;
            }

            return new IdleTimeInfo
            {
                LastInputTime = DateTime.Now.AddMilliseconds(-1 * idleTicks),
                IdleTime = new TimeSpan(0, 0, 0, 0, idleTicks),
                SystemUptimeMilliseconds = systemUptime,
            };
        }
    }

    public class IdleTimeInfo
    {
        public DateTime LastInputTime { get; internal set; }

        public TimeSpan IdleTime { get; internal set; }

        public int SystemUptimeMilliseconds { get; internal set; }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }
}
