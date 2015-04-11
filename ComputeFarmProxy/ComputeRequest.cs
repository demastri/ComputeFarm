using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputeFarm
{
    public class ComputeRequest
    {
        public string requestType;      // Init, etc
        public string messageID;        // guid string val
        public string commandString;    // actual sent string
        public string replyString;      // actual received string
        public string replyType;

        public bool IsAck { get { return replyType == "Ack"; } }
        public bool HasReply { get { return replyString != null; } }

        public ComputeRequest(string reqType)
        {
            replyString = null;
            messageID = System.Guid.NewGuid().ToString();
            replyType = "";
            commandString = reqType + "|" + messageID + "|" + " " + "|" + "0";
        }
        public ComputeRequest(string reqType, string strParameter, int intParameter)
        {
            replyString = null;
            messageID = System.Guid.NewGuid().ToString();
            replyType = "";
            commandString = reqType + "|" + messageID + "|" + strParameter + "|" + intParameter.ToString();
        }

    }
}
