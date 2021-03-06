﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QueueCommon;
using ComputeFarmProxy;
using ComputeFarmWorkerProxy;
using ProcessWrappers;

namespace WorkerWrapper
{
    class WorkWrapper
    {
        /// what does a workwrapper for the compute farm have to do?
        ///     connect / listen to the appropriate queue
        ///     listen for work/update requests
        ///     post update / completion status
        ///     listen for shutdown requests
        static void Main(string[] args)
        {
            ClientWrapper myClient = new ClientWrapper(args);

            myClient.Start();
            if (myClient.useQueueIO)
                myClient.UpdatePostRoute("workRequest", "workComplete");

            string temp;    // Display the read text to the console 
            bool done = false;
            // Wait for 'sync message' from the server. 
            do
            {
                myClient.ClientMessage("[CLIENT] Wait for sync...");
                temp = myClient.ClientReadLine();
            }
            while (!temp.StartsWith("SYNC"));
            myClient.ClientMessage("[CLIENT] Received sync...");

            // Read the server data and echo to the console. 
            Task<string> readTask = myClient.ClientReadLineAsync();
            do
            {
                if (readTask.IsCompleted)
                {
                    temp = readTask.Result;
                    myClient.ClientMessage("[CLIENT] Echo: " + temp);
                    if (temp.StartsWith("QUIT"))
                        done = true;
                    else
                        readTask = myClient.ClientReadLineAsync();
                }
                else
                {
                    System.Threading.Thread.Sleep(750);
                    myClient.ClientMessage("[CLIENT] Wait...");
                }
            }
            while (!done);

            myClient.ClientMessage("[CLIENT] Press Enter to Quit...");
            temp = myClient.ClientReadLine();

            myClient.ClientMessage("[CLIENT] quitting client process...");
            myClient.ClientMessage("QUIT"); // mark to the server that we're done...

            myClient.Cleanup();
        }
    }
    class WorkWrapperStamdalone
    {
        // Queue based client wrapper??

        static System.Timers.Timer contextTimer;

        static IWorker myWorker = null;
        static WorkerStatus myStatus = WorkerStatus.Init;

        // this really has to be turned into an ExchDetail object...
        static QueueingModel thisExch = null;

        static int sleepIncrement = 50;

        static int OldMain(string[] args)
        {
            // the job of this process is to:
            //  instantiate a worker
            //  intantiate a connection to the queues
            //  listen for incoming messages
            //  process them

            // testing instantiates a worker locally and exercises it through the IWorker interface 
            //  to ensure a) the worker works, and b) wrapper process logic works

            bool testing = true;
            if (testing)
            {
                TestMe();
                return 0;
            }

            Init(args[0].Split('|'));

            if (myWorker == null || thisExch == null || !thisExch.IsOpen)
                return 0;

            myStatus = WorkerStatus.Running;
            contextTimer.Enabled = true;

            while (myStatus != WorkerStatus.Shutdown)
            {
                System.Threading.Thread.Sleep(sleepIncrement);
            }
            return 1;
        }

        static private void Init(string[] paramList)
        {
            // by convention, the args are a stringified set of parameters, in this order:
            // host, exch, uid, pwd, queuebasename, routekey, port, worker dir, typeID
            string thisTypeID = paramList[(int)StringifiedParameters.TypeIDOffset];

            contextTimer = new System.Timers.Timer(sleepIncrement);
            contextTimer.Elapsed += contextTimer_Elapsed;
            contextTimer.AutoReset = true;
            contextTimer.Enabled = false;

            InstantiateWorker(paramList[(int)StringifiedParameters.WorkerDirOffset], paramList[(int)StringifiedParameters.TypeIDOffset]);
            InstantiateExchAndQueues(paramList);
        }
        static private void Cleanup()
        {
            if( contextTimer != null )
            contextTimer.Enabled = false;

            ShutdownWorker();
            ShutdownQueues();
        }
        static void ShutdownWorker()
        {
            if (myWorker != null)
            {
                myWorker.Shutdown();
                int retry = 0;
                while (myWorker.GetStatus() != WorkerStatus.Shutdown && retry++ < 5)
                    System.Threading.Thread.Sleep(sleepIncrement / 5);
                if (myWorker.GetStatus() != WorkerStatus.Shutdown)
                    myWorker.Kill();
                myWorker = null;
            }
        }
        static void ShutdownQueues()
        {
            thisExch.CloseConnections();
            thisExch = null;
        }

