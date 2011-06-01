using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Xml;
using FtpLib;

namespace FTPbox
{
    public partial class fMain : Form
    {
        FtpConnection ftp;

        //FTP account info
        public string ftpUserName = FTPbox.Properties.Settings.Default.ftpUsername;
        public string ftpPassWord = FTPbox.Properties.Settings.Default.ftpPass;
        public string ftpHost = FTPbox.Properties.Settings.Default.ftpHost;
        public int ftpPort = FTPbox.Properties.Settings.Default.ftpPort;

        //Form instances
        NewFTP frmNewFtp;
        fNewDir newDir;

        List<FileSystemWatcher> fWatchers = new List<FileSystemWatcher>();
        List<FileSystemWatcher> dirWatchers = new List<FileSystemWatcher>();

        Timer tmrCheckRemote;

        List<string> log = new List<string>();
        
        List<int> indexes = new List<int>();
        bool runningstartupcheck = false;

        public fMain()
        {
            InitializeComponent();
            frmNewFtp = new NewFTP();
            frmNewFtp.Tag = this;
            newDir = new fNewDir();
            newDir.Tag = this;
        }

        private void fMain_Load(object sender, EventArgs e)
        {
            try
            {
                ftp = new FtpConnection(ftpHost, ftpPort, ftpUserName, ftpPassWord);
                ftp.Open();
                ftp.Login();
                Update_Info_Labels();
                Update_Boxes();
            }
            catch
            {
                frmNewFtp.ShowDialog();
                Update_Info_Labels();
            }
            if (FTPbox.Properties.Settings.Default.Boxes != null)
            {
                bgWork.RunWorkerAsync();
            }

            //CheckForUpdate();
            setTimer();
        }

        /// <summary>
        /// update the form's labels with FTP account's info
        /// </summary>
        public void Update_Info_Labels()
        {
            lHost.Text = ftpHost;
            lUsername.Text = ftpUserName;
            lPort.Text = ftpPort.ToString();
        }

        /// <summary>
        /// gets the boxes on app's start-up and sets a watcher for each one
        /// </summary>
        public void Update_Boxes()
        {
            chkStartUp.Checked = FTPbox.Properties.Settings.Default.startup;
            chkShowNots.Checked = FTPbox.Properties.Settings.Default.shownots;
            chkCloseToTray.Checked = FTPbox.Properties.Settings.Default.closetotray;

            foreach (string comb in FTPbox.Properties.Settings.Default.Boxes)
            {
                string[] x = comb.Split('|', '|');
                ListViewItem item = new ListViewItem();
                item.Text = x[0];
                item.SubItems.Add(x[1]);
                item.SubItems.Add(x[2]);
                item.SubItems.Add(x[3]);
                lSyncedFiles.Items.Add(item);

                //if local to remote
                if (comb != null)
                {
                    bool subdir;
                    if (x[3] == "Yes") { subdir = true; }
                    else { subdir = false; }
                    Set_local_watcher(x[1], lSyncedFiles.Items.IndexOf(item), subdir);
                }
            }
        }

        /// <summary>
        /// called from newFTP form
        /// updates the variables related to the FTP account
        /// </summary>
        /// <param name="host">the host</param>
        /// <param name="un">the FTP username</param>
        /// <param name="pass">the FTP password</param>
        /// <param name="port">the port used</param>
        public void Update_Acc_info(string host, string un, string pass, int port)
        {
            ftpHost = host;
            ftpPort = port;
            ftpUserName = un;
            ftpPassWord = pass;
            Update_Info_Labels();
            Save_Settings();
        }

        /// <summary>
        /// void called from "new directory" form
        /// adds new directory to listbox and 
        /// starts synchronizing
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="localpath"></param>
        /// <param name="method"></param>
        public void Get_new_directory(string dir, string localpath, bool subdirs, bool delrem)
        {
            ListViewItem item = new ListViewItem();
            item.Text = dir;
            item.SubItems.Add(localpath);
            if (subdirs) 
                item.SubItems.Add("Yes"); 
            else  
                item.SubItems.Add("No");
            if (delrem)
                item.SubItems.Add("No");
            else
                item.SubItems.Add("Yes");
             
            lSyncedFiles.Items.Add(item);

            int i = lSyncedFiles.Items.IndexOf(item);
            

            Save_Settings();
            if (runningstartupcheck)
            {
                indexes.Add(i);
            }
            else
            {
                Set_local_watcher(localpath, i, subdirs);
                First_Check(item.Text + @"|" + item.SubItems[1].Text + @"|" + item.SubItems[2].Text + @"|" + item.SubItems[3].Text);
            }
        }

