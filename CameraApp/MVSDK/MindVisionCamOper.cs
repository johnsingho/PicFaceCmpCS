using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using JohnKit;
using MVSDK;

namespace CameraApp
{
    //using MvApi = MVSDK.MvApi;
    using CameraHandle = System.Int32;

    /// <summary>
    /// 使用MindVision工业摄像头
    /// 
    /// </summary>
    public class MindVisionCamOper
    {
        private readonly int LANG=1;
        private CameraHandle m_hCamera = 0;
        private IntPtr m_ImageBufferSnapshot;

        public bool Init()
        {
            return (CameraSdkStatus.CAMERA_STATUS_SUCCESS == MvApi.CameraSdkInit(LANG));
        }
        

        void UnInitCamera()
        {
            if (0!=m_hCamera)
            {
                MvApi.CameraStop(m_hCamera);
                MvApi.CameraUnInit(m_hCamera);
            }

            Marshal.FreeHGlobal(m_ImageBufferSnapshot);
            m_ImageBufferSnapshot = IntPtr.Zero;
        }


        public bool InitCamera()
        {
            const int MAX_CAMS = 2;
            tSdkCameraDevInfo[] sCameraList=null;
            int iCameraNums = MAX_CAMS;
            tSdkCameraCapbility sCameraInfo = new tSdkCameraCapbility();
            
            CameraSdkStatus status = MvApi.CameraEnumerateDevice(out sCameraList);
            if (status != CameraSdkStatus.CAMERA_STATUS_SUCCESS || iCameraNums == 0)
            {
                return false;
            }

            //只假设连接了一个相机。
            if ((status = MvApi.CameraInit(ref sCameraList[0], -1, -1, ref m_hCamera)) != CameraSdkStatus.CAMERA_STATUS_SUCCESS)
            {
                string msg = string.Format("***Failed to init the camera! Error code is {0}", status);
                WinCall.TraceMessage(msg);
                return false;
            }

            //Get properties description for this camera.
            MvApi.CameraGetCapability(m_hCamera, out sCameraInfo);
            m_ImageBufferSnapshot = Marshal.AllocHGlobal(sCameraInfo.sResolutionRange.iWidthMax * sCameraInfo.sResolutionRange.iHeightMax * 3 + 1024);
            return true;
        }

        public bool Play()
        {
            return CameraSdkStatus.CAMERA_STATUS_SUCCESS == MvApi.CameraPlay(m_hCamera);
        }

        public bool Pause()
        {
            return CameraSdkStatus.CAMERA_STATUS_SUCCESS == MvApi.CameraPause(m_hCamera);
        }
        public bool Stop()
        {
            return CameraSdkStatus.CAMERA_STATUS_SUCCESS == MvApi.CameraStop(m_hCamera);
        }

        public Bitmap QueryFrame(uint wTimes = 1000)
        {
            tSdkFrameHead tFrameHead;
            IntPtr uRawBuffer;
            Bitmap bmFrame = null;
            if (MvApi.CameraSnapToBuffer(m_hCamera, out tFrameHead, out uRawBuffer, 800) == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
            {
                //将相机输出的原始数据转换为RGB格式到内存m_ImageBufferSnapshot中
                MvApi.CameraImageProcess(m_hCamera, uRawBuffer, m_ImageBufferSnapshot, ref tFrameHead);
                MvApi.CameraReleaseImageBuffer(m_hCamera, uRawBuffer);
                Image img = MvApi.CSharpImageFromFrame(m_ImageBufferSnapshot, ref tFrameHead);
                bmFrame = (Bitmap) img;
            }
            return bmFrame;
        }

        //public Bitmap QueryFrame(uint wTimes=1000 )
        //{
        //    int nWidth = 0;
        //    int nHeight = 0;
        //    IntPtr pPicBuf = (IntPtr) MvApi.CameraGetImageBufferEx(m_hCamera, ref nWidth, ref nHeight, wTimes);
        //    if (pPicBuf==IntPtr.Zero) { return null; }

        //    int nDataLen = nWidth*nHeight*3;
        //    byte[] byPic = new byte[nDataLen];
        //    Marshal.Copy(pPicBuf, byPic, 0, nDataLen);

        //    Bitmap bmCur = null;
        //    using (MemoryStream ms1 = new MemoryStream(byPic))
        //    {
        //        try
        //        {
        //            var img = Image.FromStream(ms1); //! todo
        //            bmCur = (Bitmap)img;
        //        }
        //        catch (ArgumentException)
        //        {
        //            bmCur = null;
        //        }
        //    }
        //    return bmCur;

        //    ////////////////////////////
        //    //if (nWidth != m_curFrameSize.width || nHeight != m_curFrameSize.height)
        //    //{
        //    //    if (m_pFrame)
        //    //    {
        //    //        cvReleaseImage(&m_pFrame);
        //    //    }
        //    //    m_curFrameSize = cvSize(nWidth, nHeight);
        //    //    m_pFrame = cvCreateImage(m_curFrameSize, IPL_DEPTH_8U, 3);
        //    //}
        //    //if (!m_pFrame)
        //    //{
        //    //    return NULL;
        //    //}
        //    //memcpy(m_pFrame->imageData, pPicBuf, m_pFrame->imageSize);
        //    //return m_pFrame;
        //}
    }
}