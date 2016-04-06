using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using JohnKit;

namespace CameraApp
{
    public class ComIdCardReader : IDisposable
    {
        private SerialPort comPort = new SerialPort();
        private byte[] mRecvData = new byte[2048];

        private byte[] pucIIN = new byte[4];//证/卡芯片管理号
        private byte[] pucSN  = new byte[8];//证/卡芯片序列号
        private byte[] pucBaseText = new byte[260];//固定信息文本最多256个字节
        private byte[] pucPhoto = new byte[1024];  //固定信息照片最多1024个字节
        private byte[] pucExtra = new byte[72];    //追加信息最多70字节
        private int _pucBaseTextLen = 0;
        private int _pucPhotoLen = 0;
        private string strLastIDPhotoFile=string.Empty;  //最后一次成功解码的照片位置

        public ComIdCardReader()
        {
            WinCall.ZeroArr(mRecvData);
            comPort.BaudRate = 115200;
            comPort.DataBits = 8;
            comPort.StopBits = StopBits.One;
            comPort.Handshake = Handshake.None; //!
            comPort.Parity = Parity.None;
           
            comPort.ReadTimeout = 1000;
            comPort.WriteTimeout = 1000;
        }

        public void Dispose()
        {
            comPort.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 尝试打开串口
        /// </summary>
        /// <param name="idReaderCom"></param>
        /// <returns>
        /// -1 COM口不存在
        /// 0  COM口打开失败
        /// 1  COM口打开成功
        /// </returns>
        public int TryOpenCOM(int idReaderCom)
        {
            string strPort = string.Format("COM{0}", idReaderCom);
            comPort.PortName = strPort;
            try
            {
                comPort.Open();
            }
            catch (IOException)
            {
                return -1;
            }
            catch (Exception ex)
            {
                WinCall.TraceException(ex);
                return 0;
            }
            if(comPort.IsOpen)
            {
                if (SAM_GetState())
                {
                    return 1;
                }
                else
                {
                    comPort.Close();
                }
            }
            return 0;
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
        private int SendCommand(byte cmd, byte para, out byte sw1, out byte sw2, out byte sw3, int nTimeout)
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
            cSendData[7] = cmd;
            cSendData[8] = para;
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
            int nlen = ((int)mRecvData[5]<<8) | mRecvData[6];
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
            if (mRecvData[nrecv-1] != cks)
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

            int nGet = 0;
            int nToRead = nRead;
            try
            {   
                while (nGet < nRead)
                {
                    nGet += comPort.Read(mRecvData, iStart+nGet, nToRead);
                    nToRead = nRead - nGet;
                }
                
            }
            catch (System.Exception ex)
            {
                WinCall.TraceException("ComRead", ex);
            }
            return (nGet == nRead);
        }

        private bool ComSend(byte[] cSendData, int iStart, int n)
        {
            try
            {
                comPort.Write(cSendData, iStart, n);
            }
            catch (System.Exception ex)
            {
                WinCall.TraceException("ComSend", ex);
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
        
        //复位SAM_V 
        int SAM_Reset()
        {
            byte sw1, sw2, sw3;
            int ret;
            ret = SendCommand(0x10, 0xff, out sw1, out sw2, out sw3, 1000);
            if ((ret > 0) && (sw3 == 0x90))
                return 0;
            return ret;
        }

        //设置COM口波特率
        //默认是115200
        internal int SetBaudRate(int nBaud)
        {
            byte sw1, sw2, sw3;
            int ret;
            byte nRate = 0x00;
            switch (nBaud)
            {
                case 57600: nRate = 0x01; break;
                case 38400: nRate = 0x02; break;
                case 19200: nRate = 0x03; break;
                case 9600: nRate = 0x04; break;
                default: nRate = 0x00; break;
            }
            ret = SendCommand(0x60, nRate, out sw1, out sw2, out sw3, 1000);
            if ((ret > 0) && (sw3 == 0x90))
            {
                return 0;
            }
            return -1;
        }

        //寻找证/卡
        int SAM_FindCard()
        {
            byte sw1 = 0;
            byte sw2 = 0;
            byte sw3 = 0;
            int ret = 0;
            ret = SendCommand(0x20, 0x01, out sw1, out sw2, out sw3, 1000);
            if ((ret > 0) && (sw3 == 0x9f))
            {
                WinCall.CopyArr(pucIIN, 0, mRecvData, 10, 4);
                //memcpy(pucIIN, &mRecvData[10], 4);
                //printf("FindCard,sw:%d,%d,%d.\n",sw1,sw2,sw3);
                return 0;
            }
            return ret;
        }

        int SAM_SelectCard()
        {
            byte sw1 = 0;
            byte sw2 = 0;
            byte sw3 = 0;
            int ret = 0;
            //aa aa aa 96 69 00 03 20 01 22 ed
            ret = SendCommand(0x20, 0x02, out sw1, out sw2, out sw3, 1000);
            if ((ret > 0) && (sw3 == 0x90))
            {
                WinCall.CopyArr(pucSN, 0, mRecvData, 10, 8);
                return 0;
            }
            return ret;
        }


        //读固定信息 
        int SAM_ReadBaseMsg()
        {
            byte sw1, sw2, sw3;
            int ret = SendCommand(0x30, 0x01, out sw1, out sw2, out sw3, 3000);
            const int RET_HEAD = 10;

            if ((ret > 0) && (sw3 == 0x90))
            {
                //所有固定文本信息,其实都是256
                _pucBaseTextLen = (mRecvData[RET_HEAD] << 8) | mRecvData[RET_HEAD + 1];
                WinCall.CopyArr(pucBaseText, 0, mRecvData, RET_HEAD+4, _pucBaseTextLen);

                //读出的照片数据,其实都是1024
                _pucPhotoLen = (mRecvData[RET_HEAD + 2] << 8) | mRecvData[RET_HEAD + 3];
                WinCall.CopyArr(pucPhoto, 0, mRecvData, RET_HEAD+4+_pucBaseTextLen, _pucPhotoLen); //?

                return 0;
            }
            return -1;
        }

        public int ReadCard()
        {   
            ResetData();
            //comPort.DiscardInBuffer();

            int nRet = SAM_FindCard();
            if (nRet != 0)
                return nRet;
            nRet = SAM_SelectCard();
            if (nRet != 0)
            {
                return nRet;
            }
            nRet = SAM_ReadBaseMsg();
            return nRet;
        }


        int SAM_ReadExtraMsg()
        {
            byte sw1, sw2, sw3;
            int ret = SendCommand(0x30, 0x03, out sw1, out sw2, out sw3, 3000);
            const int RET_HEAD = 10;

            if ((ret > 0) && (sw3 == 0x90))
            {
                WinCall.CopyArr(pucExtra, 0, mRecvData, RET_HEAD, 70);
                return 0;
            }
            return ret;
        }

        void ResetData()
        {
            WinCall.ZeroArr(mRecvData);
            WinCall.ZeroArr(pucIIN);
            WinCall.ZeroArr(pucSN);
            WinCall.ZeroArr(pucBaseText);
            WinCall.ZeroArr(pucPhoto);
            WinCall.ZeroArr(pucExtra);
            WinCall.ZeroArr(mRecvData);

            //m_bLastRead = false;
            _pucBaseTextLen = 0;
            _pucPhotoLen = 0;
            strLastIDPhotoFile = string.Empty;
        }



        private static void PrintGetBmpError(int nRet)
        {
            string strRet = "ok";
            switch (nRet)
            {
                case 1:
                    strRet = "相片解码正确";
                    break;
                case 0:
                    strRet = "调用sdtapi.dll错误";
                    break;
                case -1:
                    strRet = "相片解码错误";
                    break;
                case -2:
                    strRet = "wlt文件后缀错误";
                    break;
                case -3:
                    strRet = "wlt文件打开错误";
                    break;
                case -4:
                    strRet = "wlt文件格式错误";
                    break;
                case -5:
                    strRet = "软件未授权";
                    break;
                case -6:
                    strRet = "设备连接错误";
                    break;
                default:
                    strRet = "unknown";
                    break;
            }

            WinCall.TraceMessage(string.Format("***GetBmp return,{0}", strRet));
        }

        
        [DllImport("WltRS.dll", EntryPoint = "GetBmp", CharSet = CharSet.Ansi)]
        public static extern int GetBmp([MarshalAs(UnmanagedType.LPStr)]string wltfile,
                                        [MarshalAs(UnmanagedType.U4)] int intf);


        //wlt file  ----> bmp file
        // 1成功
        // 0或负数失败
        static int wlt2bmp(string filename)
        {
            int ret = 0;
            ret=GetBmp(filename, 2);
            PrintGetBmpError(ret);
            return ret;
        }

        //取读取到的固定信息
        public byte[] GetBaseText() { return pucBaseText; }
        //取读取的图片数据
        public byte[] GetPhoto() { return pucPhoto; }

        public bool WritePhotoFile(string strOutDir, string strID)
        {
            string sOutFile = Path.Combine(Application.StartupPath, strOutDir);
            string strRela;
            if (string.IsNullOrEmpty(strID))
            {
                strRela="tempPic.wlt";
            }
            else
            {
                strRela=string.Format("{0}.wlt", strID);
            }

            Directory.CreateDirectory(sOutFile);
            sOutFile = Path.Combine(sOutFile, strRela);
            //File.Delete(sOutFile);

            byte[] pbyPhoto = GetPhoto();
            bool bRet = false;
            using (FileStream fileStream = File.Create(sOutFile))
            {
                fileStream.Write(pbyPhoto, 0, 1024);
                bRet = true;
            }
            if (bRet)
            {
                bRet = (1==wlt2bmp(sOutFile));
                strLastIDPhotoFile = string.Empty;
                if (bRet)
                {
                    strLastIDPhotoFile = sOutFile.Replace(".wlt", ".bmp");
                }
            }
            return bRet;
        }

        //获取最后一次成功解码的身份证照片路径
        public string GetLastIDPhotoFile()
        {
            return strLastIDPhotoFile;
        }
    }
}