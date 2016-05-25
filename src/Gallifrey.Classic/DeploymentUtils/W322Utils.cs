using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace Gallifrey.DeploymentUtils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FLASHWINFO
    {
        public uint cbSize;
        public IntPtr hwnd;
        public uint dwFlags;
        public uint uCount;
        public uint dwTimeout;
    }

    public class Win32Utils
    {

        [DllImport("user32.Dll")]
        private static extern int EnumWindows(EnumWindowsCallbackDelegate callback, IntPtr lParam);
        [DllImport("User32.Dll")]
        private static extern void GetWindowText(int h, StringBuilder s, int nMaxCount);
        [DllImport("User32.Dll")]
        private static extern void GetClassName(int h, StringBuilder s, int nMaxCount);
        [DllImport("User32.Dll")]
        private static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsCallbackDelegate lpEnumFunc, IntPtr lParam);
        [DllImport("User32.Dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private delegate bool EnumWindowsCallbackDelegate(IntPtr hwnd, IntPtr lParam);

        private const int BM_CLICK = 0x00F5;

        public IntPtr SearchForTopLevelWindow(string WindowTitle)
        {
            ArrayList windowHandles = new ArrayList();
            /* Create a GCHandle for the ArrayList */
            GCHandle gch = GCHandle.Alloc(windowHandles);
            try
            {
                EnumWindows(new EnumWindowsCallbackDelegate(EnumProc), (IntPtr)gch);
                /* the windowHandles array list contains all of the
                    window handles that were passed to EnumProc.  */
            }
            finally
            {
                /* Free the handle */
                gch.Free();
            }

            /* Iterate through the list and get the handle thats the best match */
            foreach (IntPtr handle in windowHandles)
            {
                StringBuilder sb = new StringBuilder(1024);
                GetWindowText((int)handle, sb, sb.Capacity);
                if (sb.Length > 0)
                {
                    if (sb.ToString().StartsWith(WindowTitle))
                    {
                        return handle;
                    }
                }
            }

            return IntPtr.Zero;
        }

        public IntPtr SearchForChildWindow(IntPtr ParentHandle, string Caption)
        {
            ArrayList windowHandles = new ArrayList();
            /* Create a GCHandle for the ArrayList */
            GCHandle gch = GCHandle.Alloc(windowHandles);
            try
            {
                EnumChildWindows(ParentHandle, new EnumWindowsCallbackDelegate(EnumProc), (IntPtr)gch);
                /* the windowHandles array list contains all of the
                    window handles that were passed to EnumProc.  */
            }
            finally
            {
                /* Free the handle */
                gch.Free();
            }

            /* Iterate through the list and get the handle thats the best match */
            foreach (IntPtr handle in windowHandles)
            {
                StringBuilder sb = new StringBuilder(1024);
                GetWindowText((int)handle, sb, sb.Capacity);
                if (sb.Length > 0)
                {
                    if (sb.ToString().StartsWith(Caption))
                    {
                        return handle;
                    }
                }
            }

            return IntPtr.Zero;

        }

        static bool EnumProc(IntPtr hWnd, IntPtr lParam)
        {
            /* get a reference to the ArrayList */
            GCHandle gch = (GCHandle)lParam;
            ArrayList list = (ArrayList)(gch.Target);
            /* and add this window handle */
            list.Add(hWnd);
            return true;
        }

        public static void DoButtonClick(IntPtr ButtonHandle)
        {
            SendMessage(ButtonHandle, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
        }


        public static bool FlashWindowAPI(IntPtr handleToWindow)
        {
            FLASHWINFO flashwinfo1 = new FLASHWINFO();
            flashwinfo1.cbSize = (uint)Marshal.SizeOf(flashwinfo1);
            flashwinfo1.hwnd = handleToWindow;
            flashwinfo1.dwFlags = 15;
            flashwinfo1.uCount = uint.MaxValue;
            flashwinfo1.dwTimeout = 0;
            return (Win32Utils.FlashWindowEx(ref flashwinfo1) == 0);
        }

        [DllImport("user32.dll")]
        private static extern short FlashWindowEx(ref FLASHWINFO pwfi);


        // Fields
        public const uint FLASHW_ALL = 3;
        public const uint FLASHW_CAPTION = 1;
        public const uint FLASHW_STOP = 0;
        public const uint FLASHW_TIMER = 4;
        public const uint FLASHW_TIMERNOFG = 12;
        public const uint FLASHW_TRAY = 2;
    }
}
