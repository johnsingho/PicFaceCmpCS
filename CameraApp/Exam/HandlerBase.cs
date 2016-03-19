
namespace CameraApp.Exam
{
    abstract class HandlerBase
    {
        private JobManager m_pMgr = null;

        public void Bind(JobManager pMgr) { m_pMgr = pMgr; }
        public JobManager GetMgr() { return m_pMgr; }
        
        public virtual void PreDo(object pData) { }
        public abstract void Do(object pData);
    };
}

