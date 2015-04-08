using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using ComputeFarm;
namespace ComputeFarmService
{
    public partial class ComputeFarmService : ServiceBase
    {
        System.Timers.Timer baseTimer;
        System.Diagnostics.EventLog myEventLog = new System.Diagnostics.EventLog();

        ComputeFarm.ComputeFarm thisFarm;

        public ComputeFarmService()
        {
            InitializeComponent();
            InitalizeLog();
        }

        private void InitalizeLog()
        {
            myEventLog = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("Compute Farm Audit"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "Compute Farm Audit", "Compute Farm");
            }

            myEventLog.Source = "Compute Farm Audit";
            myEventLog.Log = "Compute Farm";
        }

        protected override void OnStart(string[] args)
        {
            myEventLog.WriteEntry("Service Starting");
            baseTimer = new System.Timers.Timer();
            baseTimer.Interval = 1000;
            baseTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            baseTimer.Start();
            myEventLog.WriteEntry("Service Started");

            thisFarm = new ComputeFarm.ComputeFarm(myEventLog);
            thisFarm.Init();
        }

        protected override void OnStop()
        {
            myEventLog.WriteEntry("Service Stopping");
            baseTimer.Stop();
            thisFarm.Shutdown();
            myEventLog.WriteEntry("Service Stopped");
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            //myEventLog.WriteEntry("Ticking");

            thisFarm.CheckControlRequests();
        }
    }
}
