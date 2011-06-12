using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using FtpLib;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;

namespace FTPbox
{
    public partial class frmMain : Form
    {
        Dictionary<string, DateTime> FullList = new Dictionary<string, DateTime>();

        //FileSystemWatcher fWatcher;     //file watcher
        //FileSystemWatcher dirWatcher;   //folder watcher

        FtpConnection ftp;

        //Form instances
        NewFTP fNewFtp;
        fNewDir newDir;

        //logs:
        List<string> log = new List<string>(FTPbox.Properties.Settings.Default.log.Split('|', '|'));
        List<string> rDL  = new List<string>(FTPbox.Properties.Settings.Default.rDateLog.Split('|', '|'));
        List<string> lDL = new List<string>(FTPbox.Properties.Settings.Default.lDateLog.Split('|', '|'));

        public StringDictionary rLog = new StringDictionary();
        StringDictionary lLog = new StringDictionary();

        //recent files
        List<string> recentfiles = new List<string>();

        //Server's time
        TimeSpan timedif;

        //link:
        public string link = null;

        //for not flooding with balloon tips:
        public string lasttip = null;

        public string LastChangedFileFromRem = "";
        public string LastChangedFolderFromRem = "";

        public bool loggedIn = false;
        public bool gotpaths = false;
        
        bool OfflineMode = false;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            rLog = FTPbox.Properties.Settings.Default.rLog;
            lLog = FTPbox.Properties.Settings.Default.lLog;

            //ClearLog();
            if (rLog == null)
            {
                rLog = new StringDictionary();
                lLog = new StringDictionary();
            }
            else
            {
                foreach (KeyValuePair<string, string> s in rLog)
                {
                    if (s.Key != "" && s.Key != null)
                    {
                        Log.Write("in log: {0} - {1} - {2}", s.Key, s.Value, lLog[s.Key]);
                    }
                }
            }
            KillPrevInstances();
            
            fNewFtp = new NewFTP();
            fNewFtp.Tag = this;
            newDir = new fNewDir();
            newDir.Tag = this;

            int i = 0;
            while (i < 5)
            {
                recentfiles.Add("Not available");
                i++;
            }

            StartUpWork();
            
            CheckForUpdate();

            //WatchRemote.Start();
        }

        private void StartUpWork()
        {   
            CheckAccount();

            GetServerTime();

            UpdateDetails();

            CheckPaths();

            UpdateDetails();

            Syncing();
            try
            {
                //StartupCheck();
            }
            catch
            {
                //MessageBox.Show(ex.Message);
            }

            ListAllFiles();

            SetLocalWatcher();

            DoneSyncing();
        }
        
        /// <summary>
        /// checks if account's information used the last time haven't changed
        /// </summary>
        private void CheckAccount()
        {
            try
            {
                ftp = new FtpConnection(ftpHost(), ftpPort(), ftpUser(), ftpPass());
                ftp.Open();
                ftp.Login();
                this.ShowInTaskbar = false;
                this.Hide();
                this.ShowInTaskbar = true;
                loggedIn = true;
                
            }
            catch
            {
                fNewFtp.ShowDialog();
                if (!loggedIn)
                {
                    try
                    {
                        Process p = Process.GetCurrentProcess();
                        p.Kill();
                    }
                    catch { }
                }

                ftp = new FtpConnection(ftpHost(), ftpPort(), ftpUser(), ftpPass());
                ftp.Open();
                ftp.Login();
                this.ShowInTaskbar = false;
                this.Hide();
                this.ShowInTaskbar = true;
                loggedIn = true;                 
            }
        }

