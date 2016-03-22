using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CameraApp.Exam
{
    class JobManager
    {
        //空闲等待时间
        public static readonly int IDLE_WAIT_MS = 200;

        public static readonly HandlerBase sHandlerTest = new EmptyHandler(); //! test
        public static readonly HandlerBase sHandlerException = new HandlerException();
        public static readonly HandlerBase sHandlerReadIDCard = new HandlerReadIDCard();
        public static readonly HandlerBase sHandlerFaceCmp = new HandlerFaceCmp();

        public static readonly HandlerBase sHandlerTicketCheck = sHandlerTest; //!todo        

        #region MainCode
        public void doWork(object pData)
        {
            m_pCurHandler.Do(pData);
        }

        public static void Sleep(int nMS)
        {
            Thread.Sleep(nMS);
        }

        public void disPatch(HandlerBase pHandler, object pData)
        {
            m_pCurHandler = pHandler;
            pHandler.Bind(this);
            pHandler.PreDo(pData);
        }

        private HandlerBase m_pCurHandler;
        #endregion
    }

}
