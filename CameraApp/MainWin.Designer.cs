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
            this.picCtrlTicket = new CameraApp.PicCtrl();
            this.lblInfo = new System.Windows.Forms.Label();
            this.btnTestExit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // picCtrlTicket
            // 
            this.picCtrlTicket.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.picCtrlTicket.Location = new System.Drawing.Point(732, 481);
            this.picCtrlTicket.Name = "picCtrlTicket";
            this.picCtrlTicket.Size = new System.Drawing.Size(260, 195);
            this.picCtrlTicket.TabIndex = 0;
            // 
            // lblInfo
            // 
            this.lblInfo.BackColor = System.Drawing.Color.Transparent;
            this.lblInfo.Font = new System.Drawing.Font("Microsoft YaHei", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblInfo.ForeColor = System.Drawing.Color.MediumBlue;
            this.lblInfo.Location = new System.Drawing.Point(331, 481);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(358, 225);
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
            this.Controls.Add(this.picCtrlTicket);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "MainWin";
            this.ShowIcon = false;
            this.Text = "MainWin";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
            this.ResumeLayout(false);

        }

        #endregion

        private PicCtrl picCtrlTicket;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Button btnTestExit;

    }
}

