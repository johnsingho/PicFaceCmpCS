using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JohnKit
{
    /// <summary>
    /// H.Z.XIN 2016-03-08 
    /// normal windows api
    /// </summary>
    class WinCall
    {
        public static readonly int VK_LBUTTON = 0x01;

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
    }

}
