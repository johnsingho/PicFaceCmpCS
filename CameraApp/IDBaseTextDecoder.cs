using System;
using System.Text;

namespace CameraApp
{
    public class IDBaseTextDecoder
    {
        #region Fields
        //private byte[] m_pBaseText;
        public string m_strName;
        public string m_strSex;
        public string m_strEthnic;
        public string m_strBirthDay;
        public string m_strAddress;
        public string m_strID;
        public string m_strDepartment;
        public string m_strExpireBegin;
        public string m_strExpireEnd;
        public string m_strAddition;        
        #endregion
        public ComIdCardReader IdCardReader { get; set; }

        public IDBaseTextDecoder()
        {
            Reset();
        }

        private void Reset()
        {
            m_strName=string.Empty;
            m_strSex = string.Empty;
            m_strEthnic = string.Empty;
            m_strBirthDay = string.Empty;
            m_strAddress = string.Empty;
            m_strID = string.Empty;
            m_strDepartment = string.Empty;
            m_strExpireBegin = string.Empty;
            m_strExpireEnd = string.Empty;
            m_strAddition = string.Empty;
        }

        public bool Decode()
        {
            byte[] pBaseText = IdCardReader.GetBaseText();
            if (pBaseText == null || pBaseText.Length == 0)
            {
                return false;
            }

            #region 解析文本
            int iPos = 0;
            int iLen = 30;
            m_strName = DecodeStr(pBaseText, iPos, iLen);
            iPos += iLen;
            iLen = 2;
            short nSex = Int16.Parse(DecodeStr(pBaseText, iPos, iLen));
            switch (nSex)
            {
                case 1: m_strSex = "男"; break;
                case 2: m_strSex = "女"; break;
                case 9:
                default: m_strSex = "其他"; break;
            }
            iPos += iLen;
            iLen = 4;
            //民族
            int nEthnic = Int32.Parse(DecodeStr(pBaseText, iPos, iLen));
            m_strEthnic = DecodeEthnic(nEthnic);
            iPos += iLen;
            iLen = 16;
            //出生日期
            m_strBirthDay = DecodeStr(pBaseText, iPos, iLen);
            iPos += iLen;
            iLen = 70;
            //地址
            m_strAddress = DecodeStr(pBaseText, iPos, iLen);
            iPos += iLen;
            iLen = 36;
            //身份证号
            m_strID = DecodeStr(pBaseText, iPos, iLen);
            iPos += iLen;
            iLen = 30;
            //签发单位
            m_strDepartment = DecodeStr(pBaseText, iPos, iLen);
            iPos += iLen;
            iLen = 16;
            //有效期起始
            m_strExpireBegin = DecodeStr(pBaseText, iPos, iLen);
            iPos += iLen;
            iLen = 16;
            //有效期结束
            m_strExpireEnd = DecodeStr(pBaseText, iPos, iLen);

            iPos += iLen;
            iLen = 36;
            //追加信息
            m_strAddition = DecodeStr(pBaseText, iPos, iLen);
            #endregion

            return true;
        }
        
        private static string DecodeStr(byte[] pBaseText, int iPos, int iLen)
        {
            //m_strName = BitConverter.ToString(pBaseText, iPos, iLen);
            string str = Encoding.Unicode.GetString(pBaseText, iPos, iLen);
            str = str.TrimEnd(new char[] { ' ', '\t' });
            return str;
        }

        private static string DecodeEthnic(int nEthnic)
        {
            switch (nEthnic)
            {
                case 1: return "汉族";
                case 2: return "蒙古族";
                case 3: return "回族";
                case 4: return "藏族";
                case 5: return "维吾尔族";
                case 6: return "苗族";
                case 7: return "彝族";
                case 8: return "壮族";
                case 9: return "布依族";
                case 10: return "朝鲜族";
                case 11: return "满族";
                case 12: return "侗族";
                case 13: return "瑶族";
                case 14: return "白族";
                case 15: return "土家族";
                case 16: return "哈尼族";
                case 17: return "哈萨克族";
                case 18: return "傣族";
                case 19: return "黎族";
                case 20: return "傈僳族";
                case 21: return "佤族";
                case 22: return "畲族";
                case 23: return "高山族";
                case 24: return "拉祜族";
                case 25: return "水族";
                case 26: return "东乡族";
                case 27: return "纳西族";
                case 28: return "景颇族";
                case 29: return "柯尔克孜族";
                case 30: return "土族";
                case 31: return "达斡尔族";
                case 32: return "仫佬族";
                case 33: return "羌族";
                case 34: return "布朗族";
                case 35: return "撒拉族";
                case 36: return "毛难族";
                case 37: return "仡佬族";
                case 38: return "锡伯族";
                case 39: return "阿昌族";
                case 40: return "普米族";
                case 41: return "塔吉克族";
                case 42: return "怒族";
                case 43: return "乌孜别克族";
                case 44: return "俄罗斯族";
                case 45: return "鄂温克族";
                case 46: return "崩龙族";
                case 47: return "保安族";
                case 48: return "裕固族";
                case 49: return "京族";
                case 50: return "塔塔尔族";
                case 51: return "独龙族";
                case 52: return "鄂伦春族";
                case 53: return "赫哲族";
                case 54: return "门巴族";
                case 55: return "珞巴族";
                case 56: return "基诺族";
                case 97: return "其他";
                case 98: return "外国血统";
                default: return "其他";
            }
            return "其他";
        }
        
    }
}