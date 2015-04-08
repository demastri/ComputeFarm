namespace ComputeFarmService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ComputeFarmServiceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.ComputeFarmServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // ComputeFarmServiceProcessInstaller1
            // 
            this.ComputeFarmServiceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.ComputeFarmServiceProcessInstaller1.Password = null;
            this.ComputeFarmServiceProcessInstaller1.Username = null;
            // 
            // ComputeFarmServiceInstaller
            // 
            this.ComputeFarmServiceInstaller.Description = "Helper for distributed computing support";
            this.ComputeFarmServiceInstaller.DisplayName = "Compute Farm";
            this.ComputeFarmServiceInstaller.ServiceName = "ComputeFarm";
            this.ComputeFarmServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.ComputeFarmServiceProcessInstaller1,
            this.ComputeFarmServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller ComputeFarmServiceProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller ComputeFarmServiceInstaller;
    }
}