using System;
using System.Runtime.InteropServices;

namespace SndVolPlus
{
    class Unmanaged
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        internal static extern bool SetWindowPos(int hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("shell32.dll")]
        internal static extern IntPtr SHAppBarMessage(uint dwMessage, [In] ref APPBARDATA pData);

        [StructLayout(LayoutKind.Sequential)]
        internal struct APPBARDATA
        {
            public int cbSize; // initialize this field using: Marshal.SizeOf(typeof(APPBARDATA));
            public IntPtr hWnd;
            public uint uCallbackMessage;
            public uint uEdge;
            public RECT rc;
            public int lParam;
            public static APPBARDATA NewAPPBARDATA()
            {
                APPBARDATA abd = new APPBARDATA();
                abd.cbSize = Marshal.SizeOf(typeof(APPBARDATA));
                return abd;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        internal const int ABM_GETTASKBARPOS = 0x00000005;
    }
}
