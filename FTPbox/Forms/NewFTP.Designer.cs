namespace FTPbox
{
    partial class NewFTP
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewFTP));
            this.bDone = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.gDetails = new System.Windows.Forms.GroupBox();
            this.labEncryption = new System.Windows.Forms.Label();
            this.cEncryption = new System.Windows.Forms.ComboBox();
            this.cMode = new System.Windows.Forms.ComboBox();
            this.labMode = new System.Windows.Forms.Label();
            this.nPort = new System.Windows.Forms.NumericUpDown();
            this.labPort = new System.Windows.Forms.Label();
            this.labHost = new System.Windows.Forms.Label();
            this.tHost = new System.Windows.Forms.TextBox();
            this.labPass = new System.Windows.Forms.Label();
            this.labUN = new System.Windows.Forms.Label();
            this.tPass = new System.Windows.Forms.TextBox();
            this.tUsername = new System.Windows.Forms.TextBox();
            this.gDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nPort)).BeginInit();
            this.SuspendLayout();
            // 
            // bDone
            // 
            this.bDone.AccessibleName = "Done";
            this.bDone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bDone.Location = new System.Drawing.Point(221, 206);
            this.bDone.Name = "bDone";
            this.bDone.Size = new System.Drawing.Size(75, 23);
            this.bDone.TabIndex = 4;
            this.bDone.Text = "Done";
            this.bDone.UseVisualStyleBackColor = true;
            this.bDone.Click += new System.EventHandler(this.bDone_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // gDetails
            // 
            this.gDetails.AccessibleName = "FTP Details";
            this.gDetails.Controls.Add(this.labEncryption);
            this.gDetails.Controls.Add(this.cEncryption);
            this.gDetails.Controls.Add(this.cMode);
            this.gDetails.Controls.Add(this.labMode);
            this.gDetails.Controls.Add(this.nPort);
            this.gDetails.Controls.Add(this.labPort);
            this.gDetails.Controls.Add(this.labHost);
            this.gDetails.Controls.Add(this.tHost);
            this.gDetails.Controls.Add(this.labPass);
            this.gDetails.Controls.Add(this.labUN);
            this.gDetails.Controls.Add(this.tPass);
            this.gDetails.Controls.Add(this.tUsername);
            this.gDetails.Location = new System.Drawing.Point(11, 12);
            this.gDetails.Name = "gDetails";
            this.gDetails.Size = new System.Drawing.Size(286, 188);
            this.gDetails.TabIndex = 32;
            this.gDetails.TabStop = false;
            this.gDetails.Text = "FTP Login Details";
            // 
            // labEncryption
            // 
            this.labEncryption.AutoSize = true;
            this.labEncryption.Location = new System.Drawing.Point(6, 54);
            this.labEncryption.Name = "labEncryption";
            this.labEncryption.Size = new System.Drawing.Size(60, 13);
            this.labEncryption.TabIndex = 26;
            this.labEncryption.Text = "Encryption:";
            // 
            // cEncryption
            // 
            this.cEncryption.AccessibleName = "Encryption";
            this.cEncryption.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cEncryption.FormattingEnabled = true;
            this.cEncryption.Items.AddRange(new object[] {
            "None",
            "require explicit FTP over TLS",
            "require implicit FTP over TLS"});
            this.cEncryption.Location = new System.Drawing.Point(100, 51);
            this.cEncryption.Name = "cEncryption";
            this.cEncryption.Size = new System.Drawing.Size(172, 21);
            this.cEncryption.TabIndex = 25;
            // 
            // cMode
            // 
            this.cMode.AccessibleName = "FTP Protocol";
            this.cMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cMode.FormattingEnabled = true;
            this.cMode.Items.AddRange(new object[] {
            "FTP",
            "SFTP"});
            this.cMode.Location = new System.Drawing.Point(100, 24);
            this.cMode.Name = "cMode";
            this.cMode.Size = new System.Drawing.Size(57, 21);
            this.cMode.TabIndex = 24;
            this.cMode.SelectedIndexChanged += new System.EventHandler(this.cMode_SelectedIndexChanged);
            // 
            // labMode
            // 
            this.labMode.AutoSize = true;
            this.labMode.Location = new System.Drawing.Point(6, 27);
            this.labMode.Name = "labMode";
            this.labMode.Size = new System.Drawing.Size(37, 13);
            this.labMode.TabIndex = 23;
            this.labMode.Text = "Mode:";
            // 
            // nPort
            // 
            this.nPort.AccessibleName = "Port";
            this.nPort.Location = new System.Drawing.Point(100, 156);
            this.nPort.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.nPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nPort.Name = "nPort";
            this.nPort.Size = new System.Drawing.Size(57, 20);
            this.nPort.TabIndex = 3;
            this.nPort.Value = new decimal(new int[] {
            21,
            0,
            0,
            0});
            // 
            // labPort
            // 
            this.labPort.AutoSize = true;
            this.labPort.Location = new System.Drawing.Point(6, 159);
            this.labPort.Name = "labPort";
            this.labPort.Size = new System.Drawing.Size(29, 13);
            this.labPort.TabIndex = 22;
            this.labPort.Text = "Port:";
            // 
            // labHost
            // 
            this.labHost.AutoSize = true;
            this.labHost.Location = new System.Drawing.Point(6, 81);
            this.labHost.Name = "labHost";
            this.labHost.Size = new System.Drawing.Size(32, 13);
            this.labHost.TabIndex = 20;
            this.labHost.Text = "Host:";
            // 
            // tHost
            // 
            this.tHost.AccessibleName = "Host";
            this.tHost.Location = new System.Drawing.Point(100, 78);
            this.tHost.Name = "tHost";
            this.tHost.Size = new System.Drawing.Size(172, 20);
            this.tHost.TabIndex = 0;
            // 
            // labPass
            // 
            this.labPass.AutoSize = true;
            this.labPass.Location = new System.Drawing.Point(6, 133);
            this.labPass.Name = "labPass";
            this.labPass.Size = new System.Drawing.Size(56, 13);
            this.labPass.TabIndex = 17;
            this.labPass.Text = "Password:";
            // 
            // labUN
            // 
            this.labUN.AutoSize = true;
            this.labUN.Location = new System.Drawing.Point(6, 107);
            this.labUN.Name = "labUN";
            this.labUN.Size = new System.Drawing.Size(58, 13);
            this.labUN.TabIndex = 16;
            this.labUN.Text = "Username:";
            // 
            // tPass
            // 
            this.tPass.AccessibleName = "Password";
            this.tPass.Location = new System.Drawing.Point(100, 130);
            this.tPass.Name = "tPass";
            this.tPass.PasswordChar = '●';
            this.tPass.Size = new System.Drawing.Size(172, 20);
            this.tPass.TabIndex = 2;
            // 
            // tUsername
            // 
            this.tUsername.AccessibleName = "Username";
            this.tUsername.Location = new System.Drawing.Point(100, 104);
            this.tUsername.Name = "tUsername";
            this.tUsername.Size = new System.Drawing.Size(172, 20);
            this.tUsername.TabIndex = 1;
            // 
            // NewFTP
            // 
            this.AcceptButton = this.bDone;
            this.AccessibleName = "New FTP account";
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 236);
            this.Controls.Add(this.gDetails);
            this.Controls.Add(this.bDone);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewFTP";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add FTP Account";
            this.Load += new System.EventHandler(this.NewFTP_Load);
            this.gDetails.ResumeLayout(false);
            this.gDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nPort)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bDone;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.GroupBox gDetails;
        private System.Windows.Forms.NumericUpDown nPort;
        private System.Windows.Forms.Label labPort;
        private System.Windows.Forms.Label labHost;
        private System.Windows.Forms.TextBox tHost;
        private System.Windows.Forms.Label labPass;
        private System.Windows.Forms.Label labUN;
        private System.Windows.Forms.TextBox tPass;
        private System.Windows.Forms.TextBox tUsername;
        private System.Windows.Forms.ComboBox cMode;
        private System.Windows.Forms.Label labMode;
        private System.Windows.Forms.ComboBox cEncryption;
        private System.Windows.Forms.Label labEncryption;
    }
}