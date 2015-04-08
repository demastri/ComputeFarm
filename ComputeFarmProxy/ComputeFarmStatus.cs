using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputeFarmProxy
{
    public class ComputeFarmStatus
    {
        List<FabricManager> exchanges;
        public void AddExch(FabricManager ed) { exchanges.Add(ed); }
        public bool Connected
        {
            get
            {
                return GetConnectedExchange() != null;
            }
        }
        public FabricManager GetConnectedExchange()
        {
            foreach (FabricManager ed in exchanges)
                if (ed.IsOpen)
                    return ed;
            return null;
        }

        public ComputeFarmStatus()
        {
            exchanges = new List<FabricManager>();
        }

        internal FabricManager FindExchange(FabricManager exch)
        {
            foreach (FabricManager ed in exchanges)
                if (ed == exch)
                    return ed;
            return null;
        }

        public override string ToString()
        {
            string outString = "Exchange Count: " + exchanges.Count.ToString();
            foreach (FabricManager ed in exchanges)
            {
                outString += ed.ToString() + Environment.NewLine;
            }
            return outString;
        }
    }
}
