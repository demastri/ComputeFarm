using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComputeFarmWorkerProxy;

namespace TestWorker
{
    public class PrintSort : IWorker
    {
        // ===============
        // local attributes

        string myRequest;
        string myResult;
        int updateIncrement = 250;
        int timeoutLimit = 10000;
        WorkerStatus myStatus;
        DateTime firstRunningTime;
        DateTime stopTime;
        DateTime updateTime;
        DateTime timeoutTime;

        // ===============
        // interface methods

        public event WorkerUpdateHandler WorkerUpdateEvent;
        public event WorkerCompleteHandler WorkerCompleteEvent;

        System.Timers.Timer contextTimer;

        public PrintSort()  // needs to be public for an external user to instantiate
        {
            myRequest = null;
            myResult = null;
            myStatus = WorkerStatus.Init;
            firstRunningTime = DateTime.MinValue;
        }

        public void Init()
        {
            contextTimer = new System.Timers.Timer(50.0);
            contextTimer.Elapsed += contextTimer_Elapsed;
            contextTimer.AutoReset = true;
            contextTimer.Enabled = true;

            myRequest = null;
            myResult = null;
            myStatus = WorkerStatus.Init;
        }
        public bool Start(string wr)
        {
            myRequest = wr;
            firstRunningTime = DateTime.Now;
            stopTime = firstRunningTime.AddSeconds(wr.Length == 0 ? 0 : (Char.IsLetter(wr[0]) ? Char.ToLower(wr[0]) - 'a' + 1 : 10));
            updateTime = firstRunningTime.AddMilliseconds(updateIncrement);
            timeoutTime = firstRunningTime.AddMilliseconds(timeoutLimit);
            myStatus = WorkerStatus.Running;
            return true;
        }
        public bool Kill()
        {
            Init();
            myStatus = WorkerStatus.Shutdown;
            return true;
        }
        public void Shutdown()
        {
            // nothing to release for this class
            Init();
            myStatus = WorkerStatus.Shutdown;
        }
        public WorkerStatus GetStatus()
        {
            return myStatus;
        }
        public string GetWorkResult()
        {
            return myResult;
        }

        // ===============
        // internal methods

        void contextTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (GetStatus() == WorkerStatus.Running)
            {
                if (firstRunningTime == DateTime.MinValue)
                    firstRunningTime = DateTime.Now;
                TimeSpan runTime = DateTime.Now - firstRunningTime;

                if (DateTime.Now >= stopTime)
                {
                    myStatus = WorkerStatus.Completed;
                    WorkerCompleteEvent(myRequest);
                }
                else if (DateTime.Now >= timeoutTime)
                {
                    myStatus = WorkerStatus.Timeout;
                    WorkerCompleteEvent("failed <" + myRequest + "> timeout");
                }
                else if (DateTime.Now >= updateTime)
                {
                    updateTime = updateTime.AddMilliseconds(updateIncrement);
                    WorkerUpdateEvent("*");
                }
            }
        }
    }
}
