using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputeFarmWorkerProxy
{
    public enum WorkerStatus { Init, Idle, Running, Completed, Timeout, Error, Shutdown };
    public delegate void WorkerUpdateHandler(string result);
    public delegate void WorkerCompleteHandler(string result);

    public enum StringifiedParameters
    {
        HostOffset = 0,
        ExchOffset = 1,
        UidOffset = 2,
        PwdOffset = 3,
        QueueBaseNameOffset = 4,
        RouteKeyOffset = 5,
        PortOffset = 6,
        WorkerDirOffset = 7,
        TypeIDOffset = 8
    };


    public interface IWorker
    {
        event WorkerUpdateHandler WorkerUpdateEvent;
        event WorkerCompleteHandler WorkerCompleteEvent;

        void Init();
        bool Start(string workRequest);
        bool Kill();
        void Shutdown();

        WorkerStatus GetStatus();
        string GetWorkResult();
    }
}
