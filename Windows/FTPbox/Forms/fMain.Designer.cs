namespace FTPbox.Forms
{
    partial class fMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fMain));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.gLanguage = new System.Windows.Forms.GroupBox();
            this.bTranslate = new System.Windows.Forms.Button();
            this.cLanguages = new System.Windows.Forms.ComboBox();
            this.gLinks = new System.Windows.Forms.GroupBox();
            this.rOpenLocal = new System.Windows.Forms.RadioButton();
            this.labLinkClicked = new System.Windows.Forms.Label();
            this.rCopy2Clipboard = new System.Windows.Forms.RadioButton();
            this.rOpenInBrowser = new System.Windows.Forms.RadioButton();
            this.gApp = new System.Windows.Forms.GroupBox();
            this.chkShellMenus = new System.Windows.Forms.CheckBox();
            this.bBrowseLogs = new System.Windows.Forms.Button();
            this.chkEnableLogging = new System.Windows.Forms.CheckBox();
            this.chkShowNots = new System.Windows.Forms.CheckBox();
            this.chkStartUp = new System.Windows.Forms.CheckBox();
            this.tabAccount = new System.Windows.Forms.TabPage();
            this.bRemoveAccount = new System.Windows.Forms.Button();
            this.bAddAccount = new System.Windows.Forms.Button();
            this.cProfiles = new System.Windows.Forms.ComboBox();
            this.gAccount = new System.Windows.Forms.GroupBox();
            this.tTempPrefix = new System.Windows.Forms.TextBox();
            this.labTempPrefix = new System.Windows.Forms.Label();
            this.rBothWaySync = new System.Windows.Forms.RadioButton();
            this.labWayOfSync = new System.Windows.Forms.Label();
            this.rRemoteToLocalOnly = new System.Windows.Forms.RadioButton();
            this.rLocalToRemoteOnly = new System.Windows.Forms.RadioButton();
            this.labViewInBrowser = new System.Windows.Forms.LinkLabel();
            this.chkWebInt = new System.Windows.Forms.CheckBox();
            this.bConfigureAccount = new System.Windows.Forms.Button();
            this.labAccount = new System.Windows.Forms.Label();
            this.tabFilters = new System.Windows.Forms.TabPage();
            this.gFileFilters = new System.Windows.Forms.GroupBox();
            this.bConfigureSelectiveSync = new System.Windows.Forms.Button();
            this.bConfigureExtensions = new System.Windows.Forms.Button();
            this.labSelectiveSync = new System.Windows.Forms.Label();
            this.labAlsoIgnore = new System.Windows.Forms.Label();
            this.cIgnoreOldFiles = new System.Windows.Forms.CheckBox();
            this.dtpLastModTime = new System.Windows.Forms.DateTimePicker();
            this.labSelectExtensions = new System.Windows.Forms.Label();
            this.cIgnoreTempFiles = new System.Windows.Forms.CheckBox();
            this.cIgnoreDotfiles = new System.Windows.Forms.CheckBox();
            this.tabBandwidth = new System.Windows.Forms.TabPage();
            this.gSyncing = new System.Windows.Forms.GroupBox();
            this.labSeconds = new System.Windows.Forms.Label();
            this.labSyncWhen = new System.Windows.Forms.Label();
            this.nSyncFrequency = new System.Windows.Forms.NumericUpDown();
            this.cAuto = new System.Windows.Forms.RadioButton();
            this.cManually = new System.Windows.Forms.RadioButton();
            this.gLimits = new System.Windows.Forms.GroupBox();
            this.labNoLimits = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.nUpLimit = new System.Windows.Forms.NumericUpDown();
            this.nDownLimit = new System.Windows.Forms.NumericUpDown();
            this.labUpSpeed = new System.Windows.Forms.Label();
            this.labDownSpeed = new System.Windows.Forms.Label();
            this.tabAbout = new System.Windows.Forms.TabPage();
            this.labSupportMail = new System.Windows.Forms.Label();
            this.linkLabel4 = new System.Windows.Forms.LinkLabel();
            this.linkLabel3 = new System.Windows.Forms.LinkLabel();
            this.label19 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.lVersion = new System.Windows.Forms.Label();
            this.labLangUsed = new System.Windows.Forms.Label();
            this.labContact = new System.Windows.Forms.Label();
            this.labSite = new System.Windows.Forms.Label();
            this.labTeam = new System.Windows.Forms.Label();
            this.labCurVersion = new System.Windows.Forms.Label();
            this.gContribute = new System.Windows.Forms.GroupBox();
            this.labDonate = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.gNotes = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.labContactMe = new System.Windows.Forms.Label();
            this.labFree = new System.Windows.Forms.Label();
            this.tray = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.recentFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.SyncToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.gLanguage.SuspendLayout();
            this.gLinks.SuspendLayout();
            this.gApp.SuspendLayout();
            this.tabAccount.SuspendLayout();
            this.gAccount.SuspendLayout();
            this.tabFilters.SuspendLayout();
            this.gFileFilters.SuspendLayout();
            this.tabBandwidth.SuspendLayout();
            this.gSyncing.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nSyncFrequency)).BeginInit();
            this.gLimits.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUpLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nDownLimit)).BeginInit();
            this.tabAbout.SuspendLayout();
            this.gContribute.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.gNotes.SuspendLayout();
            this.trayMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.AccessibleDescription = "";
            this.tabControl1.Controls.Add(this.tabGeneral);
            this.tabControl1.Controls.Add(this.tabAccount);
            this.tabControl1.Controls.Add(this.tabFilters);
            this.tabControl1.Controls.Add(this.tabBandwidth);
            this.tabControl1.Controls.Add(this.tabAbout);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(472, 392);
            this.tabControl1.TabIndex = 12;
            // 
            // tabGeneral
            // 
            this.tabGeneral.AccessibleDescription = "";
            this.tabGeneral.Controls.Add(this.gLanguage);
            this.tabGeneral.Controls.Add(this.gLinks);
            this.tabGeneral.Controls.Add(this.gApp);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(464, 366);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // gLanguage
            // 
            this.gLanguage.Controls.Add(this.bTranslate);
            this.gLanguage.Controls.Add(this.cLanguages);
            this.gLanguage.Location = new System.Drawing.Point(8, 242);
            this.gLanguage.Name = "gLanguage";
            this.gLanguage.Size = new System.Drawing.Size(449, 54);
            this.gLanguage.TabIndex = 13;
            this.gLanguage.TabStop = false;
            this.gLanguage.Text = "Language";
            // 
            // bTranslate
            // 
            this.bTranslate.Location = new System.Drawing.Point(191, 17);
            this.bTranslate.Name = "bTranslate";
            this.bTranslate.Size = new System.Drawing.Size(89, 23);
            this.bTranslate.TabIndex = 11;
            this.bTranslate.Text = "Translate";
            this.bTranslate.UseVisualStyleBackColor = true;
            this.bTranslate.Click += new System.EventHandler(this.bTranslate_Click);
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
            this.cLanguages.Location = new System.Drawing.Point(9, 19);
            this.cLanguages.Name = "cLanguages";
            this.cLanguages.Size = new System.Drawing.Size(176, 21);
            this.cLanguages.TabIndex = 1;
            // 
            // gLinks
            // 
            this.gLinks.AccessibleDescription = "";
            this.gLinks.Controls.Add(this.rOpenLocal);
            this.gLinks.Controls.Add(this.labLinkClicked);
            this.gLinks.Controls.Add(this.rCopy2Clipboard);
            this.gLinks.Controls.Add(this.rOpenInBrowser);
            this.gLinks.Location = new System.Drawing.Point(8, 6);
            this.gLinks.Name = "gLinks";
            this.gLinks.Size = new System.Drawing.Size(449, 111);
            this.gLinks.TabIndex = 12;
            this.gLinks.TabStop = false;
            this.gLinks.Text = "Links";
            // 
            // rOpenLocal
            // 
            this.rOpenLocal.AccessibleDescription = "";
            this.rOpenLocal.AccessibleName = "open the local file";
            this.rOpenLocal.Location = new System.Drawing.Point(22, 86);
            this.rOpenLocal.Name = "rOpenLocal";
            this.rOpenLocal.Size = new System.Drawing.Size(408, 17);
            this.rOpenLocal.TabIndex = 3;
            this.rOpenLocal.Text = "open the local file";
            this.rOpenLocal.UseVisualStyleBackColor = true;
            this.rOpenLocal.CheckedChanged += new System.EventHandler(this.rOpenLocal_CheckedChanged);
            // 
            // labLinkClicked
            // 
            this.labLinkClicked.AccessibleDescription = "";
            this.labLinkClicked.Location = new System.Drawing.Point(9, 19);
            this.labLinkClicked.Name = "labLinkClicked";
            this.labLinkClicked.Size = new System.Drawing.Size(437, 13);
            this.labLinkClicked.TabIndex = 18;
            this.labLinkClicked.Text = "When tray notification or recent file is clicked:";
            // 
            // rCopy2Clipboard
            // 
            this.rCopy2Clipboard.AccessibleDescription = "";
            this.rCopy2Clipboard.AccessibleName = "copy link to clipboard";
            this.rCopy2Clipboard.Location = new System.Drawing.Point(22, 63);
            this.rCopy2Clipboard.Name = "rCopy2Clipboard";
            this.rCopy2Clipboard.Size = new System.Drawing.Size(408, 17);
            this.rCopy2Clipboard.TabIndex = 2;
            this.rCopy2Clipboard.Text = "copy link to clipboard";
            this.rCopy2Clipboard.UseVisualStyleBackColor = true;
            this.rCopy2Clipboard.CheckedChanged += new System.EventHandler(this.rCopy2Clipboard_CheckedChanged);
            // 
            // rOpenInBrowser
            // 
            this.rOpenInBrowser.AccessibleDescription = "";
            this.rOpenInBrowser.AccessibleName = "open link in default browser";
            this.rOpenInBrowser.Checked = true;
            this.rOpenInBrowser.Location = new System.Drawing.Point(22, 40);
            this.rOpenInBrowser.Name = "rOpenInBrowser";
            this.rOpenInBrowser.Size = new System.Drawing.Size(408, 17);
            this.rOpenInBrowser.TabIndex = 1;
            this.rOpenInBrowser.TabStop = true;
            this.rOpenInBrowser.Text = "Open link in default browser";
            this.rOpenInBrowser.UseVisualStyleBackColor = true;
            this.rOpenInBrowser.CheckedChanged += new System.EventHandler(this.rOpenInBrowser_CheckedChanged);
            // 
            // gApp
            // 
            this.gApp.AccessibleDescription = "";
            this.gApp.Controls.Add(this.chkShellMenus);
            this.gApp.Controls.Add(this.bBrowseLogs);
            this.gApp.Controls.Add(this.chkEnableLogging);
            this.gApp.Controls.Add(this.chkShowNots);
            this.gApp.Controls.Add(this.chkStartUp);
            this.gApp.Location = new System.Drawing.Point(8, 123);
            this.gApp.Name = "gApp";
            this.gApp.Size = new System.Drawing.Size(449, 113);
            this.gApp.TabIndex = 3;
            this.gApp.TabStop = false;
            this.gApp.Text = "Application";
            // 
            // chkShellMenus
            // 
            this.chkShellMenus.Location = new System.Drawing.Point(9, 88);
            this.chkShellMenus.Name = "chkShellMenus";
            this.chkShellMenus.Size = new System.Drawing.Size(434, 17);
            this.chkShellMenus.TabIndex = 11;
            this.chkShellMenus.Text = "Add to context menu";
            this.chkShellMenus.UseVisualStyleBackColor = true;
            this.chkShellMenus.CheckedChanged += new System.EventHandler(this.chkShellMenus_CheckedChanged);
            // 
            // bBrowseLogs
            // 
            this.bBrowseLogs.Location = new System.Drawing.Point(191, 61);
            this.bBrowseLogs.Name = "bBrowseLogs";
            this.bBrowseLogs.Size = new System.Drawing.Size(89, 23);
            this.bBrowseLogs.TabIndex = 10;
            this.bBrowseLogs.Text = "View Log";
            this.bBrowseLogs.UseVisualStyleBackColor = true;
            this.bBrowseLogs.Click += new System.EventHandler(this.bBrowseLogs_Click);
            // 
            // chkEnableLogging
            // 
            this.chkEnableLogging.Location = new System.Drawing.Point(9, 65);
            this.chkEnableLogging.Name = "chkEnableLogging";
            this.chkEnableLogging.Size = new System.Drawing.Size(434, 17);
            this.chkEnableLogging.TabIndex = 9;
            this.chkEnableLogging.Text = "Enable logging";
            this.chkEnableLogging.UseVisualStyleBackColor = true;
            this.chkEnableLogging.CheckedChanged += new System.EventHandler(this.chkEnableLogging_CheckedChanged);
            // 
            // chkShowNots
            // 
            this.chkShowNots.AccessibleDescription = "";
            this.chkShowNots.AccessibleName = "show notifications";
            this.chkShowNots.Checked = true;
            this.chkShowNots.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowNots.Location = new System.Drawing.Point(9, 42);
            this.chkShowNots.Name = "chkShowNots";
            this.chkShowNots.Size = new System.Drawing.Size(434, 17);
            this.chkShowNots.TabIndex = 7;
            this.chkShowNots.Text = "Show notifications";
            this.chkShowNots.UseVisualStyleBackColor = true;
            this.chkShowNots.CheckedChanged += new System.EventHandler(this.chkShowNots_CheckedChanged);
            // 
            // chkStartUp
            // 
            this.chkStartUp.AccessibleDescription = "";
            this.chkStartUp.AccessibleName = "start on system startup";
            this.chkStartUp.Location = new System.Drawing.Point(9, 19);
            this.chkStartUp.Name = "chkStartUp";
            this.chkStartUp.Size = new System.Drawing.Size(434, 17);
            this.chkStartUp.TabIndex = 6;
            this.chkStartUp.Text = "Start on system start up";
            this.chkStartUp.UseVisualStyleBackColor = true;
            this.chkStartUp.CheckedChanged += new System.EventHandler(this.chkStartUp_CheckedChanged);
            // 
            // tabAccount
            // 
            this.tabAccount.AccessibleDescription = "";
            this.tabAccount.Controls.Add(this.bRemoveAccount);
            this.tabAccount.Controls.Add(this.bAddAccount);
            this.tabAccount.Controls.Add(this.cProfiles);
            this.tabAccount.Controls.Add(this.gAccount);
            this.tabAccount.Location = new System.Drawing.Point(4, 22);
            this.tabAccount.Name = "tabAccount";
            this.tabAccount.Padding = new System.Windows.Forms.Padding(3);
            this.tabAccount.Size = new System.Drawing.Size(464, 366);
            this.tabAccount.TabIndex = 1;
            this.tabAccount.Text = "Account";
            this.tabAccount.UseVisualStyleBackColor = true;
            // 
            // bRemoveAccount
            // 
            this.bRemoveAccount.Location = new System.Drawing.Point(380, 10);
            this.bRemoveAccount.Name = "bRemoveAccount";
            this.bRemoveAccount.Size = new System.Drawing.Size(75, 23);
            this.bRemoveAccount.TabIndex = 17;
            this.bRemoveAccount.Text = "Remove";
            this.bRemoveAccount.UseVisualStyleBackColor = true;
            this.bRemoveAccount.Click += new System.EventHandler(this.bRemoveAccount_Click);
            // 
            // bAddAccount
            // 
            this.bAddAccount.Location = new System.Drawing.Point(299, 10);
            this.bAddAccount.Name = "bAddAccount";
            this.bAddAccount.Size = new System.Drawing.Size(75, 23);
            this.bAddAccount.TabIndex = 16;
            this.bAddAccount.Text = "Add New";
            this.bAddAccount.UseVisualStyleBackColor = true;
            this.bAddAccount.Click += new System.EventHandler(this.bAddAccount_Click);
            // 
            // cProfiles
            // 
            this.cProfiles.AccessibleDescription = "";
            this.cProfiles.AccessibleName = "Profiles";
            this.cProfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cProfiles.FormattingEnabled = true;
            this.cProfiles.Location = new System.Drawing.Point(8, 11);
            this.cProfiles.Name = "cProfiles";
            this.cProfiles.Size = new System.Drawing.Size(285, 21);
            this.cProfiles.TabIndex = 15;
            this.cProfiles.SelectedIndexChanged += new System.EventHandler(this.cProfiles_SelectedIndexChanged);
            // 
            // gAccount
            // 
            this.gAccount.AccessibleDescription = "";
            this.gAccount.AccessibleName = "";
            this.gAccount.Controls.Add(this.tTempPrefix);
            this.gAccount.Controls.Add(this.labTempPrefix);
            this.gAccount.Controls.Add(this.rBothWaySync);
            this.gAccount.Controls.Add(this.labWayOfSync);
            this.gAccount.Controls.Add(this.rRemoteToLocalOnly);
            this.gAccount.Controls.Add(this.rLocalToRemoteOnly);
            this.gAccount.Controls.Add(this.labViewInBrowser);
            this.gAccount.Controls.Add(this.chkWebInt);
            this.gAccount.Controls.Add(this.bConfigureAccount);
            this.gAccount.Controls.Add(this.labAccount);
            this.gAccount.Location = new System.Drawing.Point(8, 50);
            this.gAccount.Name = "gAccount";
            this.gAccount.Size = new System.Drawing.Size(447, 213);
            this.gAccount.TabIndex = 3;
            this.gAccount.TabStop = false;
            this.gAccount.Text = "Profile";
            // 
            // tTempPrefix
            // 
            this.tTempPrefix.Location = new System.Drawing.Point(21, 181);
            this.tTempPrefix.Name = "tTempPrefix";
            this.tTempPrefix.Size = new System.Drawing.Size(408, 20);
            this.tTempPrefix.TabIndex = 24;
            this.tTempPrefix.TextChanged += new System.EventHandler(this.tTempPrefix_TextChanged);
            this.tTempPrefix.Leave += new System.EventHandler(this.tTempPrefix_Leave);
            // 
            // labTempPrefix
            // 
            this.labTempPrefix.AccessibleDescription = "";
            this.labTempPrefix.Location = new System.Drawing.Point(8, 160);
            this.labTempPrefix.Name = "labTempPrefix";
            this.labTempPrefix.Size = new System.Drawing.Size(437, 13);
            this.labTempPrefix.TabIndex = 23;
            this.labTempPrefix.Text = "Temporary file prefix:";
            // 
            // rBothWaySync
            // 
            this.rBothWaySync.AccessibleDescription = "";
            this.rBothWaySync.AccessibleName = "open the local file";
            this.rBothWaySync.Checked = true;
            this.rBothWaySync.Location = new System.Drawing.Point(21, 136);
            this.rBothWaySync.Name = "rBothWaySync";
            this.rBothWaySync.Size = new System.Drawing.Size(408, 17);
            this.rBothWaySync.TabIndex = 21;
            this.rBothWaySync.TabStop = true;
            this.rBothWaySync.Text = "Both ways";
            this.rBothWaySync.UseVisualStyleBackColor = true;
            this.rBothWaySync.CheckedChanged += new System.EventHandler(this.rWayOfSync_CheckedChanged);
            // 
            // labWayOfSync
            // 
            this.labWayOfSync.AccessibleDescription = "";
            this.labWayOfSync.Location = new System.Drawing.Point(8, 69);
            this.labWayOfSync.Name = "labWayOfSync";
            this.labWayOfSync.Size = new System.Drawing.Size(437, 13);
            this.labWayOfSync.TabIndex = 22;
            this.labWayOfSync.Text = "Way of synchronization:";
            // 
            // rRemoteToLocalOnly
            // 
            this.rRemoteToLocalOnly.AccessibleDescription = "";
            this.rRemoteToLocalOnly.AccessibleName = "copy link to clipboard";
            this.rRemoteToLocalOnly.Location = new System.Drawing.Point(21, 113);
            this.rRemoteToLocalOnly.Name = "rRemoteToLocalOnly";
            this.rRemoteToLocalOnly.Size = new System.Drawing.Size(408, 17);
            this.rRemoteToLocalOnly.TabIndex = 20;
            this.rRemoteToLocalOnly.Text = "Remote to local only";
            this.rRemoteToLocalOnly.UseVisualStyleBackColor = true;
            this.rRemoteToLocalOnly.CheckedChanged += new System.EventHandler(this.rWayOfSync_CheckedChanged);
            // 
            // rLocalToRemoteOnly
            // 
            this.rLocalToRemoteOnly.AccessibleDescription = "";
            this.rLocalToRemoteOnly.AccessibleName = "open link in default browser";
            this.rLocalToRemoteOnly.Location = new System.Drawing.Point(21, 90);
            this.rLocalToRemoteOnly.Name = "rLocalToRemoteOnly";
            this.rLocalToRemoteOnly.Size = new System.Drawing.Size(408, 17);
            this.rLocalToRemoteOnly.TabIndex = 19;
            this.rLocalToRemoteOnly.Text = "Local to remote only";
            this.rLocalToRemoteOnly.UseVisualStyleBackColor = true;
            this.rLocalToRemoteOnly.CheckedChanged += new System.EventHandler(this.rWayOfSync_CheckedChanged);
            // 
            // labViewInBrowser
            // 
            this.labViewInBrowser.AccessibleDescription = "opens the web interface in browser";
            this.labViewInBrowser.AccessibleName = "View in browser";
            this.labViewInBrowser.AutoSize = true;
            this.labViewInBrowser.Location = new System.Drawing.Point(186, 46);
            this.labViewInBrowser.Name = "labViewInBrowser";
            this.labViewInBrowser.Size = new System.Drawing.Size(87, 13);
            this.labViewInBrowser.TabIndex = 16;
            this.labViewInBrowser.TabStop = true;
            this.labViewInBrowser.Text = "(View in browser)";
            this.labViewInBrowser.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.labViewInBrowser_LinkClicked);
            // 
            // chkWebInt
            // 
            this.chkWebInt.AccessibleDescription = "";
            this.chkWebInt.AccessibleName = "use the web interface?";
            this.chkWebInt.Location = new System.Drawing.Point(8, 45);
            this.chkWebInt.Name = "chkWebInt";
            this.chkWebInt.Size = new System.Drawing.Size(433, 17);
            this.chkWebInt.TabIndex = 15;
            this.chkWebInt.Text = "Use the web interface";
            this.chkWebInt.UseVisualStyleBackColor = true;
            this.chkWebInt.CheckedChanged += new System.EventHandler(this.chkWebInt_CheckedChanged);
            // 
            // bConfigureAccount
            // 
            this.bConfigureAccount.Location = new System.Drawing.Point(325, 16);
            this.bConfigureAccount.Name = "bConfigureAccount";
            this.bConfigureAccount.Size = new System.Drawing.Size(107, 23);
            this.bConfigureAccount.TabIndex = 14;
            this.bConfigureAccount.Text = "Details";
            this.bConfigureAccount.UseVisualStyleBackColor = true;
            this.bConfigureAccount.Click += new System.EventHandler(this.bConfigureAccount_Click);
            // 
            // labAccount
            // 
            this.labAccount.AccessibleDescription = "";
            this.labAccount.Location = new System.Drawing.Point(6, 21);
            this.labAccount.Name = "labAccount";
            this.labAccount.Size = new System.Drawing.Size(435, 13);
            this.labAccount.TabIndex = 4;
            this.labAccount.Text = "Account:";
            // 
            // tabFilters
            // 
            this.tabFilters.Controls.Add(this.gFileFilters);
            this.tabFilters.Location = new System.Drawing.Point(4, 22);
            this.tabFilters.Name = "tabFilters";
            this.tabFilters.Padding = new System.Windows.Forms.Padding(3);
            this.tabFilters.Size = new System.Drawing.Size(464, 366);
            this.tabFilters.TabIndex = 5;
            this.tabFilters.Text = "Filters";
            this.tabFilters.UseVisualStyleBackColor = true;
            // 
            // gFileFilters
            // 
            this.gFileFilters.Controls.Add(this.bConfigureSelectiveSync);
            this.gFileFilters.Controls.Add(this.bConfigureExtensions);
            this.gFileFilters.Controls.Add(this.labSelectiveSync);
            this.gFileFilters.Controls.Add(this.labAlsoIgnore);
            this.gFileFilters.Controls.Add(this.cIgnoreOldFiles);
            this.gFileFilters.Controls.Add(this.dtpLastModTime);
            this.gFileFilters.Controls.Add(this.labSelectExtensions);
            this.gFileFilters.Controls.Add(this.cIgnoreTempFiles);
            this.gFileFilters.Controls.Add(this.cIgnoreDotfiles);
            this.gFileFilters.Location = new System.Drawing.Point(8, 6);
            this.gFileFilters.Name = "gFileFilters";
            this.gFileFilters.Size = new System.Drawing.Size(447, 145);
            this.gFileFilters.TabIndex = 5;
            this.gFileFilters.TabStop = false;
            this.gFileFilters.Text = "File Filters";
            // 
            // bConfigureSelectiveSync
            // 
            this.bConfigureSelectiveSync.Location = new System.Drawing.Point(325, 19);
            this.bConfigureSelectiveSync.Name = "bConfigureSelectiveSync";
            this.bConfigureSelectiveSync.Size = new System.Drawing.Size(107, 23);
            this.bConfigureSelectiveSync.TabIndex = 13;
            this.bConfigureSelectiveSync.Text = "Configure";
            this.bConfigureSelectiveSync.UseVisualStyleBackColor = true;
            this.bConfigureSelectiveSync.Click += new System.EventHandler(this.bConfigureSelectiveSync_Click);
            // 
            // bConfigureExtensions
            // 
            this.bConfigureExtensions.Location = new System.Drawing.Point(325, 48);
            this.bConfigureExtensions.Name = "bConfigureExtensions";
            this.bConfigureExtensions.Size = new System.Drawing.Size(107, 23);
            this.bConfigureExtensions.TabIndex = 12;
            this.bConfigureExtensions.Text = "Configure";
            this.bConfigureExtensions.UseVisualStyleBackColor = true;
            this.bConfigureExtensions.Click += new System.EventHandler(this.bConfigureExtensions_Click);
            // 
            // labSelectiveSync
            // 
            this.labSelectiveSync.Location = new System.Drawing.Point(6, 24);
            this.labSelectiveSync.Name = "labSelectiveSync";
            this.labSelectiveSync.Size = new System.Drawing.Size(435, 13);
            this.labSelectiveSync.TabIndex = 11;
            this.labSelectiveSync.Text = "Selective Sync";
            // 
            // labAlsoIgnore
            // 
            this.labAlsoIgnore.Location = new System.Drawing.Point(6, 82);
            this.labAlsoIgnore.Name = "labAlsoIgnore";
            this.labAlsoIgnore.Size = new System.Drawing.Size(435, 13);
            this.labAlsoIgnore.TabIndex = 10;
            this.labAlsoIgnore.Text = "Also Ignore:";
            // 
            // cIgnoreOldFiles
            // 
            this.cIgnoreOldFiles.Location = new System.Drawing.Point(22, 144);
            this.cIgnoreOldFiles.Name = "cIgnoreOldFiles";
            this.cIgnoreOldFiles.Size = new System.Drawing.Size(408, 17);
            this.cIgnoreOldFiles.TabIndex = 9;
            this.cIgnoreOldFiles.Text = "Files modified before:";
            this.cIgnoreOldFiles.UseVisualStyleBackColor = true;
            this.cIgnoreOldFiles.Visible = false;
            this.cIgnoreOldFiles.CheckedChanged += new System.EventHandler(this.cIgnoreOldFiles_CheckedChanged);
            // 
            // dtpLastModTime
            // 
            this.dtpLastModTime.CustomFormat = "d MMMM yyyy - HH:mm";
            this.dtpLastModTime.Enabled = false;
            this.dtpLastModTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpLastModTime.Location = new System.Drawing.Point(40, 167);
            this.dtpLastModTime.Name = "dtpLastModTime";
            this.dtpLastModTime.Size = new System.Drawing.Size(186, 20);
            this.dtpLastModTime.TabIndex = 8;
            this.dtpLastModTime.Visible = false;
            this.dtpLastModTime.ValueChanged += new System.EventHandler(this.dtpLastModTime_ValueChanged);
            // 
            // labSelectExtensions
            // 
            this.labSelectExtensions.Location = new System.Drawing.Point(6, 53);
            this.labSelectExtensions.Name = "labSelectExtensions";
            this.labSelectExtensions.Size = new System.Drawing.Size(435, 13);
            this.labSelectExtensions.TabIndex = 5;
            this.labSelectExtensions.Text = "Ignored Extensions";
            // 
            // cIgnoreTempFiles
            // 
            this.cIgnoreTempFiles.Location = new System.Drawing.Point(22, 98);
            this.cIgnoreTempFiles.Name = "cIgnoreTempFiles";
            this.cIgnoreTempFiles.Size = new System.Drawing.Size(408, 17);
            this.cIgnoreTempFiles.TabIndex = 0;
            this.cIgnoreTempFiles.Text = "Temporary files";
            this.cIgnoreTempFiles.UseVisualStyleBackColor = true;
            this.cIgnoreTempFiles.CheckedChanged += new System.EventHandler(this.cIgnoreTempFiles_CheckedChanged);
            // 
            // cIgnoreDotfiles
            // 
            this.cIgnoreDotfiles.Location = new System.Drawing.Point(22, 121);
            this.cIgnoreDotfiles.Name = "cIgnoreDotfiles";
            this.cIgnoreDotfiles.Size = new System.Drawing.Size(408, 17);
            this.cIgnoreDotfiles.TabIndex = 1;
            this.cIgnoreDotfiles.Text = "Dotfiles";
            this.cIgnoreDotfiles.UseVisualStyleBackColor = true;
            this.cIgnoreDotfiles.CheckedChanged += new System.EventHandler(this.cIgnoreDotfiles_CheckedChanged);
            // 
            // tabBandwidth
            // 
            this.tabBandwidth.Controls.Add(this.gSyncing);
            this.tabBandwidth.Controls.Add(this.gLimits);
            this.tabBandwidth.Location = new System.Drawing.Point(4, 22);
            this.tabBandwidth.Name = "tabBandwidth";
            this.tabBandwidth.Padding = new System.Windows.Forms.Padding(3);
            this.tabBandwidth.Size = new System.Drawing.Size(464, 366);
            this.tabBandwidth.TabIndex = 3;
            this.tabBandwidth.Text = "Bandwidth";
            this.tabBandwidth.UseVisualStyleBackColor = true;
            // 
            // gSyncing
            // 
            this.gSyncing.Controls.Add(this.labSeconds);
            this.gSyncing.Controls.Add(this.labSyncWhen);
            this.gSyncing.Controls.Add(this.nSyncFrequency);
            this.gSyncing.Controls.Add(this.cAuto);
            this.gSyncing.Controls.Add(this.cManually);
            this.gSyncing.Location = new System.Drawing.Point(8, 6);
            this.gSyncing.Name = "gSyncing";
            this.gSyncing.Size = new System.Drawing.Size(447, 124);
            this.gSyncing.TabIndex = 2;
            this.gSyncing.TabStop = false;
            this.gSyncing.Text = "Syncing";
            // 
            // labSeconds
            // 
            this.labSeconds.Location = new System.Drawing.Point(116, 91);
            this.labSeconds.Name = "labSeconds";
            this.labSeconds.Size = new System.Drawing.Size(222, 13);
            this.labSeconds.TabIndex = 5;
            this.labSeconds.Text = "seconds";
            // 
            // labSyncWhen
            // 
            this.labSyncWhen.Location = new System.Drawing.Point(6, 22);
            this.labSyncWhen.Name = "labSyncWhen";
            this.labSyncWhen.Size = new System.Drawing.Size(435, 13);
            this.labSyncWhen.TabIndex = 4;
            this.labSyncWhen.Text = "Synchronize remote files:";
            // 
            // nSyncFrequency
            // 
            this.nSyncFrequency.AccessibleName = "synchronization interval in seconds";
            this.nSyncFrequency.Location = new System.Drawing.Point(35, 89);
            this.nSyncFrequency.Maximum = new decimal(new int[] {
            79228,
            0,
            0,
            0});
            this.nSyncFrequency.Name = "nSyncFrequency";
            this.nSyncFrequency.Size = new System.Drawing.Size(75, 20);
            this.nSyncFrequency.TabIndex = 2;
            this.nSyncFrequency.ValueChanged += new System.EventHandler(this.nSyncFrequency_ValueChanged);
            // 
            // cAuto
            // 
            this.cAuto.AccessibleName = "synchronize automatically";
            this.cAuto.Location = new System.Drawing.Point(22, 66);
            this.cAuto.Name = "cAuto";
            this.cAuto.Size = new System.Drawing.Size(411, 17);
            this.cAuto.TabIndex = 1;
            this.cAuto.TabStop = true;
            this.cAuto.Text = "automatically every";
            this.cAuto.UseVisualStyleBackColor = true;
            this.cAuto.CheckedChanged += new System.EventHandler(this.cAuto_CheckedChanged);
            // 
            // cManually
            // 
            this.cManually.AccessibleName = "synchronize manually";
            this.cManually.Location = new System.Drawing.Point(22, 43);
            this.cManually.Name = "cManually";
            this.cManually.Size = new System.Drawing.Size(411, 17);
            this.cManually.TabIndex = 0;
            this.cManually.TabStop = true;
            this.cManually.Text = "manually";
            this.cManually.UseVisualStyleBackColor = true;
            this.cManually.CheckedChanged += new System.EventHandler(this.cManually_CheckedChanged);
            // 
            // gLimits
            // 
            this.gLimits.Controls.Add(this.labNoLimits);
            this.gLimits.Controls.Add(this.label4);
            this.gLimits.Controls.Add(this.label3);
            this.gLimits.Controls.Add(this.nUpLimit);
            this.gLimits.Controls.Add(this.nDownLimit);
            this.gLimits.Controls.Add(this.labUpSpeed);
            this.gLimits.Controls.Add(this.labDownSpeed);
            this.gLimits.Location = new System.Drawing.Point(8, 136);
            this.gLimits.Name = "gLimits";
            this.gLimits.Size = new System.Drawing.Size(447, 158);
            this.gLimits.TabIndex = 1;
            this.gLimits.TabStop = false;
            this.gLimits.Text = "Speed Limits";
            // 
            // labNoLimits
            // 
            this.labNoLimits.Location = new System.Drawing.Point(22, 132);
            this.labNoLimits.Name = "labNoLimits";
            this.labNoLimits.Size = new System.Drawing.Size(411, 13);
            this.labNoLimits.TabIndex = 14;
            this.labNoLimits.Text = "( set to 0 for no limits )";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(119, 102);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(219, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "KB/s";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(119, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(219, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "KB/s";
            // 
            // nUpLimit
            // 
            this.nUpLimit.AccessibleName = "upload speed limit";
            this.nUpLimit.Location = new System.Drawing.Point(35, 100);
            this.nUpLimit.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nUpLimit.Name = "nUpLimit";
            this.nUpLimit.Size = new System.Drawing.Size(75, 20);
            this.nUpLimit.TabIndex = 9;
            this.nUpLimit.ValueChanged += new System.EventHandler(this.nUpLimit_ValueChanged);
            // 
            // nDownLimit
            // 
            this.nDownLimit.AccessibleName = "download speed limit";
            this.nDownLimit.Location = new System.Drawing.Point(35, 45);
            this.nDownLimit.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nDownLimit.Name = "nDownLimit";
            this.nDownLimit.Size = new System.Drawing.Size(75, 20);
            this.nDownLimit.TabIndex = 6;
            this.nDownLimit.ValueChanged += new System.EventHandler(this.nDownLimit_ValueChanged);
            // 
            // labUpSpeed
            // 
            this.labUpSpeed.Location = new System.Drawing.Point(9, 77);
            this.labUpSpeed.Name = "labUpSpeed";
            this.labUpSpeed.Size = new System.Drawing.Size(432, 13);
            this.labUpSpeed.TabIndex = 6;
            this.labUpSpeed.Text = "Limit Upload Speed:";
            // 
            // labDownSpeed
            // 
            this.labDownSpeed.Location = new System.Drawing.Point(6, 22);
            this.labDownSpeed.Name = "labDownSpeed";
            this.labDownSpeed.Size = new System.Drawing.Size(435, 13);
            this.labDownSpeed.TabIndex = 3;
            this.labDownSpeed.Text = "Limit Download Speed:";
            // 
            // tabAbout
            // 
            this.tabAbout.AccessibleDescription = "";
            this.tabAbout.Controls.Add(this.labSupportMail);
            this.tabAbout.Controls.Add(this.linkLabel4);
            this.tabAbout.Controls.Add(this.linkLabel3);
            this.tabAbout.Controls.Add(this.label19);
            this.tabAbout.Controls.Add(this.label21);
            this.tabAbout.Controls.Add(this.lVersion);
            this.tabAbout.Controls.Add(this.labLangUsed);
            this.tabAbout.Controls.Add(this.labContact);
            this.tabAbout.Controls.Add(this.labSite);
            this.tabAbout.Controls.Add(this.labTeam);
            this.tabAbout.Controls.Add(this.labCurVersion);
            this.tabAbout.Controls.Add(this.gContribute);
            this.tabAbout.Controls.Add(this.gNotes);
            this.tabAbout.Location = new System.Drawing.Point(4, 22);
            this.tabAbout.Name = "tabAbout";
            this.tabAbout.Padding = new System.Windows.Forms.Padding(3);
            this.tabAbout.Size = new System.Drawing.Size(464, 366);
            this.tabAbout.TabIndex = 2;
            this.tabAbout.Text = "About";
            this.tabAbout.UseVisualStyleBackColor = true;
            // 
            // labSupportMail
            // 
            this.labSupportMail.AccessibleDescription = "";
            this.labSupportMail.Location = new System.Drawing.Point(272, 113);
            this.labSupportMail.Name = "labSupportMail";
            this.labSupportMail.Size = new System.Drawing.Size(129, 13);
            this.labSupportMail.TabIndex = 14;
            this.labSupportMail.Text = "support@ftpbox.org";
            // 
            // linkLabel4
            // 
            this.linkLabel4.AccessibleDescription = "";
            this.linkLabel4.Location = new System.Drawing.Point(272, 67);
            this.linkLabel4.Name = "linkLabel4";
            this.linkLabel4.Size = new System.Drawing.Size(129, 13);
            this.linkLabel4.TabIndex = 9;
            this.linkLabel4.TabStop = true;
            this.linkLabel4.Text = "ftpbox.org";
            this.linkLabel4.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel4_LinkClicked);
            // 
            // linkLabel3
            // 
            this.linkLabel3.AccessibleDescription = "";
            this.linkLabel3.Location = new System.Drawing.Point(272, 44);
            this.linkLabel3.Name = "linkLabel3";
            this.linkLabel3.Size = new System.Drawing.Size(129, 13);
            this.linkLabel3.TabIndex = 8;
            this.linkLabel3.TabStop = true;
            this.linkLabel3.Text = "FTPbox team";
            this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel3_LinkClicked);
            // 
            // label19
            // 
            this.label19.AccessibleDescription = "";
            this.label19.Location = new System.Drawing.Point(272, 136);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(129, 13);
            this.label19.TabIndex = 13;
            this.label19.Text = "C# / .NET";
            // 
            // label21
            // 
            this.label21.AccessibleDescription = "";
            this.label21.Location = new System.Drawing.Point(272, 90);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(129, 13);
            this.label21.TabIndex = 11;
            this.label21.Text = "admin@ftpbox.org";
            // 
            // lVersion
            // 
            this.lVersion.AccessibleDescription = "";
            this.lVersion.Location = new System.Drawing.Point(272, 21);
            this.lVersion.Name = "lVersion";
            this.lVersion.Size = new System.Drawing.Size(129, 13);
            this.lVersion.TabIndex = 8;
            this.lVersion.Text = "X.X.X (build X)";
            // 
            // labLangUsed
            // 
            this.labLangUsed.AccessibleDescription = "";
            this.labLangUsed.AutoSize = true;
            this.labLangUsed.Location = new System.Drawing.Point(102, 136);
            this.labLangUsed.Name = "labLangUsed";
            this.labLangUsed.Size = new System.Drawing.Size(84, 13);
            this.labLangUsed.TabIndex = 7;
            this.labLangUsed.Text = "Language used:";
            // 
            // labContact
            // 
            this.labContact.AccessibleDescription = "";
            this.labContact.AutoSize = true;
            this.labContact.Location = new System.Drawing.Point(102, 90);
            this.labContact.Name = "labContact";
            this.labContact.Size = new System.Drawing.Size(47, 13);
            this.labContact.TabIndex = 5;
            this.labContact.Text = "Contact:";
            // 
            // labSite
            // 
            this.labSite.AccessibleDescription = "";
            this.labSite.AutoSize = true;
            this.labSite.Location = new System.Drawing.Point(102, 67);
            this.labSite.Name = "labSite";
            this.labSite.Size = new System.Drawing.Size(84, 13);
            this.labSite.TabIndex = 4;
            this.labSite.Text = "Official Website:";
            // 
            // labTeam
            // 
            this.labTeam.AccessibleDescription = "";
            this.labTeam.AutoSize = true;
            this.labTeam.Location = new System.Drawing.Point(102, 44);
            this.labTeam.Name = "labTeam";
            this.labTeam.Size = new System.Drawing.Size(59, 13);
            this.labTeam.TabIndex = 3;
            this.labTeam.Text = "The Team:";
            // 
            // labCurVersion
            // 
            this.labCurVersion.AccessibleDescription = "";
            this.labCurVersion.AutoSize = true;
            this.labCurVersion.Location = new System.Drawing.Point(102, 21);
            this.labCurVersion.Name = "labCurVersion";
            this.labCurVersion.Size = new System.Drawing.Size(82, 13);
            this.labCurVersion.TabIndex = 2;
            this.labCurVersion.Text = "Current Version:";
            // 
            // gContribute
            // 
            this.gContribute.AccessibleDescription = "";
            this.gContribute.Controls.Add(this.labDonate);
            this.gContribute.Controls.Add(this.pictureBox1);
            this.gContribute.Controls.Add(this.linkLabel2);
            this.gContribute.Controls.Add(this.linkLabel1);
            this.gContribute.Location = new System.Drawing.Point(8, 192);
            this.gContribute.Name = "gContribute";
            this.gContribute.Size = new System.Drawing.Size(447, 48);
            this.gContribute.TabIndex = 1;
            this.gContribute.TabStop = false;
            this.gContribute.Text = "Contribute";
            // 
            // labDonate
            // 
            this.labDonate.AutoSize = true;
            this.labDonate.Location = new System.Drawing.Point(6, 54);
            this.labDonate.Name = "labDonate";
            this.labDonate.Size = new System.Drawing.Size(45, 13);
            this.labDonate.TabIndex = 4;
            this.labDonate.Text = "Donate:";
            // 
            // pictureBox1
            // 
            this.pictureBox1.AccessibleDescription = "opens donate page";
            this.pictureBox1.AccessibleName = "Donate button";
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Image = global::FTPbox.Properties.Resources.donate;
            this.pictureBox1.Location = new System.Drawing.Point(367, 19);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(74, 21);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // linkLabel2
            // 
            this.linkLabel2.AccessibleDescription = "";
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(177, 19);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(92, 13);
            this.linkLabel2.TabIndex = 12;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "Request a feature";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AccessibleDescription = "";
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(6, 19);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(69, 13);
            this.linkLabel1.TabIndex = 11;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Report a bug";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // gNotes
            // 
            this.gNotes.AccessibleDescription = "";
            this.gNotes.Controls.Add(this.label11);
            this.gNotes.Controls.Add(this.labContactMe);
            this.gNotes.Controls.Add(this.labFree);
            this.gNotes.Location = new System.Drawing.Point(8, 246);
            this.gNotes.Name = "gNotes";
            this.gNotes.Size = new System.Drawing.Size(447, 69);
            this.gNotes.TabIndex = 0;
            this.gNotes.TabStop = false;
            this.gNotes.Text = "Notes";
            // 
            // label11
            // 
            this.label11.AccessibleDescription = "";
            this.label11.Location = new System.Drawing.Point(6, 50);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(435, 13);
            this.label11.TabIndex = 3;
            this.label11.Text = "Copyright © 2015 - ftpbox.org";
            // 
            // labContactMe
            // 
            this.labContactMe.AccessibleDescription = "";
            this.labContactMe.Location = new System.Drawing.Point(6, 35);
            this.labContactMe.Name = "labContactMe";
            this.labContactMe.Size = new System.Drawing.Size(435, 13);
            this.labContactMe.TabIndex = 2;
            this.labContactMe.Text = "- Feel free to contact me for anything.";
            // 
            // labFree
            // 
            this.labFree.AccessibleDescription = "";
            this.labFree.Location = new System.Drawing.Point(6, 19);
            this.labFree.Name = "labFree";
            this.labFree.Size = new System.Drawing.Size(435, 13);
            this.labFree.TabIndex = 0;
            this.labFree.Text = "- FTPbox is free and open-source";
            // 
            // tray
            // 
            this.tray.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.tray.ContextMenuStrip = this.trayMenu;
            this.tray.Icon = ((System.Drawing.Icon)(resources.GetObject("tray.Icon")));
            this.tray.Text = "FTPbox";
            this.tray.Visible = true;
            this.tray.BalloonTipClicked += new System.EventHandler(this.tray_BalloonTipClicked);
            this.tray.MouseClick += new System.Windows.Forms.MouseEventHandler(this.tray_MouseClick);
            this.tray.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.tray_MouseDoubleClick);
            // 
            // trayMenu
            // 
            this.trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem,
            this.toolStripSeparator2,
            this.recentFilesToolStripMenuItem,
            this.SyncToolStripMenuItem,
            this.toolStripSeparator1,
            this.aboutToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.trayMenu.Name = "trayMenu";
            this.trayMenu.Size = new System.Drawing.Size(143, 126);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.optionsToolStripMenuItem.Text = "Options";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(139, 6);
            // 
            // recentFilesToolStripMenuItem
            // 
            this.recentFilesToolStripMenuItem.AccessibleDescription = "Dropbdown menu";
            this.recentFilesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2,
            this.toolStripMenuItem3,
            this.toolStripMenuItem4,
            this.toolStripMenuItem5});
            this.recentFilesToolStripMenuItem.Name = "recentFilesToolStripMenuItem";
            this.recentFilesToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.recentFilesToolStripMenuItem.Text = "Recent Files";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Enabled = false;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(143, 22);
            this.toolStripMenuItem1.Text = "Not available";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Enabled = false;
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(143, 22);
            this.toolStripMenuItem2.Text = "Not available";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Enabled = false;
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(143, 22);
            this.toolStripMenuItem3.Text = "Not available";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Enabled = false;
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(143, 22);
            this.toolStripMenuItem4.Text = "Not available";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.toolStripMenuItem4_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Enabled = false;
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(143, 22);
            this.toolStripMenuItem5.Text = "Not available";
            this.toolStripMenuItem5.Click += new System.EventHandler(this.toolStripMenuItem5_Click);
            // 
            // SyncToolStripMenuItem
            // 
            this.SyncToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.SyncToolStripMenuItem.Name = "SyncToolStripMenuItem";
            this.SyncToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.SyncToolStripMenuItem.Text = "Start syncing";
            this.SyncToolStripMenuItem.Click += new System.EventHandler(this.SyncToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(139, 6);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // fMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(471, 384);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "fMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FTPbox Options";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.fMain_FormClosing);
            this.Load += new System.EventHandler(this.fMain_Load);
            this.RightToLeftLayoutChanged += new System.EventHandler(this.fMain_RightToLeftLayoutChanged);
            this.tabControl1.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.gLanguage.ResumeLayout(false);
            this.gLinks.ResumeLayout(false);
            this.gApp.ResumeLayout(false);
            this.tabAccount.ResumeLayout(false);
            this.gAccount.ResumeLayout(false);
            this.gAccount.PerformLayout();
            this.tabFilters.ResumeLayout(false);
            this.gFileFilters.ResumeLayout(false);
            this.tabBandwidth.ResumeLayout(false);
            this.gSyncing.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nSyncFrequency)).EndInit();
            this.gLimits.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nUpLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nDownLimit)).EndInit();
            this.tabAbout.ResumeLayout(false);
            this.tabAbout.PerformLayout();
            this.gContribute.ResumeLayout(false);
            this.gContribute.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.gNotes.ResumeLayout(false);
            this.trayMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.GroupBox gApp;
        private System.Windows.Forms.CheckBox chkShowNots;
        private System.Windows.Forms.CheckBox chkStartUp;
        private System.Windows.Forms.TabPage tabAccount;
        private System.Windows.Forms.TabPage tabAbout;
        private System.Windows.Forms.Label labSupportMail;
        private System.Windows.Forms.LinkLabel linkLabel4;
        private System.Windows.Forms.LinkLabel linkLabel3;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label lVersion;
        private System.Windows.Forms.Label labLangUsed;
        private System.Windows.Forms.Label labContact;
        private System.Windows.Forms.Label labSite;
        private System.Windows.Forms.Label labTeam;
        private System.Windows.Forms.Label labCurVersion;
        private System.Windows.Forms.GroupBox gContribute;
        private System.Windows.Forms.Label labDonate;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.GroupBox gNotes;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label labContactMe;
        private System.Windows.Forms.Label labFree;
        private System.IO.FileSystemWatcher fswFiles;
        private System.IO.FileSystemWatcher fswFolders;
        private System.Windows.Forms.NotifyIcon tray;
        private System.Windows.Forms.ContextMenuStrip trayMenu;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem recentFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.TabPage tabBandwidth;
        private System.Windows.Forms.GroupBox gLinks;
        private System.Windows.Forms.RadioButton rCopy2Clipboard;
        private System.Windows.Forms.RadioButton rOpenInBrowser;
        private System.Windows.Forms.GroupBox gAccount;
        private System.Windows.Forms.Label labAccount;
        private System.Windows.Forms.GroupBox gLimits;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nUpLimit;
        private System.Windows.Forms.NumericUpDown nDownLimit;
        private System.Windows.Forms.Label labUpSpeed;
        private System.Windows.Forms.Label labDownSpeed;
        private System.Windows.Forms.RadioButton rOpenLocal;
        private System.Windows.Forms.GroupBox gSyncing;
        private System.Windows.Forms.NumericUpDown nSyncFrequency;
        private System.Windows.Forms.RadioButton cAuto;
        private System.Windows.Forms.RadioButton cManually;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem SyncToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Label labSyncWhen;
        private System.Windows.Forms.Label labSeconds;
        private System.Windows.Forms.Label labNoLimits;
        private System.Windows.Forms.TabPage tabFilters;
        private System.Windows.Forms.GroupBox gFileFilters;
        private System.Windows.Forms.Label labSelectExtensions;
        private System.Windows.Forms.CheckBox cIgnoreTempFiles;
        private System.Windows.Forms.CheckBox cIgnoreDotfiles;
        private System.Windows.Forms.Label labAlsoIgnore;
        private System.Windows.Forms.CheckBox cIgnoreOldFiles;
        private System.Windows.Forms.DateTimePicker dtpLastModTime;
        private System.Windows.Forms.Label labLinkClicked;
        private System.Windows.Forms.ComboBox cProfiles;
        private System.Windows.Forms.CheckBox chkEnableLogging;
        private System.Windows.Forms.Button bBrowseLogs;
        private System.Windows.Forms.Button bAddAccount;
        private System.Windows.Forms.Button bRemoveAccount;
        private System.Windows.Forms.GroupBox gLanguage;
        private System.Windows.Forms.ComboBox cLanguages;
        private System.Windows.Forms.Button bTranslate;
        private System.Windows.Forms.Button bConfigureExtensions;
        private System.Windows.Forms.Label labSelectiveSync;
        private System.Windows.Forms.Button bConfigureSelectiveSync;
        private System.Windows.Forms.Button bConfigureAccount;
        private System.Windows.Forms.LinkLabel labViewInBrowser;
        private System.Windows.Forms.CheckBox chkWebInt;
        private System.Windows.Forms.RadioButton rBothWaySync;
        private System.Windows.Forms.Label labWayOfSync;
        private System.Windows.Forms.RadioButton rRemoteToLocalOnly;
        private System.Windows.Forms.RadioButton rLocalToRemoteOnly;
        private System.Windows.Forms.TextBox tTempPrefix;
        private System.Windows.Forms.Label labTempPrefix;
        private System.Windows.Forms.CheckBox chkShellMenus;
    }
}