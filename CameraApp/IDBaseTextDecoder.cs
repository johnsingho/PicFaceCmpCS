using System;

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

        IDBaseTextDecoder()
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

        bool Decode(byte[] pBaseText)
        {
            if (pBaseText == null || pBaseText.Length == 0)
            {
                return false;
            }

            #region �����ı�
            int iPos = 0;
            int iLen = 30;
            m_strName = BitConverter.ToString(pBaseText, iPos, iLen);
            iPos += iLen;
            iLen = 2;
            short nSex = BitConverter.ToInt16(pBaseText, iPos);
            switch (nSex)
            {
                case 1: m_strSex="��"; break;
                case 2: m_strSex = "Ů"; break;
                case 9:
                default:m_strSex="����"; break;
            }
            iPos += iLen;
            iLen = 4;
            //����
            int nEthnic = BitConverter.ToInt32(pBaseText, iPos);
            m_strEthnic = DecodeEthnic(nEthnic);
            iPos += iLen;
            iLen = 16;
            //��������
            m_strBirthDay = BitConverter.ToString(pBaseText, iPos, iLen);
            iPos += iLen;
            iLen = 70;
            //��ַ
            m_strAddress = BitConverter.ToString(pBaseText, iPos, iLen);
            iPos += iLen;
            iLen = 36;
            //���֤��
            m_strID = BitConverter.ToString(pBaseText, iPos, iLen);
            iPos += iLen;
            iLen = 30;
            //ǩ����λ
            m_strDepartment = BitConverter.ToString(pBaseText, iPos, iLen);
            iPos += iLen;
            iLen = 16;
            //��Ч����ʼ
            m_strExpireBegin = BitConverter.ToString(pBaseText, iPos, iLen);
            iPos += iLen;
            iLen = 16;
            //��Ч�ڽ���
            m_strExpireEnd = BitConverter.ToString(pBaseText, iPos, iLen);

            iPos += iLen;
            iLen = 36;
            //׷����Ϣ
            m_strAddition = BitConverter.ToString(pBaseText, iPos, iLen);
            #endregion

            return true;
        }

    
    private static string DecodeEthnic(int nEthnic)
    {
        switch (nEthnic)
        {
            case 1: return "����";
            case 2: return "�ɹ���";
            case 3: return "����";
            case 4: return "����";
            case 5: return "ά�����";
            case 6: return "����";
            case 7: return "����";
            case 8: return "׳��";
            case 9: return "������";
            case 10: return "������";
            case 11: return "����";
            case 12: return "����";
            case 13: return "����";
            case 14: return "����";
            case 15: return "������";
            case 16: return "������";
            case 17: return "��������";
            case 18: return "����";
            case 19: return "����";
            case 20: return "������";
            case 21: return "����";
            case 22: return "���";
            case 23: return "��ɽ��";
            case 24: return "������";
            case 25: return "ˮ��";
            case 26: return "������";
            case 27: return "������";
            case 28: return "������";
            case 29: return "�¶�������";
            case 30: return "����";
            case 31: return "���Ӷ���";
            case 32: return "������";
            case 33: return "Ǽ��";
            case 34: return "������";
            case 35: return "������";
            case 36: return "ë����";
            case 37: return "������";
            case 38: return "������";
            case 39: return "������";
            case 40: return "������";
            case 41: return "��������";
            case 42: return "ŭ��";
            case 43: return "���α����";
            case 44: return "����˹��";
            case 45: return "���¿���";
            case 46: return "������";
            case 47: return "������";
            case 48: return "ԣ����";
            case 49: return "����";
            case 50: return "��������";
            case 51: return "������";
            case 52: return "���״���";
            case 53: return "������";
            case 54: return "�Ű���";
            case 55: return "�����";
            case 56: return "��ŵ��";
            case 97: return "����";
            case 98: return "���Ѫͳ";
            default: return "����";
        }
        return "����";
    }

     
    }
}