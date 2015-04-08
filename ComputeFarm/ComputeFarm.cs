using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using QueueCommon;

namespace ComputeFarm
{
    /// <summary>
    /// This project:
    ///     1) wrangles a bunch of workers, 
    ///     
    /// Additionally, there should be some audit and other logging associated with it and
    /// there should be absolute indifference about the class of object, and some indifference about the 
    /// methods used for communication (direct compilation/inclusion vs queues vs anything else??)
    /// Scheduling is strictly FIFO for requests to first available worker
    /// 
    /// The farm is a service, and clients should be able to request that a certain number of workers be provisioned.
    /// THe farm can utilize a "last utilized" model to shut down worker objects that have been idle for awhile.
    /// 
    /// </summary>
    public class ComputeFarm
    {
        EventLog auditLog;
        List<ComputeWorker> runningWorkers;
        List<ComputeWorker> idleWorkers;
        List<ComputeWorker> failedWorkers;

        string ControlBaseName = "__ControlBase__";
        QueueingModel controlQueue;

        //===================================
        // service control methods

        public ComputeFarm(EventLog audit)
        {
            runningWorkers = new List<ComputeWorker>();
            idleWorkers = new List<ComputeWorker>();
            failedWorkers = new List<ComputeWorker>();
            auditLog = audit;
        }
        public void Init()
        {
            InitControlQueue();
        }
        public void Shutdown()
        {
            CleanupWorkers(idleWorkers);
            CleanupWorkers(failedWorkers);
            CleanupWorkers(runningWorkers);
        }
        public void CheckControlRequests()
        {
            // ### handle incoming control requests
        }

        //===================================
        // private internal methods

        private void InitControlQueue()
        {
            // ### set up the local control queues
            List<string> routes = new List<string>();
            routes.Add("*.farmRequest.proxy");
            controlQueue = new QueueingModel("ComputeFarm", "topic", ControlBaseName, routes, "localhost", "guest", "guest", 5672);
            controlQueue.SetListenerCallback(HandlePosts);
            auditLog.WriteEntry("Queue Initialized");
        }

        private void HandlePosts(byte[] msg, string routeKey)
        {
            string msgStr = System.Text.Encoding.Default.GetString(msg);
            auditLog.WriteEntry("Message Received: " + msgStr + " - " + routeKey);
            string clientID = routeKey.Split('.')[0];
            controlQueue.PostMessage("Ack", clientID+".farmResponse.farm");
        }

        private void CleanupWorkers(List<ComputeWorker> list)
        {
            while (list.Count > 0)
            {
                ComputeWorker cw = list[0];
                if (cw.status == WorkerStatus.Running)
                    cw.Kill();
                cw.Shutdown();
                list.Remove(cw);
                cw = null;
            }
        }
    }
}
