using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;

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
    }
}
