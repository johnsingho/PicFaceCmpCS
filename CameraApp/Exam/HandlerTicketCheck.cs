using System;

namespace CameraApp.Exam
{
    class HandlerTicketCheck : HandlerBase
    {
        public override void Do(object pData)
        {
            FaceDetect detector = (FaceDetect)pData;
            detector.PromptInfo("请将车票平放在验票口!");
            detector.PlayVoice(ConstValue.VOICE_PLACE_TIC);
            detector.FlashAndLight(0);

            bool bCmp = false;
            string strQrCode="";
            for (int i=0; i<detector.GetMaxTicketChkTimes(); i++)
            {
                bCmp = detector.DoCheckTicket(ref strQrCode);
                if (bCmp) { break; }
                JobManager.Sleep(JobManager.IDLE_WAIT_MS * 3);
            }
            detector.SwitchLight(0, false);

            if (bCmp)
            {
                string str = string.Format("二维码内容: {0}", strQrCode);
                detector.PromptInfo(str);
                JobManager.Sleep(200); //! for demo only
                GetMgr().disPatch(JobManager.sHandlerOpenGate, pData);
            }
            else {
                detector.PromptError("车票验证不通过！\n请走人工验票通道。");
                detector.PlayVoice(ConstValue.VOICE_FAIL_TICKCHK);
                GetMgr().disPatch(JobManager.sHandlerException, pData);
            }
        }
    }
}