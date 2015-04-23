using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QueueCommon;
using ComputeFarmWorkerProxy;
using ComputeFarmProxy;
using ComputeFarm;

namespace ComputeFarmProxy
{
    public class FabricManager
    {
        int port;
        string host;
        public bool IsOpen { get { return requestQueue != null && requestQueue.IsOpen; } }

        Exchange baseExchange;

        Queue requestQueue;
        Queue updatesQueue;
        Queue resultsQueue;

        string baseName;

        public delegate void RequestUpdateHandler(string update);
        public delegate void RequestCompleteHandler(string result);

        event RequestUpdateHandler RequestUpdateEvent;
        event RequestCompleteHandler RequestCompleteEvent;

        public string BaseName;
        string ControlBaseName = "__ControlBase__";

        int myWorkerID;
        string exchange;
        string uid;
        string pwd;

        public static bool operator ==(FabricManager lhs, FabricManager rhs)
        {
            return lhs.exchange == rhs.exchange &&
                lhs.port == rhs.port &&
                lhs.host == rhs.host &&
                lhs.uid == rhs.uid &&
                lhs.pwd == rhs.pwd;
        }
        public static bool operator !=(FabricManager lhs, FabricManager rhs)
        {
            return !(lhs == rhs);
        }

        public FabricManager(FarmSettings fs)
        {
            Init(fs);
        }
        public FabricManager()
        {
            Init(new FarmSettings() );
        }
        private void Init(FarmSettings fs)
        {
            port = fs.Port;
            host = fs.Host;
            exchange = fs.Exch;
            uid = fs.Uid;
            pwd = fs.Pwd;

            myWorkerID = -1;
        }
        internal void SetupControlQueues()
        {
            string baseName = ControlBaseName;
            string paramString = BuildControlString(baseName);
            SetupQueues(paramString);
        }
        internal void SetupWorkerQueues(string baseName)
        {
            string paramString = BuildControlString(baseName);
            SetupQueues(paramString);
        }
        internal string BuildControlString(string baseName)
        {
            string typeID = baseName;
            if (baseName != ControlBaseName)
                baseName = Guid.NewGuid().ToString();
            string routeKey = typeID;   // ###
            string workerDir = " ";     // ###
            string outString = String.Format("|{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|",
                host, exchange, uid, pwd, baseName, routeKey, port, workerDir, typeID);

            BaseName = baseName;

            // control string is an internal message so we set up the appropriate control queues on our end
            // how are they "known" on the farm/workers??  has to be a command issued to the controlQueue...
            // this is the string the farm passes to client workers so THEY can connect to the proper control queue
            // ### is this a good idea???
            return outString;
        }
        internal string BuildCommandString(string cmd, string queueName, string typeID, int count)
        {
            // ### command string is request for the farm to do something on our behalf:
            // typ create queues or establish workers listening to a particular queue to receive work requests
            return "";
        }
        string BuildWorkerString(string cmd)
        {
            // cmd is the thing the worker wants to see.  Can't think of anything at the moment that needs to be added to it...
            return "|" + Guid.NewGuid().ToString() + "|<<" + cmd + ">>|";
        }
        public string ParseWorkerString(string cmd)
        {
            string[] tokens = cmd.Split('|');
            Guid reqID = new Guid(tokens[0]);
            // cmd is returned from the worker as update or results
            int lIndex = cmd.IndexOf("|<<");
            int rIndex = cmd.IndexOf(">>|");
            if (lIndex < 0 || rIndex < 0)
                return "";
            return cmd.Substring(lIndex + 3, rIndex - lIndex - 3);
        }
        internal void SetupQueues(string paramString)
        {
            string[] paramList = (paramString[0] == '|' ?  paramString.Substring(1) : paramString).Split('|');
            // build the queues
            string thisHost = paramList[(int)StringifiedParameters.HostOffset];
            string thisExch = paramList[(int)StringifiedParameters.ExchOffset];
            string thisUid = paramList[(int)StringifiedParameters.UidOffset];
            string thisPwd = paramList[(int)StringifiedParameters.PwdOffset];
            string thisBaseName = paramList[(int)StringifiedParameters.QueueBaseNameOffset];
            string thisRouting = paramList[(int)StringifiedParameters.RouteKeyOffset];
            int thisPort = Convert.ToInt32(paramList[(int)StringifiedParameters.PortOffset]);

            ConnectionDetail conn = new ConnectionDetail(thisHost, thisPort, thisExch, "direct", "", thisRouting, thisUid, thisPwd );

            baseExchange = new Exchange(conn);
            conn.queueName = thisBaseName + ".Request";
            requestQueue = new Queue(baseExchange, conn);
            conn.queueName = thisBaseName + ".Results";
            resultsQueue = new Queue(baseExchange, conn);
            conn.queueName = thisBaseName + ".Updates";
            updatesQueue = new Queue(baseExchange, conn);
        }
        public void SetupHandlers(RequestUpdateHandler updateHandler, RequestCompleteHandler completeHandler)
        {
            if (completeHandler != null)
                RequestCompleteEvent += completeHandler;
            if (updateHandler != null)
                RequestUpdateEvent += updateHandler;
        }

        internal int RequestWorkers(string queue, string typeID, int count)
        {
            string cmdString = BuildCommandString("WorkerRequest", queue, typeID, count);
            requestQueue.PostMessage(cmdString);
            return -1;
        }
        public void SendCommand(string wr)
        {
            // ### this isn't really sending a command
            string workString = BuildWorkerString(wr);
            requestQueue.PostMessage(workString);
        }
        public void RequestWork(string wr)
        {
            string workString = BuildWorkerString(wr);
            requestQueue.PostMessage(workString);
        }
        public void CheckProgress()
        {
            // check update and result queue status and raise appropriate events
            if (!updatesQueue.IsEmpty)
            {
                string update = updatesQueue.ReadMessageAsString();
                RequestUpdateEvent(ParseWorkerString(update));
            }
            if (!resultsQueue.IsEmpty)
            {
                string results = resultsQueue.ReadMessageAsString();
                RequestCompleteEvent(ParseWorkerString(results));
            }
        }
        public void Close()
        {
            if (requestQueue != null)
                requestQueue.Close();
            if (updatesQueue != null)
                updatesQueue.Close();
            if (resultsQueue != null)
                resultsQueue.Close();
            requestQueue = updatesQueue = resultsQueue = null;
            if (baseExchange != null)
                baseExchange.Close();
            baseExchange = null;
        }
        public override string ToString()
        {
            if (port == -1)
                return "proxy not connected";

            int workerCount = 0;
            return "Exchange Name: " + exchange + " Port: " + port.ToString() + " host: " + host +
                " Queue Count: " + 3.ToString() +
                " Worker Count: " + workerCount.ToString();
        }
    }
}
