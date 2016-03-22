using System;

namespace CameraApp.Exam
{
    /// <summary>
    /// 不太重要的错误
    /// 处理完之后就回到刷身份证的画面
    /// </summary>
    class HandlerException : HandlerBase
    {
        public override void Do(object pData)
        {
            JobManager.Sleep(1500);
            GetMgr().disPatch(JobManager.sHandlerReadIDCard, pData);
        }
    }
}