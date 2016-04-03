using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CameraApp
{
    partial class IDCardPicCtrl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // IDCardPicCtrl
            // 
            this.AutoScaleDimensions = new SizeF(6F, 12F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.Black;
            this.Name = "IDCardPicCtrl";
            this.Size = new Size(270, 180);
            this.Resize += new EventHandler(this.OnResize);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
