﻿using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using CameraApp.MyCap;
using JohnKit;

namespace CameraApp
{
    /// <summary>
    /// 主要操作类
    /// H.Z.XIN 2016-03-08
    /// </summary>
    public class FaceDetect : IDisposable
    {
        private class ConfigInfo
        {
            public bool bNeedSave;
            public int IDReaderCOM;
            public int GateBoardCOM;
            public int CamFaceID;
            public int CamTicketID;
            public int MaxRetryFaceCmp;
            public int MaxRetryTicketChk;
            public int ReqFaceCmpScore;
            public int InitFaceCmpRate;
        }

        private ConfigInfo configInfo = new ConfigInfo();
        private MainWin mainWin = null;

        private ComIdCardReader idCardReader = new ComIdCardReader();
        private GateBoardOper gateBoardOper = new GateBoardOper();
        private FaceCmpEngine faceCmpEngine = new FaceCmpEngine();
        private MindVisionCamOper indusCamOper = new MindVisionCamOper();
        private Capture tickCamOper = null;

        public bool LoadConfigInfo()
        {
            configInfo.IDReaderCOM = Int32.Parse(ConfigurationManager.AppSettings["IDReaderCOM"]);
            configInfo.GateBoardCOM = Int32.Parse(ConfigurationManager.AppSettings["GateBoardCOM"]);
            configInfo.CamFaceID = Int32.Parse(ConfigurationManager.AppSettings["CamFaceID"]);
            configInfo.CamTicketID = Int32.Parse(ConfigurationManager.AppSettings["CamTicketID"]);
            configInfo.MaxRetryFaceCmp = Int32.Parse(ConfigurationManager.AppSettings["MaxRetryFaceCmp"]);
            configInfo.MaxRetryTicketChk = Int32.Parse(ConfigurationManager.AppSettings["MaxRetryTicketChk"]);
            configInfo.ReqFaceCmpScore = Int32.Parse(ConfigurationManager.AppSettings["ReqFaceCmpScore"]);
            configInfo.InitFaceCmpRate = Int32.Parse(ConfigurationManager.AppSettings["InitFaceCmpRate"]);


            return true;
        }

        public void SaveConfigInfo()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["IDReaderCOM"].Value = configInfo.IDReaderCOM.ToString();
            config.AppSettings.Settings["GateBoardCOM"].Value = configInfo.GateBoardCOM.ToString();
            config.AppSettings.Settings["CamFaceID"].Value = configInfo.CamFaceID.ToString();
            config.AppSettings.Settings["CamTicketID"].Value = configInfo.CamTicketID.ToString();
            config.AppSettings.Settings["MaxRetryFaceCmp"].Value = configInfo.MaxRetryFaceCmp.ToString();
            config.AppSettings.Settings["MaxRetryTicketChk"].Value = configInfo.MaxRetryTicketChk.ToString();
            config.AppSettings.Settings["ReqFaceCmpScore"].Value = configInfo.ReqFaceCmpScore.ToString();
            config.AppSettings.Settings["InitFaceCmpRate"].Value = configInfo.InitFaceCmpRate.ToString();

            config.Save(ConfigurationSaveMode.Full);
            configInfo.bNeedSave = false;
        }

        public void BindForm(MainWin mainWin)
        {
            this.mainWin = mainWin;
        }

        public void PromptInfo(string strInfo)
        {
            mainWin.PromptInfo(strInfo);
        }
        private void PromptError(string strErr)
        {
            mainWin.PromptError(strErr);
        }

        /// <summary>
        /// 清退工作
        /// </summary>
        public void DoExit()
        {

        }

        /// <summary>
        /// 硬件设备初始化
        /// </summary>
        public void InitEnv()
        {
            Thread thDevInit = new Thread(FuncInitEnv);
            thDevInit.Name = "DeviceInitThread";
            thDevInit.IsBackground = true;
            thDevInit.Start();
        }

        /// <summary>
        /// 线程返回值
        /// </summary>
        private class ThreadBoolRet
        {
            public bool bResult = false;
        }

