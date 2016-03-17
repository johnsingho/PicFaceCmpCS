using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace CameraApp
{
    using HWRESULT = System.UInt32;
    using HW_HANDLE = System.IntPtr;

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
                            ref HWFaceInfo pFaceInfo);

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
        private static readonly int S_OK = 0;
        private static readonly int S_FAIL = 1;
        #endregion

        private HW_HANDLE handleLib=IntPtr.Zero;
        

        public bool InitLib()
        {
            string strCurdir = Application.StartupPath;
            HWRESULT hRes = HWInitialD(strCurdir);
            if (S_OK != hRes)
            {
                return false;
            }
            hRes = HWInitial(ref handleLib, strCurdir);
            return S_OK == hRes;
        }

        public void Dispose()
        {
            if (handleLib != IntPtr.Zero)
            {
                HWRelease(ref handleLib);
                handleLib = IntPtr.Zero;
            }
            HWReleaseD();
        }

        //-----------------------------
        
    }
}