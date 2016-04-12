using System;
using System.Diagnostics;
using System.IO.Ports;
using JohnKit;

namespace CameraApp
{
    /// <summary>
    /// 灯板，闸门控制
    /// </summary>
    internal class GateBoardOper
    {
        #region Command
        private static readonly ushort MAX_CMDLEN = 64;

        //命令
        private static readonly ushort LOCK_CMD_GATE = 0x0102;
        private static readonly ushort LOCK_CMD_LED  = 0x0107;

        //命令的参数
        private static readonly ushort CMDDATA_GATEON1 = 0x0003; /*右闸门，暂时没用 H.Z.XIN 2016-01-07*/
        private static readonly ushort CMDDATA_GATEON2 = 0x0004; /*左闸门*/
        private static readonly ushort CMDDATA_LED1ON  = 0x0003; /*摄像头红灯*/
        private static readonly ushort CMDDATA_LED1OFF = 0x0004;
        private static readonly ushort CMDDATA_LED2ON  = 0x0005;
        private static readonly ushort CMDDATA_LED2OFF = 0x0006;
        private static readonly ushort CMDDATA_LED3ON  = 0x0007;
        private static readonly ushort CMDDATA_LED3OFF = 0x0008;
        private static readonly ushort CMDDATA_LED4ON  = 0x0009;
        private static readonly ushort CMDDATA_LED4OFF = 0x000a;
        private static readonly ushort CMDDATA_LED5ON  = 0x000b;
        private static readonly ushort CMDDATA_LED5OFF = 0x000c;

        private static readonly ushort MS_WAIT = 2200;
        #endregion

        private SerialPort comPort = new SerialPort();
        private int mSendlen=0;
        private byte[] mSendbuf = new byte[MAX_CMDLEN];
        private int mRecvlen=0;
        private byte[] mRecvbuf = new byte[MAX_CMDLEN];

        internal GateBoardOper()
        {
        }

        internal bool TryOpenCOM(int gateBoardCom)
        {
            comPort.BaudRate = 9600;
            comPort.DataBits = 8;
            comPort.StopBits = StopBits.One;
            comPort.Handshake = Handshake.None; //!
            comPort.Parity = Parity.None;
            comPort.ReadTimeout = 800;
            comPort.WriteTimeout = 800;

            string strPort = string.Format("com{0}", gateBoardCom);
            comPort.PortName = strPort;
            try
            {
                comPort.Open();
            }
            catch (Exception ex)
            {
                WinCall.TraceException(ex);
            }
            return comPort.IsOpen;
        }


        private static byte CalcCrc(byte[] p, int iStart, int n)
        {
            byte c = 0;
            for (int i = 0; i < n; i++)
            {
                c ^= p[iStart+i];
            }   
            return c;
        }


        private int MakeCommand(ushort Cmd, ushort CmdData)
        {
            byte pos = 0;
            byte dlen = 2 + 2;

            mSendlen = 0;

            mSendbuf[pos++] = 0xAA;
            mSendbuf[pos++] = (byte) (dlen >> 8);
            mSendbuf[pos++] = (byte) (dlen & 0xff);
            mSendbuf[pos++] = (byte) (Cmd >> 8);
            mSendbuf[pos++] = (byte) (Cmd & 0xff);
            mSendbuf[pos++] = (byte) (CmdData >> 8);
            mSendbuf[pos++] = (byte) (CmdData & 0xff);
            mSendbuf[pos++] = CalcCrc(mSendbuf, 3, dlen);
            mSendbuf[pos++] = 0x55;
            mSendlen = pos;
            return mSendlen;
        }

        private bool ComSend(byte[] cSendData, int iStart, int n)
        {
            try
            {
                comPort.Write(cSendData, iStart, n);
            }
            catch (System.Exception)
            {
                return false;
            }
            return true;
        }

        private int SendCommand(ushort Cmd, ushort CmdData)
        {
            int ret = SendCommandNoReturn(Cmd, CmdData);
            if (ret < 0)
                return ret;

            ret = RecvPackage(Cmd, CmdData);

            if (ret >= 0)
                return ret;
            else if (ret == -1)
                return ret;

            return ret;
        }


