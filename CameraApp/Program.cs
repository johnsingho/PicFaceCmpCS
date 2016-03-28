using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CameraApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 不允许进程启动多次
            bool createdNew=false;
            Mutex m = new Mutex(true, "_FACECAMERA_SANWIK_HZX_2016_", out createdNew);
            if (createdNew)
            {
                Application.Run(new MainWin());
                m.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("人脸识别通关程序已经启动过，无法重复启动！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
