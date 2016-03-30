using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace CameraApp
{
    using HWRESULT = System.UInt32;
    using HW_HANDLE = System.IntPtr;
    using JohnKit;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Drawing.Drawing2D;
    using System.Diagnostics;
    
    /// <summary>
    /// 使用汉王人脸识别库
    /// </summary>
    public class FaceCmpEngine : IDisposable
    {
        #region HWFaceRecSDK struct
        public struct Rect
        {
            public int top;
            public int bottom;
            public int left;
            public int right;
        };
        public struct Pos
        {
            public int col;
            public int row;
            public int width;
            public int confi;
        };
        public struct KeyPos
        {
            public Pos face;
            public Pos leftEye;
            public Pos rightEye;
            public Pos leftUpperEye;
            public Pos rightUpperEye;
            public Pos leftleftEye;
            public Pos leftrightEye;
            public Pos rightleftEye;
            public Pos rightrightEye;
            public Pos leftNostril;
            public Pos rightNostril;
            public Pos nosePoint;
            public Pos leftMouth;
            public Pos rightMouth;
        };

        public struct HWFaceInfo
        {
            public Rect m_FaceRect; //人脸定位框
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 81 * 2)]
            public float[] m_afKeyPoint;
            public KeyPos m_KeyPos; //
        };

        #endregion

        #region HWFaceRecSDK API

        //初始化核心, strName 输入dll所在路径，如"D:\\Prog"
        //程序加载后运行一次，先于HWInitial运行
        //return: S_OK, S_FAIL
        [DllImport("HWcompare.dll", CallingConvention=CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern HWRESULT HWInitialD([MarshalAs(UnmanagedType.LPStr)]string strName);        

        //释放核心
        //程序退出前运行一次
        //return: S_OK, S_FAIL
        [DllImport("HWcompare.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern HWRESULT HWReleaseD();

        //初始化一个HANDLE . 多线程情况下各个线程初始化各自的Handle,
        //pHandle [output]指向初始化好的Handle
        //strName [input]NULL
        //return: S_OK, S_FAIL
        [DllImport("HWcompare.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern HWRESULT HWInitial(ref HW_HANDLE pHandle, [MarshalAs(UnmanagedType.LPStr)]string strName);

        //释放Handle
        //pHandle [input]指向HWInitial初始化好的Handle
        [DllImport("HWcompare.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern HWRESULT HWRelease(ref HW_HANDLE pHandle);

        /**************************************************
            人脸定位
        *************************************************/
        //人脸定位
        //Handle [input] HWInitial初始化好的Handle
        //pImg   [input] 输入图片灰度信息，数据内容:图片从左上到右下，逐行 每行从左到右逐点排列各像素的灰度值
        //nImgWidth nImgHeight [input] 图片的宽度高度
        //pnMaxFace [input] 需要定位最多人脸个数 （1~10)
        //          [output] *pnMaxFace 为实际定位的人脸个数
        //pFaceInfo [output] 输出每个人脸定位信息。 需要外部申请*pnMaxFace个 HWFaceInfo空间。
        //return: S_OK, S_FAIL
        [DllImport("HWcompare.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern HWRESULT HWDetectFaceKeyPoints(HW_HANDLE Handle,
                            IntPtr pImg,
                            int nImgWidth, int nImgHeight,
                            ref int pnMaxFace,
                            IntPtr pFaceInfo);

        public static HWRESULT HWDetectFaceKeyPoints(HW_HANDLE Handle,
            IntPtr pImg,
            int nImgWidth, int nImgHeight,
            ref int pnMaxFace,
            ref HWFaceInfo[] pFaceInfo)
        {
            if (pnMaxFace <= 0)
            {
                return S_FAIL;
            }
            int nSizeStruct = Marshal.SizeOf(typeof(HWFaceInfo));
            IntPtr ptr= Marshal.AllocHGlobal(nSizeStruct * pnMaxFace);
            HWRESULT hr = HWDetectFaceKeyPoints(Handle, pImg, nImgWidth, nImgHeight, ref pnMaxFace, ptr);
            if (hr == S_OK)
            {
                IntPtr ptrData = ptr;
                for (int i = 0; i < pnMaxFace; i++)
                {
                    pFaceInfo[i] = (HWFaceInfo)Marshal.PtrToStructure(ptrData, typeof(HWFaceInfo));
                    ptrData += nSizeStruct;
                }
            }
            Marshal.FreeHGlobal(ptr);
            return hr;
        }

        //设置是否大头照之类的证件照片。如果确定是证件照，则可以设置iPortrait = 1,否则设为0。
        //设为1则定位较容易。
        //Handle [input] HWInitial初始化好的Handle
        //iPortrait [input] 1 是的。0 不确定。
        //return: S_OK, S_FAIL
        [DllImport("HWcompare.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern HWRESULT HWSetPortrait(HW_HANDLE Handle, int iPortrait);

        /**************************************************
            特征和比对
        /*************************************************/
        //
        //Handle [input] HWInitial初始化好的Handle
        //piFtrSize [output] 输出特征字节个数
        //return: S_OK, S_FAIL
        [DllImport("HWcompare.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern HWRESULT HWGetFeatureSize(HW_HANDLE Handle, ref int piFtrSize);


        //提取特征。
        //Handle [input] HWInitial初始化好的Handle
        //pImg   [input] 输入图片灰度信息。数据内容:图片从左上到右下，逐行 每行从左到右逐点排列各像素的灰度值
        //nImgWidth, nImgHeight[input] 图片的宽度高度
        //pFaceInfo   [input] 一个人脸信息
        //pOutFeature [output]输出特征串。特征串长度见HWGetFeatureSize， 需要外部申请好。
        //return : S_OK. other failed
        [DllImport("HWcompare.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern HWRESULT HWExtractFeature(HW_HANDLE Handle,
                          IntPtr pImg, int nImgWidth, int nImgHeight,
                          ref HWFaceInfo pFaceInfo,
                          IntPtr pOutFeature);


        //用于单独比较两张图片的特征串相似性。
        //Handle [input] HWInitial初始化好的Handle
        //pFeaA  [input] 特征串
        //pFeaB  [input] 特征串
        //fScore [output] 相似性度量值，0~1.0 ，越大越相似。
        //return : S_OK. other failed
        [DllImport("HWcompare.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern HWRESULT HWCompareFeature(HW_HANDLE Handle,
                            IntPtr pFeaA,
                            IntPtr pFeaB,
                            ref float fScore);

        #endregion

        #region Const
        private static readonly uint S_OK = 0;
        private static readonly uint S_FAIL = 1;

        private static readonly int ID_PHOTO_WID = (102 * 2);
        private static readonly int ID_PHOTO_HEI = (126 * 2);
        private static readonly int LIVE_PHOTO_WID = 640;
        private static readonly int LIVE_PHOTO_HEI = 480;

        private static readonly int MAX_DETECT_FACES = 2;
        #endregion

        private HW_HANDLE handleLib=IntPtr.Zero;
        private PicPixel idPhotoData;
        private PicPixel livePhotoData;
        
        //当前现场照片人脸
        private HWFaceInfo curLiveFaceInfo;
        //提取到人脸样本
        private HWFaceInfo[] detectFaceInfos = new HWFaceInfo[MAX_DETECT_FACES];

        public bool InitLib()
        {
            string strCurdir = Application.StartupPath;
            HWRESULT hRes = HWInitialD(strCurdir);
            if (S_OK != hRes)
            {
                return false;
            }
            hRes = HWInitial(ref handleLib, strCurdir);
            bool bInit = (S_OK == hRes);
            if(bInit)
            {
                idPhotoData = PicPixel.CreatePicPixel(ID_PHOTO_WID, ID_PHOTO_HEI);
                livePhotoData = PicPixel.CreatePicPixel(LIVE_PHOTO_WID, LIVE_PHOTO_HEI);
            }
            return bInit;
        }

        public void Dispose()
        {
            if (handleLib != IntPtr.Zero)
            {
                HWRelease(ref handleLib);
                handleLib = IntPtr.Zero;
            }
            HWReleaseD();
            if(idPhotoData!=null)
            {
                PicPixel.DeletePicPixel(ref idPhotoData);                
            }
            if (livePhotoData != null)
            {
                PicPixel.DeletePicPixel(ref livePhotoData);
            }
        }

        //像素数据
        internal class PicPixel
        {
            internal int width;
            internal int height;
            internal IntPtr pixel;

            public static PicPixel CreatePicPixel(int nW, int nH)
            {
                PicPixel pPix = new PicPixel();
                pPix.pixel = Marshal.AllocHGlobal(nW*nH);
                pPix.width = nW;
                pPix.height = nH;
                return pPix;
            }

            public static void DeletePicPixel(ref PicPixel pPicPixel)
            {
                if (pPicPixel!=null)
                {
                    Marshal.FreeHGlobal(pPicPixel.pixel);
                    pPicPixel.pixel = IntPtr.Zero;
                    pPicPixel.width = 0;
                    pPicPixel.height = 0;
                }
                pPicPixel = null;
            }
        }

        //进行一对一对比
        public float CompareAFace(float fInitFaceCmpRate, int iPorttrail)
        {
            float fScore = fInitFaceCmpRate;
            int iFtrSize = 0;
            HWGetFeatureSize(handleLib, ref iFtrSize);

            IntPtr pbFtrID = Marshal.AllocHGlobal(iFtrSize);
            IntPtr pbFtrLiveFace = Marshal.AllocHGlobal(iFtrSize);

            //如果确定是证件照，可以设Portrait= 1， 否则设Portrait = 0
            HWSetPortrait(handleLib, iPorttrail);

            int iMaxFace = 1;
            HWFaceInfo[] idFaceInfo=new HWFaceInfo[iMaxFace];
            //找身份证上的人脸
            HWRESULT iRst = HWDetectFaceKeyPoints(handleLib, idPhotoData.pixel, idPhotoData.width, idPhotoData.height, ref iMaxFace, ref idFaceInfo);
            if (iRst != S_OK)
            {
                WinCall.TraceMessage("***没找身份证上的人脸");
                Marshal.FreeHGlobal(pbFtrID);
                Marshal.FreeHGlobal(pbFtrLiveFace);
                return 0.0F;
            }
            if (S_OK != HWExtractFeature(handleLib, idPhotoData.pixel, idPhotoData.width, idPhotoData.height, ref idFaceInfo[0], pbFtrID))
            {
                Marshal.FreeHGlobal(pbFtrID);
                Marshal.FreeHGlobal(pbFtrLiveFace);
                return 0.0F;
            }

            //找出现场照片上的人脸
            if (S_OK != HWExtractFeature(handleLib, livePhotoData.pixel, livePhotoData.width, livePhotoData.height, ref curLiveFaceInfo, pbFtrLiveFace))
            {
                Marshal.FreeHGlobal(pbFtrID);
                Marshal.FreeHGlobal(pbFtrLiveFace);
                return 0.0F;
            }

            WinCall.TraceMessage("***HWExtractFeature all ok\n");
            HWCompareFeature(handleLib, pbFtrID, pbFtrLiveFace, ref fScore);

            Marshal.FreeHGlobal(pbFtrID);
            Marshal.FreeHGlobal(pbFtrLiveFace);
            return fScore;
        }

        internal void GetLivePhoto(Bitmap bmCur)
        {
            GetGrayPixel(livePhotoData, bmCur);
        }
        internal void GetIDPhoto(Bitmap bmID)
        {
            GetGrayPixel(idPhotoData, bmID);
        }

        private void GetGrayPixel(PicPixel pPix, Bitmap bmCur)
        {
            int nW = pPix.width;
            int nH = pPix.height;
            using (Bitmap bmTemp = BitmapScale(bmCur, nW, nH))
            {
                using (Bitmap bmGrapy = BitmapConvetGray(bmTemp))
                {
                    Rectangle rect = new Rectangle(0,0,nW,nH);
                    BitmapData dataGray = bmGrapy.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
                    System.IntPtr ptrGray = dataGray.Scan0;
                    WinCall.CopyMemory(pPix.pixel, ptrGray, nW*nH);
                    bmGrapy.UnlockBits(dataGray);
                }
            }            
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

        public static Bitmap BitmapConvetGray(Bitmap srcBitmap)
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

        private static Bitmap BitmapScale(Bitmap img, int wid, int hei)
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

        internal bool DetectLivePhoto()
        {
            int iMaxFace = MAX_DETECT_FACES;

            HWRESULT iRst = S_FAIL;
            iRst = HWDetectFaceKeyPoints(handleLib, livePhotoData.pixel, livePhotoData.width, livePhotoData.height, ref iMaxFace, ref detectFaceInfos);
            bool bValid = (S_OK == iRst);
            if (bValid)
            {
                if (iMaxFace > 1)
                {
                    string str = string.Format("***CFaceDetect::DetectLiveFace(), Faces={0}", iMaxFace);
                    WinCall.TraceMessage(str);
                }
                for (int i = 0; i < iMaxFace; i++)
                {
                    HWFaceInfo face = detectFaceInfos[i];
                    if (i == 0)
                    {
                        curLiveFaceInfo = face;
                        continue;
                    }
                    int nMaxH = curLiveFaceInfo.m_FaceRect.bottom - curLiveFaceInfo.m_FaceRect.top;
                    int nMaxW = curLiveFaceInfo.m_FaceRect.right - curLiveFaceInfo.m_FaceRect.left;
                    int nCurH = face.m_FaceRect.bottom - face.m_FaceRect.top;
                    int nCurW = face.m_FaceRect.right - face.m_FaceRect.left;
                    if (nCurW * nCurH > nMaxW * nMaxH)
                    {
                        curLiveFaceInfo = face;
                    }
                }
            }
            return bValid;
        }

        internal HWFaceInfo GetLiveFaceInfo()
        {
            return curLiveFaceInfo;
        }


        public float GetLiveDataWidth()
        {
            return livePhotoData.width;
        }

        public float GetLiveDataHeight()
        {
            return livePhotoData.height;
        }
    }
}