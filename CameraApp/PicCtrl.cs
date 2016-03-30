using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using JohnKit;

namespace CameraApp
{
    public partial class PicCtrl : PictureBox
    {
        public PicCtrl()
        {
            InitializeComponent();
            SetInfoCtrl();
        }

        private void SetInfoCtrl()
        {
            this.lblInfo.Visible = false;
        }
    }
}
