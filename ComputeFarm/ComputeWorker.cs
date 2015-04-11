using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ComputeFarm
{
    public class ComputeWorker
    {
        public int thisID;

        static string searchLoc = "./Workers"; // sdir of the executable??

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

        public static ComputeWorker WorkerFactory(string typeID)
        {
            /// ### there may be many ways to manage this, but the first one out of the box is:
            ///     an IWorker component in a dll that we can wrap in a WorkWrapper...
            /// I think it's easier from a workflow perspective to find the appropriate dlls for this typeid first
            /// then pass them to the workwrapper for instantiation, rather than start a process and hope it will 
            /// be able to find it.  You still have to get a positive ack that it's running, but if you don't you
            /// know it was an instantiation problem, not a location one...
            Dictionary<string, string> workerLoc = BuildWorkerMap(searchLoc);
            if (workerLoc.Keys.Contains(typeID))
            {
                /// Create a process with the proper string as startup parameters:
                /// WorkWrapper.exe is the location for the wrapper
                /// exch details
                /// queue details
                /// typeid library details
                /// typeid details
                
                /// start the process
                /// look for an ack that it started ok
            }
            return null;
        }
            
        static Dictionary<string, string> BuildWorkerMap(string dirToSearch)
        {
            // find the typeID
            Dictionary<string, string> outDict = new Dictionary<string,string>();

            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(dirToSearch);
            System.IO.FileInfo[] fis = di.GetFiles("*.dll");
            foreach (System.IO.FileInfo fi in fis)
            {
                try
                {
                    Assembly myTempAssy = System.Reflection.Assembly.ReflectionOnlyLoadFrom(fi.FullName);
                    foreach( Type t in myTempAssy.GetTypes() )
                        outDict.Add( t.FullName, fi.FullName );
                }
                catch (Exception e)
                {
                }
            }
            return outDict;
        }
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
