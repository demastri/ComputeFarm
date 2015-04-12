using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QueueCommon;

namespace ComputeFarmProxy
{
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
}
