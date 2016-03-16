﻿using System;
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

        delegate void SetInfoTextCallback(string text);


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

        private void btnTestExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        delegate void CreateTickCamOperDelegate(FaceDetect faceDetect);
        private void DoCreateTicketCamOper(FaceDetect faceDetect)
        {

            const int VIDEOWIDTH = 640;
            const int VIDEOHEIGHT = 480;
            const int VIDEOBITSPERPIXEL = 24; // BitsPerPixel values determined by device
            Capture tickCamOper = null;
            try
            {
                tickCamOper = new Capture(faceDetect.GetCamTicketID(), VIDEOWIDTH, VIDEOHEIGHT, VIDEOBITSPERPIXEL, picCtrlTicket);
            }
            catch (Exception ex)
            {
                WinCall.TraceException(ex);
            }
            faceDetect.SetTickCamOper(tickCamOper);
        }


        public void CreateTicketCamOper(FaceDetect faceDetect)
        {
            this.Invoke(new CreateTickCamOperDelegate(DoCreateTicketCamOper), faceDetect);
        }

    }
}