        /// <summary> 
        /// save the settings
        /// </summary>
        public void Save_Settings()
        {
            FTPbox.Properties.Settings.Default.ftpUsername = ftpUserName;
            FTPbox.Properties.Settings.Default.ftpPass = ftpPassWord;
            FTPbox.Properties.Settings.Default.ftpPort = ftpPort;
            FTPbox.Properties.Settings.Default.ftpHost = ftpHost;
           
            StringCollection boxes = new StringCollection();
            foreach (ListViewItem x in lSyncedFiles.Items)
            {
                string comb = x.Text + @"|" + x.SubItems[1].Text + @"|" + x.SubItems[2].Text + @"|" + x.SubItems[3].Text;
                boxes.Add(comb);
            }            
            FTPbox.Properties.Settings.Default.Boxes = boxes;

            FTPbox.Properties.Settings.Default.Save();
        }

        /// <summary>
        /// FileSystemWatcher event, 
        /// raised when a file/directory the app is watching has changed/been deleted/been created
        /// </summary>
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            string[] x = Get_remote_box_path(e.FullPath).Split('|', '|');
            string remPath = x[0];
            string locPath = x[1];
            bool delRem;

            if (x[3] == "Yes")
            {
                delRem = true;
            }
            else
            {
                delRem = false;
            }

            bool isDir = false;
            int index = 0;
            foreach (FileSystemWatcher fsw in dirWatchers)
            {
                if (source == fsw)
                {
                    isDir = true;
                    index = dirWatchers.IndexOf(fsw);
                }
                else
                {
                    foreach (FileSystemWatcher f in fWatchers)
                    {
                        if (source == f)
                        {
                            index = fWatchers.IndexOf(f);
                        }
                    }
                }
            }
            

            listBox1.Items.Add("File: " + e.FullPath + " " + e.ChangeType.ToString());

            try
            {
                tmrCheckRemote.Stop();
                Sync_local_to_rem(e.FullPath, e.Name, remPath, locPath, e.ChangeType, null, isDir, delRem);
                tmrCheckRemote.Start();
            }
            catch (Exception ex)
            {
                tmrCheckRemote.Start();
                listBox2.Items.Add(ex.Message);
                //MessageBox.Show(ex.Message);
            }            
            
        }

        /// <summary>
        /// FileSystemWatcher event, 
        /// raised when a file/directory the app is watching has been renamed
        /// </summary>
        private void OnRenamed(object source, RenamedEventArgs e)
        {
            string[] x = Get_remote_box_path(e.FullPath).Split('|', '|');
            string remPath = x[0];
            string locPath = x[1];
            bool delRem;

            if (x[3] == "Yes")
            {
                delRem = true;
            }
            else
            {
                delRem = false;
            }

            bool isDir = false;
            foreach (FileSystemWatcher fsw in dirWatchers)
            {
                if (source == fsw)
                {
                    isDir = true;
                }
            }

            listBox1.Items.Add("File: " + e.OldFullPath + " renamed to " + e.FullPath);

            try
            {
                tmrCheckRemote.Stop();
                Sync_local_to_rem(e.FullPath, e.Name, remPath, locPath, e.ChangeType, e.OldName, isDir, delRem);
                tmrCheckRemote.Start(); 
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                tmrCheckRemote.Start(); 
            }
           
        }
        
