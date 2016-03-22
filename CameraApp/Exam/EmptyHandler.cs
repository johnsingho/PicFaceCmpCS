using System.Threading;
using JohnKit;

namespace CameraApp.Exam
{
    class EmptyHandler : HandlerBase
    {
        public override void Do(object pData)
        {
            WinCall.TraceMessage("***EmptyHandler for test.");
            JobManager.Sleep(3000);
            GetMgr().disPatch(JobManager.sHandlerReadIDCard, pData);
        }
    }

}