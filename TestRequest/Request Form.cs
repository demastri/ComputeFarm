using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ComputeFarm;
using ComputeFarmProxy;

namespace TestRequest
{
    public partial class RequestForm : Form
    {
        FarmSettings fs = null;

        ComputeFarm.ComputeFarm actFarm;

        ComputeFarmProxy.ComputeFarmProxy myFarmProxy;
        WorkerHandle printSortWorker;
        WorkerHandle randomTaskWorker;
        string thisClientID;
        List<string> openRequests;
        List<string> openReplies;

        public RequestForm()
        {
            InitializeComponent();
            myFarmProxy = null;
            thisClientID = "";
            openRequests = new List<string>();
            openReplies = new List<string>();
        }

        private void GetFarmStatusButton_Click(object sender, EventArgs e)
        {
            // the magic config settings are exchange/queue/port/uid/pwd/host/routekey
            // this should ask a running service for its currently established queues/workers
            // it should get an acknowledgement, and dialog up the results

            ComputeFarmProxy.ComputeFarmStatus curStatus = myFarmProxy.RequestStatus();
            statusList.Text = curStatus.ToString();
        }

        private void RequestFarmButton_Click(object sender, EventArgs e)
        {
            // ### connect to farm
            if (thisClientID == "")
            {
                // the magic config settings are exchange/queue/port/uid/pwd
                // this should ask a running service for 4 engines of TestWorker.Worker
                // it should get an acknowledgement, and dialog up the results

                if (fs == null)
                    fs = FarmSettings.SettingsFactory("local");
                fs.ClientID = thisClientID = Guid.NewGuid().ToString();
                myFarmProxy = ComputeFarmProxy.ComputeFarmProxy.ConnectToFarm(fs);
                if (myFarmProxy != null && myFarmProxy.IsOpen)
                    MessageBox.Show("Farm Connected OK");
                else
                {
                    MessageBox.Show("Could not connect to farm");
                    thisClientID = "";
                    myFarmProxy = null;
                }
            }
            else
            {
                MessageBox.Show("there is already an open connection to the farm...");
            }
        }

        private void ConnectFabricButton_Click(object sender, EventArgs e)
        {
            // connect to fabric
            printSortWorker = myFarmProxy.ConnectWorkerFabric("TestWorker.PrintSort", 4, UpdateTask1Handler, ResultTask1Handler);
            randomTaskWorker = myFarmProxy.ConnectWorkerFabric("TestWorker.RandomTask", 4, UpdateTask2Handler, ResultTask2Handler);

            if (printSortWorker != null && randomTaskWorker != null)
                MessageBox.Show("Fabric Connected OK");
            else
            {
                MessageBox.Show("Could not connect to fabric");
                thisClientID = "";
                printSortWorker = randomTaskWorker = null;
            }
            GetFarmStatusButton_Click(sender, e);
        }

        private void DisconnectFabricButton_Click(object sender, EventArgs e)
        {
            // disconnect fabric
            myFarmProxy.Shutdown();
            myFarmProxy = null;
            MessageBox.Show("Farm proxy closed");
        }
        private void DisconnectFarmButton_Click(object sender, EventArgs e)
        {
            if (myFarmProxy != null)
                DisconnectFabricButton_Click(sender, e);

            // disconnect farm
            actFarm.Shutdown();
            actFarm = null;
            MessageBox.Show("Farm instance closed");
        }

        List<string> workStrings = new List<string>() { "joe", "bill", "fred" };

        private void DoWorkButton_Click(object sender, EventArgs e)
        {
            // ### do work

            // once the service is established, this button should send 10 requests for service to the farm
            // once all return, it should dialog up the results
            sortedResultList1.Text = "";

            foreach (string s in workStrings)
            {
                sortedResultList1.Text += s + Environment.NewLine;
                string wr = GenerateWorkRequestString(s, openRequests.Count);
                openRequests.Add(wr);
                myFarmProxy.RequestWork(printSortWorker, wr);
            }
            sortedResultList1.Text += "=====================" + Environment.NewLine;
        }
        private string GenerateWorkRequestString(string s, int seed)
        {
            return String.Format("|{0}|{1}|", seed, s);
        }
        private void UpdateTask1Handler(string update)
        {
            // show current running status messages on line (append to text)
            sortedResultList1.Text += update;
        }
        private void ResultTask1Handler(string result)
        {
            // clear this status line and write the returned detail
            int i = sortedResultList1.Text.LastIndexOf(Environment.NewLine);
            sortedResultList1.Text = sortedResultList1.Text.Substring(0, i + 1) + result + Environment.NewLine;
        }
        private void UpdateTask2Handler(string update)
        {
            // show current running status messages on line (append to text)
            sortedResultList2.Text += update;
        }
        private void ResultTask2Handler(string result)
        {
            // clear this status line and write the returned detail
            int i = sortedResultList2.Text.LastIndexOf(Environment.NewLine);
            sortedResultList2.Text = sortedResultList2.Text.Substring(0, i + 1) + result + Environment.NewLine;
        }

        private void InitFarmButton_Click(object sender, EventArgs e)
        {
            if (actFarm != null)
            {
                MessageBox.Show("there is already an open farm instance ...");
                return;
            }
            try
            {
                if (fs == null)
                    fs = FarmSettings.SettingsFactory()["local"];

                actFarm = new ComputeFarm.ComputeFarm(fs, null);
                actFarm.Init();
            }
            catch (Exception exc)
            {
            }
            if (actFarm != null && actFarm.IsOpen)
                MessageBox.Show("Local Farm Instance Is OK");
            else
            {
                MessageBox.Show("Could not create local farm");
                thisClientID = "";
                actFarm = null;
            }
        }

        private void RequestForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (myFarmProxy != null)
                myFarmProxy.Shutdown();
            if (actFarm != null)
                actFarm.Shutdown();
        }
    }
}
