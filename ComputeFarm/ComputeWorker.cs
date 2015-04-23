using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using ProcessWrappers;
using ComputeFarmWorkerProxy;

namespace ComputeFarm
{
    public class ComputeWorker
    {
        /// <summary>
        /// 
        /// this object encapsulates the external process running the IWorker for a given typeID
        /// it needs to be instantiated and killed
        /// work requests, process lifecycle, status and results can either be:
        ///     passed through this wrapper to the farm, or
        ///             NO - then we have to do queue / worker management rather than just consume when ready
        ///     passed through queues for comms to the object / wrapper / client, or
        ///             NO - seems cumbersome for the lifecycle stuff - i have a handle in the farm for these...
        ///     hybrid - lifecycle here, work requests queued
        ///             Yes On start:
        ///                 On creation, farm provides location and typeID
        ///                 cw creates a host process wrapper and passes this detail - it's a HostWrapper, not an IWorker...
        ///                 cw starts the process
        ///             On work request:
        ///                 Nothing - process receives request, processes and returns result directly from client
        ///                 Audit - proxy should send audit note to farm (req), worker should send audit note to farm (req/comp)
        ///             On kill/stop:
        ///                 farm asks cw to stop
        ///                 cw asks the process to stop
        ///                 kills process if needed
        ///                 returns to farm
        ///                 farm releases resources
        ///                 
        /// </summary>
        public int thisID;

        static List<string> searchLoc = new List<string>() {
            "./Workers", "C:\\Projects\\JPD\\BBRepos\\ComputeFarm\\TestWorker\\bin\\Debug"
        }; // sdir of the executable??

        string resultQueue;
        string routeKey;
        public string requestType;

        HostWrapper workerProcess;

        public DateTime creationTme;
        public DateTime executionStartTime;
        public DateTime executionCompleteTime;

        public TimeSpan currentRunTime;
        public TimeSpan executionTimeout;

        public WorkerStatus status { get { return /*###*/WorkerStatus.Running; } }
        public void Kill() { if (workerProcess != null) /*###*/workerProcess = null; }
        public void Shutdown() { if (workerProcess != null) /*###*/workerProcess = null; }

        public static ComputeWorker WorkerFactory(string typeID)
        {
            /// ### there may be many ways to manage this, but the first one out of the box is:
            ///     an IWorker component in a dll that we can wrap in a WorkWrapper...

            /// I think it's easier from a workflow perspective to find the appropriate dlls for this typeid first
            /// then pass them to the workwrapper for instantiation, rather than start a process and hope it will 
            /// be able to find it.  You still have to get a positive ack that it's running, but if you don't you
            /// know it was an instantiation problem, not a location one...

            /// so....
            /// we actually have a specific ClientWrapper that knows how to talk to the queues
            /// and manipulate a worker object, whose typeID and dll will be passed as parameters
            /// the hostwrapper can be pretty ignorant of that...actually, the hostwrapper could be embedded in the 
            /// actual farm calling client, if a same-machine solution was acceptable.  Having something else manage the 
            /// clientwrappers (workers) allows it to be distributed...

            /// the real question will be, do I need a computeworker, or is it just a hostwrapper?

            Dictionary<string, string> workerLoc = BuildWorkerMap(searchLoc);
            if (workerLoc.Keys.Contains(typeID))
            {
                //HostWrapper workerShell = new HostWrapper( );


                /// ### HostWrapper workerShell = new HostWrapper();
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

        static Dictionary<string, string> BuildWorkerMap(List<string> dirToSearch)
        {
            // find the typeID
            Dictionary<string, string> outDict = new Dictionary<string, string>();
            foreach (string dir in dirToSearch)
            {
                try
                {
                    System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(dir);
                    System.IO.FileInfo[] fis = di.GetFiles("*.dll");
                    foreach (System.IO.FileInfo fi in fis)
                    {
                        try
                        {
                            Assembly myTempAssy = System.Reflection.Assembly.LoadFrom(fi.FullName);
                            foreach (Type t in myTempAssy.GetTypes())
                                if (t.GetInterface(typeof(IWorker).FullName) != null )
                                    outDict.Add(t.FullName, fi.FullName);
                        }
                        catch (System.Reflection.ReflectionTypeLoadException e)
                        {
                            foreach( Exception ex1 in e.LoaderExceptions )
                            {
                                Console.WriteLine("--LoaderExceptions: " + ex1.Message);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
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
                    workerProcess = null;   // ### workerProcess.Kill();
                }
            }
        }
        public bool Start(ComputeRequest cr, ComputeFarmWorkerProxy.WorkerCompleteHandler handler)
        {
            workerProcess.Start();
            return true;    // ###
        }
    }
}
