using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComputeFarmWorkerProxy;

namespace TestWorker
{
    public class RandomTask: IWorker
    {
        // ===============
        // local attributes

        string myRequest;
        string myResult;
        int updateIncrement = 250;
        Random rand = null;
        int minTime = 5000;
        int maxTime = 15000;
        int actTime = 250;
        int timeoutLimit = 20000;
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

        public RandomTask()  // needs to be public for an external user to instantiate
        {
            rand = new Random();
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

            actTime = rand.Next(minTime, maxTime);

            stopTime = firstRunningTime.AddSeconds(actTime/1000.0);
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
                    WorkerCompleteEvent(actTime.ToString());
                }
                else if (DateTime.Now >= timeoutTime)
                {
                    myStatus = WorkerStatus.Timeout;
                    WorkerCompleteEvent("failed <" + myRequest + "> timeout");
                }
                else if (DateTime.Now >= updateTime)
                {
                    updateTime = updateTime.AddMilliseconds(updateIncrement);
                    WorkerUpdateEvent("x");
                }
            }
        }
    }
}
