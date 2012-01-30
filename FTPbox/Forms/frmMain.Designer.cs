namespace FTPbox
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recentFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.gWebInt = new System.Windows.Forms.GroupBox();
            this.labViewInBrowser = new System.Windows.Forms.LinkLabel();
            this.chkWebInt = new System.Windows.Forms.CheckBox();
            this.gApp = new System.Windows.Forms.GroupBox();
            this.labLang = new System.Windows.Forms.Label();
            this.cmbLang = new System.Windows.Forms.ComboBox();
            this.chkShowNots = new System.Windows.Forms.CheckBox();
            this.chkStartUp = new System.Windows.Forms.CheckBox();
            this.gAccount = new System.Windows.Forms.GroupBox();
            this.lMode = new System.Windows.Forms.Label();
            this.labMode = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.bAddFTP = new System.Windows.Forms.Button();
            this.lPort = new System.Windows.Forms.Label();
            this.lHost = new System.Windows.Forms.Label();
            this.lUsername = new System.Windows.Forms.Label();
            this.labPort = new System.Windows.Forms.Label();
            this.labUN = new System.Windows.Forms.Label();
            this.labHost = new System.Windows.Forms.Label();
            this.browser = new System.Windows.Forms.WebBrowser();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.gLinks = new System.Windows.Forms.GroupBox();
            this.labLinkClicked = new System.Windows.Forms.Label();
            this.tParent = new System.Windows.Forms.TextBox();
            this.labFullPath = new System.Windows.Forms.Label();
            this.rCopy2Clipboard = new System.Windows.Forms.RadioButton();
            this.rOpenInBrowser = new System.Windows.Forms.RadioButton();
            this.gDetails = new System.Windows.Forms.GroupBox();
            this.lLocPath = new System.Windows.Forms.Label();
            this.lRemPath = new System.Windows.Forms.Label();
            this.bChangeBox = new System.Windows.Forms.Button();
            this.labLocPath = new System.Windows.Forms.Label();
            this.labRemPath = new System.Windows.Forms.Label();
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
            this.bgWork = new System.ComponentModel.BackgroundWorker();
            this.tray = new System.Windows.Forms.NotifyIcon(this.components);
            this.fswFolders = new System.IO.FileSystemWatcher();
            this.fswFiles = new System.IO.FileSystemWatcher();
            this.CheckConnection = new System.Windows.Forms.Timer(this.components);
            this.trayMenu.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.gWebInt.SuspendLayout();
            this.gApp.SuspendLayout();
            this.gAccount.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.gLinks.SuspendLayout();
            this.gDetails.SuspendLayout();
            this.tabAbout.SuspendLayout();
            this.gContribute.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.gNotes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fswFolders)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fswFiles)).BeginInit();
            this.SuspendLayout();
            // 
            // trayMenu
            // 
            this.trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem,
            this.recentFilesToolStripMenuItem,
            this.aboutToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.trayMenu.Name = "trayMenu";
            this.trayMenu.Size = new System.Drawing.Size(137, 92);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.optionsToolStripMenuItem.Text = "Options";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // recentFilesToolStripMenuItem
            // 
            this.recentFilesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2,
            this.toolStripMenuItem3,
            this.toolStripMenuItem4,
            this.toolStripMenuItem5});
            this.recentFilesToolStripMenuItem.Name = "recentFilesToolStripMenuItem";
            this.recentFilesToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.recentFilesToolStripMenuItem.Text = "Recent Files";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(143, 22);
            this.toolStripMenuItem1.Text = "Not available";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(143, 22);
            this.toolStripMenuItem2.Text = "Not available";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(143, 22);
            this.toolStripMenuItem3.Text = "Not available";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(143, 22);
            this.toolStripMenuItem4.Text = "Not available";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.toolStripMenuItem4_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(143, 22);
            this.toolStripMenuItem5.Text = "Not available";
            this.toolStripMenuItem5.Click += new System.EventHandler(this.toolStripMenuItem5_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabGeneral);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabAbout);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(396, 316);
            this.tabControl1.TabIndex = 11;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.gWebInt);
            this.tabGeneral.Controls.Add(this.gApp);
            this.tabGeneral.Controls.Add(this.gAccount);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(388, 290);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // gWebInt
            // 
            this.gWebInt.Controls.Add(this.labViewInBrowser);
            this.gWebInt.Controls.Add(this.chkWebInt);
            this.gWebInt.Location = new System.Drawing.Point(8, 127);
            this.gWebInt.Name = "gWebInt";
            this.gWebInt.Size = new System.Drawing.Size(370, 57);
            this.gWebInt.TabIndex = 11;
            this.gWebInt.TabStop = false;
            this.gWebInt.Text = "Web Interface";
            // 
            // labViewInBrowser
            // 
            this.labViewInBrowser.AutoSize = true;
            this.labViewInBrowser.Location = new System.Drawing.Point(188, 25);
            this.labViewInBrowser.Name = "labViewInBrowser";
            this.labViewInBrowser.Size = new System.Drawing.Size(87, 13);
            this.labViewInBrowser.TabIndex = 2;
            this.labViewInBrowser.TabStop = true;
            this.labViewInBrowser.Text = "(View in browser)";
            this.labViewInBrowser.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.labViewInBrowser_LinkClicked);
            // 
            // chkWebInt
            // 
            this.chkWebInt.AutoSize = true;
            this.chkWebInt.Location = new System.Drawing.Point(9, 24);
            this.chkWebInt.Name = "chkWebInt";
            this.chkWebInt.Size = new System.Drawing.Size(130, 17);
            this.chkWebInt.TabIndex = 1;
            this.chkWebInt.Text = "Use the web interface";
            this.chkWebInt.UseVisualStyleBackColor = true;
            this.chkWebInt.CheckedChanged += new System.EventHandler(this.chkWebInt_CheckedChanged);
            // 
            // gApp
            // 
            this.gApp.Controls.Add(this.labLang);
            this.gApp.Controls.Add(this.cmbLang);
            this.gApp.Controls.Add(this.chkShowNots);
            this.gApp.Controls.Add(this.browser);
            this.gApp.Controls.Add(this.chkStartUp);
            this.gApp.Location = new System.Drawing.Point(8, 190);
            this.gApp.Name = "gApp";
            this.gApp.Size = new System.Drawing.Size(370, 94);
            this.gApp.TabIndex = 3;
            this.gApp.TabStop = false;
            this.gApp.Text = "Application";
            // 
            // labLang
            // 
            this.labLang.AutoSize = true;
            this.labLang.Location = new System.Drawing.Point(8, 67);
            this.labLang.Name = "labLang";
            this.labLang.Size = new System.Drawing.Size(58, 13);
            this.labLang.TabIndex = 5;
            this.labLang.Text = "Language:";
            // 
            // cmbLang
            // 
            this.cmbLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLang.FormattingEnabled = true;
            this.cmbLang.Items.AddRange(new object[] {
            "English (en)",
            "Español (es)",
            "Deutsch (de)",
            "Français (fr)",
            "Dutch (nl)",
            "Ελληνικά (el)"});
            this.cmbLang.Location = new System.Drawing.Point(72, 64);
            this.cmbLang.Name = "cmbLang";
            this.cmbLang.Size = new System.Drawing.Size(112, 21);
            this.cmbLang.TabIndex = 4;
            this.cmbLang.SelectedIndexChanged += new System.EventHandler(this.cmbLang_SelectedIndexChanged);
            // 
            // chkShowNots
            // 
            this.chkShowNots.AutoSize = true;
            this.chkShowNots.Location = new System.Drawing.Point(9, 42);
            this.chkShowNots.Name = "chkShowNots";
            this.chkShowNots.Size = new System.Drawing.Size(112, 17);
            this.chkShowNots.TabIndex = 2;
            this.chkShowNots.Text = "Show notifications";
            this.chkShowNots.UseVisualStyleBackColor = true;
            this.chkShowNots.CheckedChanged += new System.EventHandler(this.chkShowNots_CheckedChanged);
            // 
            // chkStartUp
            // 
            this.chkStartUp.AutoSize = true;
            this.chkStartUp.Location = new System.Drawing.Point(9, 19);
            this.chkStartUp.Name = "chkStartUp";
            this.chkStartUp.Size = new System.Drawing.Size(136, 17);
            this.chkStartUp.TabIndex = 1;
            this.chkStartUp.Text = "Start on system start up";
            this.chkStartUp.UseVisualStyleBackColor = true;
            this.chkStartUp.CheckedChanged += new System.EventHandler(this.chkStartUp_CheckedChanged);
            // 
            // gAccount
            // 
            this.gAccount.Controls.Add(this.lMode);
            this.gAccount.Controls.Add(this.labMode);
            this.gAccount.Controls.Add(this.button1);
            this.gAccount.Controls.Add(this.bAddFTP);
            this.gAccount.Controls.Add(this.lPort);
            this.gAccount.Controls.Add(this.lHost);
            this.gAccount.Controls.Add(this.lUsername);
            this.gAccount.Controls.Add(this.labPort);
            this.gAccount.Controls.Add(this.labUN);
            this.gAccount.Controls.Add(this.labHost);
            this.gAccount.Location = new System.Drawing.Point(8, 9);
            this.gAccount.Name = "gAccount";
            this.gAccount.Size = new System.Drawing.Size(370, 112);
            this.gAccount.TabIndex = 2;
            this.gAccount.TabStop = false;
            this.gAccount.Text = "FTP Account";
            // 
            // lMode
            // 
            this.lMode.AutoSize = true;
            this.lMode.Location = new System.Drawing.Point(92, 81);
            this.lMode.Name = "lMode";
            this.lMode.Size = new System.Drawing.Size(27, 13);
            this.lMode.TabIndex = 13;
            this.lMode.Text = "N/A";
            // 
            // labMode
            // 
            this.labMode.AutoSize = true;
            this.labMode.Location = new System.Drawing.Point(6, 81);
            this.labMode.Name = "labMode";
            this.labMode.Size = new System.Drawing.Size(37, 13);
            this.labMode.TabIndex = 12;
            this.labMode.Text = "Mode:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(278, 32);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 11;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // bAddFTP
            // 
            this.bAddFTP.Location = new System.Drawing.Point(308, 81);
            this.bAddFTP.Name = "bAddFTP";
            this.bAddFTP.Size = new System.Drawing.Size(56, 24);
            this.bAddFTP.TabIndex = 0;
            this.bAddFTP.Text = "Change";
            this.bAddFTP.UseVisualStyleBackColor = true;
            this.bAddFTP.Click += new System.EventHandler(this.bAddFTP_Click);
            // 
            // lPort
            // 
            this.lPort.AutoSize = true;
            this.lPort.Location = new System.Drawing.Point(92, 63);
            this.lPort.Name = "lPort";
            this.lPort.Size = new System.Drawing.Size(27, 13);
            this.lPort.TabIndex = 10;
            this.lPort.Text = "N/A";
            // 
            // lHost
            // 
            this.lHost.AutoSize = true;
            this.lHost.Location = new System.Drawing.Point(92, 27);
            this.lHost.Name = "lHost";
            this.lHost.Size = new System.Drawing.Size(27, 13);
            this.lHost.TabIndex = 9;
            this.lHost.Text = "N/A";
            // 
            // lUsername
            // 
            this.lUsername.AutoSize = true;
            this.lUsername.Location = new System.Drawing.Point(92, 45);
            this.lUsername.Name = "lUsername";
            this.lUsername.Size = new System.Drawing.Size(27, 13);
            this.lUsername.TabIndex = 8;
            this.lUsername.Text = "N/A";
            // 
            // labPort
            // 
            this.labPort.AutoSize = true;
            this.labPort.Location = new System.Drawing.Point(6, 63);
            this.labPort.Name = "labPort";
            this.labPort.Size = new System.Drawing.Size(29, 13);
            this.labPort.TabIndex = 6;
            this.labPort.Text = "Port:";
            // 
            // labUN
            // 
            this.labUN.AutoSize = true;
            this.labUN.Location = new System.Drawing.Point(6, 45);
            this.labUN.Name = "labUN";
            this.labUN.Size = new System.Drawing.Size(58, 13);
            this.labUN.TabIndex = 5;
            this.labUN.Text = "Username:";
            // 
            // labHost
            // 
            this.labHost.AutoSize = true;
            this.labHost.Location = new System.Drawing.Point(6, 27);
            this.labHost.Name = "labHost";
            this.labHost.Size = new System.Drawing.Size(32, 13);
            this.labHost.TabIndex = 4;
            this.labHost.Text = "Host:";
            // 
            // browser
            // 
            this.browser.Location = new System.Drawing.Point(326, 60);
            this.browser.MinimumSize = new System.Drawing.Size(20, 20);
            this.browser.Name = "browser";
            this.browser.Size = new System.Drawing.Size(38, 20);
            this.browser.TabIndex = 9;
            this.browser.Visible = false;
            this.browser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.browser_DocumentCompleted);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.gLinks);
            this.tabPage2.Controls.Add(this.gDetails);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(388, 290);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "FTPbox";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // gLinks
            // 
            this.gLinks.Controls.Add(this.labLinkClicked);
            this.gLinks.Controls.Add(this.tParent);
            this.gLinks.Controls.Add(this.labFullPath);
            this.gLinks.Controls.Add(this.rCopy2Clipboard);
            this.gLinks.Controls.Add(this.rOpenInBrowser);
            this.gLinks.Location = new System.Drawing.Point(8, 128);
            this.gLinks.Name = "gLinks";
            this.gLinks.Size = new System.Drawing.Size(374, 127);
            this.gLinks.TabIndex = 1;
            this.gLinks.TabStop = false;
            this.gLinks.Text = "Links";
            // 
            // labLinkClicked
            // 
            this.labLinkClicked.AutoSize = true;
            this.labLinkClicked.Location = new System.Drawing.Point(6, 55);
            this.labLinkClicked.Name = "labLinkClicked";
            this.labLinkClicked.Size = new System.Drawing.Size(221, 13);
            this.labLinkClicked.TabIndex = 18;
            this.labLinkClicked.Text = "When tray notification or recent file is clicked:";
            // 
            // tParent
            // 
            this.tParent.Location = new System.Drawing.Point(19, 32);
            this.tParent.Name = "tParent";
            this.tParent.Size = new System.Drawing.Size(255, 20);
            this.tParent.TabIndex = 5;
            this.tParent.TextChanged += new System.EventHandler(this.tParent_TextChanged);
            // 
            // labFullPath
            // 
            this.labFullPath.AutoSize = true;
            this.labFullPath.Location = new System.Drawing.Point(6, 16);
            this.labFullPath.Name = "labFullPath";
            this.labFullPath.Size = new System.Drawing.Size(97, 13);
            this.labFullPath.TabIndex = 2;
            this.labFullPath.Text = "Account\'s full path:";
            // 
            // rCopy2Clipboard
            // 
            this.rCopy2Clipboard.AutoSize = true;
            this.rCopy2Clipboard.Location = new System.Drawing.Point(19, 99);
            this.rCopy2Clipboard.Name = "rCopy2Clipboard";
            this.rCopy2Clipboard.Size = new System.Drawing.Size(125, 17);
            this.rCopy2Clipboard.TabIndex = 7;
            this.rCopy2Clipboard.TabStop = true;
            this.rCopy2Clipboard.Text = "copy link to clipboard";
            this.rCopy2Clipboard.UseVisualStyleBackColor = true;
            // 
            // rOpenInBrowser
            // 
            this.rOpenInBrowser.AutoSize = true;
            this.rOpenInBrowser.Location = new System.Drawing.Point(19, 76);
            this.rOpenInBrowser.Name = "rOpenInBrowser";
            this.rOpenInBrowser.Size = new System.Drawing.Size(156, 17);
            this.rOpenInBrowser.TabIndex = 6;
            this.rOpenInBrowser.TabStop = true;
            this.rOpenInBrowser.Text = "Open link in default browser";
            this.rOpenInBrowser.UseVisualStyleBackColor = true;
            this.rOpenInBrowser.CheckedChanged += new System.EventHandler(this.rOpenInBrowser_CheckedChanged);
            // 
            // gDetails
            // 
            this.gDetails.Controls.Add(this.lLocPath);
            this.gDetails.Controls.Add(this.lRemPath);
            this.gDetails.Controls.Add(this.bChangeBox);
            this.gDetails.Controls.Add(this.labLocPath);
            this.gDetails.Controls.Add(this.labRemPath);
            this.gDetails.Location = new System.Drawing.Point(8, 9);
            this.gDetails.Name = "gDetails";
            this.gDetails.Size = new System.Drawing.Size(374, 113);
            this.gDetails.TabIndex = 0;
            this.gDetails.TabStop = false;
            this.gDetails.Text = "Details";
            // 
            // lLocPath
            // 
            this.lLocPath.AutoSize = true;
            this.lLocPath.Location = new System.Drawing.Point(26, 75);
            this.lLocPath.Name = "lLocPath";
            this.lLocPath.Size = new System.Drawing.Size(27, 13);
            this.lLocPath.TabIndex = 6;
            this.lLocPath.Text = "N/A";
            // 
            // lRemPath
            // 
            this.lRemPath.AutoSize = true;
            this.lRemPath.Location = new System.Drawing.Point(26, 37);
            this.lRemPath.Name = "lRemPath";
            this.lRemPath.Size = new System.Drawing.Size(27, 13);
            this.lRemPath.TabIndex = 5;
            this.lRemPath.Text = "N/A";
            // 
            // bChangeBox
            // 
            this.bChangeBox.Location = new System.Drawing.Point(293, 84);
            this.bChangeBox.Name = "bChangeBox";
            this.bChangeBox.Size = new System.Drawing.Size(75, 23);
            this.bChangeBox.TabIndex = 4;
            this.bChangeBox.Text = "Change";
            this.bChangeBox.UseVisualStyleBackColor = true;
            this.bChangeBox.Click += new System.EventHandler(this.bChangeBox_Click);
            // 
            // labLocPath
            // 
            this.labLocPath.AutoSize = true;
            this.labLocPath.Location = new System.Drawing.Point(6, 55);
            this.labLocPath.Name = "labLocPath";
            this.labLocPath.Size = new System.Drawing.Size(61, 13);
            this.labLocPath.TabIndex = 1;
            this.labLocPath.Text = "Local Path:";
            // 
            // labRemPath
            // 
            this.labRemPath.AutoSize = true;
            this.labRemPath.Location = new System.Drawing.Point(6, 19);
            this.labRemPath.Name = "labRemPath";
            this.labRemPath.Size = new System.Drawing.Size(72, 13);
            this.labRemPath.TabIndex = 0;
            this.labRemPath.Text = "Remote Path:";
            // 
            // tabAbout
            // 
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
            this.tabAbout.Size = new System.Drawing.Size(388, 290);
            this.tabAbout.TabIndex = 2;
            this.tabAbout.Text = "About";
            this.tabAbout.UseVisualStyleBackColor = true;
            // 
            // labSupportMail
            // 
            this.labSupportMail.AutoSize = true;
            this.labSupportMail.Location = new System.Drawing.Point(234, 113);
            this.labSupportMail.Name = "labSupportMail";
            this.labSupportMail.Size = new System.Drawing.Size(100, 13);
            this.labSupportMail.TabIndex = 14;
            this.labSupportMail.Text = "support@ftpbox.org";
            // 
            // linkLabel4
            // 
            this.linkLabel4.AutoSize = true;
            this.linkLabel4.Location = new System.Drawing.Point(234, 67);
            this.linkLabel4.Name = "linkLabel4";
            this.linkLabel4.Size = new System.Drawing.Size(54, 13);
            this.linkLabel4.TabIndex = 9;
            this.linkLabel4.TabStop = true;
            this.linkLabel4.Text = "ftpbox.org";
            this.linkLabel4.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel4_LinkClicked);
            // 
            // linkLabel3
            // 
            this.linkLabel3.AutoSize = true;
            this.linkLabel3.Location = new System.Drawing.Point(234, 44);
            this.linkLabel3.Name = "linkLabel3";
            this.linkLabel3.Size = new System.Drawing.Size(70, 13);
            this.linkLabel3.TabIndex = 8;
            this.linkLabel3.TabStop = true;
            this.linkLabel3.Text = "FTPbox team";
            this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel3_LinkClicked);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(234, 136);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(79, 13);
            this.label19.TabIndex = 13;
            this.label19.Text = "Visual C# 2010";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(234, 90);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(93, 13);
            this.label21.TabIndex = 11;
            this.label21.Text = "admin@ftpbox.org";
            // 
            // lVersion
            // 
            this.lVersion.AutoSize = true;
            this.lVersion.Location = new System.Drawing.Point(234, 21);
            this.lVersion.Name = "lVersion";
            this.lVersion.Size = new System.Drawing.Size(75, 13);
            this.lVersion.TabIndex = 8;
            this.lVersion.Text = "X.X.X (build X)";
            // 
            // labLangUsed
            // 
            this.labLangUsed.AutoSize = true;
            this.labLangUsed.Location = new System.Drawing.Point(64, 136);
            this.labLangUsed.Name = "labLangUsed";
            this.labLangUsed.Size = new System.Drawing.Size(84, 13);
            this.labLangUsed.TabIndex = 7;
            this.labLangUsed.Text = "Language used:";
            // 
            // labContact
            // 
            this.labContact.AutoSize = true;
            this.labContact.Location = new System.Drawing.Point(64, 90);
            this.labContact.Name = "labContact";
            this.labContact.Size = new System.Drawing.Size(47, 13);
            this.labContact.TabIndex = 5;
            this.labContact.Text = "Contact:";
            // 
            // labSite
            // 
            this.labSite.AutoSize = true;
            this.labSite.Location = new System.Drawing.Point(64, 67);
            this.labSite.Name = "labSite";
            this.labSite.Size = new System.Drawing.Size(84, 13);
            this.labSite.TabIndex = 4;
            this.labSite.Text = "Official Website:";
            // 
            // labTeam
            // 
            this.labTeam.AutoSize = true;
            this.labTeam.Location = new System.Drawing.Point(64, 44);
            this.labTeam.Name = "labTeam";
            this.labTeam.Size = new System.Drawing.Size(59, 13);
            this.labTeam.TabIndex = 3;
            this.labTeam.Text = "The Team:";
            // 
            // labCurVersion
            // 
            this.labCurVersion.AutoSize = true;
            this.labCurVersion.Location = new System.Drawing.Point(64, 21);
            this.labCurVersion.Name = "labCurVersion";
            this.labCurVersion.Size = new System.Drawing.Size(82, 13);
            this.labCurVersion.TabIndex = 2;
            this.labCurVersion.Text = "Current Version:";
            // 
            // gContribute
            // 
            this.gContribute.Controls.Add(this.labDonate);
            this.gContribute.Controls.Add(this.pictureBox1);
            this.gContribute.Controls.Add(this.linkLabel2);
            this.gContribute.Controls.Add(this.linkLabel1);
            this.gContribute.Location = new System.Drawing.Point(237, 155);
            this.gContribute.Name = "gContribute";
            this.gContribute.Size = new System.Drawing.Size(144, 89);
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
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Image = global::FTPbox.Properties.Resources.donate;
            this.pictureBox1.Location = new System.Drawing.Point(64, 59);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(74, 21);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(6, 35);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(92, 13);
            this.linkLabel2.TabIndex = 12;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "Request a feature";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // linkLabel1
            // 
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
            this.gNotes.Controls.Add(this.label11);
            this.gNotes.Controls.Add(this.labContactMe);
            this.gNotes.Controls.Add(this.labFree);
            this.gNotes.Location = new System.Drawing.Point(7, 155);
            this.gNotes.Name = "gNotes";
            this.gNotes.Size = new System.Drawing.Size(224, 89);
            this.gNotes.TabIndex = 0;
            this.gNotes.TabStop = false;
            this.gNotes.Text = "Notes";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 67);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(152, 13);
            this.label11.TabIndex = 3;
            this.label11.Text = "- Copyright © 2011 - ftpbox.org";
            // 
            // labContactMe
            // 
            this.labContactMe.AutoSize = true;
            this.labContactMe.Location = new System.Drawing.Point(6, 35);
            this.labContactMe.Name = "labContactMe";
            this.labContactMe.Size = new System.Drawing.Size(183, 13);
            this.labContactMe.TabIndex = 2;
            this.labContactMe.Text = "- Feel free to contact me for anything.";
            // 
            // labFree
            // 
            this.labFree.AutoSize = true;
            this.labFree.Location = new System.Drawing.Point(6, 19);
            this.labFree.Name = "labFree";
            this.labFree.Size = new System.Drawing.Size(164, 13);
            this.labFree.TabIndex = 0;
            this.labFree.Text = "- FTPbox is free and open-source";
            // 
            // bgWork
            // 
            this.bgWork.WorkerReportsProgress = true;
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
            // fswFolders
            // 
            this.fswFolders.EnableRaisingEvents = true;
            this.fswFolders.IncludeSubdirectories = true;
            this.fswFolders.NotifyFilter = ((System.IO.NotifyFilters)(((System.IO.NotifyFilters.DirectoryName | System.IO.NotifyFilters.LastWrite)
                        | System.IO.NotifyFilters.LastAccess)));
            this.fswFolders.SynchronizingObject = this;
            // 
            // fswFiles
            // 
            this.fswFiles.EnableRaisingEvents = true;
            this.fswFiles.IncludeSubdirectories = true;
            this.fswFiles.NotifyFilter = ((System.IO.NotifyFilters)(((System.IO.NotifyFilters.FileName | System.IO.NotifyFilters.LastWrite)
                        | System.IO.NotifyFilters.LastAccess)));
            this.fswFiles.SynchronizingObject = this;
            // 
            // CheckConnection
            // 
            this.CheckConnection.Enabled = true;
            this.CheckConnection.Tick += new System.EventHandler(this.CheckConnection_Tick);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 312);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FTPbox | Options";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.trayMenu.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.gWebInt.ResumeLayout(false);
            this.gWebInt.PerformLayout();
            this.gApp.ResumeLayout(false);
            this.gApp.PerformLayout();
            this.gAccount.ResumeLayout(false);
            this.gAccount.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.gLinks.ResumeLayout(false);
            this.gLinks.PerformLayout();
            this.gDetails.ResumeLayout(false);
            this.gDetails.PerformLayout();
            this.tabAbout.ResumeLayout(false);
            this.tabAbout.PerformLayout();
            this.gContribute.ResumeLayout(false);
            this.gContribute.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.gNotes.ResumeLayout(false);
            this.gNotes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fswFolders)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fswFiles)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip trayMenu;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.GroupBox gApp;
        private System.Windows.Forms.CheckBox chkShowNots;
        private System.Windows.Forms.CheckBox chkStartUp;
        private System.Windows.Forms.GroupBox gAccount;
        private System.Windows.Forms.Button bAddFTP;
        private System.Windows.Forms.Label lPort;
        private System.Windows.Forms.Label lHost;
        private System.Windows.Forms.Label lUsername;
        private System.Windows.Forms.Label labPort;
        private System.Windows.Forms.Label labUN;
        private System.Windows.Forms.Label labHost;
        private System.Windows.Forms.WebBrowser browser;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabAbout;
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
        private System.ComponentModel.BackgroundWorker bgWork;
        private System.Windows.Forms.NotifyIcon tray;
        private System.Windows.Forms.GroupBox gDetails;
        private System.Windows.Forms.Label lLocPath;
        private System.Windows.Forms.Label lRemPath;
        private System.Windows.Forms.Button bChangeBox;
        private System.Windows.Forms.Label labLocPath;
        private System.Windows.Forms.Label labRemPath;
        private System.IO.FileSystemWatcher fswFolders;
        private System.IO.FileSystemWatcher fswFiles;
        private System.Windows.Forms.GroupBox gLinks;
        private System.Windows.Forms.Label labLinkClicked;
        private System.Windows.Forms.TextBox tParent;
        private System.Windows.Forms.Label labFullPath;
        private System.Windows.Forms.RadioButton rCopy2Clipboard;
        private System.Windows.Forms.RadioButton rOpenInBrowser;
        private System.Windows.Forms.Timer CheckConnection;
        private System.Windows.Forms.ToolStripMenuItem recentFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.Label labLang;
        private System.Windows.Forms.ComboBox cmbLang;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lMode;
        private System.Windows.Forms.Label labMode;
        private System.Windows.Forms.Label labSupportMail;
        private System.Windows.Forms.GroupBox gWebInt;
        private System.Windows.Forms.LinkLabel labViewInBrowser;
        private System.Windows.Forms.CheckBox chkWebInt;
    }
}