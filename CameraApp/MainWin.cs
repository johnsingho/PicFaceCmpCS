using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        private FaceDetect faceDetect=new FaceDetect();

        private static readonly Color DefPromptClr = Color.Blue;

        delegate void SetInfoTextCallback(string text, int nFontHeight);


        public MainWin()
        {
            InitializeComponent();
            LoadConfigInfo();
            InitVars();
            InitEnv();

#if !DEBUG
            //仅用于调试
            btnTestExit.Visible = false;
#endif
        }
        private void InitEnv()
        {
            faceDetect.BindForm(this);
            faceDetect.InitEnv();
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            //只有调试的时候才允许拖动
#if DEBUG
            if (e.Button == MouseButtons.Left)
            {
                WinCall.ReleaseCapture();
                WinCall.SendMessage(Handle, WinCall.WM_NCLBUTTONDOWN, WinCall.HT_CAPTION, 0);
            }
#endif
        }

        private bool LoadConfigInfo()
        {
            return faceDetect.LoadConfigInfo();
        }

        private bool InitVars()
        {
            PromptInfo("系统正在初始化，请稍候...");
            return false;
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            faceDetect.DoExit();
        }

        /// <summary>
        /// 信息提示
        /// </summary>
        /// <param name="strInfo"></param>
        public void PromptInfo(string strInfo, int nFontHeight=18)
        {
            PromptInfo(strInfo, DefPromptClr, nFontHeight);
        }
        public void PromptError(string strErr, int nFontHeight = 18)
        {
            PromptInfo(strErr, Color.Red, nFontHeight);
        }
        public void PromptInfo(string strInfo, Color clrText, int nFontHeight=18)
        {
            if (this.lblInfo.InvokeRequired)
            {
                SetInfoTextCallback d = new SetInfoTextCallback(PromptInfo);
                this.Invoke(d, new object[]{ strInfo, nFontHeight });
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
            FaceDetect faceDetect = (FaceDetect) objData;
            const int VIDEOWIDTH = 640;
            const int VIDEOHEIGHT = 480;
            const int VIDEOBITSPERPIXEL = 24; // BitsPerPixel values determined by device
            Capture tickCamOper = null;
            try
            {
                tickCamOper = new Capture(faceDetect.GetCamTicketID(), VIDEOWIDTH, VIDEOHEIGHT, VIDEOBITSPERPIXEL, tickPicCtrl);
            }
            catch (Exception ex)
            {
                WinCall.TraceException(ex);
            }
            if (tickCamOper == null)
            {
                PromptInfo("车票摄像头初始化失败！", Color.Red);
            }
            faceDetect.SetTickCamOper(tickCamOper);
        }


        public void CreateTicketCamOper(FaceDetect faceDetect)
        {
            this.Invoke(new MyFuncDelegate1(DoCreateTicketCamOper), faceDetect);
        }

        //通用无参数delegate
        delegate void MyFuncDelegate();

        private void _ResetIDCardInfo()
        {
            this.idCardGifBox.Show();
            this.idCardPicCtrl.Hide();
        }
        public void ResetIDCardInfo()
        {
            this.Invoke(new MyFuncDelegate(_ResetIDCardInfo));
        }

        private void DoShowIDCardInfo(object objData)
        {
            IDBaseTextDecoder idTextDecoder = (IDBaseTextDecoder)objData;
            this.idCardGifBox.Hide();
            this.idCardPicCtrl.Show();

            Font fontPrompt = new Font("微软雅黑", 10);
            Brush brushPrompt = new SolidBrush(Color.Blue);
            Brush brushValue = new SolidBrush(Color.Black);
            Rectangle rectPrompt = new Rectangle(5, 5, 100, 40);
            Rectangle rectText = Rectangle.Inflate(rectPrompt,0, 0);
            const int nXoff = 50;
            const int nYoff = 45;
            const int nIDPicHei = 126;
            const int nIDPicWid = 102;
            rectText.Offset(nXoff, 0);

            using (Graphics g = this.idCardPicCtrl.CreateGraphics())
            {
                g.DrawImage(global::CameraApp.Properties.Resources.IDCardBack,
                    new Rectangle(0, 0, idCardPicCtrl.Width, idCardPicCtrl.Height));

                DrawOffset(g, "姓名：", fontPrompt, brushPrompt, ref rectPrompt, nYoff);
                DrawOffset(g, idTextDecoder.m_strName, fontPrompt, brushValue, ref rectText, nYoff);
                DrawOffset(g, "性别：", fontPrompt, brushPrompt, ref rectPrompt, nYoff);
                DrawOffset(g, idTextDecoder.m_strSex, fontPrompt, brushValue, ref rectText, nYoff);
                DrawOffset(g, "身份证号：", fontPrompt, brushPrompt, ref rectPrompt, nYoff);
                DrawOffset(g, MaskID(idTextDecoder.m_strID), fontPrompt, brushValue, ref rectText, nYoff);

                string strExpr = string.Format("{0} - {1}", idTextDecoder.m_strExpireBegin, idTextDecoder.m_strExpireEnd);
                DrawOffset(g, "有效期限：", fontPrompt, brushPrompt, ref rectPrompt, nYoff);
                DrawOffset(g, strExpr, fontPrompt, brushValue, ref rectText, nYoff);
                                
                faceDetect.LoadIDPhoto();
                //画身份证照
                DrawIDPic(g, faceDetect.GetIDPhoto(), idCardPicCtrl.Width-nIDPicWid, 5, nIDPicWid, nIDPicHei);
            }

            fontPrompt.Dispose();
            brushPrompt.Dispose();
            brushValue.Dispose();
        }

        private static void DrawIDPic(Graphics g, Bitmap bitmap, int x, int y, int nIDPicWid, int nIDPicHei)
        {
            if (bitmap != null) { return; }
            g.DrawImage(bitmap, new Rectangle(x, y, nIDPicWid, nIDPicHei));
        }

        private static string MaskID(string m_strID)
        {
            StringBuilder sb = new StringBuilder(m_strID);
            for (int i = 0; i < 4; i++)
            {
                sb[i + 10] = '*';
            }            
            return sb.ToString(0, sb.Length);
        }

        private static void DrawOffset(Graphics g, string str, Font font, Brush clrBrush, ref Rectangle rectPrompt, int nYoff)
        {
            g.DrawString(str, font, clrBrush, rectPrompt);
            rectPrompt.Offset(0, nYoff);
        }

        public void ShowIDCardInfo(IDBaseTextDecoder idTextDecoder)
        {
            this.Invoke(new MyFuncDelegate1(DoShowIDCardInfo), idTextDecoder);
        }
    }
}
