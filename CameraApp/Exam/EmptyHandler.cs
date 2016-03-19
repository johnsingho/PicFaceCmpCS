using System.Threading;
using JohnKit;

namespace CameraApp.Exam
{
    class EmptyHandler : HandlerBase
    {
        public override void Do(object pData)
        {
            WinCall.TraceMessage("***EmptyHandler for test.");
            Thread.Sleep(200);
            GetMgr().disPatch(JobManager.sHandlerReadIDCard, pData);
        }
    }

}