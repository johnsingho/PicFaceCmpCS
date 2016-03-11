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

        public MainWin()
        {
            InitializeComponent();
            LoadConfigInfo();
            InitVars();
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
            //throw new NotImplementedException();
            return false;
        }

    }
}