        /// <summary>
        /// checks if paths used the last time still exist
        /// </summary>
        private void CheckPaths()
        {
            if (!ftp.DirectoryExists(rPath()) || !Directory.Exists(lPath()))
            {
                newDir.ShowDialog();
                if (!gotpaths)
                {
                    try
                    {
                        Process p = Process.GetCurrentProcess();
                        p.Kill();
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// updates the labels on options form
        /// </summary>
        public void UpdateDetails()
        {
            //Tab: General
            lHost.Text = ftpHost();
            lUsername.Text = ftpUser();
            lPort.Text = ftpPort().ToString();
            chkStartUp.Checked = StartOnStartup();
            chkShowNots.Checked = ShowNots();
            chkCloseToTray.Checked = CloseToTray();
            tParent.Text = ftpParent();

            lVersion.Text = Application.ProductVersion.ToString().Substring(0, 5) + @" Beta";

            //FTPbox Tab
            lRemPath.Text = rPath();
            lLocPath.Text = lPath();
            tParent.Text = ftpParent();

            if (delRem())
                lDelRem.Text = @"Yes";
            else
                lDelRem.Text = @"No";
            if (subdirs())
                lSubDirs.Text = @"Yes";
            else
                lSubDirs.Text = @"No";
            if (OpenInBrowser())
                rOpenInBrowser.Checked = true;
            else
                rCopy2Clipboard.Checked = true;
        }

        #region variables

        private string ftpHost()
        {
            return FTPbox.Properties.Settings.Default.ftpHost;
        }

        private string ftpUser()
        {
            return FTPbox.Properties.Settings.Default.ftpUsername;
        }

        private string ftpPass()
        {
            return FTPbox.Properties.Settings.Default.ftpPass;
        }

        private int ftpPort()
        {
            return FTPbox.Properties.Settings.Default.ftpPort;
        }

        private string rPath()
        {
            return FTPbox.Properties.Settings.Default.rPath;
        }

        private string lPath()
        {
            return FTPbox.Properties.Settings.Default.lPath;
        }

        private bool subdirs()
        {
            return FTPbox.Properties.Settings.Default.subDirs;
        }

        bool delRem()
        {
            return FTPbox.Properties.Settings.Default.delRem;
        }

        bool StartOnStartup()
        {
            return FTPbox.Properties.Settings.Default.startup;
        }

        bool ShowNots()
        {
            return FTPbox.Properties.Settings.Default.shownots;
        }
        
        bool CloseToTray()
        {
            return FTPbox.Properties.Settings.Default.closetotray;
        }

        bool OpenInBrowser()
        {
            return FTPbox.Properties.Settings.Default.openinbrowser;
        }

        string ftpParent()
        {
            return FTPbox.Properties.Settings.Default.ftpParent;
        }
        
        string currentlog()
        {
            return FTPbox.Properties.Settings.Default.log;
        }

        string currentrDateLog()
        {
            return FTPbox.Properties.Settings.Default.rDateLog;
        }

        string currentlDateLog()
        {
            return FTPbox.Properties.Settings.Default.lDateLog;
        } 

        #endregion

        /// <summary>
        /// sets the filesystemwatchers for the local files
        /// </summary>
        /// <param name="path">local path to ftpbox</param>
        /// <param name="subDirs"><c>true</c> to include subdirectories, <c>false</c> to not</param>
        public void SetLocalWatcher()
        {            
            //fWatcher = new FileSystemWatcher();     //watcher for files in specified path
            //dirWatcher = new FileSystemWatcher();   //watcher for folders in specified path

            //fWatcher.Path = lPath();                
            //dirWatcher.Path = lPath();                
            fswFiles.Path = lPath();                //set the path
            fswFolders.Path = lPath();              //set the path

            //fWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
            //dirWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.DirectoryName;

            //fWatcher.Filter = "*";
            //dirWatcher.Filter = "*";

            fswFiles.Changed += new FileSystemEventHandler(OnChanged);
            fswFiles.Created += new FileSystemEventHandler(OnChanged);
            fswFiles.Deleted += new FileSystemEventHandler(OnChanged);
            fswFiles.Renamed += new RenamedEventHandler(OnRenamed);
            fswFolders.Changed += new FileSystemEventHandler(OnChanged);
            fswFolders.Created += new FileSystemEventHandler(OnChanged);
            fswFolders.Deleted += new FileSystemEventHandler(OnChanged);
            fswFolders.Renamed += new RenamedEventHandler(OnRenamed);

            fswFiles.IncludeSubdirectories = true; //subdirs();     //whether to raise events for changes on subdirectories or not
            fswFolders.IncludeSubdirectories = true; //subdirs();

            fswFiles.EnableRaisingEvents = true;            //begin watching the files
            fswFolders.EnableRaisingEvents = true;          //begin watching the folders

        }

        /// <summary>
        /// FileSystemWatcher event, raised when a file/directory the app is watching has been changed/deleted/created
        /// </summary>
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            FileInfo f = new FileInfo(e.FullPath);
            string cPath = GetCommonPath(f.Directory.Name, false); //GetCommonPath(e.FullPath.Replace(e.Name, ""), false);
            Log.Write("||||||| " + cPath);

            string curPath = "";

            try
            {
                curPath = ftp.GetCurrentDirectory();
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }

            string rFullpath = noSlashes(rPath()) + @"/" + cPath;

            if (!OfflineMode && e.Name != LastChangedFileFromRem && e.Name != LastChangedFolderFromRem && e.Name != LastChangedFolderFromRem)
            {
                Log.Write("Gonna change {0}", e.Name);
                if (e.ChangeType == WatcherChangeTypes.Created)
                {
                    if (source == fswFiles)
                    {
                        try
                        {
                            CreateRemote(e.FullPath, cPath, false);
                        }
                        catch { DoneSyncing(); }
                    }
                    else
                    {
                        try
                        {
                            CreateRemote(e.FullPath, cPath, true);
                        }
                        catch { DoneSyncing(); }
                    }
                }
                else if (e.ChangeType == WatcherChangeTypes.Deleted)
                {
                    if (delRem())
                    {
                        if (source == fswFiles)
                        {
                            try
                            {
                                DeleteRemote(e.Name, cPath, false);
                            }
                            catch { DoneSyncing(); }
                            RemoveFromLog(cPath);
                        }
                        else
                        {
                            try
                            {
                                DeleteRemote(e.Name, cPath, true);
                            }
                            catch { DoneSyncing(); }
                        }
                    }
                }
                else if (e.ChangeType == WatcherChangeTypes.Changed)
                {
                    try
                    {
                        if (source == fswFiles)
                        {
                            try
                            {
                                ChangeRemote(e.Name, cPath);
                            }
                            catch { DoneSyncing(); }

                        }
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message);
                    }
                }
            }
            if (curPath != "")
            {
                try
                {
                    ftp.SetCurrentDirectory(curPath);
                }
                catch (Exception ex)
                {
                    Log.Write(ex.Message);
                }
            }

            LastChangedFolderFromRem = "";
            LastChangedFileFromRem = "";
        }

        /// <summary>
        /// FileSystemWatcher event
        /// raised when a file/directory the app is watching has been renamed
        /// </summary>
        private void OnRenamed(object source, RenamedEventArgs e)
        {
            string cPath = GetCommonPath(e.FullPath.Replace(e.Name, ""), false) + @"/";

            string rFullpath = noSlashes(rPath()) + @"/" + cPath;
            if (!OfflineMode)
            {
                RenameRemote(e.OldName, e.Name, cPath);
            }
           
        }       

        /// <summary>
        /// returns the common path of the two ftpBoxes (local and remote)
        /// </summary>
        /// <param name="path">path from which to crop the common path</param>
        /// <param name="fromRem">whether the path is remote or local</param>
        private string GetCommonPath(string path, bool fromRem)
        {
            string cPath = path;
            if (fromRem)
            {
                if (cPath.StartsWith(rPath()))
                {
                    cPath = cPath.Substring(rPath().Length, cPath.Length - rPath().Length);
                }
                cPath = cPath.Replace(@"/", @"\");
                cPath = noSlashes(cPath);
                return cPath + @"\";
            }
            else
            {
                if (cPath.StartsWith(lPath()))
                {
                    cPath = cPath.Substring(lPath().Length, cPath.Length - lPath().Length);
                    Log.Write("!!!!!!!!!!!!! " + cPath);
                }
                cPath = cPath.Replace(@"\", @"/");
                cPath = noSlashes(cPath);
                return cPath + @"/";
            }
        }

        /// <summary>
        /// renames a remote file or folder
        /// </summary>
        /// <param name="oldName">the old name</param>
        /// <param name="newName">the new neame</param>
        /// <param name="cPath">the common path of local and remote ftpBox</param>
        private void RenameRemote(string oldName, string newName, string cPath)
        {
            if (ftp.FileExists(oldName))
            {
                Syncing();
                string rFullpath = noSlashes(rPath()) + @"/" + cPath;
                ftp.SetCurrentDirectory(rFullpath);
                ftp.RenameFile(oldName, newName);
                if (ShowNots())
                    tray.ShowBalloonTip(100, "FTPbox", string.Format("{0} was renamed to {1}.", oldName, newName), ToolTipIcon.Info);
                Get_Link("", newName);

                DoneSyncing();
            }
            else
            {
                CreateRemote(newName, cPath, false);
            }
        }

        /// <summary>
        /// uploads a file to host or creates a new directory
        /// </summary>
        /// <param name="path">local path to folder or file</param>
        /// <param name="cPath">common path of the two ftpboxes</param>
        /// <param name="isDir">True in case of folder, false in case of file</param>
        private void CreateRemote(string path, string cPath, bool isDir)
        {
            Syncing();
            string rFullPath = noSlashes(rPath()) + @"/" + cPath;
            ftp.SetCurrentDirectory(rFullPath);
            Log.Write("&&&&&&&&&& {0} {1}", rFullPath, cPath);

            if (isDir)
            {
                DirectoryInfo di = new DirectoryInfo(path);
                ftp.CreateDirectory(di.Name);

                if (ShowNots() && lasttip != string.Format("File {0} was updated.", di.Name))
                    tray.ShowBalloonTip(100, "FTPbox", string.Format("Folder {0} was created.", di.Name), ToolTipIcon.Info);
                lasttip = string.Format("File {0} was updated.", di.Name);
                Get_Link("", di.Name);
            }
            else 
            {
                FileInfo fi = new FileInfo(path);
                ftp.PutFile(path, fi.Name);

                if (ShowNots() && lasttip != string.Format("File {0} was updated.", fi.Name))
                    tray.ShowBalloonTip(100, "FTPbox", string.Format("File {0} was created.", fi.Name), ToolTipIcon.Info);
                lasttip = string.Format("File {0} was updated.", fi.Name);
                Get_Link("", fi.Name);

                ftp.SetCurrentDirectory(rPath());

                UpdateLog(path);
            }

            DoneSyncing();
           
        }

        /// <summary>
        /// Delete a folder or file on host. Called only if deleting from remote is allowed
        /// </summary>
        /// <param name="name">name of file/folder to delete</param>
        /// <param name="cPath">common path of local and remote</param>
        /// <param name="isDir">true in case of folder, false in case of file</param>
        private void DeleteRemote(string name, string cPath, bool isDir)
        {
            Syncing();
            string rFullPath = noSlashes(rPath()) + @"/" + cPath;
            ftp.SetCurrentDirectory(rFullPath);

            if (isDir)
            {
                ftp.RemoveDirectory(name);

                if (ShowNots() && lasttip != string.Format("File {0} was updated.", name))
                    tray.ShowBalloonTip(100, "FTPbox", string.Format("Folder {0} was deleted.", name), ToolTipIcon.Info);
                link = null;
            }
            else 
            {
                ftp.RemoveFile(name);

                if (ShowNots() && lasttip != string.Format("File {0} was updated.", name))
                    tray.ShowBalloonTip(100, "FTPbox", string.Format("File {0} was deleted.", name), ToolTipIcon.Info);
                link = null;

                string locPath = noSlashes(lPath()) + @"\" + noSlashes(cPath) + @"\" + name;
                RemoveFromLog(cPath);

            }
            DoneSyncing();
        }

        /// <summary>
        /// change a file on remote server. Only files change.
        /// </summary>
        /// <param name="name">name of file</param>
        /// <param name="cPath">common path</param>
        private void ChangeRemote(string name, string cPath)
        {
            if (ftp.FileExists(name))
            {
                Syncing();
                string rFullPath = noSlashes(rPath()) + @"/" + cPath;
                string locPath = noSlashes(lPath()) + @"\" + noSlashes(cPath.Replace(@"/", @"\"));
                locPath = noSlashes(locPath) + @"\" + name;

                ftp.SetCurrentDirectory(rFullPath);

                ftp.RemoveFile(name);
                ftp.PutFile(locPath, name);

                if (ShowNots() && lasttip != string.Format("File {0} was updated.", name))
                    tray.ShowBalloonTip(100, "FTPbox", string.Format("File {0} was updated.", name), ToolTipIcon.Info);
                lasttip = string.Format("File {0} was updated.", name);
                Get_Link("", name);


                UpdateLog(locPath);

                DoneSyncing();
            }
            else
            {
                CreateRemote(name, cPath, false);
            }
        }

        /// <summary>
        /// removes slashes from beggining and end of paths
        /// </summary>
        /// <param name="x">the path from which to remove the slashes</param>
        /// <returns>path without slashes</returns>
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
        /// called when syncing starts
        /// </summary>
        private void Syncing()
        {
            tray.Icon = FTPbox.Properties.Resources.syncing;
            tray.Text = "FTPbox - Syncing...";
        }

        /// <summary>
        /// called when syncing ends
        /// </summary>
        private void DoneSyncing()
        {
            tray.Icon = FTPbox.Properties.Resources.all_synced;
            tray.Text = "FTPbox - All files synced";
        }

        private void bAddFTP_Click(object sender, EventArgs e)
        {
            fNewFtp.ShowDialog();
        }

        private void bChangeBox_Click(object sender, EventArgs e)
        {
            newDir.ShowDialog();
            //StartupCheck();
        }

        private void rOpenInBrowser_CheckedChanged(object sender, EventArgs e)
        {
            FTPbox.Properties.Settings.Default.openinbrowser = rOpenInBrowser.Checked;
            FTPbox.Properties.Settings.Default.Save();
        }

        private void tray_BalloonTipClicked(object sender, EventArgs e)
        {
            //string link = noSlashes(ftpHost()) + "/" + noSlashes(ftpParent()) + "/" + noSlashes(last5items[0]);
            if ((Control.MouseButtons & MouseButtons.Right) != MouseButtons.Right)
            {
                if (OpenInBrowser())
                {
                    if (link != null && link != "")
                    {
                        try
                        {
                            Process.Start(link);
                        }
                        catch { }
                    }
                }
                else
                {
                    try
                    {
                        Clipboard.SetText(link);
                    }
                    catch { }

                    if (ShowNots())
                    {
                        tray.ShowBalloonTip(30, "FTPbox", "Link copied to clipboard", ToolTipIcon.Info);
                        link = null;
                    }
                }
            }
        }

        private void tParent_TextChanged(object sender, EventArgs e)
        {
            FTPbox.Properties.Settings.Default.ftpParent = tParent.Text;
            FTPbox.Properties.Settings.Default.Save();
        }

        public void StartupCheck()
        {
            ftp.SetCurrentDirectory(rPath());

            foreach (FtpFileInfo rf in ftp.GetFiles())
            {
                string lfilepath = noSlashes(lPath()) + @"\" + rf.Name;
                FileInfo lf = new FileInfo(lfilepath);
                try{
                    //MessageBox.Show(rf.LastWriteTimeUtc.Value.ToString("yyyy-MM-dd HH:mm tt"), "remote");
                }
                catch { }
                if (File.Exists(lfilepath))
                {
                    //MessageBox.Show(lfilepath + " exists");
                    ChecKSameName(lfilepath, rf.Name, lf.LastWriteTimeUtc, rf.LastWriteTimeUtc.Value.AddHours(timedif.Hours));
                    
                }
                else
                {
                    //MessageBox.Show(lfilepath + " !exists");
                    ftp.SetLocalDirectory(lPath());
                    ftp.GetFile(rf.Name, RemoveSymbols(rf.Name) ,false);
                    if (ShowNots() && lasttip != string.Format("File {0} was updated.", rf.Name))
                    {
                        tray.ShowBalloonTip(100, "FTPbox", string.Format("File {0} was updated.", rf.Name), ToolTipIcon.Info);
                        lasttip = string.Format("File {0} was updated.", rf.Name);
                    }
                }

                UpdateLog(lfilepath);
            }

            foreach (string lf in Directory.GetFiles(lPath(), "*", SearchOption.AllDirectories))
            {
                FileInfo fi = new FileInfo(lf);

                if (!ftp.FileExists(fi.Name))
                {                    
                    ftp.PutFile(lf);
                    if (ShowNots() && lasttip != string.Format("File {0} was updated.", fi.Name))
                    {
                        tray.ShowBalloonTip(100, "FTPbox", string.Format("File {0} was uploaded.", fi.Name), ToolTipIcon.Info);
                        lasttip = string.Format("File {0} was updated.", fi.Name);
                    }
                }
            }
        }

        /// <summary>
        /// checks for changes in files with same name, comparing datetimes to log's values
        /// </summary>
        /// <param name="path">path to local file</param>
        /// <param name="name">common name of files</param>
        /// <param name="localFilesDT">LastWriteTimeUTC of local file</param>
        /// <param name="remoteFilesDT">LastWriteTimeUTC of remote file</param>
        public void ChecKSameName(string path, string name, DateTime localFilesDT, DateTime? remoteFilesDT)
        {
            string cPath = GetCommonPath(path, false);
            cPath = cPath.Substring(0, cPath.Length - 1);
            string FullLocalPath = noSlashes(lPath()) + @"\" + cPath.Replace(name, "").Replace(@"/", @"\");
            string FullRemPath = noSlashes(rPath()) + @"/" + cPath.Replace(name, "");
            cPath = cPath.Replace(@"/", @"\");
            if (log == null || !log.Contains(cPath))
            {
                Log.Write("CheckSameName: --> log null or doesn't contain " + cPath);
                ftp.SetLocalDirectory(FullLocalPath);
                ftp.SetCurrentDirectory(FullRemPath);
                ftp.GetFile(name, RemoveSymbols(name), false);
                if (ShowNots() && lasttip != string.Format("File {0} was updated.", name))
                    tray.ShowBalloonTip(100, "FTPbox", string.Format("File {0} was updated.", name), ToolTipIcon.Info);
                lasttip = string.Format("File {0} was updated.", name);
                UpdateLog(path);                
            }
            else
            {
                Log.Write("CheckSameName: --> contains " + cPath);
                FileInfo lf = new FileInfo(path);
                int i = log.IndexOf(cPath);
                DateTime rD = DateTime.Parse(rDL[i]);
                DateTime lD = DateTime.Parse(lDL[i]);
                //DateTime rD = DateTime.ParseExact(rDL[i], "yyyy-MM-dd HH:mm tt", null);
                //DateTime lD = DateTime.ParseExact(lDL[i], "yyyy-MM-dd HH:mm tt", null);                

                if (rD.ToString() != remoteFilesDT.ToString())
                {
                    //MessageBox.Show(rD.CompareTo(remoteFilesDT).ToString());
                    ftp.SetLocalDirectory(FullLocalPath);
                    ftp.SetCurrentDirectory(FullRemPath);
                    ftp.GetFile(name, RemoveSymbols(name), false);
                    if (ShowNots() && lasttip != string.Format("File {0} was updated.", name))
                        tray.ShowBalloonTip(100, "FTPbox", string.Format("File {0} was updated.", name), ToolTipIcon.Info);
                    lasttip = string.Format("File {0} was updated.", name);
                    UpdateLog(path);
                }

                //rD = DateTime.ParseExact(rDL[i], "yyyy-MM-dd HH:mm tt", null);
                //lD = DateTime.ParseExact(lDL[i], "yyyy-MM-dd HH:mm tt", null);
                rD = DateTime.Parse(rDL[i]);
                lD = DateTime.Parse(lDL[i]);

                if (lD.ToString() != localFilesDT.ToString())
                {
                    if (localFilesDT > remoteFilesDT)
                    {
                        //MessageBox.Show(rD.ToString() + Environment.NewLine + lD.ToString() + Environment.NewLine + remoteFilesDT.ToString() + Environment.NewLine + localFilesDT.ToString());
                        //MessageBox.Show("case 2");
                        ftp.SetCurrentDirectory(FullRemPath);
                        ftp.PutFile(path, name);
                        if (ShowNots() && lasttip != string.Format("File {0} was updated.", name))
                            tray.ShowBalloonTip(100, "FTPbox", string.Format("File {0} was updated.", name), ToolTipIcon.Info);
                        lasttip = string.Format("File {0} was updated.", name);
                        UpdateLog(path);
                    }
                    else
                    {
                        //MessageBox.Show(rD.ToString() + Environment.NewLine + lD.ToString() + Environment.NewLine + remoteFilesDT.ToString() + Environment.NewLine + localFilesDT.ToString());
                        //MessageBox.Show(lD.CompareTo(localFilesDT).ToString());
                        ftp.SetLocalDirectory(FullLocalPath);
                        ftp.SetCurrentDirectory(FullRemPath);
                        ftp.GetFile(name, RemoveSymbols(name), false);
                        if (ShowNots() && lasttip != string.Format("File {0} was updated.", name))
                            tray.ShowBalloonTip(100, "FTPbox", string.Format("File {0} was updated.", name), ToolTipIcon.Info);
                        lasttip = string.Format("File {0} was updated.", name);
                        UpdateLog(path);
                    }
                }
            }
        }

        public void UpdateLog(string lpath)
        {/*
            FileInfo lfi = new FileInfo(lpath);
            string LocPath = lpath;
            if (LocPath.StartsWith(lPath()))
            {
                int i = lPath().Length;
                LocPath = LocPath.Substring(i, LocPath.Length - i);
            }
            string name = lfi.Name;
            string rDate = "";

            foreach (KeyValuePair<string, DateTime> FileOrDir in FullList)
            {
                if (FileOrDir.Key == noSlashes((rPath() + @"/" + noSlashes(LocPath).Replace(@"\", @"/")).Replace(@"\", @"/")))
                {
                    rDate = TimeZoneInfo.ConvertTimeToUtc(FileOrDir.Value).AddHours(timedif.Hours).ToString();
                }
            }

            if (log == null || !log.Contains(LocPath))
            {
                log.Add(LocPath);
                lDL.Add(lfi.LastWriteTimeUtc.ToString());
                rDL.Add(rDate);
            }
            else
            {
                RemoveFromLog(LocPath);
                log.Add(LocPath);
                lDL.Add(lfi.LastWriteTimeUtc.ToString());
                rDL.Add(rDate);
            }

            FTPbox.Properties.Settings.Default.log += "|" + LocPath;
            FTPbox.Properties.Settings.Default.lDateLog += "|" + lfi.LastWriteTimeUtc.ToString();
            FTPbox.Properties.Settings.Default.rDateLog += "|" + rDate;
            FTPbox.Properties.Settings.Default.Save();

            if (link != "" && link != null)
            {
                Get_Recent(name);
            }*/
        } 

        public void SaveLog()
        {
            string combLog = null;
            foreach (string s in log)
            {
                combLog = combLog + @"|" + s;
            }
            string comblDate = null;
            foreach (string s in lDL)
            {
                comblDate = comblDate + @"|" + s;
            }
            string combrDate = null;
            foreach (string s in rDL)
            {
                combrDate = combrDate + @"|" + s;
            }
            FTPbox.Properties.Settings.Default.log = combLog;
            FTPbox.Properties.Settings.Default.lDateLog = comblDate;
            FTPbox.Properties.Settings.Default.rDateLog = combrDate;
            FTPbox.Properties.Settings.Default.Save();
        }

        private string RemoveSymbols(string name)
        {
            string newname = name;
            newname = newname.Replace(@"*", @"_");
            newname = newname.Replace(@"/", @"_");
            newname = newname.Replace(@"\", @"_");
            newname = newname.Replace(@"?", @"_");
            newname = newname.Replace(@"<", @"_");
            newname = newname.Replace(@">", @"_");
            newname = newname.Replace(@"|", @"_");

            return newname;
        }

        private void chkStartUp_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                SetStartup(chkStartUp.Checked);
            }
            catch { }

            FTPbox.Properties.Settings.Default.startup = chkStartUp.Checked;
            FTPbox.Properties.Settings.Default.Save();
        }

        private void chkShowNots_CheckedChanged(object sender, EventArgs e)
        {
            FTPbox.Properties.Settings.Default.shownots = chkShowNots.Checked;
            FTPbox.Properties.Settings.Default.Save();
        }

        private void chkCloseToTray_CheckedChanged(object sender, EventArgs e)
        {
            FTPbox.Properties.Settings.Default.closetotray = chkCloseToTray.Checked;
            FTPbox.Properties.Settings.Default.Save();
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

        #region Update System

        /// <summary>
        /// checks for an update
        /// called on each start-up of FTPbox.
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
                // show dialog box for  download now, learn more and remind me next time
            }
        }

        #endregion

        #region About Tab
        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://sourceforge.net/users/johnthegr8");
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"http://sharpmindprojects.com");
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://sourceforge.net/projects/ftpbox/");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://sourceforge.net/tracker/?group_id=538656&atid=2187305");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://sourceforge.net/tracker/?group_id=538656&atid=2187308");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Process.Start(@"http://sharpmindprojects.com/about");
        }
        #endregion

        #region tray menu
        bool ExitedFromTray = false;
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (CloseToTray() && !ExitedFromTray)
            {
                e.Cancel = true;
                this.Hide();
            }
        }


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitedFromTray = true;
            Application.Exit();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
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
            this.BringToFront();
        }
        #endregion


        private void Get_Link(string subpath, string name)
        {
            string newlink = noSlashes(ftpParent()) + @"/";
            
            if (!noSlashes(newlink).StartsWith("http://") || !noSlashes(newlink).StartsWith("https://"))
            {
                newlink = @"http://" + newlink;
            }
            /*
            if (ftpParent() != null && noSlashes(ftpParent()) != null)
            {
                newlink = newlink + noSlashes(ftpParent()) + @"/";
            } */

            if (noSlashes(rPath()) != null)
            {
                if ((rPath() == @"/"))
                {
                    newlink = "";
                }
                else if (rPath().StartsWith(@"/"))
                {
                    newlink = newlink + noSlashes(rPath()).Substring(1, noSlashes(rPath()).Length - 1) + @"/";
                }
                else
                {
                    newlink = newlink + noSlashes(rPath()) + @"/";
                }                
            }
            if (subpath != null && noSlashes(subpath) != null && noSlashes(subpath) != "")
            {
                newlink = newlink + noSlashes(subpath) + @"/";
            }
            newlink = newlink + name;
            link = newlink.Replace(" ", "%20");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (InternetOn())
            {
                MessageBox.Show(FTPbox.Properties.Settings.Default.log);
                MessageBox.Show(FTPbox.Properties.Settings.Default.lDateLog);
                MessageBox.Show(FTPbox.Properties.Settings.Default.rDateLog);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ClearLog();
        }

        public void ClearLog()
        {
            FTPbox.Properties.Settings.Default.log = null;
            FTPbox.Properties.Settings.Default.lDateLog = null;
            FTPbox.Properties.Settings.Default.rDateLog = null;
            FTPbox.Properties.Settings.Default.Save();
        }

        #region check internet connection
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        public static bool InternetOn()
        {
            int Desc;
            return InternetGetConnectedState(out Desc, 0);
        }

        #endregion

        List<string> oldList = new List<string>();
        List<string> newList = new List<string>();
        private void WatchRemote_Tick(object sender, EventArgs e)
        {
            try
            {
                if (InternetOn())
                {
                    if (OfflineMode)
                    {
                        StartUpWork();
                    }
                    OfflineMode = false;
                }
                else
                {
                    if (!OfflineMode)
                    {
                        ftp.Close();
                        fswFiles.Dispose();
                        fswFolders.Dispose();
                    }
                    OfflineMode = true;
                    tray.Icon = FTPbox.Properties.Resources.offline1;
                    tray.Text = "FTPbox - Offline";
                }

                if (!OfflineMode)
                {
                    newList.Clear();
                    
                    foreach (FtpFileInfo rfi in ftp.GetFiles())
                    {
                        newList.Add(RemoveSymbols(rfi.Name));
                        string path = noSlashes(lPath()) + @"\" + rfi.Name;
                        if (File.Exists(path))
                        {
                            FileInfo fi = new FileInfo(path);
                            ChecKSameName(path, fi.Name, fi.LastWriteTimeUtc, rfi.LastWriteTimeUtc.Value.AddHours(timedif.Hours));
                        }
                        else
                        {
                            Syncing();
                            ftp.SetLocalDirectory(lPath());
                            ftp.GetFile(rfi.Name, RemoveSymbols(rfi.Name), false);
                            if (ShowNots() && lasttip != string.Format("File {0} was updated.", rfi.Name))
                            {
                                tray.ShowBalloonTip(100, "FTPbox", string.Format("File {0} was updated.", rfi.Name), ToolTipIcon.Info);
                                lasttip = string.Format("File {0} was updated.", rfi.Name);
                            }
                            Get_Link("", rfi.Name);
                            UpdateLog(path);
                            DoneSyncing();
                        }
                    }

                    oldList.Clear();
                    oldList.AddRange(Directory.GetFiles(lPath(), "*", SearchOption.AllDirectories));

                    foreach (string s in oldList)
                    {
                        FileInfo f = new FileInfo(s);
                        if (!newList.Contains(f.Name))
                        {
                            string delpath = noSlashes(lPath()) + @"\" + f.Name;

                            File.Delete(delpath);

                            if (ShowNots() && lasttip != string.Format("File {0} was updated.", f.Name))
                            {
                                tray.ShowBalloonTip(50, "FTPbox", string.Format("File {0} was deleted.", f.Name), ToolTipIcon.Info);
                                lasttip = string.Format("File {0} was updated.", f.Name);
                            }
                        }
                    }
                }
                lasttip = null;
                
            }
            catch { }
        }

        private void Get_Recent(string name)
        {
            recentfiles[4] = recentfiles[3];
            recentfiles[3] = recentfiles[2];
            recentfiles[2] = recentfiles[1];
            recentfiles[1] = recentfiles[0];
            recentfiles[0] = name + @"|" + link;          

            foreach (string s in recentfiles)
            {
                string thename = s.Split('|', '|')[0];
                int i = recentfiles.IndexOf(s);

                recentFilesToolStripMenuItem.DropDownItems[i].Text = thename;
            }
        }

        #region Recent item clicked

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (recentfiles[0] != "" && recentfiles[0] != null && recentfiles[0] != "Not available")
            {
                if (OpenInBrowser())
                {
                    try
                    {
                        Process.Start(recentfiles[0].Split('|', '|')[1].Replace(@" ", @"%20"));
                    }
                    catch { }

                }
                else
                {
                    try
                    {
                        Clipboard.SetText(recentfiles[0].Split('|', '|')[1].Replace(@" ", @"%20"));
                        if (ShowNots())
                        {
                            link = null;
                            tray.ShowBalloonTip(30, "FTPbox", "Link copied to clipboard", ToolTipIcon.Info);
                        }
                    }
                    catch { }
                }  
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (recentfiles[1] != "" && recentfiles[1] != null && recentfiles[1] != "Not available")
            {
                if (OpenInBrowser())
                {
                    try
                    {
                        Process.Start(recentfiles[1].Split('|', '|')[1].Replace(@" ", @"%20"));
                    }
                    catch { }

                }
                else
                {
                    try
                    {
                        Clipboard.SetText(recentfiles[1].Split('|', '|')[1].Replace(@" ", @"%20"));
                        if (ShowNots())
                        {
                            link = null;
                            tray.ShowBalloonTip(30, "FTPbox", "Link copied to clipboard", ToolTipIcon.Info);
                        }
                    }
                    catch { }
                } 
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (recentfiles[2] != "" && recentfiles[2] != null && recentfiles[2] != "Not available")
            {
                if (OpenInBrowser())
                {
                    try
                    {
                        Process.Start(recentfiles[2].Split('|', '|')[1].Replace(@" ", @"%20"));
                    }
                    catch { }

                }
                else
                {
                    try
                    {
                        Clipboard.SetText(recentfiles[2].Split('|', '|')[1].Replace(@" ", @"%20"));
                        if (ShowNots())
                        {
                            link = null;
                            tray.ShowBalloonTip(30, "FTPbox", "Link copied to clipboard", ToolTipIcon.Info);
                        }
                    }
                    catch { }
                }  
            }
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            if (recentfiles[3] != "" && recentfiles[3] != null && recentfiles[3] != "Not available")
            {
                if (OpenInBrowser())
                {
                    try
                    {
                        Process.Start(recentfiles[3].Split('|', '|')[1].Replace(@" ", @"%20"));
                    }
                    catch { }

                }
                else
                {
                    try
                    {
                        Clipboard.SetText(recentfiles[3].Split('|', '|')[1].Replace(@" ", @"%20"));
                        if (ShowNots())
                        {
                            link = null;
                            tray.ShowBalloonTip(30, "FTPbox", "Link copied to clipboard", ToolTipIcon.Info);
                        }
                    }
                    catch { }
                } 
            }
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            if (recentfiles[4] != "" && recentfiles[4] != null && recentfiles[4] != "Not available")
            {
                if (OpenInBrowser())
                {
                    try
                    {
                        Process.Start(recentfiles[4].Split('|', '|')[1].Replace(@" ", @"%20"));
                    }
                    catch { }
                    
                }
                else
                {
                    try
                    {
                        Clipboard.SetText(recentfiles[4].Split('|', '|')[1].Replace(@" ", @"%20"));
                        if (ShowNots())
                        {
                            link = null;
                            tray.ShowBalloonTip(30, "FTPbox", "Link copied to clipboard", ToolTipIcon.Info);                            
                        }
                    }
                    catch { }
                } 
            }
        }

        #endregion

        private void GetServerTime()
        {
            try
            {
                ftp.CreateDirectory("tempfolderasdpjf");
                DateTime now = DateTime.UtcNow;
                foreach (FtpDirectoryInfo f in ftp.GetDirectories())
                {
                    if (f.Name == "tempfolderasdpjf")
                    {
                        DateTime rnow = f.LastWriteTimeUtc.Value;
                        TimeSpan x = now - rnow;
                        timedif = x;
                    }
                }

                ftp.RemoveDirectory("tempfolderasdpjf");
            }
            catch
            {
                
                Application.Exit();
            }
        }

        public void Set_logged_in()
        {
            loggedIn = true;
        }

        private void KillPrevInstances()
        {
            try
            {
                string procname = Process.GetCurrentProcess().ProcessName;
                Process[] allprocesses = Process.GetProcessesByName(procname);
                if (allprocesses.Length > 0)
                {
                    foreach (Process p in allprocesses)
                    {
                        if (p.Id != Process.GetCurrentProcess().Id)
                        {
                            p.Kill();
                        }
                    }
                }
            }
            catch { }
        }

        private void ListAllFiles()
        {
            try
            {
                //We are using forms, so let's use a background worker.
                //cause NoFate is gay and loves background workers :3
                BackgroundWorker lRemoteWrk = new BackgroundWorker();
                lRemoteWrk.DoWork += new DoWorkEventHandler(lRemoteWrk_DoWork);
                lRemoteWrk.RunWorkerCompleted += new RunWorkerCompletedEventHandler(lRemoteWrk_RunWorkerCompleted);
                Log.Write("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                lRemoteWrk.RunWorkerAsync();
            }
            catch {
                Log.Write("Could not list remote files");
            }
        }

        void lRemoteWrk_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Retrieve the dictionary from the result
            Dictionary<String, DateTime> fdDict = e.Result as Dictionary<String, DateTime>;
            FullList = fdDict;

            if (!OfflineMode)
            {
                CheckRemoteFiles();
                //try
               // {                    
                    //CheckAllRemote();
               // }
               // catch (Exception ex)
                //{
                 //   Log.Write(ex.Message);
                //}
                //Thread.Sleep(5000);
                //ListAllFiles();
            }

            Log.Write("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");

            

            // You can access form controls -HERE-
        }

        void lRemoteWrk_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                ftp.SetCurrentDirectory(rPath());
                // Get the remote directories/files
                Dictionary<String, DateTime> rtDict = listRemote();
                // And our work is complete!
                e.Result = rtDict;
            }
            catch { }
        }

        //////////////////////////
        //Contribution of NoFaTe//
        // (on Subdirectories)  //
        //  thanks a lot man!   //
        //////////////////////////

        /// <summary>
        /// Gets a list of all directories and files in a path
        /// </summary>
        /// <returns>A dictionary containing directory/file paths and last write times</returns>
        private Dictionary<String, DateTime> listRemote()
        {
            // Set up the dictionary that will hold the directories and files
            Dictionary<String, DateTime> fdDict = new Dictionary<String, DateTime>();

            // Set our current directory
            ftp.SetCurrentDirectory(rPath());

            // Loop through files
            foreach (FtpFileInfo fInfo in ftp.GetFiles())
            {
                if (fInfo.Name != ".ftpquota")
                {
                    // Got it? Add it!
                    fdDict.Add(noSlashes(String.Format("{0}/{1}", noSlashes(rPath()), fInfo.Name)), fInfo.LastWriteTimeUtc.Value);
                    Log.Write("{0}/{1}", noSlashes(rPath()), fInfo.Name);
                    //CheckRemoteFile(noSlashes(rPath()), fInfo.Name, fInfo.LastWriteTimeUtc.Value);
                }
            }

            // And directories
            foreach (FtpDirectoryInfo dInfo in ftp.GetDirectories())
            {
                // You can't trick me!
                if (dInfo.Name != "." && dInfo.Name != "..")
                {
                    getSetDirFiles(dInfo.Name, dInfo.LastWriteTimeUtc.Value, ref fdDict);
                }
            }

            return fdDict;
        }

        /// <summary>
        /// A nice internal function that loops through the directories and files
        /// </summary>
        /// <param name="path">The path to look into</param>
        /// <param name="lastWriteTime">Last write time of the path</param>
        /// <param name="fdDict">The dictionary in which to put found directories/files</param>
        private void getSetDirFiles(string path, DateTime lastWriteTime, ref Dictionary<String, DateTime> fdDict)
        {
            // Clean up the path and add it to the dictionary
            String newPath = noSlashes(String.Format("{0}/{1}", noSlashes(rPath()), path)) + @"/";
            fdDict.Add(newPath, lastWriteTime);

            Log.Write("Drct ({0})", newPath);

            // Set our current directory
            ftp.SetCurrentDirectory(newPath);

            // Loop through files
            foreach (FtpFileInfo fInfo in ftp.GetFiles())
            {
                if (fInfo.Name != ".ftpquota")
                {
                    // Got it? Add it!
                    Log.Write("File ({0})", noSlashes(String.Format("{0}{1}", newPath, fInfo.Name)));
                    fdDict.Add(noSlashes(String.Format("{0}{1}", newPath, fInfo.Name)), fInfo.LastWriteTimeUtc.Value);
                    //CheckRemoteFile(newPath, fInfo.Name, fInfo.LastWriteTimeUtc.Value);
                }
            }

            // And directories
            foreach (FtpDirectoryInfo dInfo in ftp.GetDirectories())
            {
                // You can't trick me!
                if (dInfo.Name != "." && dInfo.Name != "..")
                {
                    // Spawn another loop
                    getSetDirFiles(noSlashes(String.Format("{0}/{1}", path, dInfo.Name)), dInfo.LastWriteTimeUtc.Value, ref fdDict);
                }
            }
        }

        /////////////////////
        // End of NoFate's //
        //  Contribution   //  
        /////////////////////

        public void CheckRemoteFile(string cPath, string name, DateTime rDT)
        {
            string prevDirectory = ftp.GetCurrentDirectory();
            string FullRemPath = rPath();
            string FullLocalPath = noSlashes(lPath());
            string LocalFileDirParent = lPath();
            if (cPath == "")
            {
                FullLocalPath = FullLocalPath + @"\" + name;
            }
            else
            {
                FullLocalPath = FullLocalPath + @"\" + noSlashes(cPath.Replace(@"/", @"\")) + @"\" + name;
                FullRemPath = noSlashes(FullRemPath) + cPath;
                LocalFileDirParent = noSlashes(LocalFileDirParent) + cPath.Replace(@"/", @"\");
            }
            Log.Write("cPath {0} -> name {1} -> rDT {2} ->Flp {3}", cPath, name, rDT.ToString(), FullLocalPath);
            Log.Write(FullRemPath + " " + LocalFileDirParent);
            Log.Write(prevDirectory);

            if (rLog == null || lLog == null)
            {
                Log.Write("Log is null");
                ftp.SetCurrentDirectory(FullRemPath);
                ftp.SetLocalDirectory(LocalFileDirParent);
                LastChangedFileFromRem = name;
                ftp.GetFile(name, false);            
            }
            else
            {

            }

            ftp.SetCurrentDirectory(prevDirectory);
        }

        public void CheckRemoteFiles()
        {
            foreach (KeyValuePair<string, DateTime> s in FullList)
            {
                DateTime rDT = s.Value;

                if (s.Key.EndsWith(@"/"))
                {

                }
                else
                {
                    int i = s.Key.LastIndexOf(@"/");
                    string cPath = s.Key.Substring(0, i);
                    string name = s.Key.Substring(i + 1, s.Key.Length - i - 1);
                    string prevDirectory = ftp.GetCurrentDirectory();
                    string FullRemPath = rPath();
                    string FullLocalPath = noSlashes(lPath());
                    string LocalFileDirParent = lPath();
                    

                    if (cPath == "")
                    {
                        FullLocalPath = FullLocalPath + @"\" + name;
                    }
                    else
                    {
                        FullLocalPath = FullLocalPath + @"\" + noSlashes(cPath.Replace(@"/", @"\")) + @"\" + name;
                        FullRemPath = noSlashes(FullRemPath) + cPath;
                        LocalFileDirParent = noSlashes(LocalFileDirParent) + cPath.Replace(@"/", @"\");
                    }
                    Log.Write("cPath {0} -> name {1} -> rDT {2} ->Flp {3}", cPath, name, rDT.ToString(), FullLocalPath);
                    Log.Write(FullRemPath + " " + LocalFileDirParent);
                    Log.Write(prevDirectory);

                    if (rLog == null || lLog == null)
                    {
                        Log.Write("Log is null, gonna get {0}", name);
                        ftp.SetCurrentDirectory(FullRemPath);
                        ftp.SetLocalDirectory(LocalFileDirParent);
                        LastChangedFileFromRem = name;
                        LastChangedFolderFromRem = GetParentFolder(cPath);
                        ftp.GetFile(name, false);
                        UpdateTheLog(s.Key, s.Value);
                    }
                    else
                    {

                    }
                }

            }
        }

        public string GetParentFolder(string path)
        {
            string s = path;
            if (s == "" || s == @"/")
            {
                return @"/";
            }
            else
            {
                if (s.EndsWith(@"/"))
                    s = s.Substring(0, s.Length - 1);
                int i = s.LastIndexOf(@"/");
                s = s.Substring(i, s.Length - i);
                return s;
            }            
        }

        /// <summary>
        /// Saves any file/folder changes to the local log
        /// </summary>
        /// <param name="cPath">common path to file/folder that changed</param>
        /// <param name="rDTlog">remote file's LastWriteTimeUtc</param>
        public void UpdateTheLog(string cPath, DateTime rDTlog)
        {
            string FullLocalPath = noSlashes(lPath());
            string name = "";
            DateTime lDTlog;

            if (cPath.EndsWith(@"/"))
            {
                name = GetParentFolder(cPath);
                if (cPath != "")
                    FullLocalPath = FullLocalPath + @"\" + noSlashes(cPath.Replace(@"/", @"\")) + @"\" + name;
                DirectoryInfo dInfo = new DirectoryInfo(FullLocalPath);
                lDTlog = dInfo.LastWriteTimeUtc;
            }
            else
            {
               name = cPath.Substring(cPath.LastIndexOf("/") + 1, cPath.Length - cPath.LastIndexOf("/") - 1);
               if (cPath != "")
               {
                   FullLocalPath = FullLocalPath + @"\" + noSlashes(cPath.Replace(@"/", @"\"));
               }
               FileInfo fInfo = new FileInfo(FullLocalPath);
               lDTlog = fInfo.LastWriteTimeUtc;
            }

            RemoveFromLog(cPath);
            rLog.Add(cPath, rDTlog.ToString());
            lLog.Add(cPath, lDTlog.ToString());
            SaveTheLog();

            Log.Write("##########");
            Log.Write("FLP {0} + name: {1} + rDTlog: {2} + lDTlog: {3} + cPath: {4}", FullLocalPath, name, rDTlog.ToString(), lDTlog.ToString(), cPath);
            Log.Write("#########");
        }

        public void RemoveFromLog(string cPath)
        {
            if (rLog != null)
            {
                if (rLog.ContainsKey(cPath))
                {
                    rLog.Remove(cPath);
                }
                if (lLog.ContainsKey(cPath))
                {
                    lLog.Remove(cPath);
                }
            }
        }

        public void SaveTheLog()
        {
            FTPbox.Properties.Settings.Default.rLog = rLog;
            FTPbox.Properties.Settings.Default.lLog = lLog;
            FTPbox.Properties.Settings.Default.Save();
        }
        
    }
}