using System;

namespace CameraApp.Exam
{
    class HandlerFaceCmp : HandlerBase
    {
        public override void PreDo(object pData)
        {
            FaceDetect detector = (FaceDetect)pData;
            detector.FlashLight(1, 200);
        }
        public override void Do(object pData)
        {
            FaceDetect detector = (FaceDetect)pData;
            detector.SwitchLight(2, true);
            detector.SwitchLight(3, true);
            detector.PlayVoice(ConstValue.VOICE_VIEW_CAM);

            float fScore = 0.0f;
            bool bCmp = false;
            for (int i=0;i<detector.GetMaxFaceCmpTimes(); i++)
            {
                bCmp = detector.DoFaceCmp(ref fScore);
                if (bCmp) { break; }
                JobManager.Sleep(JobManager.IDLE_WAIT_MS);
            }

            detector.SwitchLight(2, false);
            detector.SwitchLight(3, false);
            detector.KeepCompareInfo(fScore);

            if (bCmp)
            {
                detector.PromptInfo("人脸识别通过");
                JobManager.Sleep(100); //for test
                GetMgr().disPatch(JobManager.sHandlerTicketCheck, pData);
            }
            else {
                detector.PromptError("人脸识别不通过");
                detector.PlayVoice(ConstValue.VOICE_FAIL_FACECMP);
                GetMgr().disPatch(JobManager.sHandlerException, pData);
            }

            detector.WriteFaceCmpLog(fScore);
        }
    }
}