﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QueueCommon;

namespace ComputeFarmProxy
{
    public class ComputeFarmProxy
    {
        /// <summary>
        /// so, a farm exists on an rMQ host
        /// each exchange hosts a number of queues
        /// each queue hosts a single type of worker
        /// external methods used by clients:
        ///  FarmProxy constructor
        ///  ConfigDetails farm.GetStatus()
        ///  ConfigDetails.ToString() for display detail
        ///  QueueDetail farm.RequestQueue( typeID, count )
        ///  queue.RequestWork( string )
        ///  Update and Work Handler
        ///  ===================
        ///  better interface:
        ///  FarmProxy constructor( host, port )
        ///  FarmStatus farm.GetStatus()
        ///  FarmStatus.ToString() for display detail
        ///  FarmWorker farm.RequestWorker( typeID, count )
        ///  worker.RequestWork( string )
        /// </summary>
        /// 

        /// ### something doesn't feel exactly right here
        /// this is the level where I'm connecting to the service, it should also be where I cachethe connection to the rMQ service
        /// in the rMQWeap model, each queue manages its own connection to the service, exchange and queue details
        /// in actuality, there should be one cached connection to the service and exchange (here)
        /// and each queue should only know about the specifics of that connection
        /// might require a change to the QueueCommon wrappers to expose the single conn/exchange object

        /// ### also, I should knock out the whole exch / queue / routing key comms model, build it once and be done with it
        /// can we use a topic exchange for keys like {clientID}.{requestType}.{typeID} as a routing key where
        /// queue name is actually irrelevant in this model...
        /// requestType is one of {farmCommand, farmResponse, workerCommand, workerResponse, workRequest, workUpdate, workComplete}
        /// if this is correct, then the Fabric Manager is too heavy - just need a well managed queue.  
        /// actually, the idea is right - keep an exchange, a control queue and a list of worker queues, it's just simpler - do it here...

        /// client/proxy opens a control queue to the farm
        /// client/proxy opens a worker queue for each worker type - the topic / routing key determines who's listening
        /// client/proxy posts to {me}.farmCommand/workRequest.{myWorker} messages
        /// client/proxy listens to {me}.farmResponse/workUpdate/workComplete.{myWorker} messages

        /// worker listens to the "magic" queue and routing established on startup
        /// worker posts to *.workUpdate/workComplete/workerCommand.{myWorker} messages
        /// worker listens to *.workRequest/workerCommand.{myWorker} messages

        /// farm listens to a "magic" control queue and routing
        /// farm posts to *.farmResponse/workerCommand.* messages
        /// farm listens to *.*.* messages

        string host;
        int port;
        string exchange;
        string uid;
        string pwd;
        string thisClientID;
        ComputeFarmStatus currentConfig;

        string ControlBaseName = "__ControlBase__";

        Exchange baseExchange;
        Queue controlQueue;
        List<Queue> workerQueues;
        System.Timers.Timer contextTimer;
        int sleepIncrement = 50;
        int commandTimeout = 2000;

        public delegate void RequestUpdateHandler(string update);
        public delegate void RequestCompleteHandler(string result);

        event RequestUpdateHandler RequestUpdateEvent;
        event RequestCompleteHandler RequestCompleteEvent;

        public class WorkerHandle
        {
            Queue refQueue;
            string refTypeID;
            public Queue worker { get { return refQueue; } }
            public string typeID { get { return refTypeID; } }

            public WorkerHandle(Queue thisQueue, string thisTypeID)
            {
                refQueue = thisQueue;
                refTypeID = thisTypeID;
            }
        }

        static public ComputeFarmProxy ConnectToFarm(int thisPort, string thisHost, string refExch, string refuid, string refpwd)
        {
            return ConnectToFarm(thisPort, thisHost, refExch, refuid, refpwd, Guid.NewGuid().ToString());
        }
        static public ComputeFarmProxy ConnectToFarm(int thisPort, string thisHost, string refExch, string refuid, string refpwd, string clientID)
        {
            ComputeFarmProxy outFarm = new ComputeFarmProxy(thisPort, thisHost, refExch, refuid, refpwd, clientID);
            outFarm.SetupExchange();
            outFarm.SetupControlQueue();
                
            return outFarm;
        }

        public bool IsOpen { get { return controlQueue != null && controlQueue.IsOpen; } }

