using System;
using System.Windows.Forms;

namespace CameraApp
{
    partial class MainWin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWin));
            this.lblInfo = new System.Windows.Forms.Label();
            this.btnTestExit = new System.Windows.Forms.Button();
            this.idCardGifBox = new CNSpinner.ProgressSpinner();
            this.tickGifBox = new CNSpinner.ProgressSpinner();
            this.lblTimer = new System.Windows.Forms.Label();
            this.livePicCtrl = new System.Windows.Forms.PictureBox();
            this.tickPicCtrl = new System.Windows.Forms.PictureBox();
            this.idCardPicCtrl = new CameraApp.IDCardPicCtrl();
            this.lblTestInfo = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.livePicCtrl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tickPicCtrl)).BeginInit();
            this.SuspendLayout();
            // 
            // lblInfo
            // 
            this.lblInfo.BackColor = System.Drawing.Color.Transparent;
            this.lblInfo.Font = new System.Drawing.Font("Microsoft YaHei", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblInfo.ForeColor = System.Drawing.Color.MediumBlue;
            this.lblInfo.Location = new System.Drawing.Point(345, 493);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(339, 190);
            this.lblInfo.TabIndex = 1;
            this.lblInfo.Text = "Initi初始化....";
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnTestExit
            // 
            this.btnTestExit.Location = new System.Drawing.Point(233, 12);
            this.btnTestExit.Name = "btnTestExit";
            this.btnTestExit.Size = new System.Drawing.Size(71, 26);
            this.btnTestExit.TabIndex = 2;
            this.btnTestExit.Text = "Test Exit";
            this.btnTestExit.UseVisualStyleBackColor = true;
            this.btnTestExit.Click += new System.EventHandler(this.btnTestExit_Click);
            // 
            // idCardGifBox
            // 
            this.idCardGifBox.BackColor = System.Drawing.Color.Transparent;
            this.idCardGifBox.LoadGIFImage = global::CameraApp.Properties.Resources.PlaceIDCard;
            this.idCardGifBox.Location = new System.Drawing.Point(34, 469);
            this.idCardGifBox.Name = "idCardGifBox";
            this.idCardGifBox.Size = new System.Drawing.Size(285, 257);
            this.idCardGifBox.TabIndex = 5;
            // 
            // tickGifBox
            // 
            this.tickGifBox.BackColor = System.Drawing.Color.Transparent;
            this.tickGifBox.LoadGIFImage = global::CameraApp.Properties.Resources.PlaceTicket;
            this.tickGifBox.Location = new System.Drawing.Point(732, 481);
            this.tickGifBox.Name = "tickGifBox";
            this.tickGifBox.Size = new System.Drawing.Size(260, 195);
            this.tickGifBox.TabIndex = 3;
            // 
            // lblTimer
            // 
            this.lblTimer.BackColor = System.Drawing.Color.Transparent;
            this.lblTimer.Font = new System.Drawing.Font("Microsoft YaHei", 13F);
            this.lblTimer.ForeColor = System.Drawing.SystemColors.Control;
            this.lblTimer.Location = new System.Drawing.Point(701, 9);
            this.lblTimer.Name = "lblTimer";
            this.lblTimer.Size = new System.Drawing.Size(311, 26);
            this.lblTimer.TabIndex = 7;
            this.lblTimer.Text = "2016年3月20日 星期日 09:40:40";
            // 
            // livePicCtrl
            // 
            this.livePicCtrl.BackColor = System.Drawing.Color.Black;
            this.livePicCtrl.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.livePicCtrl.Location = new System.Drawing.Point(305, 85);
            this.livePicCtrl.Name = "livePicCtrl";
            this.livePicCtrl.Size = new System.Drawing.Size(422, 338);
            this.livePicCtrl.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.livePicCtrl.TabIndex = 6;
            this.livePicCtrl.TabStop = false;
            // 
            // tickPicCtrl
            // 
            this.tickPicCtrl.BackColor = System.Drawing.Color.Black;
            this.tickPicCtrl.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.tickPicCtrl.Location = new System.Drawing.Point(732, 481);
            this.tickPicCtrl.Name = "tickPicCtrl";
            this.tickPicCtrl.Size = new System.Drawing.Size(260, 195);
            this.tickPicCtrl.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.tickPicCtrl.TabIndex = 0;
            this.tickPicCtrl.TabStop = false;
            this.tickPicCtrl.Visible = false;
            // 
            // idCardPicCtrl
            // 
            this.idCardPicCtrl.BackColor = System.Drawing.Color.Black;
            this.idCardPicCtrl.BackPic = global::CameraApp.Properties.Resources.IDCardBack;
            this.idCardPicCtrl.Location = new System.Drawing.Point(34, 477);
            this.idCardPicCtrl.Name = "idCardPicCtrl";
            this.idCardPicCtrl.Size = new System.Drawing.Size(268, 167);
            this.idCardPicCtrl.TabIndex = 4;
            this.idCardPicCtrl.TabStop = false;
            this.idCardPicCtrl.Visible = false;
            // 
            // lblTestInfo
            // 
            this.lblTestInfo.AutoSize = true;
            this.lblTestInfo.BackColor = System.Drawing.Color.Transparent;
            this.lblTestInfo.Font = new System.Drawing.Font("Microsoft YaHei", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblTestInfo.ForeColor = System.Drawing.Color.Yellow;
            this.lblTestInfo.Location = new System.Drawing.Point(312, 13);
            this.lblTestInfo.Name = "lblTestInfo";
            this.lblTestInfo.Size = new System.Drawing.Size(79, 20);
            this.lblTestInfo.TabIndex = 8;
            this.lblTestInfo.Text = "V2.0测试版";
            // 
            // MainWin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.BackgroundImage = global::CameraApp.Properties.Resources.bg;
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.ControlBox = false;
            this.Controls.Add(this.lblTestInfo);
            this.Controls.Add(this.lblTimer);
            this.Controls.Add(this.livePicCtrl);
            this.Controls.Add(this.btnTestExit);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.tickGifBox);
            this.Controls.Add(this.tickPicCtrl);
            this.Controls.Add(this.idCardGifBox);
            this.Controls.Add(this.idCardPicCtrl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "MainWin";
            this.Text = "MainWin";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
            ((System.ComponentModel.ISupportInitialize)(this.livePicCtrl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tickPicCtrl)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Button btnTestExit;
        private PictureBox tickPicCtrl;
        private CNSpinner.ProgressSpinner tickGifBox;
        private IDCardPicCtrl idCardPicCtrl;
        private CNSpinner.ProgressSpinner idCardGifBox;
        private PictureBox livePicCtrl;
        private System.Windows.Forms.Label lblTimer;
        private Label lblTestInfo;
    }
}