        /// <summary>
        /// set a directory watcher to raise an event each time something has been changed
        /// </summary>
        /// <param name="path">path to the directory</param>
        /// <param name="index"></param>
        /// <param name="subdirs">include subdirectories or not</param>
        private void Set_local_watcher(string path, int index, bool subdirs)
        {
            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher fWatcher = new FileSystemWatcher();   //watcher for files in specified path
            FileSystemWatcher dirWatcher = new FileSystemWatcher(); //watcher for folders in specified path

            fWatcher.Path = path;
            dirWatcher.Path = path;

            fWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
            dirWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.DirectoryName;

            fWatcher.Filter = "*";
            dirWatcher.Filter = "*";

            // Add event handlers.
            fWatcher.Changed += new FileSystemEventHandler(OnChanged);
            fWatcher.Created += new FileSystemEventHandler(OnChanged);
            fWatcher.Deleted += new FileSystemEventHandler(OnChanged);
            fWatcher.Renamed += new RenamedEventHandler(OnRenamed);
            dirWatcher.Changed += new FileSystemEventHandler(OnChanged);
            dirWatcher.Created += new FileSystemEventHandler(OnChanged);
            dirWatcher.Deleted += new FileSystemEventHandler(OnChanged);
            dirWatcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            fWatcher.EnableRaisingEvents = true;
            dirWatcher.EnableRaisingEvents = true;

            fWatchers.Add(fWatcher);
            dirWatchers.Add(dirWatcher);
        }

