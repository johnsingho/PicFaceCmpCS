using System;

namespace CameraApp.Exam
{
    class HandlerTicketCheck : HandlerBase
    {
        public override void Do(object pData)
        {
            FaceDetect detector = (FaceDetect)pData;
            detector.SwitchTicketCam(true);
            
            bool bCmp = false;
            string strQrCode="";
            for (int i=0; i<detector.GetMaxTicketChkTimes(); i++)
            {
                bCmp = detector.DoCheckTicket(ref strQrCode);
                if (bCmp) { break; }
                JobManager.Sleep(JobManager.IDLE_WAIT_MS * 2);
            }
            detector.SwitchTicketCam(false);
            
            if (bCmp)
            {
                string str = string.Format("二维码内容: {0}", strQrCode);
                detector.PromptInfo(str);
                JobManager.Sleep(200); //! for demo only
                GetMgr().disPatch(JobManager.sHandlerOpenGate, pData);
            }
            else {
                detector.PromptError("车票验证不通过！\n请走人工验票通道。");
                detector.PlayVoice(ConstValue.VOICE_FAIL_TICKCHK, true);
                GetMgr().disPatch(JobManager.sHandlerException, pData);
            }
        }
    }
}