        private bool ComRead(int iStart, int nRead, int nTimeout)
        {
            comPort.ReadTimeout = nTimeout;

            int nGet = 0;
            int nToRead = nRead;
            try
            {
                while (nGet < nRead)
                {
                    nGet += comPort.Read(mRecvbuf, iStart + nGet, nToRead);
                    nToRead = nRead - nGet;
                }

            }
            catch (System.Exception ex)
            {
                WinCall.TraceException("GateBoardOper.ComRead", ex);
            }
            return (nGet == nRead);
        }

        //返回0成功
        //-1 失败
        //-2 可以重试
        private int RecvPackage(ushort Cmd, ushort CmdData)
        {
            int len, packlen;
            byte crc=0;
            int repcmd, repdat;

            if (!ComRead(0, 3, MS_WAIT))
            {
                return -1;
            }

            len = (mRecvbuf[1] << 8) + mRecvbuf[2];
            if (len + 4 > MAX_CMDLEN)
                return -2;

            int nDat = len + 4 - 2;
            if (!ComRead(3, nDat, MS_WAIT))
            {
                return -2;
            }

            repcmd = (mRecvbuf[3] << 8) | mRecvbuf[4];
            repdat = (mRecvbuf[5] << 8) | mRecvbuf[6];

            if (repcmd != Cmd)
                return -2;
            
            packlen = nDat + 3;  //整包数据
            crc = CalcCrc(mRecvbuf, 3, packlen-5);
            if (mRecvbuf[packlen - 2] != crc)
            {
                WinCall.TraceMessage("***RecvPack,BAD CRC.");
                return -2;
            }

            mRecvlen = packlen;
            return 0;
        }
        

        // 发送命令，不读取返回值
        // 0表示已经发出
        private int SendCommandNoReturn(ushort Cmd, ushort CmdData)
        {
            int ret = 0;
            ret = MakeCommand(Cmd, CmdData);
            if (ret < 0)
                return ret;
            if (!ComSend(mSendbuf, 0, mSendlen))
            {
                return -1;
            }
            return ret;
        }

        private static readonly ushort[] g_ArrLightCmdOpen = new ushort[]{
            CMDDATA_LED1ON,
            CMDDATA_LED2ON,
            CMDDATA_LED3ON,
            CMDDATA_LED4ON,
            CMDDATA_LED5ON
        };


        private static readonly ushort[] g_ArrLightCmdClose = new ushort[]{
            CMDDATA_LED1OFF,
            CMDDATA_LED2OFF,
            CMDDATA_LED3OFF,
            CMDDATA_LED4OFF,
            CMDDATA_LED5OFF
        };

        private static readonly ushort[] g_ArrGeteOpen = new ushort[]{
            CMDDATA_GATEON1,
            CMDDATA_GATEON2
        };

        internal bool SwitchLight(int iLight, bool bOpen)
        {
            bool bOk = false;
            int nMax = g_ArrLightCmdOpen.Length;
            if (0 > iLight || iLight > nMax) { return false; }

            ushort sData = bOpen ? g_ArrLightCmdOpen[iLight] : g_ArrLightCmdClose[iLight];
            bOk = SendCommand(LOCK_CMD_LED, sData) == 0;
            if (!bOk)
            {
                string str = string.Format("***获取命令反馈失败: cmd={0}, data={1}", LOCK_CMD_LED, sData);
                WinCall.TraceMessage(str);
            }
            return bOk;
        }

        internal bool TurnoffAllLight()
        {
            foreach (ushort cmd in g_ArrLightCmdClose)
            {
                SendCommandNoReturn(LOCK_CMD_LED, cmd);
            }
            return true;
        }

        // iGate=0右闸门
        // iGate=1左闸门
        internal bool OpenGate(int iGate)
        {
            bool bOk = false;
            int nMax = g_ArrGeteOpen.Length;
            if (0 > iGate || iGate > nMax) { return false; }

            ushort sData = g_ArrGeteOpen[iGate];
            bOk = SendCommand(LOCK_CMD_GATE, g_ArrGeteOpen[iGate]) == 0;
            if (!bOk)
            {
                string str = string.Format("***发送闸门命令失败: cmd={0}, data={1}", LOCK_CMD_GATE, sData);
                WinCall.TraceMessage(str);
            }
            return bOk;
        }
        
    }
}