using System;

namespace CameraApp.Exam
{
    class HandlerOpenGate : HandlerBase
    {
        // 验证通过，允许通行
        public override void Do(object pData)
        {
            FaceDetect detector = (FaceDetect)pData;
            detector.LetGo();
            JobManager.Sleep(800); //! for demo only
            GetMgr().disPatch(JobManager.sHandlerReadIDCard, pData);
        }
    }
}