        /// <summary>
        /// Makes changes to remote file/directory when local one was changed
        /// </summary>
        /// <param name="lChangedFullPath">Path to local file/Directory that was changed</param>
        /// <param name="ChangedName">Name of changed file/directory</param>
        /// <param name="remPath">Remote path</param>
        /// <param name="locPath">local path of ftpBox</param>
        /// <param name="type">change method</param>
        /// <param name="oldName">old name, used if method is 'rename'</param>
        /// <param name="isDir">true if what changed is a folder, false if it's a file</param>
        public void Sync_local_to_rem(string lChangedFullPath, string ChangedName, string remPath, string locPath, WatcherChangeTypes type, string oldName, bool isDir, bool delRemote)
        {
            string lPath = lChangedFullPath.Replace(ChangedName, "");                                               //path to what was locally changed
            string rPath = noSlashes(remPath) + @"/" + noSlashes(lPath.Replace(locPath, "").Replace(@"\", @"/"));   //remote path were changes will take effect
            string rFullPath = noSlashes(rPath) + @"/" + ChangedName;

            if (type == WatcherChangeTypes.Created)
            {
                if (isDir)
                {
                    Syncing();
                    ftp.SetCurrentDirectory(rPath);
                    ftp.CreateDirectory(ChangedName);
                    if (ShowNots())
                    {
                        tray.ShowBalloonTip(100, "FTPbox", string.Format("Folder {0} was successfully created.", ChangedName), ToolTipIcon.Info);
                    }
                    Update_log_XML(remPath, lPath, rFullPath, "Created", DateTime.Now);
                    DoneSyncing();
                }
                else
                {
                    Syncing();
                    ftp.SetCurrentDirectory(rPath);
                    ftp.PutFile(lChangedFullPath);
                    if (ShowNots())
                    {
                        tray.ShowBalloonTip(100, "FTPbox", string.Format("File {0} was successfully created.", ChangedName), ToolTipIcon.Info);
                    }
                    DoneSyncing();
                }
            }
            else if (type == WatcherChangeTypes.Deleted)
            {
                if (isDir && delRemote)
                {
                    Syncing();
                    ftp.SetCurrentDirectory(rPath);
                    ftp.RemoveDirectory(ChangedName);
                    if (ShowNots())
                    {
                        tray.ShowBalloonTip(100, "FTPbox", string.Format("Folder {0} was successfully deleted.", ChangedName), ToolTipIcon.Info);
                    }
                    DoneSyncing();
                }
                else if (!isDir && delRemote)
                {
                    Syncing();
                    ftp.SetCurrentDirectory(rPath);
                    ftp.RemoveFile(ChangedName);
                    if (ShowNots())
                    {
                        tray.ShowBalloonTip(100, "FTPbox", string.Format("File {0} was successfully deleted.", ChangedName), ToolTipIcon.Info);
                    }
                    DoneSyncing();
                }
            }
            else if (type == WatcherChangeTypes.Renamed)
            {
                if (isDir)
                {
                    Syncing();
                    ftp.SetCurrentDirectory(rPath);
                    ftp.RenameFile(oldName, ChangedName);
                    if (ShowNots())
                    {
                        tray.ShowBalloonTip(100, "FTPbox", string.Format("Folder {0} was successfully renamed to {1}", oldName, ChangedName), ToolTipIcon.Info);
                    }
                    DoneSyncing();
                }
                else
                {
                    Syncing();
                    ftp.SetCurrentDirectory(rPath);
                    ftp.RenameFile(oldName, ChangedName);
                    if (ShowNots())
                    {
                        tray.ShowBalloonTip(100, "FTPbox", string.Format("File {0} was successfully renamed to {1}", oldName, ChangedName), ToolTipIcon.Info);
                    }
                    DoneSyncing();
                }
            }
            else if (type == WatcherChangeTypes.Changed)
            {
                if (!isDir)
                {
                    Syncing();
                    ftp.SetCurrentDirectory(rPath);
                    ftp.RemoveFile(ChangedName);
                    ftp.PutFile(lChangedFullPath);
                    if (ShowNots())
                    {
                        tray.ShowBalloonTip(100, "FTPbox", string.Format("File {0} was successfully changed.", ChangedName), ToolTipIcon.Info);
                    }
                    DoneSyncing();
                }
            }

        }

        /// <summary>
        /// removes slashes from beggining and end of paths
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private string noSlashes(string x)
        {
            string noslashes = x;
            if (noslashes.StartsWith(@"\")) 
            {
                noslashes = noslashes.Substring(1, noslashes.Length - 1);
            }
            if (noslashes.EndsWith(@"/") || noslashes.EndsWith(@"\"))
            {
                noslashes = noslashes.Substring(0, noslashes.Length - 1);
            }
            return noslashes;
        }

        /// <summary>
        /// gets the path to the remote folder associated with 'local'
        /// </summary>
        /// <param name="local"></param>
        /// <returns></returns>
        public string Get_remote_box_path(string local)
        {
            string Paths = null;
            foreach (string s in FTPbox.Properties.Settings.Default.Boxes)
            {
                string[] pars = s.Split('|', '|');
                if (local.StartsWith(pars[1]))
                {
                    Paths = s;
                }
            }
            return Paths;
        }


        private void First_Check(string box)
        {
            string[] pars = box.Split('|', '|');
            string rPath = pars[0];
            string lPath = pars[1];
            string method = pars[2];
            string subDirs = pars[3];
            string[] list;

            if (subDirs != "Yes")
            {
                list = Directory.GetFiles(lPath, "*");
            }
            else
            {
                list = Directory.GetFiles(lPath, "*", SearchOption.AllDirectories);
            }
            foreach (string path in list)
            {
                if (method == "local to remote")
                {
                    First_Check_LtR(rPath, path, lPath);
                }
            }

            foreach (int i in indexes)
            {
                ListViewItem item = lSyncedFiles.Items[i];
                First_Check(item.Text + @"|" + item.SubItems[1].Text + @"|" + item.SubItems[2].Text + @"|" + item.SubItems[3].Text);
            }

            check_local();

        }

        /// <summary>
        /// makes any changes done while the app was not running to the remote folder
        /// For 'local to remote' mode
        /// </summary>
        /// <param name="rPath">path to remote ftpbox</param>
        /// <param name="path">path to file to be checked</param>
        /// <param name="lPath">local path to ftpbox</param>
        private void First_Check_LtR(string rPath, string path, string lPath)
        {
            FileAttributes attr = System.IO.File.GetAttributes(path);
            FileInfo fi = new FileInfo(path); 
            string loc; // = fi.Name;

            loc = path.Replace(lPath, "");
            loc = loc.Replace(fi.Name, "");
            loc = loc.Replace(@"\", @"/");
            loc = noSlashes(rPath) + @"/" + noSlashes(loc);
            
            ftp.SetCurrentDirectory(loc);
            
            if ((attr & FileAttributes.Directory) != FileAttributes.Directory)
            {
                string rDir = path.Replace(lPath, "");  //remove the path to ftpBox from the path   
                rDir = "/" + noSlashes(loc) + "/" + rDir;
                rDir = rDir.Replace(@"\", @"/");
                string name = fi.Name;
                rDir = rDir.Replace(name, "");
                ftp.SetCurrentDirectory(rDir);

                if (ftp.FileExists(fi.Name))
                {
                    //string rDir = path.Replace(lPath, "");    //remove the path to ftpBox from the path
                    //string name = fi.Name;                      //get the name of the file
                    //rDir = rDir.Replace(name, "");            //remove the name from the path
                    //rDir = rDir.Replace(@"\", @"/");          //replace \ with /
                    //rDir = rPath + "/" + noSlashes(rDir);     //get the full path to the new remote directory where the new file will be created

                    ftp.SetCurrentDirectory(rDir);
                    FtpFileInfo ftpfi = new FtpFileInfo(ftp, name);
                    ftpfi.Attributes.GetHashCode();                    

                    int rHash = ftpfi.GetHashCode();
                    int lHash = fi.GetHashCode();
                    
                    if (rHash == lHash && ftpfi.LastWriteTime != fi.LastWriteTime)
                    {
                        ftp.RemoveFile(name);
                        ftp.PutFile(path, name);
                        if (ShowNots())
                        {
                            tray.ShowBalloonTip(100, "FTPbox", string.Format("File {0} was successfully updated.", name), ToolTipIcon.Info);
                        }
                    }
                }
                else
                {
                    //string name = fi.Name;                  //get the name of the file
                    /*
                    rDir = rDir.Replace(name, "");          //remove the name from the path
                    rDir = rDir.Replace(@"\", @"/");        //replace \ with /
                    rDir = rPath + "/" + noSlashes(rDir);   //get the full path to the new remote directory where the new file will be created */
                    
                    ftp.PutFile(path, name);
                    if (ShowNots())
                    {
                        tray.ShowBalloonTip(100, "FTPbox", string.Format("File {0} was successfully uploaded.", name), ToolTipIcon.Info);
                    }

                }
            }
            
            else if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                string rDir = path.Replace(lPath, "");  //remove the path to ftpBox from the path     
                string name = fi.Name;                  //get the name of the file
                rDir = "/" + noSlashes(loc) + "/" + rDir;
                ftp.SetCurrentDirectory(rDir);

                if (!ftp.DirectoryExists(name))
                {                    
                    /*
                    rDir = rDir.Replace(name, "");          //remove the name from the path
                    rDir = rDir.Replace(@"\", @"/");        //replace \ with /
                    rDir = rPath + "/" + noSlashes(rDir);   //get the full path to the new remote directory where the new file will be created */
                    
                    ftp.CreateDirectory(name);
                    if (ShowNots())
                    {
                        tray.ShowBalloonTip(100, "FTPbox", string.Format("Folder {0} was successfully created.", name), ToolTipIcon.Info);
                    }
                }
            }
        }

        #region System Tray
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            tabControl1.SelectedTab = tabPage3;
        }

        private void tray_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }
        #endregion

        /// <summary>
        /// first check and sync takes some time, 
        /// so I put it to work on background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bgWork_DoWork(object sender, DoWorkEventArgs e)
        {
            Syncing();
            foreach (string comb in FTPbox.Properties.Settings.Default.Boxes)
            {
                runningstartupcheck = true;
                First_Check(comb);
            }
        }

        private void bgWork_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DoneSyncing();
            tmrCheckRemote.Start();
            runningstartupcheck = false;
        }

        private void fMain_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                //this.Hide();
            }
        }

        /// <summary>
        /// run FTPbox on windows startup
        /// <param name="enable"><c>true</c> to add it to system startup, <c>false</c> to remove it</param>
        /// </summary>
        private void SetStartup(bool enable)
        {
            string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            Microsoft.Win32.RegistryKey startupKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(runKey);

            if (enable)
            {
                if (startupKey.GetValue(Application.ProductName) == null)
                {
                    startupKey.Close();
                    startupKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(runKey, true);
                    // Add startup reg key
                    startupKey.SetValue(Application.ProductName, Application.ExecutablePath.ToString());
                    startupKey.Close();
                }
            }
            else
            {
                // remove startup
                startupKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(runKey, true);
                startupKey.DeleteValue(Application.ProductName, false);
                startupKey.Close();
            }
        }

        public void Watch_remote(string rPath, string lPath)
        {
            ftp.SetCurrentDirectory(rPath);
            List<string> list = ftp.List();
            
            foreach (string s in list)
            {
                bool isDir;
                try
                {
                    ftp.SetCurrentDirectory("/" + noSlashes(rPath) + "/" + noSlashes(s));       //check if path can be set as directory
                    isDir = true;                                                               //return true if it can be set
                }
                catch
                {
                    isDir = false;                                                              //isDir is false if it throws an error
                }
            }

        }

        public void Sync()
        {

        }

        /// <summary>
        /// updates the log on host so that the
        /// app knows when each change was made
        /// </summary>
        /// <param name="rPath">remote path to FTPbox</param>
        /// <param name="ftpBoxPath">local path to FTPbox</param>
        /// <param name="filepath">path</param>
        /// <param name="changetype">either <c>Created</c>, <c>Renamed</c> or <c>Deleted</c></param>
        /// <param name="date">datetime of change</param>
        public void Update_log(string rPath, string ftpBoxPath, string filepath, string changetype, DateTime date)
        {
            string txtpath = ftpBoxPath + "\ftpbox_log.txt";
            if (File.Exists(txtpath))
            {
               
            }

            StreamWriter txt = File.CreateText(txtpath);
            string args = @"{" + filepath + @"|" + date.ToOADate().ToString() + @"|" + changetype + @"}";
            txt.WriteLine(args);
            txt.Close();

            //upload new log
            ftp.SetCurrentDirectory(rPath);
            try
            {
                ftp.RemoveFile(@"ftpbox_log.txt");  //if a log already exists, remove it so it'll be replaced
            }
            catch { }
            ftp.PutFile(ftpBoxPath + "\ftpbox_log.txt", @"ftpbox_log.txt"); //upload new log
        }

        public void Get_log(string rPath, string lPath)
        {
            ftp.SetCurrentDirectory(rPath);
            ftp.SetLocalDirectory(lPath);
            ftp.GetFile("ftpbox_log.txt", false);
            string path = lPath + @"\ftpbox_log.txt";

            using (StreamReader sr = File.OpenText(path))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    log.Add(s);
                }
            }

            File.Delete(path);  //delete local log

        }