        static void contextTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // if there's a message on the queue
            // ### this is the wrong model - workers should just pick up messages they care about...
            if (WorkerIsAvailable())
            {
                if (RequestIsAvailable())
                {
                    string request = ReadRequst();
                    if (ControlRequest(request))
                    {
                        HandleControlRequest(GetControlRequest(request));
                    }
                    if (WorkRequest(request))
                    {
                        myWorker.Start(request);
                        myStatus = WorkerStatus.Running;
                    }
                }
            }
        }
        static bool WorkerIsAvailable()
        {
            if (myWorker == null)
                return false;
            switch (myWorker.GetStatus())
            {
                case WorkerStatus.Init:
                case WorkerStatus.Idle:
                case WorkerStatus.Completed:
                    return true;
                case WorkerStatus.Timeout:
                case WorkerStatus.Error:
                case WorkerStatus.Shutdown:
                    return false;
            }
            return false;
        }

        static void InstantiateWorker(string dirToSearch, string thisTypeID)
        {
            // find the typeID
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(dirToSearch);
            System.IO.FileInfo[] fis = di.GetFiles("*.dll");
            foreach (System.IO.FileInfo fi in fis)
            {
                try
                {
                    object myTempObj = Activator.CreateInstanceFrom(fi.FullName, thisTypeID).Unwrap();
                    myWorker = (IWorker)myTempObj;
                    break;
                }
                catch (Exception e)
                {
                }
            }
            if (myWorker != null)
            {
                myWorker.Init();
                myWorker.WorkerUpdateEvent += WorkerUpdateAvailable;
                myWorker.WorkerCompleteEvent += WorkerResultsAvailable;

                // ### SendAckToControlQueue();
            }
        }
        static void InstantiateExchAndQueues(string[] paramList)
        {
            // by convention, the args are a stringified set of parameters, in this order:
            // typeID, host, exch, uid, pwd, port, typeID

            /// worker listens to the "magic" queue and routing established on startup
            /// worker posts to *.workUpdate/workComplete/workerCommand.{myWorker} messages
            /// worker listens to *.workRequest/workerCommand.{myWorker} messages

            // build the queues
            string typeID = paramList[(int)StringifiedParameters.TypeIDOffset];
            string thisHost = paramList[(int)StringifiedParameters.HostOffset];
            string thisExchName = paramList[(int)StringifiedParameters.ExchOffset];
            string thisUid = paramList[(int)StringifiedParameters.UidOffset];
            string thisPwd = paramList[(int)StringifiedParameters.PwdOffset];

            int thisPort = Convert.ToInt32(paramList[(int)StringifiedParameters.PortOffset]);

            List<string> routes = new List<string>();
            routes.Add("*.workRequest." + typeID);
            routes.Add("*.workerCommand." + typeID);
            string queueName = "worker." + typeID;

            thisExch = new QueueingModel(thisExchName, "topic", queueName, routes, thisHost, thisUid, thisPwd, thisPort);
        }
        static bool RequestIsAvailable()
        {
            return false;/// !requestQueue.QueueEmpty();
        }
        static bool ControlRequest(string req)
        {
            string[] tokens = req.Split('|');
            if (tokens.Count() >= 2)
                if (tokens[0] == "__Control__")
                    return true;
            return false;
        }
        static string GetControlRequest(string req)
        {
            string[] tokens = req.Split('|');
            if (tokens.Count() >= 2)
                return tokens[0];
            return "";
        }
        static void HandleControlRequest(string req)
        {
            switch (req.ToLower())
            {
                case "shutdown":
                    Cleanup();
                    myStatus = WorkerStatus.Shutdown;
                    break;
                default:
                    break;
            }
        }
        static bool WorkRequest(string req)
        {
            return !ControlRequest(req);
        }
        static string ReadRequst()
        {
            return "";/// thisExch.ParseWorkerString(System.Text.Encoding.Default.GetString(requestQueue.ReadMessage()));
        }
        static void WorkerUpdateAvailable(string s)
        {
            ///if (updatesQueue != null)
            ///    updatesQueue.PostMessage(s);
            ///else
                System.Console.Write(s);
        }
        static void WorkerResultsAvailable(string s)
        {
            ///if (resultsQueue != null)
            ///    resultsQueue.PostMessage(s);
            ///else
                System.Console.WriteLine(s);
        }

        static void TestMe()
        {
            string thisPath = "C:\\Projects\\JPD\\BBRepos\\ComputeFarm\\ComputeFarm\\TestWorker\\bin\\Debug";
            string thisType = "TestWorker.PrintSort";

            InstantiateWorker(thisPath, thisType);

            if (myWorker != null)
            {
                myWorker.Start("Dawg");
                while (myWorker.GetStatus() == WorkerStatus.Running)
                    System.Threading.Thread.Sleep(50);

                myWorker.Start("TestString");
                while (myWorker.GetStatus() == WorkerStatus.Running)
                    System.Threading.Thread.Sleep(50);

                myWorker.Start("Batty");
                while (myWorker.GetStatus() == WorkerStatus.Running)
                    System.Threading.Thread.Sleep(50);

                myWorker.Shutdown();
                while (myWorker.GetStatus() != WorkerStatus.Shutdown)
                {
                    System.Threading.Thread.Sleep(sleepIncrement);
                }
                Cleanup();
            }
        }
    }
}
