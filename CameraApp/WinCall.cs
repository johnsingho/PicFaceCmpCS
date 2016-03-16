using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
        
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr Destination, IntPtr Source, [MarshalAs(UnmanagedType.U4)] int Length);

        public static void ZeroArr(byte[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = 0x0;
            }
        }

        public static void CopyArr(byte[] dstArr, int iDstStart, byte[] srcArr, int iSrcStart, int n)
        {
            for (int i = 0; i < n; i++)
            {
                dstArr[iDstStart + i] = srcArr[iSrcStart + i];
            }
        }

        public static void TraceException(Exception exception)
        {
            Trace.WriteLine("***Exception: " + exception.Message);
        }
    }

}
