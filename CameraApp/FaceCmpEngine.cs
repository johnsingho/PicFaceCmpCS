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
    using System.Diagnostics;/// <summary>
                             /// ʹ�ú�������ʶ���
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
            public Rect m_FaceRect; //������λ��
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 81 * 2)]
            public float[] m_afKeyPoint;
            public KeyPos m_KeyPos; //
        };

        #endregion

        #region HWFaceRecSDK API

        //��ʼ������, strName ����dll����·������"D:\\Prog"
        //������غ�����һ�Σ�����HWInitial����
        //return: S_OK, S_FAIL
        [DllImport("HWcompare.dll", CallingConvention=CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern HWRESULT HWInitialD([MarshalAs(UnmanagedType.LPStr)]string strName);

        //�ͷź���
        //�����˳�ǰ����һ��
        //return: S_OK, S_FAIL
        [DllImport("HWcompare.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern HWRESULT HWReleaseD();

        //��ʼ��һ��HANDLE . ���߳�����¸����̳߳�ʼ�����Ե�Handle,
        //pHandle [output]ָ���ʼ���õ�Handle
        //strName [input]NULL
        //return: S_OK, S_FAIL
        [DllImport("HWcompare.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern HWRESULT HWInitial(ref HW_HANDLE pHandle, [MarshalAs(UnmanagedType.LPStr)]string strName);

        //�ͷ�Handle
        //pHandle [input]ָ��HWInitial��ʼ���õ�Handle
        [DllImport("HWcompare.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern HWRESULT HWRelease(ref HW_HANDLE pHandle);

        /**************************************************

            ������λ

        *************************************************/
        //������λ
        //Handle [input] HWInitial��ʼ���õ�Handle
        //pImg   [input] ����ͼƬ�Ҷ���Ϣ����������:ͼƬ�����ϵ����£����� ÿ�д�����������и����صĻҶ�ֵ
        //nImgWidth nImgHeight [input] ͼƬ�Ŀ�ȸ߶�
        //pnMaxFace [input] ��Ҫ��λ����������� ��1~10)
        //          [output] *pnMaxFace Ϊʵ�ʶ�λ����������
        //pFaceInfo [output] ���ÿ��������λ��Ϣ�� ��Ҫ�ⲿ����*pnMaxFace�� HWFaceInfo�ռ䡣
        //return: S_OK, S_FAIL
        [DllImport("HWcompare.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern HWRESULT HWDetectFaceKeyPoints(HW_HANDLE Handle,
                            IntPtr pImg,
                            int nImgWidth, int nImgHeight,
                            ref int pnMaxFace,
                            ref HWFaceInfo[] pFaceInfo);

        //�����Ƿ��ͷ��֮���֤����Ƭ�����ȷ����֤���գ����������iPortrait = 1,������Ϊ0��
        //��Ϊ1��λ�����ס�
        //Handle [input] HWInitial��ʼ���õ�Handle
        //iPortrait [input] 1 �ǵġ�0 ��ȷ����
        //return: S_OK, S_FAIL
        [DllImport("HWcompare.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern HWRESULT HWSetPortrait(HW_HANDLE Handle, int iPortrait);

        /**************************************************
            �����ͱȶ�
        /*************************************************/
        //
        //Handle [input] HWInitial��ʼ���õ�Handle
        //piFtrSize [output] ��������ֽڸ���
        //return: S_OK, S_FAIL
        [DllImport("HWcompare.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern HWRESULT HWGetFeatureSize(HW_HANDLE Handle, ref int piFtrSize);


        //��ȡ������
        //Handle [input] HWInitial��ʼ���õ�Handle
        //pImg   [input] ����ͼƬ�Ҷ���Ϣ����������:ͼƬ�����ϵ����£����� ÿ�д�����������и����صĻҶ�ֵ
        //nImgWidth, nImgHeight[input] ͼƬ�Ŀ�ȸ߶�
        //pFaceInfo   [input] һ��������Ϣ
        //pOutFeature [output]��������������������ȼ�HWGetFeatureSize�� ��Ҫ�ⲿ����á�
        //return : S_OK. other failed
        [DllImport("HWcompare.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern HWRESULT HWExtractFeature(HW_HANDLE Handle,
                          IntPtr pImg, int nImgWidth, int nImgHeight,
                          ref HWFaceInfo pFaceInfo,
                          IntPtr pOutFeature);


        //���ڵ����Ƚ�����ͼƬ�������������ԡ�
        //Handle [input] HWInitial��ʼ���õ�Handle
        //pFeaA  [input] ������
        //pFeaB  [input] ������
        //fScore [output] �����Զ���ֵ��0~1.0 ��Խ��Խ���ơ�
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
        
        //��ǰ�ֳ���Ƭ����
        private HWFaceInfo curLiveFaceInfo = new HWFaceInfo();
        //��ȡ����������
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
                livePhotoData = PicPixel.CreatePicPixel(ID_PHOTO_WID, ID_PHOTO_HEI);
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

        //��������
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

        //����һ��һ�Ա�
        public float CompareAFace(float fInitFaceCmpRate, int iPorttrail)
        {
            float fScore = fInitFaceCmpRate;
            int iFtrSize = 0;
            HWGetFeatureSize(handleLib, ref iFtrSize);

            IntPtr pbFtrID = Marshal.AllocHGlobal(iFtrSize);
            IntPtr pbFtrLiveFace = Marshal.AllocHGlobal(iFtrSize);

            //���ȷ����֤���գ�������Portrait= 1�� ������Portrait = 0
            HWSetPortrait(handleLib, iPorttrail);

            int iMaxFace = 1;
            HWFaceInfo[] idFaceInfo=new HWFaceInfo[iMaxFace];
            //�����֤�ϵ�����
            HWRESULT iRst = HWDetectFaceKeyPoints(handleLib, idPhotoData.pixel, idPhotoData.width, idPhotoData.height, ref iMaxFace, ref idFaceInfo);
            if (iRst != S_OK)
            {
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

            //�ҳ��ֳ���Ƭ�ϵ�����
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
            using (Bitmap bmTemp = BitmapScale(bmCur, LIVE_PHOTO_WID, LIVE_PHOTO_HEI))
            {
                using (Bitmap bmGrapy = BitmapConvetGray(bmTemp))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bmGrapy.Save(ms, bmGrapy.RawFormat);
                        byte[] byData = ms.GetBuffer();
                        Debug.Assert((pPix.width * pPix.height >= byData.Length), "�ҶȻ�����������");
                        Marshal.Copy(byData, 0, pPix.pixel, byData.Length);
                    }
                }
            }            
        }

        public static Bitmap BitmapConvetGray(Bitmap img)
        {   
            int h = img.Height;
            int w = img.Width;
            int gray = 0;

            Bitmap bmpOut = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            BitmapData dataIn = img.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData dataOut = bmpOut.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* pIn = (byte*)(dataIn.Scan0.ToPointer());
                byte* pOut = (byte*)(dataOut.Scan0.ToPointer());

                for (int y = 0; y < dataIn.Height; y++)
                {
                    for (int x = 0; x < dataIn.Width; x++)
                    {
                        //gray = (pIn[0] * 19595 + pIn[1] * 38469 + pIn[2] * 7472) >> 16;
                        gray = (pIn[0] * 299 + pIn[1] * 587 + pIn[2] * 114 + 500) / 1000;
                        pOut[0] = (byte)gray;
                        pOut[1] = (byte)gray;
                        pOut[2] = (byte)gray;
                        pIn += 3; pOut += 3;
                    }
                    pIn += dataIn.Stride - dataIn.Width * 3;
                    pOut += dataOut.Stride - dataOut.Width * 3;
                }
            }
            bmpOut.UnlockBits(dataOut);
            img.UnlockBits(dataIn);
            return bmpOut;
        }

        private static Bitmap BitmapScale(Bitmap img, int wid, int hei)
        {
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


        //-----------------------------

    }
}