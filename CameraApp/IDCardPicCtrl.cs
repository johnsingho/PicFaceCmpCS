using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using CameraApp.Properties;

namespace CameraApp
{
    /// <summary>
    /// 用来显示身份证
    /// </summary>
    public partial class IDCardPicCtrl : UserControl
    {
        private Bitmap bmBack;
        //private int nX, nY;
        private int nW,nH;
        private IDBaseTextDecoder idTextDecoder;
        private Bitmap bmIDPhoto;
        private readonly Font fontPrompt = new Font("微软雅黑", 10);
        private readonly Font fontIDNum  = new Font("Arial Black", 12);
        private readonly Brush brushPrompt = new SolidBrush(Color.Blue);
        private readonly Brush brushValue = new SolidBrush(Color.Black);

        public IDCardPicCtrl()
        {
            InitializeComponent();

            nW = this.Width;
            nH = this.Height;

            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (BackPic != null)
            {
                e.Graphics.DrawImage(BackPic, e.ClipRectangle);
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            nW = this.Width;
            nH = this.Height;
        }

        public Bitmap BackPic
        {
            get { return bmBack; }
            set
            {
                if (bmBack != null)
                {
                    bmBack.Dispose();
                }
                bmBack = value;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            if (idTextDecoder != null)
            {
                DoDrawIDInfo(e.Graphics);
            }
        }

        private void DoDrawIDInfo(Graphics g)
        {
            Rectangle rectPrompt = new Rectangle(1, 20, 100, 30);
            Rectangle rectText = Rectangle.Inflate(rectPrompt, 50, 0);
            const int nXoff = 66;
            const int nYoff = 30;
            const int nYoffMin = 17;
            const int nIDPicHei = 126;
            const int nIDPicWid = 102;
            rectText.Offset(50+nXoff, 0);

            DrawOffset(g, "姓名：", fontPrompt, brushPrompt, ref rectPrompt, nYoff);
            DrawOffset(g, idTextDecoder.m_strName, fontPrompt, brushValue, ref rectText, nYoff);
            DrawOffset(g, "性别：", fontPrompt, brushPrompt, ref rectPrompt, nYoff);
            DrawOffset(g, idTextDecoder.m_strSex, fontPrompt, brushValue, ref rectText, nYoff);

            DrawOffset(g, "有效期限：", fontPrompt, brushPrompt, ref rectPrompt, nYoffMin);
            DrawOffset(g, idTextDecoder.m_strExpireBegin, fontPrompt, brushValue, ref rectText, nYoffMin);
            DrawOffset(g, "         ", fontPrompt, brushPrompt, ref rectPrompt, nYoff + 5);
            DrawOffset(g, idTextDecoder.m_strExpireEnd, fontPrompt, brushValue, ref rectText, nYoff + 5);
            DrawOffset(g, "身份证号：", fontPrompt, brushPrompt, ref rectPrompt, nYoff);
            DrawOffset(g, MaskID(idTextDecoder.m_strID), fontIDNum, brushValue, ref rectText, nYoff);

            if (bmIDPhoto != null)
            {
                g.DrawImage(bmIDPhoto, new Rectangle(nW - nIDPicWid-2, 4, nIDPicWid, nIDPicHei));
            }
        }


        private static void DrawOffset(Graphics g, string str, Font font, Brush clrBrush, ref Rectangle rectPrompt, int nYoff)
        {
            g.DrawString(str, font, clrBrush, rectPrompt);
            rectPrompt.Offset(0, nYoff);
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

        public void DrawIDInfo(IDBaseTextDecoder idTextDecoder, Bitmap idPhoto)
        {
            this.idTextDecoder = idTextDecoder;
            this.bmIDPhoto = idPhoto;
            this.Invalidate();
        }
    }
}
