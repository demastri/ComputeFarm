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
        List<ComputeWorker> workers;
        ConnectionDetail settings;
        
        string ControlBaseName = "__ControlBase__";
        QueueingModel controlQueue;

        //===================================
        // service control methods

        public ComputeFarm(ConnectionDetail fs, EventLog audit)
        {
            settings = fs;
            workers = new List<ComputeWorker>();
            auditLog = audit;
        }
        public ComputeFarm(EventLog audit)
        {
            settings = new ConnectionDetail();
            workers = new List<ComputeWorker>();
            auditLog = audit;
        }
        public bool IsOpen { get { return controlQueue != null && controlQueue.IsOpen; } }
        public void Init()
        {
            InitControlQueue();
        }
        public void Shutdown()
        {
            controlQueue.CloseConnections();
            CleanupWorkers(workers);
        }
        public void CheckControlRequests()
        {
            // ### handle incoming control requests
        }

        //===================================
        // private internal methods

        private void InitControlQueue()
        {
            // set up the local control queues
            List<string> routes = new List<string>();
            routes.Add("*.farmRequest.proxy");
            // the queueingmodel class binds an exchange and queue for straightforward applications where only one channel is needed
            controlQueue = new QueueingModel(
                settings.exchName, "topic", ControlBaseName, routes, 
                settings.host, settings.user, settings.pass, settings.port );
            controlQueue.SetListenerCallback(HandlePosts);
            if (auditLog != null)
                auditLog.WriteEntry("Queue Initialized");
        }

        private void HandlePosts(byte[] msg, string routeKey)
        {
            string msgStr = System.Text.Encoding.Default.GetString(msg);
            if (auditLog != null)
                auditLog.WriteEntry("Message Received: " + msgStr + " - " + routeKey);

            string[] msgSet = msgStr.Split('|');
            string[] paramSet = routeKey.Split('.');
            string clientID = paramSet[0];
            string reply = ProcessCommand(msgSet, paramSet);
            controlQueue.PostMessage(reply, clientID + ".farmResponse.farm");
        }
        string ProcessCommand(string[] msgSet, string[] paramSet)
        {
            string clientID = (paramSet.Count() >= 1 ? paramSet[0] : "");

            string commandType = (msgSet.Count() >= 1 ? msgSet[0] : "");
            string commandID = (msgSet.Count() >= 2 ? msgSet[1] : "");
            string typeID = (msgSet.Count() >= 3 ? msgSet[2] : "");
            int count = Convert.ToInt32(msgSet.Count() >= 4 ? msgSet[3] : "0");
            string outString = "Nack|" + commandID;
            switch (commandType)
            {
                case "Init":
                    outString = "Ack|" + commandID;
                    break;
                case "WorkerRequest":
                    if (typeID != "" && count > 0 && CreateWorkers(typeID, count) )
                        outString = "Ack|" + commandID;
                    break;
            }
            return outString;
        }
        private bool CreateWorkers(string typeID, int count) {
            for (int i = 0; i < count; i++)
            {
                ComputeWorker thisWorker = ComputeWorker.WorkerFactory(typeID, settings);
                if (thisWorker == null)
                    return false;
                workers.Add(thisWorker);
            }
            return true;
        }

        private void CleanupWorkers(List<ComputeWorker> list)
        {
            while (list.Count > 0)
            {
                ComputeWorker cw = list[0];
                if (cw.status == ComputeFarmWorkerProxy.WorkerStatus.Running)
                    cw.Kill();
                cw.Shutdown();
                list.Remove(cw);
                cw = null;
            }
        }
    }
}
