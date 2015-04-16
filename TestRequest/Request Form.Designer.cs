namespace TestRequest
{
    partial class RequestForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.RequestFarmButton = new System.Windows.Forms.Button();
            this.GetFarmStatusButton = new System.Windows.Forms.Button();
            this.DoWorkButton = new System.Windows.Forms.Button();
            this.sortedResultList1 = new System.Windows.Forms.TextBox();
            this.DisconnectButton = new System.Windows.Forms.Button();
            this.DisconnectFarmButton = new System.Windows.Forms.Button();
            this.ConnectFabricButton = new System.Windows.Forms.Button();
            this.sortedResultList2 = new System.Windows.Forms.TextBox();
            this.statusList = new System.Windows.Forms.TextBox();
            this.InitFarmButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // RequestFarmButton
            // 
            this.RequestFarmButton.Location = new System.Drawing.Point(248, 41);
            this.RequestFarmButton.Name = "RequestFarmButton";
            this.RequestFarmButton.Size = new System.Drawing.Size(105, 23);
            this.RequestFarmButton.TabIndex = 0;
            this.RequestFarmButton.Text = "Connect to Farm";
            this.RequestFarmButton.UseVisualStyleBackColor = true;
            this.RequestFarmButton.Click += new System.EventHandler(this.RequestFarmButton_Click);
            // 
            // GetFarmStatusButton
            // 
            this.GetFarmStatusButton.Location = new System.Drawing.Point(248, 418);
            this.GetFarmStatusButton.Name = "GetFarmStatusButton";
            this.GetFarmStatusButton.Size = new System.Drawing.Size(105, 23);
            this.GetFarmStatusButton.TabIndex = 1;
            this.GetFarmStatusButton.Text = "Get Status";
            this.GetFarmStatusButton.UseVisualStyleBackColor = true;
            this.GetFarmStatusButton.Click += new System.EventHandler(this.GetFarmStatusButton_Click);
            // 
            // DoWorkButton
            // 
            this.DoWorkButton.Location = new System.Drawing.Point(248, 99);
            this.DoWorkButton.Name = "DoWorkButton";
            this.DoWorkButton.Size = new System.Drawing.Size(105, 23);
            this.DoWorkButton.TabIndex = 2;
            this.DoWorkButton.Text = "Do Work";
            this.DoWorkButton.UseVisualStyleBackColor = true;
            this.DoWorkButton.Click += new System.EventHandler(this.DoWorkButton_Click);
            // 
            // sortedResultList1
            // 
            this.sortedResultList1.Location = new System.Drawing.Point(13, 12);
            this.sortedResultList1.Multiline = true;
            this.sortedResultList1.Name = "sortedResultList1";
            this.sortedResultList1.Size = new System.Drawing.Size(229, 139);
            this.sortedResultList1.TabIndex = 3;
            // 
            // DisconnectButton
            // 
            this.DisconnectButton.Location = new System.Drawing.Point(248, 128);
            this.DisconnectButton.Name = "DisconnectButton";
            this.DisconnectButton.Size = new System.Drawing.Size(105, 23);
            this.DisconnectButton.TabIndex = 4;
            this.DisconnectButton.Text = "Disconnect Fabric";
            this.DisconnectButton.UseVisualStyleBackColor = true;
            this.DisconnectButton.Click += new System.EventHandler(this.DisconnectFabricButton_Click);
            // 
            // DisconnectFarmButton
            // 
            this.DisconnectFarmButton.Location = new System.Drawing.Point(248, 157);
            this.DisconnectFarmButton.Name = "DisconnectFarmButton";
            this.DisconnectFarmButton.Size = new System.Drawing.Size(105, 23);
            this.DisconnectFarmButton.TabIndex = 5;
            this.DisconnectFarmButton.Text = "Disconnect Farm";
            this.DisconnectFarmButton.UseVisualStyleBackColor = true;
            this.DisconnectFarmButton.Click += new System.EventHandler(this.DisconnectFarmButton_Click);
            // 
            // ConnectFabricButton
            // 
            this.ConnectFabricButton.Location = new System.Drawing.Point(248, 70);
            this.ConnectFabricButton.Name = "ConnectFabricButton";
            this.ConnectFabricButton.Size = new System.Drawing.Size(105, 23);
            this.ConnectFabricButton.TabIndex = 6;
            this.ConnectFabricButton.Text = "Connect Fabric";
            this.ConnectFabricButton.UseVisualStyleBackColor = true;
            this.ConnectFabricButton.Click += new System.EventHandler(this.ConnectFabricButton_Click);
            // 
            // sortedResultList2
            // 
            this.sortedResultList2.Location = new System.Drawing.Point(13, 157);
            this.sortedResultList2.Multiline = true;
            this.sortedResultList2.Name = "sortedResultList2";
            this.sortedResultList2.Size = new System.Drawing.Size(229, 139);
            this.sortedResultList2.TabIndex = 7;
            // 
            // statusList
            // 
            this.statusList.Location = new System.Drawing.Point(12, 302);
            this.statusList.Multiline = true;
            this.statusList.Name = "statusList";
            this.statusList.Size = new System.Drawing.Size(229, 139);
            this.statusList.TabIndex = 8;
            // 
            // InitFarmButton
            // 
            this.InitFarmButton.Location = new System.Drawing.Point(248, 12);
            this.InitFarmButton.Name = "InitFarmButton";
            this.InitFarmButton.Size = new System.Drawing.Size(105, 23);
            this.InitFarmButton.TabIndex = 9;
            this.InitFarmButton.Text = "Init Farm";
            this.InitFarmButton.UseVisualStyleBackColor = true;
            this.InitFarmButton.Click += new System.EventHandler(this.InitFarmButton_Click);
            // 
            // RequestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(365, 453);
            this.Controls.Add(this.InitFarmButton);
            this.Controls.Add(this.statusList);
            this.Controls.Add(this.sortedResultList2);
            this.Controls.Add(this.ConnectFabricButton);
            this.Controls.Add(this.DisconnectFarmButton);
            this.Controls.Add(this.DisconnectButton);
            this.Controls.Add(this.sortedResultList1);
            this.Controls.Add(this.DoWorkButton);
            this.Controls.Add(this.GetFarmStatusButton);
            this.Controls.Add(this.RequestFarmButton);
            this.Name = "RequestForm";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RequestForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button RequestFarmButton;
        private System.Windows.Forms.Button GetFarmStatusButton;
        private System.Windows.Forms.Button DoWorkButton;
        private System.Windows.Forms.TextBox sortedResultList1;
        private System.Windows.Forms.Button DisconnectButton;
        private System.Windows.Forms.Button DisconnectFarmButton;
        private System.Windows.Forms.Button ConnectFabricButton;
        private System.Windows.Forms.TextBox sortedResultList2;
        private System.Windows.Forms.TextBox statusList;
        private System.Windows.Forms.Button InitFarmButton;
    }
}