        private void FuncInitEnv()
        {
            //初始化身份证读取模块
            ThreadBoolRet thDataIDCard = new ThreadBoolRet();
            Thread thIDCard = new Thread(TryInitIDCardReader);
            thIDCard.IsBackground = true;
            thIDCard.Start(thDataIDCard);

            //初始化人脸识别库
            ThreadBoolRet thDataFaceCmp = new ThreadBoolRet();
            Thread thFaceCmp = new Thread(TryInitFaceCmp);
            thFaceCmp.IsBackground = true;
            thFaceCmp.Start(thDataFaceCmp);

            //初始化两个摄像头
            ThreadBoolRet thDataInitCam = new ThreadBoolRet();
            Thread thInitCam = new Thread(TryInitCameras);
            thInitCam.IsBackground = true;
            thInitCam.Start(thDataInitCam);

            //初始化灯板、闸门
            ThreadBoolRet thDataGateBoard = new ThreadBoolRet();
            Thread thGateBoard = new Thread(TryInitGateBoard);
            thGateBoard.IsBackground = true;
            thGateBoard.Start(thDataGateBoard);

            thIDCard.Join();
            thFaceCmp.Join();
            thInitCam.Join();
            thGateBoard.Join();

            if (thDataIDCard.bResult && thDataFaceCmp.bResult && thDataInitCam.bResult && thDataGateBoard.bResult)
            {
                string str = string.Format("\n欢迎使用{0}", ConstValue.DEF_SYS_NAME);
                PromptInfo(str);
                //StartMainThread();

                PlayVoice(ConstValue.VOICE_INIT_OK);
            }
            else
            {
                PlayVoice(ConstValue.VOICE_INIT_FAIL);
            }
        }

        private void PlayVoice(string strVoice)
        {

        }

        private void TryInitIDCardReader(object o)
        {
            ThreadBoolRet thRet = (ThreadBoolRet)o;
            bool bOpen = idCardReader.TryOpenCOM(this.configInfo.IDReaderCOM);
            if (bOpen)
            {
                thRet.bResult = true;
                return;
            }
            const int MAX_RETRY_COM = 20;
            for (int i = 0; i < MAX_RETRY_COM; i++)
            {
                if (i == this.configInfo.IDReaderCOM)
                {
                    continue;
                }
                bOpen = idCardReader.TryOpenCOM(i);
                if (bOpen)
                {
                    this.configInfo.IDReaderCOM = i;
                    this.configInfo.bNeedSave = true;
                    break;
                }
            }
            thRet.bResult = bOpen;
        }

        private void TryInitGateBoard(object o)
        {
            ThreadBoolRet thRet = (ThreadBoolRet)o;
            bool bOpen = gateBoardOper.TryOpenCOM(configInfo.GateBoardCOM);
            thRet.bResult = bOpen;
        }

        private void TryInitCameras(object o)
        {
            ThreadBoolRet thRet = (ThreadBoolRet)o;
            //(1)车票摄像头初始化
            mainWin.CreateTicketCamOper(this);

            //(2)人脸摄像头初始化
            bool bCamFace = indusCamOper.Init();
            if (!bCamFace)
            {
                PromptError("初始化人脸摄像头SDK失败");
            }
            else
            {
                bCamFace = indusCamOper.InitCamera();
                if (!bCamFace)
                {
                    PromptError("连接到人脸摄像头失败");
                }
            }

            thRet.bResult=bCamFace;
        }

        /// <summary>
        /// 初始化人脸识别库
        /// </summary>
        /// <returns></returns>
        private void TryInitFaceCmp(object o)
        {
            ThreadBoolRet thRet = (ThreadBoolRet)o;
            bool bRet = faceCmpEngine.InitLib();
            if (!bRet)
            {
                PromptError("人脸识别模块初始化失败");
            }
            thRet.bResult = bRet;
        }

        public void Dispose()
        {
            idCardReader.Dispose();
            faceCmpEngine.Dispose();
        }
        

        public int GetCamTicketID()
        {
            return configInfo.CamTicketID;
        }

        public void SetTickCamOper(Capture tickCamOper)
        {
            this.tickCamOper = tickCamOper;
        }
    }
}
