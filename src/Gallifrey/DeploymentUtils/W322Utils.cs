using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace Gallifrey.DeploymentUtils
{
    public class Win32Utils
    {
        [DllImport("user32.Dll")]
        private static extern int EnumWindows(EnumWindowsCallbackDelegate callback, IntPtr lParam);

        [DllImport("User32.Dll")]
        private static extern void GetWindowText(int h, StringBuilder s, int nMaxCount);

        [DllImport("User32.Dll")]
        private static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsCallbackDelegate lpEnumFunc, IntPtr lParam);

        [DllImport("User32.Dll")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private delegate bool EnumWindowsCallbackDelegate(IntPtr hwnd, IntPtr lParam);

        // ReSharper disable once InconsistentNaming
        private const int BM_CLICK = 0x00F5;

        public IntPtr SearchForTopLevelWindow(string windowTitle)
        {
            var windowHandles = new ArrayList();
            /* Create a GCHandle for the ArrayList */
            var gch = GCHandle.Alloc(windowHandles);
            try
            {
                EnumWindows(EnumProc, (IntPtr)gch);
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
                var sb = new StringBuilder(1024);
                GetWindowText((int)handle, sb, sb.Capacity);
                if (sb.Length > 0)
                {
                    if (sb.ToString().StartsWith(windowTitle))
                    {
                        return handle;
                    }
                }
            }

            return IntPtr.Zero;
        }

        public IntPtr SearchForChildWindow(IntPtr parentHandle, string caption)
        {
            var windowHandles = new ArrayList();
            /* Create a GCHandle for the ArrayList */
            var gch = GCHandle.Alloc(windowHandles);
            try
            {
                EnumChildWindows(parentHandle, EnumProc, (IntPtr)gch);
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
                var sb = new StringBuilder(1024);
                GetWindowText((int)handle, sb, sb.Capacity);
                if (sb.Length > 0)
                {
                    if (sb.ToString().StartsWith(caption))
                    {
                        return handle;
                    }
                }
            }

            return IntPtr.Zero;
        }

        private static bool EnumProc(IntPtr hWnd, IntPtr lParam)
        {
            /* get a reference to the ArrayList */
            var gch = (GCHandle)lParam;
            var list = (ArrayList)(gch.Target);
            /* and add this window handle */
            list.Add(hWnd);
            return true;
        }

        public static void DoButtonClick(IntPtr buttonHandle)
        {
            SendMessage(buttonHandle, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
