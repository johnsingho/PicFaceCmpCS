using System.Threading;

namespace CameraApp.Exam
{
    class HandlerReadIDCard : HandlerBase
    {
        public override void PreDo(object pData)
        {
            FaceDetect detector = (FaceDetect) pData;
            detector.ResetIDCardInfo();
        }

        public override void Do(object pData)
        {
            FaceDetect detector = (FaceDetect)pData;
            if (detector.ReadIDCardInfo())
            {
                GetMgr().disPatch(JobManager.sHandlerFaceCmp, pData);
            }
            else
            {
                Thread.Sleep(JobManager.DEF_WAIT_POLLING);
            }
        }
    }
}