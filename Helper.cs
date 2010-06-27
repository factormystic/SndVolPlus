using System;
using System.Windows.Forms;

namespace SndVolPlus
{
    static class Helper
    {
        internal static DockStyle GetTaskbarEdge()
        {
            IntPtr taskBarWnd = Unmanaged.FindWindow("Shell_TrayWnd", null);

            Unmanaged.APPBARDATA abd = new Unmanaged.APPBARDATA();
            abd.hWnd = taskBarWnd;
            Unmanaged.SHAppBarMessage(Unmanaged.ABM_GETTASKBARPOS, ref abd);

            if (abd.rc.top == abd.rc.left && abd.rc.bottom > abd.rc.right)
            {
                return DockStyle.Left;
            }
            else if (abd.rc.top == abd.rc.left && abd.rc.bottom < abd.rc.right)
            {
                return DockStyle.Top;
            }
            else if (abd.rc.top > abd.rc.left)
            {
                return DockStyle.Bottom;
            }
            else
            {
                return DockStyle.Right;
            }
        }
    }
}
