using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Threading;

namespace CameraApp
{
    /// <summary>
    /// 主要操作类
    /// H.Z.XIN 2016-03-08
    /// </summary>
    public class FaceDetect
    {
        private class ConfigInfo
        {
            public int IDReaderCOM;
            public int GateBoardCOM;
            public int CamFaceID;
            public int CamTicketID;
            public int MaxRetryFaceCmp;
            public int MaxRetryTicketChk;
            public int ReqFaceCmpScore;
            public int InitFaceCmpRate;
        }

        private ConfigInfo configInfo=new ConfigInfo();
        private MainWin mainWin;
        private ComIdCardReader idCardReader=new ComIdCardReader();

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
        }

        public void BindForm(MainWin mainWin)
        {
            this.mainWin = mainWin;
        }

        public void PromptInfo(string strInfo)
        {
            mainWin.PromptInfo(strInfo);
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
            thDevInit.Start();
        }

        private void FuncInitEnv()
        {
            //初始化身份证读取模块
            bool bInitID = TryInitIDCardReader();

            //初始化人脸识别库
            bool bInitFace = TryInitFaceCmp();
            //初始化两个摄像头
            bool bInitCam = TryInitCameras();
            //初始化灯板、闸门
            bool bInitGateBoard = TryInitGateBoard();

            if (bInitID && bInitFace && bInitCam && bInitGateBoard)
            {
                string str=string.Format("欢迎使用\n%s", ConstValue.DEF_SYS_NAME);
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

        private bool TryInitGateBoard()
        {
            return false;
        }

        private bool TryInitCameras()
        {
            return false;
        }

        private bool TryInitFaceCmp()
        {
            return false;
        }

        private bool TryInitIDCardReader()
        {
            bool bOpen = idCardReader.TryOpenCOM(this.configInfo.IDReaderCOM);
            if (bOpen) { return true; }
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
                    break;
                }
            }
            return bOpen;
        }
    }
}