        #region Update_log_xml
        public void Update_log_XML(string rPath, string ftpBoxPath, string filepath, string changetype, DateTime date)
        {
            string xmlpath = ftpBoxPath + "ftpbox_log.xml";
            if (!File.Exists(xmlpath))
            {
                //FTPbox.Properties.Resources.Box.Save(
            }

            XmlTextWriter xml = new XmlTextWriter(xmlpath, null);
            xml.Formatting = Formatting.Indented;
            xml.WriteStartDocument();
            xml.WriteStartElement(changetype);
            xml.WriteStartElement("path");
            xml.WriteString(filepath);
            xml.WriteEndElement();
            xml.WriteStartElement("Date");
            xml.WriteString(date.ToOADate().ToString());
            xml.WriteEndElement();
            xml.WriteEndElement();
            xml.WriteEndDocument();
            xml.Flush();
            xml.Close();

            ftp.SetCurrentDirectory(rPath);
            try
            {
                ftp.RemoveFile(@"ftpbox_log.xml");
            }
            catch { }
            ftp.PutFile(ftpBoxPath + "\ftpbox_log.xml", @"ftpbox_log.xml");

        }
        #endregion

        /// <summary>
        /// gets a list of files in the specified path and its subdirectories
        /// </summary>
        /// <param name="path">path to search</param>
        List<string> RemFullList = new List<string>();
        public void GetFullRemoteDirList(string path)
        {
            ftp.SetCurrentDirectory(path);
            foreach (var dir in ftp.GetDirectories())
            {
                RemFullList.Add(dir.Name);
                
                GetFullRemoteDirList(dir.Name);
            }

            foreach (var file in ftp.GetFiles())
            {
                RemFullList.Add(file.Name);
            }

        }

