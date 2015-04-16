using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ComputeFarm
{
    public class FarmSettings
    {
        string settingsName;

        string host;
        string UID;
        string PWD;
        int port;

        string exch;
        string clientID;

        public string Name { get { return settingsName; } }

        public string Host { get { return host; } }
        public string Uid { get { return UID; } }
        public string Pwd { get { return PWD; } }
        public int Port { get { return port; } }

        public string Exch { get { return exch; } set { exch = value; } }
        public string ClientID { get { return clientID; } set { clientID = value; } }

        static public Dictionary<string, FarmSettings> SettingsFactory()
        {
            Dictionary<string, FarmSettings> outDict = new Dictionary<string, FarmSettings>();
            XmlDocument settingsDoc = new XmlDocument();
            settingsDoc.Load("FarmSettings.xml");

            XmlNodeList farmNodes = settingsDoc.SelectNodes("/Settings/Farm");
            foreach (XmlNode n in farmNodes)
            {
                FarmSettings s = new FarmSettings(n);
                outDict.Add(s.settingsName, s);
            }
            return outDict;
        }
        static public FarmSettings SettingsFactory(string which)
        {
            return SettingsFactory()[which];
        }

        public FarmSettings()
        {
            settingsName = "default";

            host = "localhost";
            UID = "guest";
            PWD = "guest";
            port = 5672;

            exch = "refExch";
            clientID = Guid.NewGuid().ToString();
        }
        public FarmSettings(XmlNode n)
        {
            settingsName = n.Attributes["name"].Value;

            host = n.Attributes["host"].Value;
            UID = n.Attributes["uid"].Value;
            PWD = n.Attributes["pwd"].Value;
            port = Convert.ToInt32(n.Attributes["port"].Value);

            exch = n.Attributes["exch"].Value;
            clientID = n.Attributes["clientID"].Value;
        }
        public FarmSettings(string setName, string thisHost, string thisUID, string thisPass, int thisPort)
        {
            settingsName = setName;

            host = thisHost;
            UID = thisUID;
            PWD = thisPass;
            port = thisPort;

            exch = "";
            clientID = "";
        }
        public FarmSettings(string setName, string thisHost, string thisUID, string thisPass, int thisPort, string thisExch, string thisClientID)
        {
            settingsName = setName;

            host = thisHost;
            UID = thisUID;
            PWD = thisPass;
            port = thisPort;

            exch = thisExch;
            clientID = thisClientID;
        }
    }
}
