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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("English (en)", 0);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("Espanol (es)", 12);
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("Deutsch (de)", 13);
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("Francais (fr)", 19);
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("Dutch (nl)", 14);
            System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem("Ελληνικά (el)", 2);
            System.Windows.Forms.ListViewItem listViewItem7 = new System.Windows.Forms.ListViewItem("Italian (it)", 6);
            System.Windows.Forms.ListViewItem listViewItem8 = new System.Windows.Forms.ListViewItem("Turkish (tr)", 7);
            System.Windows.Forms.ListViewItem listViewItem9 = new System.Windows.Forms.ListViewItem("Português Brasileiro (pt-BR)", 15);
            System.Windows.Forms.ListViewItem listViewItem10 = new System.Windows.Forms.ListViewItem("Foroyskt (fo)", 10);
            System.Windows.Forms.ListViewItem listViewItem11 = new System.Windows.Forms.ListViewItem("Swedish (sv)", 18);
            System.Windows.Forms.ListViewItem listViewItem12 = new System.Windows.Forms.ListViewItem("Albanian (sq)", 1);
            System.Windows.Forms.ListViewItem listViewItem13 = new System.Windows.Forms.ListViewItem("Romanian (ro)", 17);
            System.Windows.Forms.ListViewItem listViewItem14 = new System.Windows.Forms.ListViewItem("Korean (ko)", 16);
            System.Windows.Forms.ListViewItem listViewItem15 = new System.Windows.Forms.ListViewItem("Russian (ru)", 5);
            System.Windows.Forms.ListViewItem listViewItem16 = new System.Windows.Forms.ListViewItem("Japanese (ja)", 3);
            System.Windows.Forms.ListViewItem listViewItem17 = new System.Windows.Forms.ListViewItem("Vietnamese (vi)", 8);
            System.Windows.Forms.ListViewItem listViewItem18 = new System.Windows.Forms.ListViewItem("Norwegian (no)", 4);
            System.Windows.Forms.ListViewItem listViewItem19 = new System.Windows.Forms.ListViewItem("Hungarian (hu)", 20);
            System.Windows.Forms.ListViewItem listViewItem20 = new System.Windows.Forms.ListViewItem("中國傳統 (zh_HANT)", 9);
            System.Windows.Forms.ListViewItem listViewItem21 = new System.Windows.Forms.ListViewItem("简体中文 (zh_HANS)", 9);
            System.Windows.Forms.ListViewItem listViewItem22 = new System.Windows.Forms.ListViewItem("Lithuanian (lt)", 21);
            System.Windows.Forms.ListViewItem listViewItem23 = new System.Windows.Forms.ListViewItem("Dansk (da)", 22);
            System.Windows.Forms.ListViewItem listViewItem24 = new System.Windows.Forms.ListViewItem("Polish (pl)", 23);
            System.Windows.Forms.ListViewItem listViewItem25 = new System.Windows.Forms.ListViewItem("Croatian (hr)", 24);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fMain));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.gLinks = new System.Windows.Forms.GroupBox();
            this.rOpenLocal = new System.Windows.Forms.RadioButton();
            this.labLinkClicked = new System.Windows.Forms.Label();
            this.tParent = new System.Windows.Forms.TextBox();
            this.labFullPath = new System.Windows.Forms.Label();
            this.rCopy2Clipboard = new System.Windows.Forms.RadioButton();
            this.rOpenInBrowser = new System.Windows.Forms.RadioButton();
            this.gWebInt = new System.Windows.Forms.GroupBox();
            this.labViewInBrowser = new System.Windows.Forms.LinkLabel();
            this.chkWebInt = new System.Windows.Forms.CheckBox();
            this.gApp = new System.Windows.Forms.GroupBox();
            this.labLang = new System.Windows.Forms.Label();
            this.cmbLang = new System.Windows.Forms.ComboBox();
            this.chkShowNots = new System.Windows.Forms.CheckBox();
            this.chkStartUp = new System.Windows.Forms.CheckBox();
            this.tabAccount = new System.Windows.Forms.TabPage();
            this.gAccount = new System.Windows.Forms.GroupBox();
            this.lMode = new System.Windows.Forms.Label();
            this.labMode = new System.Windows.Forms.Label();
            this.lPort = new System.Windows.Forms.Label();
            this.lHost = new System.Windows.Forms.Label();
            this.lUsername = new System.Windows.Forms.Label();
            this.labPort = new System.Windows.Forms.Label();
            this.labUN = new System.Windows.Forms.Label();
            this.labHost = new System.Windows.Forms.Label();
            this.gDetails = new System.Windows.Forms.GroupBox();
            this.lLocPath = new System.Windows.Forms.Label();
            this.lRemPath = new System.Windows.Forms.Label();
            this.labLocPath = new System.Windows.Forms.Label();
            this.labRemPath = new System.Windows.Forms.Label();
            this.bChangeBox = new System.Windows.Forms.Button();
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
            this.tabLanguage = new System.Windows.Forms.TabPage();
            this.listView1 = new System.Windows.Forms.ListView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
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
            this.gLinks.SuspendLayout();
            this.gWebInt.SuspendLayout();
            this.gApp.SuspendLayout();
            this.tabAccount.SuspendLayout();
            this.gAccount.SuspendLayout();
            this.gDetails.SuspendLayout();
            this.tabBandwidth.SuspendLayout();
            this.gSyncing.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nSyncFrequency)).BeginInit();
            this.gLimits.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUpLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nDownLimit)).BeginInit();
            this.tabLanguage.SuspendLayout();
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
            this.tabControl1.Controls.Add(this.tabBandwidth);
            this.tabControl1.Controls.Add(this.tabLanguage);
            this.tabControl1.Controls.Add(this.tabAbout);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(482, 357);
            this.tabControl1.TabIndex = 12;
            // 
            // tabGeneral
            // 
            this.tabGeneral.AccessibleDescription = "";
            this.tabGeneral.Controls.Add(this.gLinks);
            this.tabGeneral.Controls.Add(this.gWebInt);
            this.tabGeneral.Controls.Add(this.gApp);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(474, 331);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // gLinks
            // 
            this.gLinks.AccessibleDescription = "";
            this.gLinks.Controls.Add(this.rOpenLocal);
            this.gLinks.Controls.Add(this.labLinkClicked);
            this.gLinks.Controls.Add(this.tParent);
            this.gLinks.Controls.Add(this.labFullPath);
            this.gLinks.Controls.Add(this.rCopy2Clipboard);
            this.gLinks.Controls.Add(this.rOpenInBrowser);
            this.gLinks.Location = new System.Drawing.Point(8, 6);
            this.gLinks.Name = "gLinks";
            this.gLinks.Size = new System.Drawing.Size(449, 148);
            this.gLinks.TabIndex = 12;
            this.gLinks.TabStop = false;
            this.gLinks.Text = "Links";
            // 
            // rOpenLocal
            // 
            this.rOpenLocal.AccessibleDescription = "";
            this.rOpenLocal.AccessibleName = "open the local file";
            this.rOpenLocal.AutoSize = true;
            this.rOpenLocal.Location = new System.Drawing.Point(19, 122);
            this.rOpenLocal.Name = "rOpenLocal";
            this.rOpenLocal.Size = new System.Drawing.Size(108, 17);
            this.rOpenLocal.TabIndex = 3;
            this.rOpenLocal.Text = "open the local file";
            this.rOpenLocal.UseVisualStyleBackColor = true;
            this.rOpenLocal.CheckedChanged += new System.EventHandler(this.rOpenLocal_CheckedChanged);
            // 
            // labLinkClicked
            // 
            this.labLinkClicked.AccessibleDescription = "";
            this.labLinkClicked.AutoSize = true;
            this.labLinkClicked.Location = new System.Drawing.Point(6, 55);
            this.labLinkClicked.Name = "labLinkClicked";
            this.labLinkClicked.Size = new System.Drawing.Size(221, 13);
            this.labLinkClicked.TabIndex = 18;
            this.labLinkClicked.Text = "When tray notification or recent file is clicked:";
            // 
            // tParent
            // 
            this.tParent.AccessibleDescription = "";
            this.tParent.AccessibleName = "account\'s http path";
            this.tParent.Location = new System.Drawing.Point(19, 32);
            this.tParent.Name = "tParent";
            this.tParent.Size = new System.Drawing.Size(345, 20);
            this.tParent.TabIndex = 0;
            this.tParent.TextChanged += new System.EventHandler(this.tParent_TextChanged);
            // 
            // labFullPath
            // 
            this.labFullPath.AccessibleDescription = "";
            this.labFullPath.AutoSize = true;
            this.labFullPath.Location = new System.Drawing.Point(6, 16);
            this.labFullPath.Name = "labFullPath";
            this.labFullPath.Size = new System.Drawing.Size(97, 13);
            this.labFullPath.TabIndex = 2;
            this.labFullPath.Text = "Account\'s full path:";
            // 
            // rCopy2Clipboard
            // 
            this.rCopy2Clipboard.AccessibleDescription = "";
            this.rCopy2Clipboard.AccessibleName = "copy link to clipboard";
            this.rCopy2Clipboard.AutoSize = true;
            this.rCopy2Clipboard.Location = new System.Drawing.Point(19, 99);
            this.rCopy2Clipboard.Name = "rCopy2Clipboard";
            this.rCopy2Clipboard.Size = new System.Drawing.Size(125, 17);
            this.rCopy2Clipboard.TabIndex = 2;
            this.rCopy2Clipboard.Text = "copy link to clipboard";
            this.rCopy2Clipboard.UseVisualStyleBackColor = true;
            this.rCopy2Clipboard.CheckedChanged += new System.EventHandler(this.rCopy2Clipboard_CheckedChanged);
            // 
            // rOpenInBrowser
            // 
            this.rOpenInBrowser.AccessibleDescription = "";
            this.rOpenInBrowser.AccessibleName = "open link in default browser";
            this.rOpenInBrowser.AutoSize = true;
            this.rOpenInBrowser.Checked = true;
            this.rOpenInBrowser.Location = new System.Drawing.Point(19, 76);
            this.rOpenInBrowser.Name = "rOpenInBrowser";
            this.rOpenInBrowser.Size = new System.Drawing.Size(156, 17);
            this.rOpenInBrowser.TabIndex = 1;
            this.rOpenInBrowser.TabStop = true;
            this.rOpenInBrowser.Text = "Open link in default browser";
            this.rOpenInBrowser.UseVisualStyleBackColor = true;
            this.rOpenInBrowser.CheckedChanged += new System.EventHandler(this.rOpenInBrowser_CheckedChanged);
            // 
            // gWebInt
            // 
            this.gWebInt.AccessibleDescription = "";
            this.gWebInt.Controls.Add(this.labViewInBrowser);
            this.gWebInt.Controls.Add(this.chkWebInt);
            this.gWebInt.Location = new System.Drawing.Point(8, 160);
            this.gWebInt.Name = "gWebInt";
            this.gWebInt.Size = new System.Drawing.Size(449, 57);
            this.gWebInt.TabIndex = 11;
            this.gWebInt.TabStop = false;
            this.gWebInt.Text = "Web Interface";
            // 
            // labViewInBrowser
            // 
            this.labViewInBrowser.AccessibleDescription = "opens the web interface in browser";
            this.labViewInBrowser.AccessibleName = "View in browser";
            this.labViewInBrowser.AutoSize = true;
            this.labViewInBrowser.Location = new System.Drawing.Point(188, 25);
            this.labViewInBrowser.Name = "labViewInBrowser";
            this.labViewInBrowser.Size = new System.Drawing.Size(87, 13);
            this.labViewInBrowser.TabIndex = 5;
            this.labViewInBrowser.TabStop = true;
            this.labViewInBrowser.Text = "(View in browser)";
            this.labViewInBrowser.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.labViewInBrowser_LinkClicked);
            // 
            // chkWebInt
            // 
            this.chkWebInt.AccessibleDescription = "";
            this.chkWebInt.AccessibleName = "use the web interface?";
            this.chkWebInt.AutoSize = true;
            this.chkWebInt.Location = new System.Drawing.Point(9, 24);
            this.chkWebInt.Name = "chkWebInt";
            this.chkWebInt.Size = new System.Drawing.Size(130, 17);
            this.chkWebInt.TabIndex = 4;
            this.chkWebInt.Text = "Use the web interface";
            this.chkWebInt.UseVisualStyleBackColor = true;
            this.chkWebInt.CheckedChanged += new System.EventHandler(this.chkWebInt_CheckedChanged);
            // 
            // gApp
            // 
            this.gApp.AccessibleDescription = "";
            this.gApp.Controls.Add(this.labLang);
            this.gApp.Controls.Add(this.cmbLang);
            this.gApp.Controls.Add(this.chkShowNots);
            this.gApp.Controls.Add(this.chkStartUp);
            this.gApp.Location = new System.Drawing.Point(8, 223);
            this.gApp.Name = "gApp";
            this.gApp.Size = new System.Drawing.Size(449, 69);
            this.gApp.TabIndex = 3;
            this.gApp.TabStop = false;
            this.gApp.Text = "Application";
            // 
            // labLang
            // 
            this.labLang.AccessibleDescription = "";
            this.labLang.AccessibleName = "Language";
            this.labLang.AutoSize = true;
            this.labLang.Location = new System.Drawing.Point(8, 67);
            this.labLang.Name = "labLang";
            this.labLang.Size = new System.Drawing.Size(58, 13);
            this.labLang.TabIndex = 5;
            this.labLang.Text = "Language:";
            this.labLang.Visible = false;
            // 
            // cmbLang
            // 
            this.cmbLang.AccessibleDescription = "";
            this.cmbLang.AccessibleName = "select language";
            this.cmbLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLang.FormattingEnabled = true;
            this.cmbLang.Items.AddRange(new object[] {
            "English (en)",
            "Español (es)",
            "Deutsch (de)",
            "Français (fr)",
            "Dutch (nl)",
            "Ελληνικά (el)",
            "Italian (it)",
            "Turkish (tr)",
            "Brazilian Portuguese (pt-BR)",
            "Føroyskt (fo)",
            "Swedish (sv)",
            "Albanian (sq)",
            "Romanian (ro)",
            "Korean (ko)",
            "Russian (ru)",
            "Japanese (ja)",
            "Vietnamese (vi)",
            "Norwegian (no)",
            "Hungarian (hu)"});
            this.cmbLang.Location = new System.Drawing.Point(72, 64);
            this.cmbLang.Name = "cmbLang";
            this.cmbLang.Size = new System.Drawing.Size(164, 21);
            this.cmbLang.TabIndex = 8;
            this.cmbLang.Visible = false;
            this.cmbLang.SelectedIndexChanged += new System.EventHandler(this.cmbLang_SelectedIndexChanged);
            // 
            // chkShowNots
            // 
            this.chkShowNots.AccessibleDescription = "";
            this.chkShowNots.AccessibleName = "show notifications";
            this.chkShowNots.AutoSize = true;
            this.chkShowNots.Checked = true;
            this.chkShowNots.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowNots.Location = new System.Drawing.Point(9, 42);
            this.chkShowNots.Name = "chkShowNots";
            this.chkShowNots.Size = new System.Drawing.Size(112, 17);
            this.chkShowNots.TabIndex = 7;
            this.chkShowNots.Text = "Show notifications";
            this.chkShowNots.UseVisualStyleBackColor = true;
            this.chkShowNots.CheckedChanged += new System.EventHandler(this.chkShowNots_CheckedChanged);
            // 
            // chkStartUp
            // 
            this.chkStartUp.AccessibleDescription = "";
            this.chkStartUp.AccessibleName = "start on system startup";
            this.chkStartUp.AutoSize = true;
            this.chkStartUp.Location = new System.Drawing.Point(9, 19);
            this.chkStartUp.Name = "chkStartUp";
            this.chkStartUp.Size = new System.Drawing.Size(136, 17);
            this.chkStartUp.TabIndex = 6;
            this.chkStartUp.Text = "Start on system start up";
            this.chkStartUp.UseVisualStyleBackColor = true;
            this.chkStartUp.CheckedChanged += new System.EventHandler(this.chkStartUp_CheckedChanged);
            // 
            // tabAccount
            // 
            this.tabAccount.AccessibleDescription = "";
            this.tabAccount.Controls.Add(this.gAccount);
            this.tabAccount.Controls.Add(this.gDetails);
            this.tabAccount.Controls.Add(this.bChangeBox);
            this.tabAccount.Location = new System.Drawing.Point(4, 22);
            this.tabAccount.Name = "tabAccount";
            this.tabAccount.Padding = new System.Windows.Forms.Padding(3);
            this.tabAccount.Size = new System.Drawing.Size(474, 331);
            this.tabAccount.TabIndex = 1;
            this.tabAccount.Text = "FTPbox";
            this.tabAccount.UseVisualStyleBackColor = true;
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
            this.gAccount.Location = new System.Drawing.Point(8, 6);
            this.gAccount.Name = "gAccount";
            this.gAccount.Size = new System.Drawing.Size(447, 102);
            this.gAccount.TabIndex = 3;
            this.gAccount.TabStop = false;
            this.gAccount.Text = "FTP Account";
            // 
            // lMode
            // 
            this.lMode.AccessibleDescription = "";
            this.lMode.AutoSize = true;
            this.lMode.Location = new System.Drawing.Point(92, 75);
            this.lMode.Name = "lMode";
            this.lMode.Size = new System.Drawing.Size(27, 13);
            this.lMode.TabIndex = 13;
            this.lMode.Text = "N/A";
            // 
            // labMode
            // 
            this.labMode.AccessibleDescription = "";
            this.labMode.AutoSize = true;
            this.labMode.Location = new System.Drawing.Point(6, 75);
            this.labMode.Name = "labMode";
            this.labMode.Size = new System.Drawing.Size(37, 13);
            this.labMode.TabIndex = 12;
            this.labMode.Text = "Mode:";
            // 
            // lPort
            // 
            this.lPort.AccessibleDescription = "";
            this.lPort.AutoSize = true;
            this.lPort.Location = new System.Drawing.Point(92, 57);
            this.lPort.Name = "lPort";
            this.lPort.Size = new System.Drawing.Size(27, 13);
            this.lPort.TabIndex = 10;
            this.lPort.Text = "N/A";
            // 
            // lHost
            // 
            this.lHost.AccessibleDescription = "";
            this.lHost.AutoSize = true;
            this.lHost.Location = new System.Drawing.Point(92, 21);
            this.lHost.Name = "lHost";
            this.lHost.Size = new System.Drawing.Size(27, 13);
            this.lHost.TabIndex = 9;
            this.lHost.Text = "N/A";
            // 
            // lUsername
            // 
            this.lUsername.AccessibleDescription = "";
            this.lUsername.AutoSize = true;
            this.lUsername.Location = new System.Drawing.Point(92, 39);
            this.lUsername.Name = "lUsername";
            this.lUsername.Size = new System.Drawing.Size(27, 13);
            this.lUsername.TabIndex = 8;
            this.lUsername.Text = "N/A";
            // 
            // labPort
            // 
            this.labPort.AccessibleDescription = "";
            this.labPort.AutoSize = true;
            this.labPort.Location = new System.Drawing.Point(6, 57);
            this.labPort.Name = "labPort";
            this.labPort.Size = new System.Drawing.Size(29, 13);
            this.labPort.TabIndex = 6;
            this.labPort.Text = "Port:";
            // 
            // labUN
            // 
            this.labUN.AccessibleDescription = "";
            this.labUN.AutoSize = true;
            this.labUN.Location = new System.Drawing.Point(6, 39);
            this.labUN.Name = "labUN";
            this.labUN.Size = new System.Drawing.Size(58, 13);
            this.labUN.TabIndex = 5;
            this.labUN.Text = "Username:";
            // 
            // labHost
            // 
            this.labHost.AccessibleDescription = "";
            this.labHost.AutoSize = true;
            this.labHost.Location = new System.Drawing.Point(6, 21);
            this.labHost.Name = "labHost";
            this.labHost.Size = new System.Drawing.Size(32, 13);
            this.labHost.TabIndex = 4;
            this.labHost.Text = "Host:";
            // 
            // gDetails
            // 
            this.gDetails.AccessibleDescription = "";
            this.gDetails.Controls.Add(this.lLocPath);
            this.gDetails.Controls.Add(this.lRemPath);
            this.gDetails.Controls.Add(this.labLocPath);
            this.gDetails.Controls.Add(this.labRemPath);
            this.gDetails.Location = new System.Drawing.Point(8, 114);
            this.gDetails.Name = "gDetails";
            this.gDetails.Size = new System.Drawing.Size(447, 98);
            this.gDetails.TabIndex = 0;
            this.gDetails.TabStop = false;
            this.gDetails.Text = "Details";
            // 
            // lLocPath
            // 
            this.lLocPath.AccessibleDescription = "";
            this.lLocPath.AutoSize = true;
            this.lLocPath.Location = new System.Drawing.Point(26, 75);
            this.lLocPath.Name = "lLocPath";
            this.lLocPath.Size = new System.Drawing.Size(27, 13);
            this.lLocPath.TabIndex = 6;
            this.lLocPath.Text = "N/A";
            // 
            // lRemPath
            // 
            this.lRemPath.AccessibleDescription = "";
            this.lRemPath.AutoSize = true;
            this.lRemPath.Location = new System.Drawing.Point(26, 37);
            this.lRemPath.Name = "lRemPath";
            this.lRemPath.Size = new System.Drawing.Size(27, 13);
            this.lRemPath.TabIndex = 5;
            this.lRemPath.Text = "N/A";
            // 
            // labLocPath
            // 
            this.labLocPath.AccessibleDescription = "";
            this.labLocPath.AutoSize = true;
            this.labLocPath.Location = new System.Drawing.Point(6, 55);
            this.labLocPath.Name = "labLocPath";
            this.labLocPath.Size = new System.Drawing.Size(61, 13);
            this.labLocPath.TabIndex = 1;
            this.labLocPath.Text = "Local Path:";
            // 
            // labRemPath
            // 
            this.labRemPath.AccessibleDescription = "";
            this.labRemPath.AutoSize = true;
            this.labRemPath.Location = new System.Drawing.Point(6, 19);
            this.labRemPath.Name = "labRemPath";
            this.labRemPath.Size = new System.Drawing.Size(72, 13);
            this.labRemPath.TabIndex = 0;
            this.labRemPath.Text = "Remote Path:";
            // 
            // bChangeBox
            // 
            this.bChangeBox.AccessibleDescription = "opens change paths dialog";
            this.bChangeBox.AccessibleName = "Change Paths";
            this.bChangeBox.Location = new System.Drawing.Point(380, 218);
            this.bChangeBox.Name = "bChangeBox";
            this.bChangeBox.Size = new System.Drawing.Size(75, 23);
            this.bChangeBox.TabIndex = 0;
            this.bChangeBox.Text = "Change";
            this.bChangeBox.UseVisualStyleBackColor = true;
            this.bChangeBox.Click += new System.EventHandler(this.bChangeBox_Click);
            // 
            // tabBandwidth
            // 
            this.tabBandwidth.Controls.Add(this.gSyncing);
            this.tabBandwidth.Controls.Add(this.gLimits);
            this.tabBandwidth.Location = new System.Drawing.Point(4, 22);
            this.tabBandwidth.Name = "tabBandwidth";
            this.tabBandwidth.Padding = new System.Windows.Forms.Padding(3);
            this.tabBandwidth.Size = new System.Drawing.Size(474, 331);
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
            this.gSyncing.Size = new System.Drawing.Size(447, 134);
            this.gSyncing.TabIndex = 2;
            this.gSyncing.TabStop = false;
            this.gSyncing.Text = "Syncing";
            // 
            // labSeconds
            // 
            this.labSeconds.AutoSize = true;
            this.labSeconds.Location = new System.Drawing.Point(116, 96);
            this.labSeconds.Name = "labSeconds";
            this.labSeconds.Size = new System.Drawing.Size(47, 13);
            this.labSeconds.TabIndex = 5;
            this.labSeconds.Text = "seconds";
            // 
            // labSyncWhen
            // 
            this.labSyncWhen.AutoSize = true;
            this.labSyncWhen.Location = new System.Drawing.Point(18, 32);
            this.labSyncWhen.Name = "labSyncWhen";
            this.labSyncWhen.Size = new System.Drawing.Size(124, 13);
            this.labSyncWhen.TabIndex = 4;
            this.labSyncWhen.Text = "Synchronize remote files:";
            // 
            // nSyncFrequency
            // 
            this.nSyncFrequency.AccessibleName = "synchronization interval in seconds";
            this.nSyncFrequency.Location = new System.Drawing.Point(35, 94);
            this.nSyncFrequency.Name = "nSyncFrequency";
            this.nSyncFrequency.Size = new System.Drawing.Size(75, 20);
            this.nSyncFrequency.TabIndex = 2;
            this.nSyncFrequency.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nSyncFrequency.ValueChanged += new System.EventHandler(this.nSyncFrequency_ValueChanged);
            // 
            // cAuto
            // 
            this.cAuto.AccessibleName = "synchronize automatically";
            this.cAuto.AutoSize = true;
            this.cAuto.Location = new System.Drawing.Point(21, 71);
            this.cAuto.Name = "cAuto";
            this.cAuto.Size = new System.Drawing.Size(115, 17);
            this.cAuto.TabIndex = 1;
            this.cAuto.TabStop = true;
            this.cAuto.Text = "automatically every";
            this.cAuto.UseVisualStyleBackColor = true;
            this.cAuto.CheckedChanged += new System.EventHandler(this.cAuto_CheckedChanged);
            // 
            // cManually
            // 
            this.cManually.AccessibleName = "synchronize manually";
            this.cManually.AutoSize = true;
            this.cManually.Location = new System.Drawing.Point(21, 48);
            this.cManually.Name = "cManually";
            this.cManually.Size = new System.Drawing.Size(66, 17);
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
            this.gLimits.Location = new System.Drawing.Point(8, 146);
            this.gLimits.Name = "gLimits";
            this.gLimits.Size = new System.Drawing.Size(447, 169);
            this.gLimits.TabIndex = 1;
            this.gLimits.TabStop = false;
            this.gLimits.Text = "Speed Limits";
            // 
            // labNoLimits
            // 
            this.labNoLimits.AutoSize = true;
            this.labNoLimits.Location = new System.Drawing.Point(18, 142);
            this.labNoLimits.Name = "labNoLimits";
            this.labNoLimits.Size = new System.Drawing.Size(109, 13);
            this.labNoLimits.TabIndex = 14;
            this.labNoLimits.Text = "( set to 0 for no limits )";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(109, 112);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "kb/s";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(109, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "kb/s";
            // 
            // nUpLimit
            // 
            this.nUpLimit.AccessibleName = "upload speed limit";
            this.nUpLimit.Location = new System.Drawing.Point(35, 110);
            this.nUpLimit.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nUpLimit.Name = "nUpLimit";
            this.nUpLimit.Size = new System.Drawing.Size(68, 20);
            this.nUpLimit.TabIndex = 9;
            this.nUpLimit.ValueChanged += new System.EventHandler(this.nUpLimit_ValueChanged);
            // 
            // nDownLimit
            // 
            this.nDownLimit.AccessibleName = "download speed limit";
            this.nDownLimit.Location = new System.Drawing.Point(35, 51);
            this.nDownLimit.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.nDownLimit.Name = "nDownLimit";
            this.nDownLimit.Size = new System.Drawing.Size(68, 20);
            this.nDownLimit.TabIndex = 6;
            this.nDownLimit.ValueChanged += new System.EventHandler(this.nDownLimit_ValueChanged);
            // 
            // labUpSpeed
            // 
            this.labUpSpeed.AutoSize = true;
            this.labUpSpeed.Location = new System.Drawing.Point(18, 84);
            this.labUpSpeed.Name = "labUpSpeed";
            this.labUpSpeed.Size = new System.Drawing.Size(102, 13);
            this.labUpSpeed.TabIndex = 6;
            this.labUpSpeed.Text = "Limit Upload Speed:";
            // 
            // labDownSpeed
            // 
            this.labDownSpeed.AutoSize = true;
            this.labDownSpeed.Location = new System.Drawing.Point(18, 25);
            this.labDownSpeed.Name = "labDownSpeed";
            this.labDownSpeed.Size = new System.Drawing.Size(116, 13);
            this.labDownSpeed.TabIndex = 3;
            this.labDownSpeed.Text = "Limit Download Speed:";
            // 
            // tabLanguage
            // 
            this.tabLanguage.Controls.Add(this.listView1);
            this.tabLanguage.Location = new System.Drawing.Point(4, 22);
            this.tabLanguage.Name = "tabLanguage";
            this.tabLanguage.Padding = new System.Windows.Forms.Padding(3);
            this.tabLanguage.Size = new System.Drawing.Size(474, 331);
            this.tabLanguage.TabIndex = 4;
            this.tabLanguage.Text = "Language";
            this.tabLanguage.UseVisualStyleBackColor = true;
            // 
            // listView1
            // 
            this.listView1.AutoArrange = false;
            this.listView1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4,
            listViewItem5,
            listViewItem6,
            listViewItem7,
            listViewItem8,
            listViewItem9,
            listViewItem10,
            listViewItem11,
            listViewItem12,
            listViewItem13,
            listViewItem14,
            listViewItem15,
            listViewItem16,
            listViewItem17,
            listViewItem18,
            listViewItem19,
            listViewItem20,
            listViewItem21,
            listViewItem22,
            listViewItem23,
            listViewItem24,
            listViewItem25});
            this.listView1.Location = new System.Drawing.Point(8, 16);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(447, 299);
            this.listView1.SmallImageList = this.imageList1;
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.List;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "gb.png");
            this.imageList1.Images.SetKeyName(1, "al.png");
            this.imageList1.Images.SetKeyName(2, "gr.png");
            this.imageList1.Images.SetKeyName(3, "jp.png");
            this.imageList1.Images.SetKeyName(4, "no.png");
            this.imageList1.Images.SetKeyName(5, "ru.png");
            this.imageList1.Images.SetKeyName(6, "it.png");
            this.imageList1.Images.SetKeyName(7, "tr.png");
            this.imageList1.Images.SetKeyName(8, "vn.png");
            this.imageList1.Images.SetKeyName(9, "cn.png");
            this.imageList1.Images.SetKeyName(10, "fo.png");
            this.imageList1.Images.SetKeyName(11, "fr.png");
            this.imageList1.Images.SetKeyName(12, "es.png");
            this.imageList1.Images.SetKeyName(13, "de.png");
            this.imageList1.Images.SetKeyName(14, "nl.png");
            this.imageList1.Images.SetKeyName(15, "br.png");
            this.imageList1.Images.SetKeyName(16, "kr.png");
            this.imageList1.Images.SetKeyName(17, "ro.png");
            this.imageList1.Images.SetKeyName(18, "se.png");
            this.imageList1.Images.SetKeyName(19, "fr.png");
            this.imageList1.Images.SetKeyName(20, "hu.png");
            this.imageList1.Images.SetKeyName(21, "lt.png");
            this.imageList1.Images.SetKeyName(22, "dk.png");
            this.imageList1.Images.SetKeyName(23, "pl.png");
            this.imageList1.Images.SetKeyName(24, "hr.png");
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
            this.tabAbout.Size = new System.Drawing.Size(474, 331);
            this.tabAbout.TabIndex = 2;
            this.tabAbout.Text = "About";
            this.tabAbout.UseVisualStyleBackColor = true;
            // 
            // labSupportMail
            // 
            this.labSupportMail.AccessibleDescription = "";
            this.labSupportMail.AutoSize = true;
            this.labSupportMail.Location = new System.Drawing.Point(272, 113);
            this.labSupportMail.Name = "labSupportMail";
            this.labSupportMail.Size = new System.Drawing.Size(100, 13);
            this.labSupportMail.TabIndex = 14;
            this.labSupportMail.Text = "support@ftpbox.org";
            // 
            // linkLabel4
            // 
            this.linkLabel4.AccessibleDescription = "";
            this.linkLabel4.AutoSize = true;
            this.linkLabel4.Location = new System.Drawing.Point(272, 67);
            this.linkLabel4.Name = "linkLabel4";
            this.linkLabel4.Size = new System.Drawing.Size(54, 13);
            this.linkLabel4.TabIndex = 9;
            this.linkLabel4.TabStop = true;
            this.linkLabel4.Text = "ftpbox.org";
            this.linkLabel4.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel4_LinkClicked);
            // 
            // linkLabel3
            // 
            this.linkLabel3.AccessibleDescription = "";
            this.linkLabel3.AutoSize = true;
            this.linkLabel3.Location = new System.Drawing.Point(272, 44);
            this.linkLabel3.Name = "linkLabel3";
            this.linkLabel3.Size = new System.Drawing.Size(70, 13);
            this.linkLabel3.TabIndex = 8;
            this.linkLabel3.TabStop = true;
            this.linkLabel3.Text = "FTPbox team";
            this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel3_LinkClicked);
            // 
            // label19
            // 
            this.label19.AccessibleDescription = "";
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(272, 136);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(79, 13);
            this.label19.TabIndex = 13;
            this.label19.Text = "Visual C# 2010";
            // 
            // label21
            // 
            this.label21.AccessibleDescription = "";
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(272, 90);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(93, 13);
            this.label21.TabIndex = 11;
            this.label21.Text = "admin@ftpbox.org";
            // 
            // lVersion
            // 
            this.lVersion.AccessibleDescription = "";
            this.lVersion.AutoSize = true;
            this.lVersion.Location = new System.Drawing.Point(272, 21);
            this.lVersion.Name = "lVersion";
            this.lVersion.Size = new System.Drawing.Size(75, 13);
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
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 50);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(146, 13);
            this.label11.TabIndex = 3;
            this.label11.Text = "Copyright © 2012 - ftpbox.org";
            // 
            // labContactMe
            // 
            this.labContactMe.AccessibleDescription = "";
            this.labContactMe.AutoSize = true;
            this.labContactMe.Location = new System.Drawing.Point(6, 35);
            this.labContactMe.Name = "labContactMe";
            this.labContactMe.Size = new System.Drawing.Size(183, 13);
            this.labContactMe.TabIndex = 2;
            this.labContactMe.Text = "- Feel free to contact me for anything.";
            // 
            // labFree
            // 
            this.labFree.AccessibleDescription = "";
            this.labFree.AutoSize = true;
            this.labFree.Location = new System.Drawing.Point(6, 19);
            this.labFree.Name = "labFree";
            this.labFree.Size = new System.Drawing.Size(164, 13);
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
            this.ClientSize = new System.Drawing.Size(471, 349);
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
            this.tabControl1.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.gLinks.ResumeLayout(false);
            this.gLinks.PerformLayout();
            this.gWebInt.ResumeLayout(false);
            this.gWebInt.PerformLayout();
            this.gApp.ResumeLayout(false);
            this.gApp.PerformLayout();
            this.tabAccount.ResumeLayout(false);
            this.gAccount.ResumeLayout(false);
            this.gAccount.PerformLayout();
            this.gDetails.ResumeLayout(false);
            this.gDetails.PerformLayout();
            this.tabBandwidth.ResumeLayout(false);
            this.gSyncing.ResumeLayout(false);
            this.gSyncing.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nSyncFrequency)).EndInit();
            this.gLimits.ResumeLayout(false);
            this.gLimits.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUpLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nDownLimit)).EndInit();
            this.tabLanguage.ResumeLayout(false);
            this.tabAbout.ResumeLayout(false);
            this.tabAbout.PerformLayout();
            this.gContribute.ResumeLayout(false);
            this.gContribute.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.gNotes.ResumeLayout(false);
            this.gNotes.PerformLayout();
            this.trayMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.GroupBox gWebInt;
        private System.Windows.Forms.LinkLabel labViewInBrowser;
        private System.Windows.Forms.CheckBox chkWebInt;
        private System.Windows.Forms.GroupBox gApp;
        private System.Windows.Forms.Label labLang;
        private System.Windows.Forms.ComboBox cmbLang;
        private System.Windows.Forms.CheckBox chkShowNots;
        private System.Windows.Forms.WebBrowser browser;
        private System.Windows.Forms.CheckBox chkStartUp;
        private System.Windows.Forms.TabPage tabAccount;
        private System.Windows.Forms.GroupBox gDetails;
        private System.Windows.Forms.Label lLocPath;
        private System.Windows.Forms.Label lRemPath;
        private System.Windows.Forms.Button bChangeBox;
        private System.Windows.Forms.Label labLocPath;
        private System.Windows.Forms.Label labRemPath;
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
        private System.Windows.Forms.Label labLinkClicked;
        private System.Windows.Forms.TextBox tParent;
        private System.Windows.Forms.Label labFullPath;
        private System.Windows.Forms.RadioButton rCopy2Clipboard;
        private System.Windows.Forms.RadioButton rOpenInBrowser;
        private System.Windows.Forms.GroupBox gAccount;
        private System.Windows.Forms.Label lMode;
        private System.Windows.Forms.Label labMode;
        private System.Windows.Forms.Label lPort;
        private System.Windows.Forms.Label lHost;
        private System.Windows.Forms.Label lUsername;
        private System.Windows.Forms.Label labPort;
        private System.Windows.Forms.Label labUN;
        private System.Windows.Forms.Label labHost;
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
        private System.Windows.Forms.TabPage tabLanguage;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ImageList imageList1;
    }
}