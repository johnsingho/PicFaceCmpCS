using System;

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
            this.tickPicCtrl = new CameraApp.PicCtrl();
            this.idCardPicCtrl = new CameraApp.PicCtrl();
            this.SuspendLayout();
            // 
            // lblInfo
            // 
            this.lblInfo.BackColor = System.Drawing.Color.Transparent;
            this.lblInfo.Font = new System.Drawing.Font("Microsoft YaHei", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblInfo.ForeColor = System.Drawing.Color.MediumBlue;
            this.lblInfo.Location = new System.Drawing.Point(345, 497);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(339, 189);
            this.lblInfo.TabIndex = 1;
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
            // tickPicCtrl
            // 
            this.tickPicCtrl.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.tickPicCtrl.Location = new System.Drawing.Point(732, 481);
            this.tickPicCtrl.Name = "tickPicCtrl";
            this.tickPicCtrl.Size = new System.Drawing.Size(260, 195);
            this.tickPicCtrl.TabIndex = 0;
            this.tickPicCtrl.Visible = false;
            // 
            // tickGifBox
            // 
            this.tickGifBox.BackColor = System.Drawing.Color.Transparent;
            this.tickGifBox.LoadGIFImage = global::CameraApp.Properties.Resources.PlaceTicket;
            this.tickGifBox.Location = this.tickPicCtrl.Location;
            this.tickGifBox.Name = "tickGifBox";
            this.tickGifBox.Size = this.tickPicCtrl.Size;
            this.tickGifBox.TabIndex = 3;
            // 
            // idCardPicCtrl
            // 
            this.idCardPicCtrl.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.idCardPicCtrl.Location = new System.Drawing.Point(67, 477);
            this.idCardPicCtrl.Name = "idCardPicCtrl";
            this.idCardPicCtrl.Size = new System.Drawing.Size(235, 167);
            this.idCardPicCtrl.TabIndex = 4;
            this.idCardPicCtrl.Visible = false;
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
            // MainWin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.BackgroundImage = global::CameraApp.Properties.Resources.bg;
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.ControlBox = false;
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
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
            this.ResumeLayout(false);

        }

        #endregion
        
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Button btnTestExit;
        private PicCtrl tickPicCtrl;
        private CNSpinner.ProgressSpinner tickGifBox;
        private PicCtrl idCardPicCtrl;
        private CNSpinner.ProgressSpinner idCardGifBox;
    }
}

