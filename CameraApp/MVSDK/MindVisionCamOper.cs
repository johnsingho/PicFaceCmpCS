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
        private Bitmap bmCur=null;

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

            //if (m_pFrame)
            //{
            //    cvReleaseImage(&m_pFrame);
            //    m_pFrame = NULL;
            //}
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

            return true;
        }

        bool Play()
        {
            return CameraSdkStatus.CAMERA_STATUS_SUCCESS == MvApi.CameraPlay(m_hCamera);
        }

        bool Pause()
        {
            return CameraSdkStatus.CAMERA_STATUS_SUCCESS == MvApi.CameraPause(m_hCamera);
        }


        public bool QueryFrame(uint wTimes=1000 )
        {
            int nWidth = 0;
            int nHeight = 0;
            IntPtr pPicBuf = (IntPtr) MvApi.CameraGetImageBufferEx(m_hCamera, ref nWidth, ref nHeight, wTimes);
            if (pPicBuf==IntPtr.Zero) { return false; }

            int nDataLen = nWidth*nHeight*3;
            byte[] byPic = new byte[nDataLen];
            Marshal.Copy(pPicBuf, byPic, 0, nDataLen);

            if (bmCur != null)
            {
                bmCur.Dispose();
                bmCur = null;
            }
            using (MemoryStream ms1 = new MemoryStream(byPic))
            {
                bmCur = (Bitmap)Image.FromStream(ms1);
            }
            return (bmCur != null);

            ////////////////////////////
            //if (nWidth != m_curFrameSize.width || nHeight != m_curFrameSize.height)
            //{
            //    if (m_pFrame)
            //    {
            //        cvReleaseImage(&m_pFrame);
            //    }
            //    m_curFrameSize = cvSize(nWidth, nHeight);
            //    m_pFrame = cvCreateImage(m_curFrameSize, IPL_DEPTH_8U, 3);
            //}
            //if (!m_pFrame)
            //{
            //    return NULL;
            //}
            //memcpy(m_pFrame->imageData, pPicBuf, m_pFrame->imageSize);
            //return m_pFrame;
        }



    }
}