        public ComputeFarmProxy(int thisPort, string thisHost, string refExch, string refuid, string refpwd, string clientID)
        {
            Init(thisPort, thisHost, refExch, refuid, refpwd, clientID);
        }
        private void Init(int thisPort, string thisHost, string refExch, string refuid, string refpwd, string clientID)
        {
            workerQueues = new List<Queue>();
            thisClientID = clientID;
            port = thisPort;
            host = thisHost;
            exchange = refExch;
            uid = refuid;
            pwd = refpwd;
        }

        //===============
        private void SetupExchange()
        {
            baseExchange = new Exchange(exchange, "topic", host, uid, pwd, port);

        }
        internal void SetupControlQueue()
        {
            controlQueue = new Queue(baseExchange, ControlBaseName, thisClientID + ".farmResponse.farm");

            string paramString = BuildCommandString("Init", thisClientID);
            controlQueue.PostMessage(paramString, thisClientID + ".farmRequest.proxy");
            if (!WaitForAck())
                controlQueue = null;
        }

        internal string BuildCommandString(string cmd, string parameter)
        {
            return BuildCommandString(cmd, parameter, "", 0);
        }
        internal string BuildCommandString(string cmd, string parameter, string typeID, int count)
        {
            // command string is request for the farm to do something on our behalf:
            // typ create queues or establish workers listening to a particular queue to receive work requests
            string outString = "";
            switch (cmd)
            {
                case "Init":
                    outString = cmd + "|" + parameter;
                    break;
                case "WorkerRequest":
                    outString = cmd + "|" + typeID + "|" + count;
                    break;
            }
            return outString;
        }

        internal int SetupWorkerQueue(string typeID)
        {
            int index = FindWorkerQueue(typeID);
            if (index >= 0)
                return index;
            List<string> routes = new List<string>();
            routes.Add(thisClientID + ".workUpdate." + typeID);
            routes.Add(thisClientID + ".workComplete." + typeID);
            Queue newWorkerQueue = new Queue(baseExchange, thisClientID + "." + typeID, routes);

            workerQueues.Add(newWorkerQueue);
            return workerQueues.IndexOf(newWorkerQueue);
        }
        private int FindWorkerQueue(string typeID)
        {
            foreach (Queue q in workerQueues)
                if (q.name == thisClientID + "." + typeID)
                    return workerQueues.IndexOf(q);
            return -1;
        }

        public ComputeFarmStatus RequestStatus()
        {
            // ### build the status object (really?)
            return currentConfig;
        }
        public WorkerHandle ConnectWorkerFabric(string typeID, int count)
        {
            return ConnectWorkerFabric(typeID, count, null, null);
        }
        public WorkerHandle ConnectWorkerFabric(string typeID, int count, RequestUpdateHandler updateHandler, RequestCompleteHandler completeHandler)
        {
            // ### the count is just an estimate of the clients expected workload

            // the returned object contains the offset for the queue this client will use
            // if there's an existing queue that meets this requirement, return that detail
            // if not, create and bind it, then return that detail

            string cmd = BuildCommandString("WorkerRequest", thisClientID, typeID, count);
            controlQueue.PostMessage(cmd, thisClientID + ".farmRequest.proxy");
            if (WaitForAck())
            {
                SetupWorkerQueue(typeID);
                Queue workerQueue = workerQueues[FindWorkerQueue(typeID)];
                workerQueue.SetListenerCallback(WorkerCallback);

                RequestUpdateEvent += updateHandler;
                RequestCompleteEvent += completeHandler;

                return new WorkerHandle(workerQueue, typeID);
            }
            return null;
        }
        void WorkerCallback(byte[] msg, string routeKey)
        {
            string gotOne = System.Text.Encoding.Default.GetString(msg);
            if (routeKey.Split('.')[2] == "workUpdate")
                RequestUpdateEvent(gotOne);
            if (routeKey.Split('.')[2] == "workComplete")
                RequestCompleteEvent(gotOne);
        }

        private bool WaitForAck()
        {
            string replyStr = "";
            for (int i = 0; replyStr == "" && i < commandTimeout; i += sleepIncrement)
            {
                if (controlQueue.IsEmpty)
                    System.Threading.Thread.Sleep(sleepIncrement);
                else
                    replyStr = controlQueue.ReadMessageAsString();
            }
            string[] tokens = replyStr.Split('|');
            if (replyStr.Trim() == "" || tokens.Count() == 0 || tokens[0] != "Ack")
                return false;
            return true;
        }
        public void RequestWork(WorkerHandle handle, string request)
        {
            handle.worker.PostMessage(request, thisClientID+".workRequest."+handle.typeID);
        }

    }
}