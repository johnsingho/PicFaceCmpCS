using System;
using System.IO.Ports;
using JohnKit;

namespace CameraApp
{
    internal class ComIdCardReader : IDisposable
    {
        private SerialPort comPort = new SerialPort();
        private byte[] mRecvData = new byte[2048];

        public ComIdCardReader()
        {
            WinCall.ZeroArr(mRecvData);
            comPort.BaudRate = 115200;
            comPort.DataBits = 8;
            comPort.StopBits = StopBits.One;
            comPort.Handshake = Handshake.None; //!
            
            comPort.ReadTimeout = 500;
            comPort.WriteTimeout = 500;
        }
        public bool TryOpenCOM(int idReaderCom)
        {
            string strPort = string.Format("com{0}", idReaderCom);
            comPort.PortName = strPort;
            bool bRet = false;
            try
            {
                comPort.Open();
            }
            catch
            {
            }
            if(comPort.IsOpen)
            {
                if (SAM_GetState())
                {
                    return true;
                }
                else
                {
                    comPort.Close();
                }
            }
            return false;
        }

        private bool SAM_GetState()
        {
            byte sw1=0;
            byte sw2=0;
            byte sw3=0;
            int ret = SendCommand(0x11, 0xff, out sw1, out sw2, out sw3, 1000);
            if ((ret > 0) && (sw3 == 0x90))
                return true;
            return false;
        }

        /// <summary>
        /// 返回>0成功，从串口接收结果放 mRecvData
        /// mRecvData的前十个字节是固定的状态返回
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="para"></param>
        /// <param name="sw1"></param>
        /// <param name="sw2"></param>
        /// <param name="sw3"></param>
        /// <param name="nTimeout"></param>
        /// <returns></returns>
        private int SendCommand(int cmd, int para, out byte sw1, out byte sw2, out byte sw3, int nTimeout)
        {
            byte[] cSendData = new byte[32];
            byte sendlen = 3;  //sendlen固定为3,cmd+para+chksum

            sw1 = 0;
            sw2 = 0;
            sw3 = 0;
            cSendData[0] = 0XAA;
            cSendData[1] = 0XAA;
            cSendData[2] = 0XAA;
            cSendData[3] = 0X96;
            cSendData[4] = 0X69;
            cSendData[5] = 0;
            cSendData[6] = sendlen;
            cSendData[7] = (byte) cmd;
            cSendData[8] = (byte) para;
            cSendData[9] = SumCheck(cSendData, 5, 4);

            if (!ComSend(cSendData, 0, 10))
            {
                return -1;
            }

            if (!ComRead(0, 7, nTimeout))
            {
                return -1;
            }

            //数据头5字节，数据长度2字节
            int nlen = (mRecvData[5] << 8) | mRecvData[6];
            //数据长度不可能小于3
            if (nlen < 3)
            {   
                return -1;
            }

            if (!ComRead(7, nlen, 2000))
            {
                return -1;
            }

            int nrecv = 7 + nlen;
            byte cks = SumCheck(mRecvData, 5, nlen+2-1); //2位长度要算，但最后1位不算
            if (mRecvData[nrecv - 1] != cks)
            {
                return -1;
            }

            sw1 = mRecvData[7];
            sw2 = mRecvData[8];
            sw3 = mRecvData[9];

            return nrecv;
        }

        private bool ComRead(int iStart, int nRead, int nTimeout)
        {
            comPort.ReadTimeout = nTimeout;
            int nRet = 0;
            try
            {
                nRet=comPort.Read(mRecvData, iStart, nRead);
            }
            catch (System.Exception)
            {
            }
            return (nRet == nRead);
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

        private static byte SumCheck(byte[] byArr, int iStart, int n)
        {
            byte bRet = 0;
            for(int i = 0; i < n; i++)
            {
                bRet = (byte) (bRet ^ byArr[iStart+i]);
            }
            return bRet;
        }

        public void Dispose()
        {
            comPort.Dispose();
        }
    }
}