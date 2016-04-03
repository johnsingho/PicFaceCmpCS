using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CameraApp.MyCap;
using JohnKit;

namespace CameraApp
{
    public partial class MainWin : Form
    {
        private FaceDetect faceDetect = null;
        private static readonly Color DefPromptClr = Color.Blue;
        private Timer timerRefreshClock;

        //用来访问车票摄像头
        private Capture tickCamOper;

        delegate void SetInfoTextCallback(string text, Color clrText, int nFontHeight);


        public MainWin()
        {
            InitializeComponent();
#if !DEBUG
    //仅用于调试
            btnTestExit.Visible = false;
            lblTestInfo.Visible = false;
#endif
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            DoInitStart();
        }

        private void InitClock()
        {
            timerRefreshClock = new Timer();
            timerRefreshClock.Tick += new EventHandler(TimerRefreshClock);

            timerRefreshClock.Interval = 1000;
            timerRefreshClock.Start();
        }

        private void TimerRefreshClock(object sender, EventArgs e)
        {
            string str = DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss dddd");
            lblTimer.Text = str;
        }

        private void InitEnv()
        {
            faceDetect.BindForm(this);
            faceDetect.InitEnv();
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                WinCall.ReleaseCapture();
                WinCall.SendMessage(Handle, WinCall.WM_NCLBUTTONDOWN, WinCall.HT_CAPTION, 0);
            }
        }

        private bool LoadConfigInfo()
        {
            return faceDetect.LoadConfigInfo();
        }

        private bool InitVars()
        {
            PromptInfo("系统正在初始化，请稍候...");
            faceDetect = new FaceDetect();
            return false;
        }

        private void DoInitStart()
        {
            InitVars();
            LoadConfigInfo();
            InitEnv();
            InitClock();
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            if (tickCamOper != null)
            {
                tickCamOper.Dispose();
            }

            timerRefreshClock.Stop();
            faceDetect.DoExit();
            faceDetect.Dispose();
        }

        /// <summary>
        /// 信息提示
        /// </summary>
        /// <param name="strInfo"></param>
        public void PromptInfo(string strInfo, int nFontHeight = 18)
        {
            PromptInfo(strInfo, DefPromptClr, nFontHeight);
        }

        public void PromptError(string strErr, int nFontHeight = 18)
        {
            PromptInfo(strErr, Color.Red, nFontHeight);
        }

        public void PromptInfo(string strInfo, Color clrText, int nFontHeight = 18)
        {
            if (this.lblInfo.InvokeRequired)
            {
                SetInfoTextCallback d = new SetInfoTextCallback(PromptInfo);
                this.Invoke(d, new object[] {strInfo, clrText, nFontHeight});
            }
            else
            {
                this.lblInfo.ForeColor = clrText;
                this.lblInfo.Text = strInfo;
                //!todo
                //Font fontInfo = this.lblInfo.Font;
                //fontInfo.Size = nFontHeight;
            }
        }

        private void btnTestExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //通用单参数Delegate
        delegate void MyFuncDelegate1(object objData);

        private void DoCreateTicketCamOper(object objData)
        {
            WinCall.ThreadBoolRet pData = (WinCall.ThreadBoolRet) objData;
            FaceDetect faceDetect = (FaceDetect) pData.pExtra;
            const int VIDEOWIDTH = 640;
            const int VIDEOHEIGHT = 480;
            const int VIDEOBITSPERPIXEL = 24; // BitsPerPixel values determined by device

            try
            {
                tickCamOper = new Capture(faceDetect.GetCamTicketID(), VIDEOWIDTH, VIDEOHEIGHT, VIDEOBITSPERPIXEL,
                    tickPicCtrl);
            }
            catch (Exception ex)
            {
                WinCall.TraceException(ex);
            }
            bool bInit = (tickCamOper != null);
            if (!bInit)
            {
                PromptError("车票摄像头初始化失败！");
            }
            pData.bResult = bInit;
        }


        public void CreateTicketCamOper(WinCall.ThreadBoolRet pData)
        {
            while (!this.InvokeRequired)
            {
            }
            this.Invoke(new MyFuncDelegate1(DoCreateTicketCamOper), pData);
        }

        //通用无参数delegate
        delegate void MyFuncDelegate();

        private void _ResetIDCardInfo()
        {
            this.idCardGifBox.Show();
            this.idCardPicCtrl.Hide();
            string str = string.Format("欢迎使用\n{0}", ConstValue.DEF_SYS_NAME);
            PromptInfo(str);
        }

        public void ResetIDCardInfo()
        {
            this.Invoke(new MyFuncDelegate(_ResetIDCardInfo));
        }

        private void DoShowIDCardInfo(object objData)
        {
            IDBaseTextDecoder idTextDecoder = (IDBaseTextDecoder) objData;
            this.idCardGifBox.Hide();
            this.idCardPicCtrl.Show();

            faceDetect.LoadIDPhoto(); //load id photo
            this.idCardPicCtrl.DrawIDInfo(idTextDecoder, faceDetect.GetIDPhoto());
        }

        public void ShowIDCardInfo(IDBaseTextDecoder idTextDecoder)
        {
            this.Invoke(new MyFuncDelegate1(DoShowIDCardInfo), idTextDecoder);
        }

        private void DoRefreshLiveCam(object sender, DoWorkEventArgs e)
        {
            //Bitmap bmLive = (Bitmap)objData;
            Bitmap bmLive = (Bitmap) e.Argument;
            if (bmLive == null)
            {
                return;
            }
            var rect = this.livePicCtrl.ClientRectangle;
            using (Graphics g = this.livePicCtrl.CreateGraphics())
            {
                g.DrawImage(bmLive, rect);
            }
            bmLive.Dispose();
        }

        public void RefreshLiveCam(Bitmap bitmap)
        {
            //this.Invoke(new MyFuncDelegate1(DoRefreshLiveCam), bitmap);
            var worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(DoRefreshLiveCam);
            worker.RunWorkerAsync(bitmap);
        }

        public Control GetLivePicCtrl()
        {
            return livePicCtrl;
        }

        private void DoShowTicketCam(object obj)
        {
            bool bShow = (bool) obj;
            tickGifBox.Visible = !bShow;
            tickPicCtrl.Visible = bShow;
        }

        public void ShowTicketCam(bool bShow)
        {
            this.Invoke(new MyFuncDelegate1(DoShowTicketCam), bShow);
        }

        private void DoCapTicket(object obj)
        {
            FaceDetect.DataTickCam data = (FaceDetect.DataTickCam) obj;
            IntPtr bmData = tickCamOper.Click();
            if (bmData == IntPtr.Zero)
            {
                return;
            }
            Bitmap b = new Bitmap(tickCamOper.Width, tickCamOper.Height, tickCamOper.Stride,
                                   PixelFormat.Format24bppRgb, bmData);
            data.bm = b;
        }
        public bool CapTicket(FaceDetect.DataTickCam data)
        {
            this.Invoke(new MyFuncDelegate1(DoCapTicket), data);
            return (data.bm != null);
        }
    }
}