        /// <summary>
        /// checks for an update
        /// called on each start-up of this app.
        /// </summary>
        private void CheckForUpdate()
        {
            try
            {
                browser.Navigate(@"http://sharpmindprojects.com/project_versions.txt");
            }
            catch { }
        }

        private void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            string data = browser.Document.Body.InnerText;
            string[] nVersion = data.Split(':', ';');
            //MessageBox.Show(data, "asd");
            if (nVersion[5] != Application.ProductVersion)
            {
                newversion nvform = new newversion(nVersion[5]);
                nvform.ShowDialog();
                this.Show();
                // show dialog box for download now, learn more and remind me next time
            }
        }

        #region Application's behaviour of checkboxes and listbox
        private void lSyncedFiles_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (lSyncedFiles.CheckedItems.Count > 0)
            {
                bRemove.Enabled = true;
            }
            else
            {
                bRemove.Enabled = false;
            }
        }

        private void chkStartUp_CheckedChanged(object sender, EventArgs e)
        {
            if (chkStartUp.Checked)
            {
                try
                {
                    SetStartup(true);
                }
                catch { }
            }
            else
            {
                try
                {
                    SetStartup(false);
                }
                catch { }
            }
            FTPbox.Properties.Settings.Default.startup = chkStartUp.Checked;
            FTPbox.Properties.Settings.Default.Save();
        }

        private void chkCloseToTray_CheckedChanged(object sender, EventArgs e)
        {
            FTPbox.Properties.Settings.Default.closetotray = chkCloseToTray.Checked;
            FTPbox.Properties.Settings.Default.Save();
        }

        private void chkShowNots_CheckedChanged(object sender, EventArgs e)
        {
            FTPbox.Properties.Settings.Default.shownots = chkShowNots.Checked;
            FTPbox.Properties.Settings.Default.Save();
        }
        #endregion

        private void tmrCheckRemote_Tick(object sender, EventArgs e)
        {
            tray.ShowBalloonTip(10, "started", "CheckRemote", ToolTipIcon.Info);
            foreach (string comb in FTPbox.Properties.Settings.Default.Boxes)
            {
                string[] x = comb.Split('|', '|');
                string rPath = x[0];
                string lPath = x[1];
                string subdirs = x[2];
                string delrem = x[3];
                bool subDirs;
                if (subdirs == "Yes")
                {
                    subDirs = true;
                }
                else
                {
                    subDirs = false;
                }
                
                try
                {
                    Sync_Remote_to_Local(rPath, lPath, subDirs);
                }
                catch { }

            }

            //check_local();
        }

        private void Sync_Remote_to_Local(string rPath, string lPath, bool subdirs)
        {
            if (!subdirs)
            {
                ftp.SetCurrentDirectory(rPath);
                listBox2.Items.Clear();
                foreach (var file in ftp.GetFiles())
                {
                    listBox2.Items.Add(file.Name);
                    string filepath = noSlashes(lPath) + @"\" + file.Name;
                    if (!File.Exists(filepath))
                    {
                        Syncing();

                        ftp.SetLocalDirectory(lPath);
                        ftp.GetFile(file.Name, false);
                        if (ShowNots())
                        {
                            tray.ShowBalloonTip(100, "FTPbox", string.Format("File {0} was successfully created.", file.Name), ToolTipIcon.Info);
                        }
                        
                        DoneSyncing();
                    }
                }
            }
        }

        private void Syncing()
        {
            tray.Icon = FTPbox.Properties.Resources.syncBox;
            tray.Text = "FTPbox - Syncing...";
        }

        private void DoneSyncing()
        {
            tray.Icon = FTPbox.Properties.Resources.synced;
            tray.Text = "FTPbox - All files synced";
        }

        bool ShowNots()
        {
            return chkShowNots.Checked;
        }

        private bool CloseToTray()
        {
            return chkCloseToTray.Checked;
        }

        private void fMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CloseToTray())
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void Startup_Check(string box)
        {
            string[] args = box.Split('|', '|');
            string rPath = args[0];
            string lPath = args[1];
            string subdirs = args[2];
            string delrem = args[3];
            bool subDirs;
            if (subdirs == "Yes")
                subDirs = true;
            else
                subDirs = false;

            if (!subDirs)
            {
                ftp.SetCurrentDirectory(rPath);
                foreach (var file in ftp.GetFiles())
                {
                    if (File.Exists(noSlashes(lPath) + @"\" + file.Name))
                    {
                        
                    }
                }
            }

        }

        private void bRemove_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem l in lSyncedFiles.CheckedItems)
            {
                lSyncedFiles.Items.Remove(l);
            }
            Save_Settings();
            bRemove.Enabled = false;
        }

        private void bAddFTP_Click(object sender, EventArgs e)
        {
            frmNewFtp.ShowDialog();
        }

        private void bAddDir_Click(object sender, EventArgs e)
        {
            newDir.ShowDialog();
        }


        public void Check_Same_Name(string rPath, string ftpboxPath, string filename, DateTime rDatetime, DateTime lDatetime)
        {
            /*
            string line = null;
            foreach (string s in FTPbox.Properties.Settings.Default.log)
            {
                if (s.StartsWith(ftpboxPath))
                {
                    line = s;
                }
            }
            string[] args = line.Split('|', '|');
            string logDateTime = args[2];
            string logMD5 = args[3];
            DateTime logDate = DateTime.Parse(logDateTime);

            if (logDate != rDatetime)
            {
               
            } */

        }

        public void Update_log(string rPath, string filepath)
        {/*
            foreach (string s in FTPbox.Properties.Settings.Default.log)
            {
                if (s == filepath)
                {
                    FTPbox.Properties.Settings.Default.log.Remove(s);
                    FileInfo f = new FileInfo(filepath);
                    string Date = f.LastWriteTimeUtc.ToString();
                    string md5 = GetMD5(filepath);
                    string comb = filepath + @"|" + rPath + @"|" + Date + @"|" + md5;
                    FTPbox.Properties.Settings.Default.log.Add(comb);
                }
            }
          * */
        }

        public string GetMD5(string path)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(path);
            bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            string password = s.ToString();
            return password;
        }

        private void check_local()
        {
            Syncing();
            foreach (string comb in FTPbox.Properties.Settings.Default.Boxes)
            {
                string[] x = comb.Split('|', '|');
                string rPath = x[0];
                string lPath = x[1];
                string subdirs = x[2];
                string delrem = x[3];
                bool subDirs;
                if (subdirs == "Yes")
                    subDirs = true;
                else
                    subDirs = false;

                if (!subDirs)
                {
                    foreach (string path in Directory.GetFiles(lPath, "*", SearchOption.TopDirectoryOnly))
                    {
                        ftp.SetCurrentDirectory(rPath);

                        FileAttributes fa = System.IO.File.GetAttributes(path);


                        if ((fa & FileAttributes.Directory) != FileAttributes.Directory)
                        {
                            FileInfo f = new FileInfo(path);
                            if (!ftp.FileExists(f.Name))
                            {
                                File.Delete(path);
                            }
                        }
                        else
                        {
                            DirectoryInfo d = new DirectoryInfo(path);
                            if (!ftp.DirectoryExists(d.Name))
                            {
                                Directory.Delete(path);
                            }
                        }
                    }
                }
                else
                {
                    foreach (string path in Directory.GetFiles(lPath, "*", SearchOption.AllDirectories))
                    {
                        string commonpath;
                        string rFullPath;
                        FileAttributes fa = System.IO.File.GetAttributes(path);
                        if ((fa & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            commonpath = noSlashes(path.Replace(lPath, ""));
                            rFullPath = noSlashes(rPath) + @"/" + commonpath.Replace(@"\", @"/");
                            ftp.SetCurrentDirectory(rFullPath);

                            DirectoryInfo d = new DirectoryInfo(path);
                            if (!ftp.DirectoryExists(d.Name))
                            {
                                Directory.Delete(path);
                            }
                        }
                        else
                        {
                            FileInfo f = new FileInfo(path);
                            commonpath = noSlashes(path.Replace(lPath, "")).Replace(f.Name, "");
                            rFullPath = noSlashes(rPath) + @"/" + commonpath.Replace(@"\", @"/");
                            ftp.SetCurrentDirectory(rFullPath);
                            if (!ftp.FileExists(f.Name))
                            {
                                File.Delete(path);
                            }
                        }
                    }
                }
            }
            DoneSyncing();
        }

        private void setTimer()
        {
            tmrCheckRemote = new Timer();
            tmrCheckRemote.Interval = 5000;
            tmrCheckRemote.Enabled = false;
            tmrCheckRemote.Tick += new EventHandler(tmrCheckRemote_Tick);            
        }
    }

    
}