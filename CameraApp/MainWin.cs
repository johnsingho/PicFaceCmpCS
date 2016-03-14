using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JohnKit;

namespace CameraApp
{
    public partial class MainWin : Form
    {
        private FaceDetect faceDetect=new FaceDetect();

        delegate void SetInfoTextCallback(string text);


        public MainWin()
        {
            InitializeComponent();
            LoadConfigInfo();
            InitVars();
            InitEnv();
        }
        private void InitEnv()
        {
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
            faceDetect.BindForm(this);
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
        public void PromptInfo(string strInfo)
        {
            if (this.lblInfo.InvokeRequired)
            {
                SetInfoTextCallback d = new SetInfoTextCallback(PromptInfo);
                this.Invoke(d, new object[]{ strInfo});
            }
            else
            {
                this.lblInfo.Text = strInfo;
            }
        }

    }
}
