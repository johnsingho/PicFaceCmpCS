using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CameraApp.Exam
{
    class JobManager
    {
        public static int DEF_WAIT_POLLING = 200;

        public static readonly HandlerBase sHandlerReadIDCard = new HandlerReadIDCard();
        public static readonly HandlerBase sHandlerFaceCmp = new EmptyHandler(); //! test

        #region MainCode
        public void doWork(object pData)
        {
            m_pCurHandler.Do(pData);
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
