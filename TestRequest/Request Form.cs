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
        ComputeFarm.ComputeFarm actFarm;

        ComputeFarmProxy.ComputeFarmProxy myFarm;
        WorkerHandle printSortWorker;
        WorkerHandle randomTaskWorker;
        string thisClientID;
        List<string> openRequests;
        List<string> openReplies;

        public RequestForm()
        {
            InitializeComponent();
            myFarm = null;
            thisClientID = "";
            openRequests = new List<string>();
            openReplies = new List<string>();
        }

        private void GetFarmStatusButton_Click(object sender, EventArgs e)
        {
            // the magic config settings are exchange/queue/port/uid/pwd/host/routekey
            // this should ask a running service for its currently established queues/workers
            // it should get an acknowledgement, and dialog up the results

            ComputeFarmProxy.ComputeFarmStatus curStatus = myFarm.RequestStatus();
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

                thisClientID = Guid.NewGuid().ToString();
                myFarm = ComputeFarmProxy.ComputeFarmProxy.ConnectToFarm(5672, "localhost", "ComputeFarm", "guest", "guest", thisClientID);
                if (myFarm != null && myFarm.IsOpen)
                    MessageBox.Show("Farm Connected OK");
                else
                {
                    MessageBox.Show("Could not connect to farm");
                    thisClientID = "";
                    myFarm = null;
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
            printSortWorker = myFarm.ConnectWorkerFabric("TestWorker.PrintSort", 4, UpdateTask1Handler, ResultTask1Handler);
            randomTaskWorker = myFarm.ConnectWorkerFabric("TestWorker.RandomTask", 4, UpdateTask2Handler, ResultTask2Handler);

            if (printSortWorker != null && randomTaskWorker != null )
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
            // ### disconnect fabric
        }
        private void DisconnectFarmButton_Click(object sender, EventArgs e)
        {
            // ### disconnect farm
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
                myFarm.RequestWork(printSortWorker, wr);
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
            try
            {
                actFarm = new ComputeFarm.ComputeFarm(null);
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
    }
}
