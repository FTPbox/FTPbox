namespace FTPbox
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
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.tray = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bgWork = new System.ComponentModel.BackgroundWorker();
            this.button1 = new System.Windows.Forms.Button();
            this.browser = new System.Windows.Forms.WebBrowser();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkShowNots = new System.Windows.Forms.CheckBox();
            this.chkCloseToTray = new System.Windows.Forms.CheckBox();
            this.chkStartUp = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.bAddFTP = new System.Windows.Forms.Button();
            this.lPort = new System.Windows.Forms.Label();
            this.lHost = new System.Windows.Forms.Label();
            this.lUsername = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.bRemove = new System.Windows.Forms.Button();
            this.bAddDir = new System.Windows.Forms.Button();
            this.lSyncedFiles = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.linkLabel5 = new System.Windows.Forms.LinkLabel();
            this.linkLabel4 = new System.Windows.Forms.LinkLabel();
            this.linkLabel3 = new System.Windows.Forms.LinkLabel();
            this.label19 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.lVersion = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label12 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.trayMenu.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(597, 154);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(334, 134);
            this.listBox1.TabIndex = 4;
            // 
            // tray
            // 
            this.tray.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.tray.ContextMenuStrip = this.trayMenu;
            this.tray.Icon = ((System.Drawing.Icon)(resources.GetObject("tray.Icon")));
            this.tray.Text = "FTPbox";
            this.tray.Visible = true;
            this.tray.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.tray_MouseDoubleClick);
            // 
            // trayMenu
            // 
            this.trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem,
            this.aboutToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.trayMenu.Name = "trayMenu";
            this.trayMenu.Size = new System.Drawing.Size(117, 70);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.optionsToolStripMenuItem.Text = "Options";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // bgWork
            // 
            this.bgWork.WorkerReportsProgress = true;
            this.bgWork.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWork_DoWork);
            this.bgWork.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgWork_RunWorkerCompleted);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(937, 164);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // browser
            // 
            this.browser.Location = new System.Drawing.Point(468, 182);
            this.browser.MinimumSize = new System.Drawing.Size(20, 20);
            this.browser.Name = "browser";
            this.browser.Size = new System.Drawing.Size(38, 28);
            this.browser.TabIndex = 9;
            this.browser.Visible = false;
            this.browser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.browser_DocumentCompleted);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(5, 5);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(571, 301);
            this.tabControl1.TabIndex = 10;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.browser);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(563, 275);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chkShowNots);
            this.groupBox2.Controls.Add(this.chkCloseToTray);
            this.groupBox2.Controls.Add(this.chkStartUp);
            this.groupBox2.Location = new System.Drawing.Point(287, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(275, 109);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Application";
            // 
            // chkShowNots
            // 
            this.chkShowNots.AutoSize = true;
            this.chkShowNots.Location = new System.Drawing.Point(6, 49);
            this.chkShowNots.Name = "chkShowNots";
            this.chkShowNots.Size = new System.Drawing.Size(112, 17);
            this.chkShowNots.TabIndex = 11;
            this.chkShowNots.Text = "Show notifications";
            this.chkShowNots.UseVisualStyleBackColor = true;
            this.chkShowNots.CheckedChanged += new System.EventHandler(this.chkShowNots_CheckedChanged);
            // 
            // chkCloseToTray
            // 
            this.chkCloseToTray.AutoSize = true;
            this.chkCloseToTray.Location = new System.Drawing.Point(6, 72);
            this.chkCloseToTray.Name = "chkCloseToTray";
            this.chkCloseToTray.Size = new System.Drawing.Size(83, 17);
            this.chkCloseToTray.TabIndex = 10;
            this.chkCloseToTray.Text = "close to tray";
            this.chkCloseToTray.UseVisualStyleBackColor = true;
            this.chkCloseToTray.CheckedChanged += new System.EventHandler(this.chkCloseToTray_CheckedChanged);
            // 
            // chkStartUp
            // 
            this.chkStartUp.AutoSize = true;
            this.chkStartUp.Location = new System.Drawing.Point(6, 26);
            this.chkStartUp.Name = "chkStartUp";
            this.chkStartUp.Size = new System.Drawing.Size(136, 17);
            this.chkStartUp.TabIndex = 9;
            this.chkStartUp.Text = "Start on system start up";
            this.chkStartUp.UseVisualStyleBackColor = true;
            this.chkStartUp.CheckedChanged += new System.EventHandler(this.chkStartUp_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.bAddFTP);
            this.groupBox1.Controls.Add(this.lPort);
            this.groupBox1.Controls.Add(this.lHost);
            this.groupBox1.Controls.Add(this.lUsername);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(275, 109);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "FTP Account";
            // 
            // bAddFTP
            // 
            this.bAddFTP.Location = new System.Drawing.Point(213, 79);
            this.bAddFTP.Name = "bAddFTP";
            this.bAddFTP.Size = new System.Drawing.Size(56, 24);
            this.bAddFTP.TabIndex = 12;
            this.bAddFTP.Text = "Change";
            this.bAddFTP.UseVisualStyleBackColor = true;
            this.bAddFTP.Click += new System.EventHandler(this.bAddFTP_Click);
            // 
            // lPort
            // 
            this.lPort.AutoSize = true;
            this.lPort.Location = new System.Drawing.Point(68, 63);
            this.lPort.Name = "lPort";
            this.lPort.Size = new System.Drawing.Size(27, 13);
            this.lPort.TabIndex = 10;
            this.lPort.Text = "N/A";
            // 
            // lHost
            // 
            this.lHost.AutoSize = true;
            this.lHost.Location = new System.Drawing.Point(68, 27);
            this.lHost.Name = "lHost";
            this.lHost.Size = new System.Drawing.Size(27, 13);
            this.lHost.TabIndex = 9;
            this.lHost.Text = "N/A";
            // 
            // lUsername
            // 
            this.lUsername.AutoSize = true;
            this.lUsername.Location = new System.Drawing.Point(68, 45);
            this.lUsername.Name = "lUsername";
            this.lUsername.Size = new System.Drawing.Size(27, 13);
            this.lUsername.TabIndex = 8;
            this.lUsername.Text = "N/A";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Port:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Username:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Host:";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.bRemove);
            this.tabPage2.Controls.Add(this.bAddDir);
            this.tabPage2.Controls.Add(this.lSyncedFiles);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(563, 275);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Boxes";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // bRemove
            // 
            this.bRemove.Location = new System.Drawing.Point(405, 248);
            this.bRemove.Name = "bRemove";
            this.bRemove.Size = new System.Drawing.Size(75, 23);
            this.bRemove.TabIndex = 15;
            this.bRemove.Text = "Remove";
            this.bRemove.UseVisualStyleBackColor = true;
            this.bRemove.Click += new System.EventHandler(this.bRemove_Click);
            // 
            // bAddDir
            // 
            this.bAddDir.Location = new System.Drawing.Point(486, 248);
            this.bAddDir.Name = "bAddDir";
            this.bAddDir.Size = new System.Drawing.Size(75, 23);
            this.bAddDir.TabIndex = 14;
            this.bAddDir.Text = "Add New";
            this.bAddDir.UseVisualStyleBackColor = true;
            this.bAddDir.Click += new System.EventHandler(this.bAddDir_Click);
            // 
            // lSyncedFiles
            // 
            this.lSyncedFiles.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lSyncedFiles.CheckBoxes = true;
            this.lSyncedFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader4,
            this.columnHeader3,
            this.columnHeader5});
            this.lSyncedFiles.FullRowSelect = true;
            this.lSyncedFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lSyncedFiles.HoverSelection = true;
            this.lSyncedFiles.Location = new System.Drawing.Point(6, 6);
            this.lSyncedFiles.MultiSelect = false;
            this.lSyncedFiles.Name = "lSyncedFiles";
            this.lSyncedFiles.Size = new System.Drawing.Size(555, 236);
            this.lSyncedFiles.TabIndex = 13;
            this.lSyncedFiles.UseCompatibleStateImageBehavior = false;
            this.lSyncedFiles.View = System.Windows.Forms.View.Details;
            this.lSyncedFiles.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lSyncedFiles_ItemChecked);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Remote Directory";
            this.columnHeader1.Width = 185;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Local Folder";
            this.columnHeader4.Width = 185;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Subdirectories";
            this.columnHeader3.Width = 80;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Delete Remote";
            this.columnHeader5.Width = 85;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.linkLabel5);
            this.tabPage3.Controls.Add(this.linkLabel4);
            this.tabPage3.Controls.Add(this.linkLabel3);
            this.tabPage3.Controls.Add(this.label19);
            this.tabPage3.Controls.Add(this.label21);
            this.tabPage3.Controls.Add(this.lVersion);
            this.tabPage3.Controls.Add(this.label16);
            this.tabPage3.Controls.Add(this.label17);
            this.tabPage3.Controls.Add(this.label18);
            this.tabPage3.Controls.Add(this.label15);
            this.tabPage3.Controls.Add(this.label14);
            this.tabPage3.Controls.Add(this.label13);
            this.tabPage3.Controls.Add(this.groupBox4);
            this.tabPage3.Controls.Add(this.groupBox3);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(563, 275);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "About";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // linkLabel5
            // 
            this.linkLabel5.AutoSize = true;
            this.linkLabel5.Location = new System.Drawing.Point(182, 121);
            this.linkLabel5.Name = "linkLabel5";
            this.linkLabel5.Size = new System.Drawing.Size(78, 13);
            this.linkLabel5.TabIndex = 16;
            this.linkLabel5.TabStop = true;
            this.linkLabel5.Text = "/project/ftpbox";
            // 
            // linkLabel4
            // 
            this.linkLabel4.AutoSize = true;
            this.linkLabel4.Location = new System.Drawing.Point(182, 75);
            this.linkLabel4.Name = "linkLabel4";
            this.linkLabel4.Size = new System.Drawing.Size(115, 13);
            this.linkLabel4.TabIndex = 15;
            this.linkLabel4.TabStop = true;
            this.linkLabel4.Text = "sharpmindprojects.com";
            // 
            // linkLabel3
            // 
            this.linkLabel3.AutoSize = true;
            this.linkLabel3.Location = new System.Drawing.Point(182, 52);
            this.linkLabel3.Name = "linkLabel3";
            this.linkLabel3.Size = new System.Drawing.Size(66, 13);
            this.linkLabel3.TabIndex = 14;
            this.linkLabel3.TabStop = true;
            this.linkLabel3.Text = "JohnTheGr8";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(182, 144);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(79, 13);
            this.label19.TabIndex = 13;
            this.label19.Text = "Visual C# 2010";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(182, 98);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(90, 13);
            this.label21.TabIndex = 11;
            this.label21.Text = "jtgftw@gmail.com";
            // 
            // lVersion
            // 
            this.lVersion.AutoSize = true;
            this.lVersion.Location = new System.Drawing.Point(182, 29);
            this.lVersion.Name = "lVersion";
            this.lVersion.Size = new System.Drawing.Size(75, 13);
            this.lVersion.TabIndex = 8;
            this.lVersion.Text = "X.X.X (build X)";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(12, 144);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(84, 13);
            this.label16.TabIndex = 7;
            this.label16.Text = "Language used:";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(12, 121);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(114, 13);
            this.label17.TabIndex = 6;
            this.label17.Text = "Sourceforge.net Page:";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(12, 98);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(64, 13);
            this.label18.TabIndex = 5;
            this.label18.Text = "Contact me:";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(12, 75);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(70, 13);
            this.label15.TabIndex = 4;
            this.label15.Text = "Official Page:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(12, 52);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(55, 13);
            this.label14.TabIndex = 3;
            this.label14.Text = "Coded by:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(12, 29);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(82, 13);
            this.label13.TabIndex = 2;
            this.label13.Text = "Current Version:";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label12);
            this.groupBox4.Controls.Add(this.pictureBox1);
            this.groupBox4.Controls.Add(this.linkLabel2);
            this.groupBox4.Controls.Add(this.linkLabel1);
            this.groupBox4.Location = new System.Drawing.Point(308, 174);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(248, 89);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Contribute";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 51);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(45, 13);
            this.label12.TabIndex = 4;
            this.label12.Text = "Donate:";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox1.Image = global::FTPbox.Properties.Resources.project_support;
            this.pictureBox1.Location = new System.Drawing.Point(57, 53);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(88, 32);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(6, 35);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(92, 13);
            this.linkLabel2.TabIndex = 1;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "Request a feature";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(6, 19);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(69, 13);
            this.linkLabel1.TabIndex = 0;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Report a bug";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Location = new System.Drawing.Point(6, 174);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(296, 89);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Notes";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 67);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(213, 13);
            this.label11.TabIndex = 3;
            this.label11.Text = "- Copyright © 2011 - sharpmindprojects.com";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 51);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(183, 13);
            this.label10.TabIndex = 2;
            this.label10.Text = "- Feel free to contact me for anything.";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 35);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(280, 13);
            this.label9.TabIndex = 1;
            this.label9.Text = "- You can find the source files in the sourceforge.net page";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 19);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(227, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "- FTPbox is a free and open-source application";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label7);
            this.groupBox5.Controls.Add(this.label6);
            this.groupBox5.Controls.Add(this.label5);
            this.groupBox5.Controls.Add(this.label4);
            this.groupBox5.Location = new System.Drawing.Point(597, 27);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(393, 121);
            this.groupBox5.TabIndex = 18;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Box Info";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 84);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(105, 13);
            this.label7.TabIndex = 3;
            this.label7.Text = "Delete Remote Files:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 66);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Subdirectories:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Local Path:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(92, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Remote Directory:";
            // 
            // listBox2
            // 
            this.listBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox2.FormattingEnabled = true;
            this.listBox2.Location = new System.Drawing.Point(1029, 157);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(292, 134);
            this.listBox2.TabIndex = 19;
            // 
            // fMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(577, 307);
            this.Controls.Add(this.listBox2);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listBox1);
            this.MaximizeBox = false;
            this.Name = "fMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FTPbox | Options";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.fMain_FormClosing);
            this.Load += new System.EventHandler(this.fMain_Load);
            this.SizeChanged += new System.EventHandler(this.fMain_SizeChanged);
            this.trayMenu.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.NotifyIcon tray;
        private System.Windows.Forms.ContextMenuStrip trayMenu;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker bgWork;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.WebBrowser browser;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chkCloseToTray;
        private System.Windows.Forms.CheckBox chkStartUp;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button bAddFTP;
        private System.Windows.Forms.Label lPort;
        private System.Windows.Forms.Label lHost;
        private System.Windows.Forms.Label lUsername;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button bRemove;
        private System.Windows.Forms.Button bAddDir;
        private System.Windows.Forms.ListView lSyncedFiles;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox chkShowNots;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.LinkLabel linkLabel5;
        private System.Windows.Forms.LinkLabel linkLabel4;
        private System.Windows.Forms.LinkLabel linkLabel3;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label lVersion;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox listBox2;
    }
}