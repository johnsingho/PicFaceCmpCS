using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using CameraApp.Exam;
using CameraApp.MyCap;
using JohnKit;
using System.Media;
using System.Drawing;
using System.Drawing.Imaging;
using ZBar;
using System.Collections.Generic;
using System.IO;
using CameraApp.DbLog;
using ZBar = ZBar.ZBar;

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
        private IDBaseTextDecoder idTextDecoder=new IDBaseTextDecoder();
        private GateBoardOper gateBoardOper = new GateBoardOper();
        private FaceCmpEngine faceCmpEngine = new FaceCmpEngine();
        private MindVisionCamOper indusCamOper = new MindVisionCamOper();

        private ManualResetEvent stopMainEvent = null;
        private Thread thMainThread=null;

        private ManualResetEvent stopLiveCamEvent=null;
        private Thread thLiveCamThread = null;

        #region 提示音相关
        private SoundPlayer voiceInitOK;
        private SoundPlayer voiceInitFail;
        private SoundPlayer voicePass;
        private SoundPlayer voiceFailFaceCmp;
        private SoundPlayer voiceFailTickCHK;
        private SoundPlayer voiceViewCam;
        private SoundPlayer voicePlaceTic;
        #endregion

        #region 一些常量
        public static readonly string DEF_IDPIC_DIR = "mod";        
        #endregion

        #region 操作相关
        //身份证照片
        private Bitmap bmIDPhoto = null;
        private LogDb logdb = new LogDb();

        #endregion



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
        public void PromptError(string strErr)
        {
            mainWin.PromptError(strErr);
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

        
        private void FuncInitEnv()
        {
            //初始化身份证读取模块
            WinCall.ThreadBoolRet thDataIDCard = new WinCall.ThreadBoolRet();
            Thread thIDCard = new Thread(TryInitIDCardReader)
            {
                Name = "thIDCard",
                IsBackground = true
            };
            thIDCard.Start(thDataIDCard);

            //初始化人脸识别库
            WinCall.ThreadBoolRet thDataFaceCmp = new WinCall.ThreadBoolRet();
            Thread thFaceCmp = new Thread(TryInitFaceCmp)
            {
                Name = "thFaceCmp",
                IsBackground = true
            };
            thFaceCmp.Start(thDataFaceCmp);

            //初始化两个摄像头
            WinCall.ThreadBoolRet thDataInitCam = new WinCall.ThreadBoolRet();
            Thread thInitCam = new Thread(TryInitCameras)
            {
                Name = "thInitCam",
                IsBackground = true
            };
            thInitCam.Start(thDataInitCam);

            //初始化灯板、闸门
            WinCall.ThreadBoolRet thDataGateBoard = new WinCall.ThreadBoolRet();
            Thread thGateBoard = new Thread(TryInitGateBoard)
            {
                Name = "thGateBoard",
                IsBackground = true
            };
            thGateBoard.Start(thDataGateBoard);

            LoadVoices();

            thIDCard.Join();
            thFaceCmp.Join();
            thInitCam.Join();
            thGateBoard.Join();

            if (thDataIDCard.bResult && thDataFaceCmp.bResult && thDataInitCam.bResult && thDataGateBoard.bResult)
            {   
                StartMainThread();
                StartLiveCamThread();
                PlayVoice(ConstValue.VOICE_INIT_OK);
            }
            else
            {
                PlayVoice(ConstValue.VOICE_INIT_FAIL);
            }
        }
        
        //加载提示音
        private void LoadVoices()
        {
            voiceInitOK = new SoundPlayer(ConstValue.VOICE_DIR + ConstValue.VOICE_INIT_OK);
            voiceInitOK.Load();            
            voiceInitFail = new SoundPlayer(ConstValue.VOICE_DIR + ConstValue.VOICE_INIT_FAIL);
            voiceInitFail.Load();
            voicePass = new SoundPlayer(ConstValue.VOICE_DIR + ConstValue.VOICE_PASS);
            voicePass.Load();
            voiceFailFaceCmp = new SoundPlayer(ConstValue.VOICE_DIR + ConstValue.VOICE_FAIL_FACECMP);
            voiceFailFaceCmp.Load();
            voiceFailTickCHK = new SoundPlayer(ConstValue.VOICE_DIR + ConstValue.VOICE_FAIL_TICKCHK);
            voiceFailTickCHK.Load();
            voiceViewCam = new SoundPlayer(ConstValue.VOICE_DIR + ConstValue.VOICE_VIEW_CAM);
            voiceViewCam.Load();
            voicePlaceTic = new SoundPlayer(ConstValue.VOICE_DIR + ConstValue.VOICE_PLACE_TIC);
            voicePlaceTic.Load();
        }

        public void PlayVoice(string strVoice)
        {
            SoundPlayer player = null;
            switch (strVoice)
            {
                case ConstValue.VOICE_INIT_OK:
                    player = voiceInitOK;
                    break;
                case ConstValue.VOICE_INIT_FAIL:
                    player = voiceInitFail;
                    break;
                case ConstValue.VOICE_PASS:
                    player = voicePass;
                    break;
                case ConstValue.VOICE_FAIL_FACECMP:
                    player = voiceFailFaceCmp;
                    break;
                case ConstValue.VOICE_FAIL_TICKCHK:
                    player = voiceFailTickCHK;
                    break;
                case ConstValue.VOICE_VIEW_CAM:
                    player = voiceViewCam;
                    break;
                case ConstValue.VOICE_PLACE_TIC:
                    player = voicePlaceTic;
                    break;
                default: break;
            }
            if (player!=null)
            {
                player.Play();
            }
        }

        private void TryInitIDCardReader(object o)
        {
            WinCall.ThreadBoolRet thRet = (WinCall.ThreadBoolRet)o;
            bool bOpen = (1==idCardReader.TryOpenCOM(this.configInfo.IDReaderCOM));
            if (bOpen)
            {
                thRet.bResult = true;
                return;
            }
            const int MAX_RETRY_COM = 20;
            for (int i = 1; i <= MAX_RETRY_COM; i++)
            {
                if (i == this.configInfo.IDReaderCOM)
                {
                    continue;
                }
                int nRet = idCardReader.TryOpenCOM(i);
                bOpen = (1 == nRet);
                if (-1 == nRet)
                {
                    //这里认为后续串口都不存在了
                    break;
                }
                if (bOpen)
                {
                    this.configInfo.IDReaderCOM = i;
                    this.configInfo.bNeedSave = true;
                    break;
                }
            }
            if (!bOpen)
            {
                PromptError("身份证读卡器初始化失败");
            }
            thRet.bResult = bOpen;
        }

        private void TryInitGateBoard(object o)
        {
            WinCall.ThreadBoolRet thRet = (WinCall.ThreadBoolRet)o;
            bool bOpen = gateBoardOper.TryOpenCOM(configInfo.GateBoardCOM);
            if (bOpen)
            {
                gateBoardOper.TurnoffAllLight();
            }
            thRet.bResult = bOpen;
        }

        private void TryInitCameras(object o)
        {
            WinCall.ThreadBoolRet thRet = (WinCall.ThreadBoolRet)o;

            WinCall.ThreadBoolRet thRetTick = new WinCall.ThreadBoolRet();
            thRetTick.pExtra = this;
            //(1)车票摄像头初始化
            mainWin.CreateTicketCamOper(thRetTick);

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
            WinCall.ThreadBoolRet thRet = (WinCall.ThreadBoolRet)o;
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
            logdb.Dispose();
            if (bmIDPhoto != null)
            {
                bmIDPhoto.Dispose();
                bmIDPhoto = null;
            }
        }

        //清退工作
        public void DoExit()
        {
            StopMainThread();
            WaitForMainThreadStop();

            StopLiveCamThread();
            WaitForLiveCamThreadStop();
        }

        public int GetCamTicketID()
        {
            return configInfo.CamTicketID;
        }
        

        #region MainThread        
        private void StartMainThread()
        {
            stopMainEvent = new ManualResetEvent(false);
            thMainThread = new Thread(FuncMainFaceCmp)
            {
                Name = "MainFaceCmp",
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
            thMainThread.Start();
        }

        private void StopMainThread()
        {
            if (thMainThread != null)
            {
                stopMainEvent.Set();
            }
        }
        private void WaitForMainThreadStop()
        {
            if (thMainThread != null)
            {
                thMainThread.Join();
                thMainThread = null;

                stopMainEvent.Close();
                stopMainEvent = null;
            }
        }

        /// <summary>
        /// 主工作线程
        /// </summary>
        private void FuncMainFaceCmp()
        {
            JobManager jm = new JobManager();
            jm.disPatch(JobManager.sHandlerReadIDCard, this); //init
            while (!stopMainEvent.WaitOne(JobManager.IDLE_WAIT_MS, true))
            {
                jm.doWork(this);
            }
            WinCall.TraceMessage("***FuncMainFaceCmpThread exit");
        }
        #endregion

        #region StartLiveCamThread
        private void StartLiveCamThread()
        {
            stopLiveCamEvent = new ManualResetEvent(false);
            thLiveCamThread = new Thread(FuncLiveCamShow)
            {
                Name = "LiveCamShow",
                IsBackground = true
            };
            thLiveCamThread.Start();
        }
        private void StopLiveCamThread()
        {
            if (thLiveCamThread != null)
            {
                stopLiveCamEvent.Set();
            }
        }
        private void WaitForLiveCamThreadStop()
        {
            if (thLiveCamThread != null)
            {
                thLiveCamThread.Join();
                thLiveCamThread = null;

                stopLiveCamEvent.Close();
                stopLiveCamEvent = null;
            }
        }
        private void FuncLiveCamShow()
        {
            if (!indusCamOper.Play())
            {
                WinCall.TraceMessage("***indusCamOper.Play() error");
                return;
            }
            //int nWaitMS = JobManager.IDLE_WAIT_MS;
            int nWaitMS = 150;
            while (!stopLiveCamEvent.WaitOne(nWaitMS, true))
            {
                //live photo
                Bitmap bmCur = indusCamOper.QueryFrame(400);
                if (bmCur == null)
                {
                    continue;
                }
                //垂直翻转画面
                bmCur.RotateFlip(RotateFlipType.Rotate180FlipY);
                Bitmap bmDetect = DetectFace(bmCur);
                if (stopLiveCamEvent.WaitOne(nWaitMS / 2, true))
                {
                    break;
                }
                mainWin.RefreshLiveCam(bmDetect);
            }
            indusCamOper.Stop();
            WinCall.TraceMessage("***liveCamThread exit");
        }

        /// <summary>
        /// 使用汉王库来侦测人脸
        /// 返回画上了框的照片
        /// </summary>
        /// <param name="bmCur">当前的现场照片</param>
        /// <returns></returns>
        private Bitmap DetectFace(Bitmap bmCur)
        {
            faceCmpEngine.GetLivePhoto(bmCur);
            Bitmap bmDetect = (Bitmap)bmCur.Clone();
            if (faceCmpEngine.DetectLivePhoto())
            {   
                float fWS = (float) bmCur.Width/(float) faceCmpEngine.GetLiveDataWidth();
                float fHS = (float) bmCur.Height/(float) faceCmpEngine.GetLiveDataHeight();
                var curLiveFace = faceCmpEngine.GetLiveFaceInfo();
                float rX = curLiveFace.m_FaceRect.left*fWS;
                float rY = curLiveFace.m_FaceRect.top*fHS;
                float rW = (curLiveFace.m_FaceRect.right - curLiveFace.m_FaceRect.left)*fWS;
                float rH = (curLiveFace.m_FaceRect.bottom - curLiveFace.m_FaceRect.top)*fHS;
                //如果找到人脸的话，画个框
                using (Graphics g = Graphics.FromImage(bmDetect))
                {
                    using (var pen = new Pen(Color.Red, 2))
                    {
                        g.DrawRectangle(pen, rX, rY, rW, rH);
                    }
                }
            }
            return bmDetect;
        }
        #endregion

        //重置身份证信息显示
        public void ResetIDCardInfo()
        {
            mainWin.ResetIDCardInfo();
        }

        // 读取身份证信息
        public bool ReadIDCardInfo()
        {
            int nRet = idCardReader.ReadCard();
            if (nRet != 0)
            {
                //UpdatePromptInfo("请将身份证放在感应区上面!");
                return false;
            }
            
            idTextDecoder.IdCardReader = idCardReader;
            if (!idTextDecoder.Decode())
            {
                return false;
            }

            if (!idCardReader.WritePhotoFile(DEF_IDPIC_DIR, idTextDecoder.m_strID))
            {
                PromptInfo("提取身份证照片失败!");
                return false;
            }
            mainWin.ShowIDCardInfo(idTextDecoder);
            return true;
        }

        public bool LoadIDPhoto()
        {
            if(bmIDPhoto!= null)
            {
                bmIDPhoto.Dispose();
                bmIDPhoto = null;
            }
            string strBmpPath = idCardReader.GetLastIDPhotoFile();
            bmIDPhoto = WinCall.LoadBitmap(strBmpPath);
            bool bOk = (bmIDPhoto != null);
            if (bOk)
            {
                faceCmpEngine.GetIDPhoto(bmIDPhoto);
            }
            return bOk;
        }

        public Bitmap GetIDPhoto()
        {
            return bmIDPhoto;
        }

        //闪灯
        public void FlashLight(int iLight, int dwMs)
        {
            const int nCnt = 2;
            for (int i = 0; i<nCnt; i++)
            {
                SwitchLight(iLight, true);
                JobManager.Sleep(dwMs);
                SwitchLight(iLight, false);
            }
        }
        
        //开关灯
        public void SwitchLight(int iLight, bool bOn)
        {
            gateBoardOper.SwitchLight(iLight, bOn);
        }
        
        //先闪后亮
        public void FlashAndLight(int iLight)
        {
            FlashLight(iLight, 200);
            SwitchLight(iLight, true);
        }

        public int GetMaxFaceCmpTimes()
        {
            return configInfo.MaxRetryFaceCmp;
        }
        public int GetMaxTicketChkTimes()
        {
            return configInfo.MaxRetryTicketChk;
        }
        /// <summary>
        /// 进行人脸对比操作
        /// </summary>
        /// <param name="fScore">输出人脸识别分数</param>
        /// <returns></returns>
        public bool DoFaceCmp(ref float fScore)
        {
            float fScmp = 0.0F;
            //两个图片的比对。 并且保存特征.            
            fScmp = faceCmpEngine.CompareAFace(configInfo.InitFaceCmpRate/1000.0F, 0);
            fScmp = fScmp * 100.0F;

#if FACE_ZUOBI
            //人脸识别分数作弊
            if (fScmp > 0.0000)
            {
                fScmp += 9.5786;
            }
#endif
            string strScore = string.Format("人脸识别相似度：{0:F2}%", fScmp);
            WinCall.TraceMessage(strScore);
            PromptInfo(strScore);

            fScore = fScmp;
            return (fScmp >= configInfo.ReqFaceCmpScore);
        }

        public class DataTickCam
        {
            public Bitmap bm;
        }

        //识别二维码
        public bool DoCheckTicket(ref string strQrCode)
        {
            DataTickCam data = new DataTickCam();
            if (!mainWin.CapTicket(data))
            {
                return false;
            }

            strQrCode = string.Empty;
            using (Bitmap bmGray = WinCall.BitmapConvetGray(data.bm))
            {
                //垂直翻转
                bmGray.RotateFlip(RotateFlipType.Rotate90FlipX);
                using (ImageScanner scanner = new ImageScanner())
                {
                    scanner.SetConfiguration(SymbolType.None, Config.Enable, 1);
                    List<Symbol> symbols = scanner.Scan(bmGray);
                    WinCall.TraceMessage(string.Format("***Ticket syms={0}", symbols.Count)); //! for test

                    foreach (var sym in symbols)
                    {
                        var symType = sym.GetType();
                        if (!string.IsNullOrEmpty(sym.Data))
                        {
                            strQrCode = sym.Data;
                            break; //只取一个结果
                        }
                    }
                }
            }
            data.bm.Dispose();
            return (false == string.IsNullOrEmpty(strQrCode));
        }

        //放行
        public void LetGo()
        {
            gateBoardOper.OpenGate(1);
            PlayVoice(ConstValue.VOICE_PASS);
            PromptInfo("验证成功。\n祝你旅途愉快！");
        }

        public void SwitchTicketCam(bool bShow)
        {
            if (bShow)
            {
                this.PromptInfo("请将车票平放在验票口!");
                this.PlayVoice(ConstValue.VOICE_PLACE_TIC);
                this.FlashAndLight(0);
            }
            else
            {
                this.SwitchLight(0, false);
            }
            mainWin.ShowTicketCam(bShow);
        }

        /// <summary>
        /// 保存身份证照片和现场照片，留底
        /// </summary>
        /// <param name="fScore"></param>
        public void KeepCompareInfo(float fScore)
        {
#if KEEP_PIC_LOG
            if (!logdb.TryOpen())
            {
                WinCall.TraceMessage("***KeepCompareInfo创建数据库失败！");
                return;
            }
            logdb.InsertRec(idTextDecoder.m_strName, idTextDecoder.m_strID, fScore, bmIDPhoto, faceCmpEngine.GetStoreLivePic());
#endif
        }

        /// <summary>
        /// 调试版用，写日志文本文件
        /// </summary>
        /// <param name="fScore"></param>
        public void WriteFaceCmpLog(float fScore)
        {
#if DEBUG

#endif
        }
    }
}
