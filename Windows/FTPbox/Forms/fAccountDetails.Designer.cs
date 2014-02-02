namespace FTPbox.Forms
{
    partial class fAccountDetails
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fAccountDetails));
            this.gAccount = new System.Windows.Forms.GroupBox();
            this.lMode = new System.Windows.Forms.Label();
            this.labMode = new System.Windows.Forms.Label();
            this.lPort = new System.Windows.Forms.Label();
            this.lHost = new System.Windows.Forms.Label();
            this.lUsername = new System.Windows.Forms.Label();
            this.labPort = new System.Windows.Forms.Label();
            this.labUN = new System.Windows.Forms.Label();
            this.labHost = new System.Windows.Forms.Label();
            this.gPaths = new System.Windows.Forms.GroupBox();
            this.tParent = new System.Windows.Forms.TextBox();
            this.labFullPath = new System.Windows.Forms.Label();
            this.lLocPath = new System.Windows.Forms.Label();
            this.lRemPath = new System.Windows.Forms.Label();
            this.labLocPath = new System.Windows.Forms.Label();
            this.labRemPath = new System.Windows.Forms.Label();
            this.bDone = new System.Windows.Forms.Button();
            this.gAccount.SuspendLayout();
            this.gPaths.SuspendLayout();
            this.SuspendLayout();
            // 
            // gAccount
            // 
            this.gAccount.AccessibleDescription = "";
            this.gAccount.AccessibleName = "";
            this.gAccount.Controls.Add(this.lMode);
            this.gAccount.Controls.Add(this.labMode);
            this.gAccount.Controls.Add(this.lPort);
            this.gAccount.Controls.Add(this.lHost);
            this.gAccount.Controls.Add(this.lUsername);
            this.gAccount.Controls.Add(this.labPort);
            this.gAccount.Controls.Add(this.labUN);
            this.gAccount.Controls.Add(this.labHost);
            this.gAccount.Location = new System.Drawing.Point(12, 12);
            this.gAccount.Name = "gAccount";
            this.gAccount.Size = new System.Drawing.Size(447, 112);
            this.gAccount.TabIndex = 5;
            this.gAccount.TabStop = false;
            this.gAccount.Text = "Account";
            // 
            // lMode
            // 
            this.lMode.AccessibleDescription = "";
            this.lMode.Location = new System.Drawing.Point(92, 83);
            this.lMode.Name = "lMode";
            this.lMode.Size = new System.Drawing.Size(274, 13);
            this.lMode.TabIndex = 13;
            this.lMode.Text = "N/A";
            // 
            // labMode
            // 
            this.labMode.AccessibleDescription = "";
            this.labMode.Location = new System.Drawing.Point(6, 83);
            this.labMode.Name = "labMode";
            this.labMode.Size = new System.Drawing.Size(435, 13);
            this.labMode.TabIndex = 12;
            this.labMode.Text = "Mode:";
            // 
            // lPort
            // 
            this.lPort.AccessibleDescription = "";
            this.lPort.Location = new System.Drawing.Point(92, 63);
            this.lPort.Name = "lPort";
            this.lPort.Size = new System.Drawing.Size(274, 13);
            this.lPort.TabIndex = 10;
            this.lPort.Text = "N/A";
            // 
            // lHost
            // 
            this.lHost.AccessibleDescription = "";
            this.lHost.Location = new System.Drawing.Point(92, 23);
            this.lHost.Name = "lHost";
            this.lHost.Size = new System.Drawing.Size(274, 13);
            this.lHost.TabIndex = 9;
            this.lHost.Text = "N/A";
            // 
            // lUsername
            // 
            this.lUsername.AccessibleDescription = "";
            this.lUsername.Location = new System.Drawing.Point(92, 43);
            this.lUsername.Name = "lUsername";
            this.lUsername.Size = new System.Drawing.Size(274, 13);
            this.lUsername.TabIndex = 8;
            this.lUsername.Text = "N/A";
            // 
            // labPort
            // 
            this.labPort.AccessibleDescription = "";
            this.labPort.Location = new System.Drawing.Point(6, 63);
            this.labPort.Name = "labPort";
            this.labPort.Size = new System.Drawing.Size(435, 13);
            this.labPort.TabIndex = 6;
            this.labPort.Text = "Port:";
            // 
            // labUN
            // 
            this.labUN.AccessibleDescription = "";
            this.labUN.Location = new System.Drawing.Point(6, 43);
            this.labUN.Name = "labUN";
            this.labUN.Size = new System.Drawing.Size(435, 13);
            this.labUN.TabIndex = 5;
            this.labUN.Text = "Username:";
            // 
            // labHost
            // 
            this.labHost.AccessibleDescription = "";
            this.labHost.Location = new System.Drawing.Point(6, 23);
            this.labHost.Name = "labHost";
            this.labHost.Size = new System.Drawing.Size(435, 13);
            this.labHost.TabIndex = 4;
            this.labHost.Text = "Host:";
            // 
            // gPaths
            // 
            this.gPaths.AccessibleDescription = "";
            this.gPaths.Controls.Add(this.tParent);
            this.gPaths.Controls.Add(this.labFullPath);
            this.gPaths.Controls.Add(this.lLocPath);
            this.gPaths.Controls.Add(this.lRemPath);
            this.gPaths.Controls.Add(this.labLocPath);
            this.gPaths.Controls.Add(this.labRemPath);
            this.gPaths.Location = new System.Drawing.Point(12, 130);
            this.gPaths.Name = "gPaths";
            this.gPaths.Size = new System.Drawing.Size(447, 159);
            this.gPaths.TabIndex = 4;
            this.gPaths.TabStop = false;
            this.gPaths.Text = "Details";
            // 
            // tParent
            // 
            this.tParent.AccessibleDescription = "";
            this.tParent.AccessibleName = "account\'s http path";
            this.tParent.Location = new System.Drawing.Point(29, 125);
            this.tParent.Name = "tParent";
            this.tParent.Size = new System.Drawing.Size(400, 20);
            this.tParent.TabIndex = 7;
            this.tParent.TextChanged += new System.EventHandler(this.tParent_TextChanged);
            // 
            // labFullPath
            // 
            this.labFullPath.AccessibleDescription = "";
            this.labFullPath.Location = new System.Drawing.Point(6, 105);
            this.labFullPath.Name = "labFullPath";
            this.labFullPath.Size = new System.Drawing.Size(437, 13);
            this.labFullPath.TabIndex = 8;
            this.labFullPath.Text = "HTTP path:";
            // 
            // lLocPath
            // 
            this.lLocPath.AccessibleDescription = "";
            this.lLocPath.Location = new System.Drawing.Point(26, 85);
            this.lLocPath.Name = "lLocPath";
            this.lLocPath.Size = new System.Drawing.Size(397, 13);
            this.lLocPath.TabIndex = 6;
            this.lLocPath.Text = "N/A";
            // 
            // lRemPath
            // 
            this.lRemPath.AccessibleDescription = "";
            this.lRemPath.Location = new System.Drawing.Point(26, 43);
            this.lRemPath.Name = "lRemPath";
            this.lRemPath.Size = new System.Drawing.Size(397, 13);
            this.lRemPath.TabIndex = 5;
            this.lRemPath.Text = "N/A";
            // 
            // labLocPath
            // 
            this.labLocPath.AccessibleDescription = "";
            this.labLocPath.Location = new System.Drawing.Point(6, 63);
            this.labLocPath.Name = "labLocPath";
            this.labLocPath.Size = new System.Drawing.Size(435, 13);
            this.labLocPath.TabIndex = 1;
            this.labLocPath.Text = "Local Path:";
            // 
            // labRemPath
            // 
            this.labRemPath.AccessibleDescription = "";
            this.labRemPath.Location = new System.Drawing.Point(6, 23);
            this.labRemPath.Name = "labRemPath";
            this.labRemPath.Size = new System.Drawing.Size(435, 13);
            this.labRemPath.TabIndex = 0;
            this.labRemPath.Text = "Remote Path:";
            // 
            // bDone
            // 
            this.bDone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bDone.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.bDone.Location = new System.Drawing.Point(370, 300);
            this.bDone.Name = "bDone";
            this.bDone.Size = new System.Drawing.Size(89, 23);
            this.bDone.TabIndex = 14;
            this.bDone.Text = "Done";
            this.bDone.UseVisualStyleBackColor = true;
            this.bDone.Click += new System.EventHandler(this.bDone_Click);
            // 
            // fAccountDetails
            // 
            this.AcceptButton = this.bDone;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(469, 335);
            this.Controls.Add(this.bDone);
            this.Controls.Add(this.gAccount);
            this.Controls.Add(this.gPaths);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "fAccountDetails";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "fAccountDetails";
            this.Load += new System.EventHandler(this.fAccountDetails_Load);
            this.RightToLeftLayoutChanged += new System.EventHandler(this.fAccountDetails_RightToLeftLayoutChanged);
            this.gAccount.ResumeLayout(false);
            this.gPaths.ResumeLayout(false);
            this.gPaths.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gAccount;
        private System.Windows.Forms.Label lMode;
        private System.Windows.Forms.Label labMode;
        private System.Windows.Forms.Label lPort;
        private System.Windows.Forms.Label lHost;
        private System.Windows.Forms.Label lUsername;
        private System.Windows.Forms.Label labPort;
        private System.Windows.Forms.Label labUN;
        private System.Windows.Forms.Label labHost;
        private System.Windows.Forms.GroupBox gPaths;
        private System.Windows.Forms.TextBox tParent;
        private System.Windows.Forms.Label labFullPath;
        private System.Windows.Forms.Label lLocPath;
        private System.Windows.Forms.Label lRemPath;
        private System.Windows.Forms.Label labLocPath;
        private System.Windows.Forms.Label labRemPath;
        private System.Windows.Forms.Button bDone;
    }
}