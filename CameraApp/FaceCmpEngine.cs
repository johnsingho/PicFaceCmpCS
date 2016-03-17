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
                            ref HWFaceInfo pFaceInfo);

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