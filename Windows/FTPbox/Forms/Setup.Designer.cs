namespace FTPbox.Forms
{
    partial class Setup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Setup));
            this.cAskForPass = new System.Windows.Forms.CheckBox();
            this.gLoginDetails = new System.Windows.Forms.GroupBox();
            this.labKeyPath = new System.Windows.Forms.Label();
            this.labColon = new System.Windows.Forms.Label();
            this.cEncryption = new System.Windows.Forms.ComboBox();
            this.cMode = new System.Windows.Forms.ComboBox();
            this.tHost = new System.Windows.Forms.TextBox();
            this.tPass = new System.Windows.Forms.TextBox();
            this.tUsername = new System.Windows.Forms.TextBox();
            this.labEncryption = new System.Windows.Forms.Label();
            this.labMode = new System.Windows.Forms.Label();
            this.nPort = new System.Windows.Forms.NumericUpDown();
            this.labHost = new System.Windows.Forms.Label();
            this.labPass = new System.Windows.Forms.Label();
            this.labUN = new System.Windows.Forms.Label();
            this.gLocalFolder = new System.Windows.Forms.GroupBox();
            this.rDefaultLocalFolder = new System.Windows.Forms.RadioButton();
            this.rCustomLocalFolder = new System.Windows.Forms.RadioButton();
            this.bBrowse = new System.Windows.Forms.Button();
            this.tLocalPath = new System.Windows.Forms.TextBox();
            this.gRemoteFolder = new System.Windows.Forms.GroupBox();
            this.tFullRemotePath = new System.Windows.Forms.TextBox();
            this.labFullPath = new System.Windows.Forms.Label();
            this.tRemoteList = new System.Windows.Forms.TreeView();
            this.gSelectiveSync = new System.Windows.Forms.GroupBox();
            this.rSyncAll = new System.Windows.Forms.RadioButton();
            this.rSyncCustom = new System.Windows.Forms.RadioButton();
            this.bNext = new System.Windows.Forms.Button();
            this.bFinish = new System.Windows.Forms.Button();
            this.bPrevious = new System.Windows.Forms.Button();
            this.gLanguage = new System.Windows.Forms.GroupBox();
            this.labSelectLanguage = new System.Windows.Forms.Label();
            this.cLanguages = new System.Windows.Forms.ComboBox();
            this.gLoginDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nPort)).BeginInit();
            this.gLocalFolder.SuspendLayout();
            this.gRemoteFolder.SuspendLayout();
            this.gSelectiveSync.SuspendLayout();
            this.gLanguage.SuspendLayout();
            this.SuspendLayout();
            // 
            // cAskForPass
            // 
            this.cAskForPass.Location = new System.Drawing.Point(15, 167);
            this.cAskForPass.Name = "cAskForPass";
            this.cAskForPass.Size = new System.Drawing.Size(433, 17);
            this.cAskForPass.TabIndex = 6;
            this.cAskForPass.Text = "Always ask for password";
            this.cAskForPass.UseVisualStyleBackColor = true;
            // 
            // gLoginDetails
            // 
            this.gLoginDetails.AccessibleDescription = "";
            this.gLoginDetails.AccessibleName = "FTP Details";
            this.gLoginDetails.Controls.Add(this.labKeyPath);
            this.gLoginDetails.Controls.Add(this.labColon);
            this.gLoginDetails.Controls.Add(this.cEncryption);
            this.gLoginDetails.Controls.Add(this.cMode);
            this.gLoginDetails.Controls.Add(this.tHost);
            this.gLoginDetails.Controls.Add(this.tPass);
            this.gLoginDetails.Controls.Add(this.cAskForPass);
            this.gLoginDetails.Controls.Add(this.tUsername);
            this.gLoginDetails.Controls.Add(this.labEncryption);
            this.gLoginDetails.Controls.Add(this.labMode);
            this.gLoginDetails.Controls.Add(this.nPort);
            this.gLoginDetails.Controls.Add(this.labHost);
            this.gLoginDetails.Controls.Add(this.labPass);
            this.gLoginDetails.Controls.Add(this.labUN);
            this.gLoginDetails.Location = new System.Drawing.Point(478, 12);
            this.gLoginDetails.Name = "gLoginDetails";
            this.gLoginDetails.Size = new System.Drawing.Size(460, 205);
            this.gLoginDetails.TabIndex = 36;
            this.gLoginDetails.TabStop = false;
            this.gLoginDetails.Text = "FTP Login Details";
            // 
            // labKeyPath
            // 
            this.labKeyPath.Location = new System.Drawing.Point(324, 57);
            this.labKeyPath.Name = "labKeyPath";
            this.labKeyPath.Size = new System.Drawing.Size(124, 13);
            this.labKeyPath.TabIndex = 33;
            this.labKeyPath.Text = "KeyFilePath";
            // 
            // labColon
            // 
            this.labColon.AccessibleDescription = "";
            this.labColon.AccessibleName = "";
            this.labColon.Location = new System.Drawing.Point(375, 84);
            this.labColon.Name = "labColon";
            this.labColon.Size = new System.Drawing.Size(10, 13);
            this.labColon.TabIndex = 32;
            this.labColon.Text = ":";
            // 
            // cEncryption
            // 
            this.cEncryption.AccessibleDescription = "";
            this.cEncryption.AccessibleName = "Encryption";
            this.cEncryption.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cEncryption.FormattingEnabled = true;
            this.cEncryption.Items.AddRange(new object[] {
            "None",
            "require explicit FTP over TLS",
            "require implicit FTP over TLS"});
            this.cEncryption.Location = new System.Drawing.Point(145, 54);
            this.cEncryption.Name = "cEncryption";
            this.cEncryption.Size = new System.Drawing.Size(173, 21);
            this.cEncryption.TabIndex = 1;
            // 
            // cMode
            // 
            this.cMode.AccessibleDescription = "";
            this.cMode.AccessibleName = "FTP Protocol";
            this.cMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cMode.FormattingEnabled = true;
            this.cMode.Items.AddRange(new object[] {
            "FTP",
            "SFTP"});
            this.cMode.Location = new System.Drawing.Point(145, 27);
            this.cMode.Name = "cMode";
            this.cMode.Size = new System.Drawing.Size(57, 21);
            this.cMode.TabIndex = 0;
            this.cMode.SelectedIndexChanged += new System.EventHandler(this.cMode_SelectedIndexChanged);
            // 
            // tHost
            // 
            this.tHost.AccessibleDescription = "";
            this.tHost.AccessibleName = "Host";
            this.tHost.Location = new System.Drawing.Point(145, 81);
            this.tHost.Name = "tHost";
            this.tHost.Size = new System.Drawing.Size(224, 20);
            this.tHost.TabIndex = 2;
            // 
            // tPass
            // 
            this.tPass.AccessibleDescription = "";
            this.tPass.AccessibleName = "Password";
            this.tPass.Location = new System.Drawing.Point(145, 133);
            this.tPass.Name = "tPass";
            this.tPass.PasswordChar = '●';
            this.tPass.Size = new System.Drawing.Size(303, 20);
            this.tPass.TabIndex = 5;
            // 
            // tUsername
            // 
            this.tUsername.AccessibleDescription = "";
            this.tUsername.AccessibleName = "Username";
            this.tUsername.Location = new System.Drawing.Point(145, 107);
            this.tUsername.Name = "tUsername";
            this.tUsername.Size = new System.Drawing.Size(303, 20);
            this.tUsername.TabIndex = 4;
            // 
            // labEncryption
            // 
            this.labEncryption.AccessibleDescription = "";
            this.labEncryption.AccessibleName = "";
            this.labEncryption.Location = new System.Drawing.Point(12, 57);
            this.labEncryption.Name = "labEncryption";
            this.labEncryption.Size = new System.Drawing.Size(436, 13);
            this.labEncryption.TabIndex = 26;
            this.labEncryption.Text = "Encryption:";
            // 
            // labMode
            // 
            this.labMode.AccessibleDescription = "";
            this.labMode.AccessibleName = "";
            this.labMode.Location = new System.Drawing.Point(12, 30);
            this.labMode.Name = "labMode";
            this.labMode.Size = new System.Drawing.Size(436, 13);
            this.labMode.TabIndex = 23;
            this.labMode.Text = "Mode:";
            // 
            // nPort
            // 
            this.nPort.AccessibleName = "Port";
            this.nPort.Location = new System.Drawing.Point(391, 81);
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
            // labHost
            // 
            this.labHost.AccessibleDescription = "";
            this.labHost.AccessibleName = "";
            this.labHost.Location = new System.Drawing.Point(12, 84);
            this.labHost.Name = "labHost";
            this.labHost.Size = new System.Drawing.Size(436, 13);
            this.labHost.TabIndex = 20;
            this.labHost.Text = "Host:";
            // 
            // labPass
            // 
            this.labPass.AccessibleDescription = "";
            this.labPass.AccessibleName = "";
            this.labPass.Location = new System.Drawing.Point(12, 136);
            this.labPass.Name = "labPass";
            this.labPass.Size = new System.Drawing.Size(436, 13);
            this.labPass.TabIndex = 17;
            this.labPass.Text = "Password:";
            // 
            // labUN
            // 
            this.labUN.AccessibleDescription = "";
            this.labUN.AccessibleName = "";
            this.labUN.Location = new System.Drawing.Point(12, 110);
            this.labUN.Name = "labUN";
            this.labUN.Size = new System.Drawing.Size(436, 13);
            this.labUN.TabIndex = 16;
            this.labUN.Text = "Username:";
            // 
            // gLocalFolder
            // 
            this.gLocalFolder.AccessibleDescription = "";
            this.gLocalFolder.AccessibleName = "FTP Details";
            this.gLocalFolder.Controls.Add(this.rDefaultLocalFolder);
            this.gLocalFolder.Controls.Add(this.rCustomLocalFolder);
            this.gLocalFolder.Controls.Add(this.bBrowse);
            this.gLocalFolder.Controls.Add(this.tLocalPath);
            this.gLocalFolder.Location = new System.Drawing.Point(944, 12);
            this.gLocalFolder.Name = "gLocalFolder";
            this.gLocalFolder.Size = new System.Drawing.Size(460, 205);
            this.gLocalFolder.TabIndex = 37;
            this.gLocalFolder.TabStop = false;
            this.gLocalFolder.Text = "Local Folder";
            // 
            // rDefaultLocalFolder
            // 
            this.rDefaultLocalFolder.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.rDefaultLocalFolder.Checked = true;
            this.rDefaultLocalFolder.Location = new System.Drawing.Point(15, 30);
            this.rDefaultLocalFolder.Name = "rDefaultLocalFolder";
            this.rDefaultLocalFolder.Size = new System.Drawing.Size(434, 40);
            this.rDefaultLocalFolder.TabIndex = 0;
            this.rDefaultLocalFolder.TabStop = true;
            this.rDefaultLocalFolder.Text = "I want to use the default local folder";
            this.rDefaultLocalFolder.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.rDefaultLocalFolder.UseVisualStyleBackColor = true;
            this.rDefaultLocalFolder.CheckedChanged += new System.EventHandler(this.rDefaultLocalFolder_CheckedChanged);
            // 
            // rCustomLocalFolder
            // 
            this.rCustomLocalFolder.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.rCustomLocalFolder.Location = new System.Drawing.Point(15, 76);
            this.rCustomLocalFolder.Name = "rCustomLocalFolder";
            this.rCustomLocalFolder.Size = new System.Drawing.Size(434, 40);
            this.rCustomLocalFolder.TabIndex = 1;
            this.rCustomLocalFolder.Text = "I want to select a local folder";
            this.rCustomLocalFolder.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.rCustomLocalFolder.UseVisualStyleBackColor = true;
            this.rCustomLocalFolder.CheckedChanged += new System.EventHandler(this.rCustomLocalFolder_CheckedChanged);
            // 
            // bBrowse
            // 
            this.bBrowse.AccessibleDescription = "";
            this.bBrowse.AccessibleName = "Browse for local folder";
            this.bBrowse.Enabled = false;
            this.bBrowse.Location = new System.Drawing.Point(375, 131);
            this.bBrowse.Name = "bBrowse";
            this.bBrowse.Size = new System.Drawing.Size(74, 23);
            this.bBrowse.TabIndex = 2;
            this.bBrowse.Text = "Browse";
            this.bBrowse.UseVisualStyleBackColor = true;
            this.bBrowse.Click += new System.EventHandler(this.bBrowse_Click);
            // 
            // tLocalPath
            // 
            this.tLocalPath.AccessibleDescription = "";
            this.tLocalPath.AccessibleName = "Local Folder";
            this.tLocalPath.Enabled = false;
            this.tLocalPath.Location = new System.Drawing.Point(15, 133);
            this.tLocalPath.Name = "tLocalPath";
            this.tLocalPath.Size = new System.Drawing.Size(354, 20);
            this.tLocalPath.TabIndex = 69;
            // 
            // gRemoteFolder
            // 
            this.gRemoteFolder.AccessibleDescription = "";
            this.gRemoteFolder.AccessibleName = "FTP Details";
            this.gRemoteFolder.Controls.Add(this.tFullRemotePath);
            this.gRemoteFolder.Controls.Add(this.labFullPath);
            this.gRemoteFolder.Controls.Add(this.tRemoteList);
            this.gRemoteFolder.Location = new System.Drawing.Point(12, 284);
            this.gRemoteFolder.Name = "gRemoteFolder";
            this.gRemoteFolder.Size = new System.Drawing.Size(460, 205);
            this.gRemoteFolder.TabIndex = 73;
            this.gRemoteFolder.TabStop = false;
            this.gRemoteFolder.Text = "Select Directory";
            // 
            // tFullRemotePath
            // 
            this.tFullRemotePath.AccessibleDescription = "";
            this.tFullRemotePath.AccessibleName = "Full Path";
            this.tFullRemotePath.Enabled = false;
            this.tFullRemotePath.Location = new System.Drawing.Point(122, 29);
            this.tFullRemotePath.Name = "tFullRemotePath";
            this.tFullRemotePath.Size = new System.Drawing.Size(323, 20);
            this.tFullRemotePath.TabIndex = 69;
            this.tFullRemotePath.Text = "/";
            // 
            // labFullPath
            // 
            this.labFullPath.AccessibleDescription = "";
            this.labFullPath.AccessibleName = "Full Path";
            this.labFullPath.BackColor = System.Drawing.Color.Transparent;
            this.labFullPath.Location = new System.Drawing.Point(12, 32);
            this.labFullPath.Name = "labFullPath";
            this.labFullPath.Size = new System.Drawing.Size(433, 13);
            this.labFullPath.TabIndex = 68;
            this.labFullPath.Text = "Full Path:";
            // 
            // tRemoteList
            // 
            this.tRemoteList.AccessibleDescription = "";
            this.tRemoteList.AccessibleName = "Select remote path";
            this.tRemoteList.Location = new System.Drawing.Point(15, 55);
            this.tRemoteList.Name = "tRemoteList";
            this.tRemoteList.Size = new System.Drawing.Size(430, 135);
            this.tRemoteList.TabIndex = 67;
            this.tRemoteList.TabStop = false;
            this.tRemoteList.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tRemoteList_AfterCheck);
            this.tRemoteList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tRemoteList_AfterSelect);
            // 
            // gSelectiveSync
            // 
            this.gSelectiveSync.AccessibleDescription = "";
            this.gSelectiveSync.AccessibleName = "FTP Details";
            this.gSelectiveSync.Controls.Add(this.rSyncAll);
            this.gSelectiveSync.Controls.Add(this.rSyncCustom);
            this.gSelectiveSync.Location = new System.Drawing.Point(478, 284);
            this.gSelectiveSync.Name = "gSelectiveSync";
            this.gSelectiveSync.Size = new System.Drawing.Size(460, 205);
            this.gSelectiveSync.TabIndex = 74;
            this.gSelectiveSync.TabStop = false;
            this.gSelectiveSync.Text = "Selective Sync";
            // 
            // rSyncAll
            // 
            this.rSyncAll.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.rSyncAll.Checked = true;
            this.rSyncAll.Location = new System.Drawing.Point(15, 30);
            this.rSyncAll.Name = "rSyncAll";
            this.rSyncAll.Size = new System.Drawing.Size(434, 40);
            this.rSyncAll.TabIndex = 0;
            this.rSyncAll.TabStop = true;
            this.rSyncAll.Text = "I want to synchronize all files";
            this.rSyncAll.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.rSyncAll.UseVisualStyleBackColor = true;
            this.rSyncAll.CheckedChanged += new System.EventHandler(this.rSyncAll_CheckedChanged);
            // 
            // rSyncCustom
            // 
            this.rSyncCustom.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.rSyncCustom.Location = new System.Drawing.Point(15, 76);
            this.rSyncCustom.Name = "rSyncCustom";
            this.rSyncCustom.Size = new System.Drawing.Size(434, 40);
            this.rSyncCustom.TabIndex = 1;
            this.rSyncCustom.Text = "I want to select what files will be synchronized";
            this.rSyncCustom.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.rSyncCustom.UseVisualStyleBackColor = true;
            this.rSyncCustom.CheckedChanged += new System.EventHandler(this.rSyncCustom_CheckedChanged);
            // 
            // bNext
            // 
            this.bNext.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.bNext.Location = new System.Drawing.Point(316, 223);
            this.bNext.Name = "bNext";
            this.bNext.Size = new System.Drawing.Size(75, 23);
            this.bNext.TabIndex = 75;
            this.bNext.Text = "Next";
            this.bNext.UseVisualStyleBackColor = true;
            this.bNext.Click += new System.EventHandler(this.bNext_Click);
            // 
            // bFinish
            // 
            this.bFinish.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.bFinish.Location = new System.Drawing.Point(397, 223);
            this.bFinish.Name = "bFinish";
            this.bFinish.Size = new System.Drawing.Size(75, 23);
            this.bFinish.TabIndex = 76;
            this.bFinish.Text = "Finish";
            this.bFinish.UseVisualStyleBackColor = true;
            this.bFinish.Click += new System.EventHandler(this.bFinish_Click);
            // 
            // bPrevious
            // 
            this.bPrevious.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.bPrevious.Location = new System.Drawing.Point(235, 223);
            this.bPrevious.Name = "bPrevious";
            this.bPrevious.Size = new System.Drawing.Size(75, 23);
            this.bPrevious.TabIndex = 77;
            this.bPrevious.Text = "Previous";
            this.bPrevious.UseVisualStyleBackColor = true;
            this.bPrevious.Click += new System.EventHandler(this.bPrevious_Click);
            // 
            // gLanguage
            // 
            this.gLanguage.AccessibleDescription = "";
            this.gLanguage.AccessibleName = "FTP Details";
            this.gLanguage.Controls.Add(this.labSelectLanguage);
            this.gLanguage.Controls.Add(this.cLanguages);
            this.gLanguage.Location = new System.Drawing.Point(12, 12);
            this.gLanguage.Name = "gLanguage";
            this.gLanguage.Size = new System.Drawing.Size(460, 205);
            this.gLanguage.TabIndex = 76;
            this.gLanguage.TabStop = false;
            this.gLanguage.Text = "Language";
            // 
            // labSelectLanguage
            // 
            this.labSelectLanguage.AutoSize = true;
            this.labSelectLanguage.Location = new System.Drawing.Point(12, 30);
            this.labSelectLanguage.Name = "labSelectLanguage";
            this.labSelectLanguage.Size = new System.Drawing.Size(110, 13);
            this.labSelectLanguage.TabIndex = 30;
            this.labSelectLanguage.Text = "Select your language:";
            // 
            // cLanguages
            // 
            this.cLanguages.AccessibleDescription = "";
            this.cLanguages.AccessibleName = "Encryption";
            this.cLanguages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cLanguages.FormattingEnabled = true;
            this.cLanguages.Items.AddRange(new object[] {
            "None",
            "require explicit FTP over TLS",
            "require implicit FTP over TLS"});
            this.cLanguages.Location = new System.Drawing.Point(15, 57);
            this.cLanguages.Name = "cLanguages";
            this.cLanguages.Size = new System.Drawing.Size(256, 21);
            this.cLanguages.TabIndex = 0;
            // 
            // Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 253);
            this.Controls.Add(this.gLanguage);
            this.Controls.Add(this.bPrevious);
            this.Controls.Add(this.bFinish);
            this.Controls.Add(this.bNext);
            this.Controls.Add(this.gSelectiveSync);
            this.Controls.Add(this.gRemoteFolder);
            this.Controls.Add(this.gLocalFolder);
            this.Controls.Add(this.gLoginDetails);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Setup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Setup";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Setup_FormClosing);
            this.Load += new System.EventHandler(this.Setup_Load);
            this.RightToLeftLayoutChanged += new System.EventHandler(this.Setup_RightToLeftLayoutChanged);
            this.gLoginDetails.ResumeLayout(false);
            this.gLoginDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nPort)).EndInit();
            this.gLocalFolder.ResumeLayout(false);
            this.gLocalFolder.PerformLayout();
            this.gRemoteFolder.ResumeLayout(false);
            this.gRemoteFolder.PerformLayout();
            this.gSelectiveSync.ResumeLayout(false);
            this.gLanguage.ResumeLayout(false);
            this.gLanguage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox cAskForPass;
        private System.Windows.Forms.GroupBox gLoginDetails;
        private System.Windows.Forms.ComboBox cEncryption;
        private System.Windows.Forms.ComboBox cMode;
        private System.Windows.Forms.TextBox tHost;
        private System.Windows.Forms.TextBox tPass;
        private System.Windows.Forms.TextBox tUsername;
        private System.Windows.Forms.Label labEncryption;
        private System.Windows.Forms.Label labMode;
        private System.Windows.Forms.NumericUpDown nPort;
        private System.Windows.Forms.Label labHost;
        private System.Windows.Forms.Label labPass;
        private System.Windows.Forms.Label labUN;
        private System.Windows.Forms.GroupBox gLocalFolder;
        private System.Windows.Forms.Label labColon;
        private System.Windows.Forms.Button bBrowse;
        private System.Windows.Forms.TextBox tLocalPath;
        private System.Windows.Forms.GroupBox gRemoteFolder;
        private System.Windows.Forms.TreeView tRemoteList;
        private System.Windows.Forms.GroupBox gSelectiveSync;
        private System.Windows.Forms.Button bNext;
        private System.Windows.Forms.Button bFinish;
        private System.Windows.Forms.Button bPrevious;
        private System.Windows.Forms.GroupBox gLanguage;
        private System.Windows.Forms.Label labSelectLanguage;
        private System.Windows.Forms.ComboBox cLanguages;
        private System.Windows.Forms.RadioButton rDefaultLocalFolder;
        private System.Windows.Forms.RadioButton rCustomLocalFolder;
        private System.Windows.Forms.RadioButton rSyncAll;
        private System.Windows.Forms.RadioButton rSyncCustom;
        private System.Windows.Forms.TextBox tFullRemotePath;
        private System.Windows.Forms.Label labFullPath;
        private System.Windows.Forms.Label labKeyPath;
    }
}