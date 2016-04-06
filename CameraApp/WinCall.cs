using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace JohnKit
{
    /// <summary>
    /// H.Z.XIN 2016-03-08 
    /// normal windows api
    /// </summary>
    public class WinCall
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

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int nIndex);
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        public static extern int SetLayeredWindowAttributes(IntPtr Handle, int crKey, byte bAlpha, int dwFlags);

        public static readonly int GWL_EXSTYLE = -20;
        public static readonly int WS_EX_TRANSPARENT = 0x20;
        public static readonly int WS_EX_LAYERED = 0x80000;
        public static readonly int LWA_COLORKEY = 1;
        public static readonly int LWA_ALPHA = 2;

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
        public static void TraceException(string sText,Exception exception)
        {
            Trace.WriteLine(sText +" ,Exception:"+ exception.Message);
        }
        public static void TraceMessage(string strMsg)
        {
            Trace.WriteLine(strMsg);
        }

        //load bitmap from path
        public static Bitmap LoadBitmap(string path)
        {
            Bitmap bm = null;
            using (var binReader = new System.IO.BinaryReader(System.IO.File.Open(path, System.IO.FileMode.Open)))
            {
                var fileInfo = new System.IO.FileInfo(path);
                byte[] bytes = binReader.ReadBytes((int)fileInfo.Length);
                binReader.Close();
                bm = new Bitmap(new System.IO.MemoryStream(bytes));
            }
            return bm;
        }


        #region convert to gray
        private static Bitmap CreateGrayscaleImage(int width, int height)
        {
            // create new image
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            // set palette to grayscale
            SetGrayscalePalette(bmp);
            // return new image
            return bmp;

        }//#
        private static void SetGrayscalePalette(Bitmap srcImg)
        {
            if (srcImg.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new ArgumentException();
            ColorPalette cp = srcImg.Palette;
            for (int i = 0; i < 256; i++)
            {
                cp.Entries[i] = Color.FromArgb(i, i, i);
            }
            srcImg.Palette = cp;
        }

        internal static Bitmap BitmapConvetGray(Bitmap srcBitmap)
        {
            int width = srcBitmap.Width;
            int height = srcBitmap.Height;
            Rectangle rect = new Rectangle(0, 0, width, height);

            BitmapData srcBmData = srcBitmap.LockBits(rect,
                      ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            Bitmap dstBitmap = CreateGrayscaleImage(width, height);
            BitmapData dstBmData = dstBitmap.LockBits(rect,
                      ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

            System.IntPtr srcScan = srcBmData.Scan0;
            System.IntPtr dstScan = dstBmData.Scan0;

            unsafe
            {
                byte* srcP = (byte*)srcScan.ToPointer();
                byte* dstP = (byte*)dstScan.ToPointer();
                int srcOffset = srcBmData.Stride - width * 3;
                int dstOffset = dstBmData.Stride - width;

                byte red, green, blue;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++, srcP += 3, dstP++)
                    {
                        blue = srcP[0];
                        green = srcP[1];
                        red = srcP[2];
                        *dstP = (byte)(.299 * red + .587 * green + .114 * blue);
                        //*dstP = (byte)((red*299 + green*587 + blue*114+500)/1000.0);
                    }
                    srcP += srcOffset;
                    dstP += dstOffset;
                }
            }

            srcBitmap.UnlockBits(srcBmData);
            dstBitmap.UnlockBits(dstBmData);
            return dstBitmap;
        }
        #endregion

        internal static Bitmap BitmapScale(Bitmap img, int wid, int hei)
        {
            if (img == null)
            {
                return null;
            }
            Bitmap tarBitmap = new Bitmap(wid, hei);
            using (Graphics bmpGraphics = Graphics.FromImage(tarBitmap))
            {
                // set Drawing Quality
                bmpGraphics.InterpolationMode = InterpolationMode.High;
                bmpGraphics.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle compressionRectangle = new Rectangle(0, 0, wid, hei);
                bmpGraphics.DrawImage(img, compressionRectangle);
            }
            return tarBitmap;
        }

        /// <summary>
        /// 线程返回值
        /// </summary>
        public class ThreadBoolRet
        {
            public bool bResult = false;
            public object pExtra = null;
        }
    }

}
;