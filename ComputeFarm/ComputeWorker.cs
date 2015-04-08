using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputeFarm
{
    public class ComputeWorker
    {
        public int thisID;

        string resultQueue;
        string routeKey;
        public string requestType;

        IWorker workerProcess;

        public DateTime creationTme;
        public DateTime executionStartTime;
        public DateTime executionCompleteTime;

        public TimeSpan currentRunTime;
        public TimeSpan executionTimeout;

        public ComputeResult result { get { return workerProcess == null ? null : workerProcess.GetWorkResult(); } }
        public WorkerStatus status { get { return workerProcess == null ? WorkerStatus.Init : (currentRunTime > executionTimeout ? WorkerStatus.Timeout : workerProcess.GetStatus()); } }
        public void Kill() { if (workerProcess != null) workerProcess.Kill(); }
        public void Shutdown() { if (workerProcess != null) workerProcess.Shutdown(); }

        public ComputeWorker()
        {
            workerProcess = null;
            creationTme = DateTime.Now;
        }

        public void UpdateRunTime()
        {
            currentRunTime = DateTime.Now - executionStartTime;
        }
        public void UpdateStatus()
        {
            if (status == WorkerStatus.Running)
            {
                UpdateRunTime();
                if (currentRunTime > executionTimeout)
                {
                    workerProcess.Kill();
                }
            }
        }
        public bool Start(ComputeRequest cr, WorkerCompleteHandler handler)
        {
            workerProcess.WorkerCompleteEvent += handler;
            return workerProcess.Start(cr);
        }
    }

    public enum WorkerStatus { Init, Idle, Running, Completed, Timeout, Error };
    public delegate void WorkerUpdateHandler(ComputeResult result);
    public delegate void WorkerCompleteHandler(ComputeResult result);
    public interface IWorker
    {
        event WorkerUpdateHandler WorkerUpdateEvent;
        event WorkerCompleteHandler WorkerCompleteEvent;

        WorkerStatus GetStatus();
        void SetWorkRequest(ComputeRequest wr);
        ComputeRequest GetWorkRequest();
        ComputeResult GetWorkResult();
        void CheckProgress();
        bool Start(ComputeRequest wr);
        bool Kill();
        void Shutdown();

        void Init();
        void Reset();
    }
}
