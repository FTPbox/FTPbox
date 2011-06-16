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

        FtpConnection ftp;
        FtpConnection ftpbg;

        //Form instances
        NewFTP fNewFtp;
        fNewDir newDir;

        //logs:
        List<string> log = new List<string>(FTPbox.Properties.Settings.Default.log.Split('|', '|'));
        List<string> rDL  = new List<string>(FTPbox.Properties.Settings.Default.rDateLog.Split('|', '|'));
        List<string> lDL = new List<string>(FTPbox.Properties.Settings.Default.lDateLog.Split('|', '|'));        

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

        BackgroundWorker lRemoteWrk;

        public bool downloading = false;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            KillPrevInstances();

            //ClearLog();
            
            foreach (string s in FTPbox.Properties.Settings.Default.nLog.Split('|', '|'))
            {
                Log.Write("In Log: {0}", s);
            }
            
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

            //CheckLocal();

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
                ftpbg = new FtpConnection(ftpHost(), ftpPort(), ftpUser(), ftpPass());
                ftp.Open();
                ftp.Login();
                ftpbg.Open();
                ftpbg.Login();
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
                ftpbg = new FtpConnection(ftpHost(), ftpPort(), ftpUser(), ftpPass());
                ftp.Open();
                ftp.Login();
                ftpbg.Open();
                ftpbg.Login();
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

        string nLog()
        {
            return FTPbox.Properties.Settings.Default.nLog;
        }

        string rLog()
        {
            return FTPbox.Properties.Settings.Default.rLog;
        }

        string lLog()
        {
            return FTPbox.Properties.Settings.Default.lLog;
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
            string cPath = "";
            string name = "";
            try
            {
                if (source == fswFiles)
                {
                    FileInfo f = new FileInfo(e.FullPath);
                    cPath = GetCommonPath(f.Directory.FullName, false);
                    name = f.Name;
                    Log.Write("File {0} ;;; {1} ;;; {2} ;;; {3}", cPath, f.Directory.FullName, e.FullPath, name);
                }
                else
                {
                    DirectoryInfo d = new DirectoryInfo(e.FullPath);
                    cPath = GetCommonPath(d.Parent.FullName, false); //GetCommonPath(e.FullPath.Replace(e.Name, ""), false);
                    name = d.Name;
                    Log.Write("Dir {0} ;;; {1} ;;; {2} ;;; {3}", cPath, d.FullName, e.FullPath, name);                    
                }
            }
            catch
            {
                if (e.Name.Contains(@"\"))
                {
                    cPath = e.Name.Substring(0, e.Name.LastIndexOf(@"\"));
                    name = e.Name.Substring(e.Name.LastIndexOf(@"\"), e.Name.Length - e.Name.LastIndexOf(@"\"));
                }
                else
                {
                    cPath = @"\";
                    name = e.Name;
                }

                if (name.StartsWith(@"\"))
                    name = name.Substring(1, name.Length - 1);

                cPath = @"\" + cPath;
                cPath = cPath.Replace(@"\", @"/");
                Log.Write("cpath: {0} name: {1}", cPath, name);

            }

            string rFullpath = noSlashes(rPath()) + cPath;

            Log.Write("====} cPath: {0} rFullPath: {1}", cPath, rFullpath);


            if (!OfflineMode && e.Name != LastChangedFileFromRem && e.Name != LastChangedFolderFromRem && e.Name != LastChangedFolderFromRem)
            {
                Log.Write("Gonna change {0}", e.Name);
                if (e.ChangeType == WatcherChangeTypes.Created)
                {
                    Log.Write("ooooooooooooooooo Created");
                    if (source == fswFiles)
                    {
                        try
                        {
                            CreateRemote(e.FullPath, cPath, name, false);
                        }
                        catch { DoneSyncing(); }
                    }
                    else
                    {
                        try
                        {
                            CreateRemote(e.FullPath, cPath, name, true);
                        }
                        catch { DoneSyncing(); }
                    }
                }
                else if (e.ChangeType == WatcherChangeTypes.Deleted)
                {
                    Log.Write("ooooooooooooooooo Deleted");
                    if (delRem() && !downloading)
                    {
                        if (source == fswFiles)
                        {
                            Log.Write(">>>>>> Deleted file");
                            try
                            {
                                DeleteRemote(name, cPath, false);
                            }
                            catch { DoneSyncing(); }
                            RemoveFromLog(cPath  + name);
                        }
                        else
                        {
                            Log.Write(">>>>>> Deleted Folder");
                            try
                            {
                                DeleteRemote(name, cPath, true);
                            }
                            catch { DoneSyncing(); }
                        }
                    }
                }
                else if (e.ChangeType == WatcherChangeTypes.Changed)
                {
                    Log.Write("ooooooooooooooooo Changed");
                    try
                    {
                        if (source == fswFiles)
                        {
                            try
                            {
                                ChangeRemote(name, cPath, e.FullPath);
                            }
                            catch { DoneSyncing(); }

                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex.Message);
                    }
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
            string cPath;
            if (e.OldName.Contains(@"\"))
            {
                cPath = e.OldName.Substring(0, e.OldName.LastIndexOf(@"\"));
                cPath = @"\" + cPath;
            }
            else
                cPath = @"\";
            
            cPath = cPath.Replace(@"\", @"/");

            if (!OfflineMode)
            {
                if (source == fswFiles)
                {
                    RenameRemote(e.OldName, e.Name, cPath, e.FullPath, false);
                }
                else
                {
                    RenameRemote(e.OldName, e.Name, cPath, e.FullPath, true);
                }
                
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
        private void RenameRemote(string oldName, string newName, string cPath, string FullPath, bool isDir)
        {
            string rFullpath = noSlashes(rPath()) + cPath;
            Log.Write("About to rename {0} to {1} in path: {2}", oldName, newName, rFullpath);

            try
            {
                ftp.SetCurrentDirectory(rFullpath);
            }
            catch (Exception ex)
            {
                Log.Write("[ERROR setting dir] -> {0}", rFullpath);
            }

            string nameOld = oldName;
            string nameNew = newName;
            if (nameOld.Contains(@"\"))
                nameOld = nameOld.Substring(oldName.LastIndexOf(@"\"), oldName.Length - oldName.LastIndexOf(@"\"));
            if (nameNew.Contains(@"\"))
                nameNew = nameNew.Substring(newName.LastIndexOf(@"\"), newName.Length - newName.LastIndexOf(@"\"));
            if (nameOld.StartsWith(@"\"))
                nameOld = nameOld.Substring(1, nameOld.Length - 1);
            if (nameNew.StartsWith(@"\"))
                nameNew = nameNew.Substring(1, nameNew.Length - 1);
            Log.Write("nameOld: {0} nameNew: {1}", nameOld, nameNew);

            if (ftp.FileExists(nameOld))
            {
                Syncing();
                ftp.RenameFile(nameOld, nameNew);
                if (ShowNots())
                    tray.ShowBalloonTip(50, "FTPbox", string.Format("{0} was renamed to {1}.", nameOld, nameNew), ToolTipIcon.Info);
                lasttip = string.Format("{0} was renamed to {1}.", nameOld, nameNew);
                Get_Link(cPath, newName);

                string oldLogPath = (@"\" + noSlashes(oldName)).Replace(@"\", @"/");
                string newLogPath = (@"\" + noSlashes(newName)).Replace(@"\", @"/");
                RemoveFromLog(oldLogPath);
                UpdateTheLog(newLogPath, GetLWTof(rFullpath, newName));

                DoneSyncing();
            }
            else
            {
                CreateRemote(FullPath, cPath, nameNew, isDir);
            }
        }

        /// <summary>
        /// uploads a file to host or creates a new directory
        /// </summary>
        /// <param name="path">local path to folder or file</param>
        /// <param name="cPath">common path of the two ftpboxes</param>
        /// <param name="isDir">True in case of folder, false in case of file</param>
        private void CreateRemote(string path, string cPath, string name, bool isDir)
        {
            Syncing();
            string rFullPath = noSlashes(rPath()) + cPath;
            ftp.SetCurrentDirectory(rFullPath);
            Log.Write("&&&&&&&&&& {0} {1} {2}", rFullPath, cPath, name);

            if (isDir)
            {
                //DirectoryInfo di = new DirectoryInfo(path);
                ftp.CreateDirectory(name);
                Log.Write("????> Created Directory: {0} (remote)", path);

                if (ShowNots() && lasttip != string.Format("Folder {0} was created.", name))
                    tray.ShowBalloonTip(50, "FTPbox", string.Format("Folder {0} was created.", name), ToolTipIcon.Info);
                lasttip = string.Format("Folder {0} was created.", name);
                link = null;
            }
            else 
            {
                FileInfo fi = new FileInfo(path);
                ftp.PutFile(path, fi.Name);

                if (ShowNots() && lasttip != string.Format("File {0} was updated.", fi.Name))
                    tray.ShowBalloonTip(50, "FTPbox", string.Format("File {0} was updated.", fi.Name), ToolTipIcon.Info);
                lasttip = string.Format("File {0} was updated.", fi.Name);
                Get_Link(cPath, fi.Name);

                ftp.SetCurrentDirectory(rPath());

                string comPath = noSlashes(cPath) + @"/" + fi.Name;
                Log.Write("~~~~~~~~~~~~~> comPath: {0}", comPath);
                UpdateTheLog(comPath, GetLWTof(rFullPath, fi.Name));
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
            string rFullPath = noSlashes(rPath()) + cPath;
            ftp.SetCurrentDirectory(rFullPath);
            Log.Write("===+> Gonna delete file, name: {0} cPath {1} rFullPath {2}", name, cPath, rFullPath);

            if (isDir)
            {
                ftp.RemoveDirectory(name);

                if (ShowNots() && lasttip != string.Format("Folder {0} was deleted.", name))
                    tray.ShowBalloonTip(50, "FTPbox", string.Format("Folder {0} was deleted.", name), ToolTipIcon.Info);
                lasttip = string.Format("Folder {0} was deleted.", name);
                link = null;
            }
            else 
            {
                ftp.RemoveFile(name);

                if (ShowNots() && lasttip != string.Format("File {0} was deleted.", name))
                    tray.ShowBalloonTip(50, "FTPbox", string.Format("File {0} was deleted.", name), ToolTipIcon.Info);
                lasttip = string.Format("File {0} was deleted.", name);
                link = null;

                string locPath = noSlashes(lPath()) + @"\" + noSlashes(cPath) + @"\" + name;

                RemoveFromLog(noSlashes(cPath) + @"/" + name);

            }
            DoneSyncing();
        }

        /// <summary>
        /// change a file on remote server. Only files change.
        /// </summary>
        /// <param name="name">name of file</param>
        /// <param name="cPath">common path</param>
        private void ChangeRemote(string name, string cPath, string FullPath)
        {
            if (ftp.FileExists(name))
            {
                
                Syncing();
                string rFullPath = noSlashes(rPath()) + cPath;
                Log.Write("===+> Gonna change file, name: {0} cPath {1} FullPath {2} rFullPath {3}", name, cPath, FullPath, rFullPath);
                
                ftp.SetCurrentDirectory(rFullPath);

                ftp.RemoveFile(name);
                ftp.PutFile(FullPath, name);

                if (ShowNots() && lasttip != string.Format("File {0} was updated.", name))
                    tray.ShowBalloonTip(50, "FTPbox", string.Format("File {0} was updated.", name), ToolTipIcon.Info);
                lasttip = string.Format("File {0} was updated.", name);
                Get_Link(cPath, name);

                string comPath = noSlashes(cPath) + @"/" + name;
                Log.Write("~~~~~~~~~~~~~> comPath: {0}", comPath);
                UpdateTheLog(comPath, GetLWTof(rFullPath, name));

                Get_Link(cPath,name);

                DoneSyncing();
            }
            else
            {
                CreateRemote(FullPath, cPath, name, !PathIsFile(FullPath));
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
            tray.Icon = FTPbox.Properties.Resources.AS;
            tray.Text = "FTPbox - All files synced";
        }

        private void bAddFTP_Click(object sender, EventArgs e)
        {
            fNewFtp.ShowDialog();
        }

        private void bChangeBox_Click(object sender, EventArgs e)
        {
            newDir.ShowDialog();
            ClearLog();
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
                        tray.ShowBalloonTip(50, "FTPbox", string.Format("File {0} was updated.", rf.Name), ToolTipIcon.Info);
                        lasttip = string.Format("File {0} was updated.", rf.Name);
                    }
                }

                //UpdateLog(lfilepath);
            }

            foreach (string lf in Directory.GetFiles(lPath(), "*", SearchOption.AllDirectories))
            {
                FileInfo fi = new FileInfo(lf);

                if (!ftp.FileExists(fi.Name))
                {                    
                    ftp.PutFile(lf);
                    if (ShowNots() && lasttip != string.Format("File {0} was updated.", fi.Name))
                    {
                        tray.ShowBalloonTip(50, "FTPbox", string.Format("File {0} was uploaded.", fi.Name), ToolTipIcon.Info);
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
                    tray.ShowBalloonTip(50, "FTPbox", string.Format("File {0} was updated.", name), ToolTipIcon.Info);
                lasttip = string.Format("File {0} was updated.", name);
                //UpdateLog(path);                
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
                        tray.ShowBalloonTip(50, "FTPbox", string.Format("File {0} was updated.", name), ToolTipIcon.Info);
                    lasttip = string.Format("File {0} was updated.", name);
                    //UpdateLog(path);
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
                            tray.ShowBalloonTip(50, "FTPbox", string.Format("File {0} was updated.", name), ToolTipIcon.Info);
                        lasttip = string.Format("File {0} was updated.", name);
                        //UpdateLog(path);
                    }
                    else
                    {
                        //MessageBox.Show(rD.ToString() + Environment.NewLine + lD.ToString() + Environment.NewLine + remoteFilesDT.ToString() + Environment.NewLine + localFilesDT.ToString());
                        //MessageBox.Show(lD.CompareTo(localFilesDT).ToString());
                        ftp.SetLocalDirectory(FullLocalPath);
                        ftp.SetCurrentDirectory(FullRemPath);
                        ftp.GetFile(name, RemoveSymbols(name), false);
                        if (ShowNots() && lasttip != string.Format("File {0} was updated.", name))
                            tray.ShowBalloonTip(50, "FTPbox", string.Format("File {0} was updated.", name), ToolTipIcon.Info);
                        lasttip = string.Format("File {0} was updated.", name);
                        //UpdateLog(path);
                    }
                }
            }
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
            Process.Start(@"http://sharpmindprojects.com");
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"http://ftpbox.org");
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
            Process.Start(@"http://ftpbox.org/contribute");
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
            
            if (!noSlashes(newlink).StartsWith("http://") && !noSlashes(newlink).StartsWith("https://"))
            {
                newlink = @"http://" + newlink;
            }

            if (noSlashes(rPath()) != null)
            {
                if ((rPath() == @"/"))
                {
                    
                }
                else if (rPath().StartsWith(@"/public_html/"))
                {
                    newlink = newlink + noSlashes(rPath().Substring(@"/public_html/".Length + 1, rPath().Length - @"/public_html/".Length - 1)) + @"/";
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

            if (subpath != null && subpath != "" && noSlashes(subpath) != null && noSlashes(subpath) != "")
            {
                newlink = newlink + noSlashes(subpath) + @"/";
            }
            newlink = newlink + name;
            link = newlink.Replace(" ", "%20");
            Log.Write("-----------------> link: {0}", link);
            Get_Recent(name);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (InternetOn())
            {
                Log.Write(FTPbox.Properties.Settings.Default.nLog);
                Log.Write(FTPbox.Properties.Settings.Default.lLog);
                Log.Write(FTPbox.Properties.Settings.Default.rLog);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ClearLog();
        }

        public void ClearLog()
        {
            FTPbox.Properties.Settings.Default.nLog = "";
            FTPbox.Properties.Settings.Default.lLog = "";
            FTPbox.Properties.Settings.Default.rLog = "";
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
                if (ftp.DirectoryExists("tempfolderasdpjf"))
                    ftp.RemoveDirectory("tempfolderasdpjf");

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
            catch (Exception ex)
            {
                Log.Write(ex.Message);
                MessageBox.Show("Error creating directory. Make sure your FTP account has permissions to create directories!" + Environment.NewLine + "If your account has the permissions needed but this error keeps on showing, feel free to contact me!", "Error", MessageBoxButtons.OK);
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
                lRemoteWrk = new BackgroundWorker();
                FullList = new Dictionary<string, DateTime>();
                lRemoteWrk.WorkerSupportsCancellation = true;
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
            if (!e.Cancelled)
            {
                // Retrieve the dictionary from the result
                Dictionary<String, DateTime> fdDict = e.Result as Dictionary<String, DateTime>;

                FullList = new Dictionary<string, DateTime>();
                FullList = fdDict;

                CheckList();
            }
        }

        void lRemoteWrk_DoWork(object sender, DoWorkEventArgs e)
        {
            if (lRemoteWrk.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                try
                {
                    ftpbg.SetCurrentDirectory(rPath());
                    // Get the remote directories/files
                    Dictionary<String, DateTime> rtDict = listRemote();
                    // And our work is complete!
                    e.Result = rtDict;
                }
                catch { }
            }
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
            ftpbg.SetCurrentDirectory(rPath());

            // Loop through files
            foreach (FtpFileInfo fInfo in ftpbg.GetFiles())
            {
                if (fInfo.Name != ".ftpquota")
                {
                    // Got it? Add it!
                    fdDict.Add(noSlashes(String.Format("{0}/{1}", noSlashes(rPath()), fInfo.Name)), fInfo.LastWriteTimeUtc.Value.AddHours(timedif.Hours));
                    Log.Write("{0}/{1}", noSlashes(rPath()), fInfo.Name);
                    //CheckRemoteFile(noSlashes(rPath()), fInfo.Name, fInfo.LastWriteTimeUtc.Value);
                }
            }

            // And directories
            foreach (FtpDirectoryInfo dInfo in ftpbg.GetDirectories())
            {
                // You can't trick me!
                if (dInfo.Name != "." && dInfo.Name != "..")
                {
                    getSetDirFiles(dInfo.Name, dInfo.LastWriteTimeUtc.Value.AddHours(timedif.Hours), ref fdDict);
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
            ftpbg.SetCurrentDirectory(newPath);

            // Loop through files
            foreach (FtpFileInfo fInfo in ftpbg.GetFiles())
            {
                if (fInfo.Name != ".ftpquota")
                {
                    // Got it? Add it!
                    Log.Write("File ({0})", noSlashes(String.Format("{0}{1}", newPath, fInfo.Name)));
                    fdDict.Add(noSlashes(String.Format("{0}{1}", newPath, fInfo.Name)), fInfo.LastWriteTimeUtc.Value.AddHours(timedif.Hours));
                }
            }

            // And directories
            foreach (FtpDirectoryInfo dInfo in ftpbg.GetDirectories())
            {
                // You can't trick me!
                if (dInfo.Name != "." && dInfo.Name != "..")
                {
                    // Spawn another loop
                    getSetDirFiles(noSlashes(String.Format("{0}/{1}", path, dInfo.Name)), dInfo.LastWriteTimeUtc.Value.AddHours(timedif.Hours), ref fdDict);
                }
            }
        }

        /////////////////////
        // End of NoFate's //
        //  Contribution   //  
        /////////////////////

        public void CheckRemoteFiles(Dictionary<string, DateTime> allfilesandfolders)
        {
            if(allfilesandfolders != null)
            {
                foreach (KeyValuePair<string, DateTime> s in allfilesandfolders)
                {
                    DateTime rDT = s.Value;

                    if (s.Key.EndsWith(@"/"))
                    {
                        //means it's a folder
                        string path = lPath();
                        string remPath = s.Key;
                        if (remPath.StartsWith(rPath()))
                        {
                            remPath = remPath.Substring(rPath().Length, remPath.Length - rPath().Length);
                        }
                        path = path + @"\" + noSlashes(remPath.Replace(@"/", @"\"));
                        
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                            Log.Write("????> Created Directory: {0} (local)", path);
                            DirectoryInfo d = new DirectoryInfo(path);
                            if (ShowNots() && lasttip != string.Format("Folder {0} was created.", d.Name))
                                tray.ShowBalloonTip(50, "FTPbox", string.Format("Folder {0} was created.", d.Name), ToolTipIcon.Info);
                            lasttip = string.Format("Folder {0} was created.", d.Name);
                            link = null;
                        }
                    }
                    else
                    {
                        //means it's a file
                        int i = s.Key.LastIndexOf(@"/");
                        string cPath = s.Key.Substring(0, i);
                        string name = s.Key.Substring(i + 1, s.Key.Length - i - 1);
                        string FullRemPath = noSlashes(cPath) + @"/";
                        string FullLocalPath = noSlashes(lPath());
                        string LocalFileDirParent = lPath();

                        string fLocalPath = noSlashes(lPath());
                        
                        string comPath = cPath;

                        if (comPath.StartsWith(rPath()))
                        {
                            comPath = comPath.Substring(rPath().Length, comPath.Length - rPath().Length);
                        }

                        if (cPath == "")
                        {
                            FullLocalPath = FullLocalPath + @"\" + name;
                            fLocalPath = fLocalPath + @"\" + name;
                        }
                        else
                        {
                            FullLocalPath = FullLocalPath + @"\" + noSlashes(comPath.Replace(@"/", @"\")) + @"\" + name;
                            fLocalPath = fLocalPath + @"\" + noSlashes(comPath.Replace(@"/", @"\")) + @"\" + name;
                            //FullRemPath = noSlashes(FullRemPath) + cPath;
                            LocalFileDirParent = noSlashes(LocalFileDirParent) + @"\" + noSlashes(comPath.Replace(@"/", @"\"));
                        }
                        Log.Write("cPath {0} -> name {1} -> fullRP {2} ->Flp {3} -> comPath {4} -> LFDP {5}", cPath, name, FullRemPath, FullLocalPath, comPath, LocalFileDirParent);
                        Log.Write(FullRemPath + " " + LocalFileDirParent);

                        if (nLog() == null || nLog() == "" || !nLog().Contains(s.Key))
                        {
                            try
                            {
                                Log.Write("Log is null, gonna get {0}", name);
                                ftpbg.SetCurrentDirectory(FullRemPath);
                                ftpbg.SetLocalDirectory(LocalFileDirParent);
                                LastChangedFileFromRem = name;
                                LastChangedFolderFromRem = GetParentFolder(cPath);
                                downloading = true;
                                fswFiles.EnableRaisingEvents = false;
                                ftpbg.GetFile(name, false);
                                fswFiles.EnableRaisingEvents = true;

                                if (ShowNots() && lasttip != string.Format("File {0} was updated.", name))
                                    tray.ShowBalloonTip(50, "FTPbox", string.Format("File {0} was updated.", name), ToolTipIcon.Info);
                                lasttip = string.Format("File {0} was updated.", name);

                                UpdateTheLog(s.Key, s.Value);
                                Get_Link(comPath, name);
                                downloading = false;
                            }
                            catch (Exception ex)
                            {
                                Log.Write("[ERROR] -> " + ex.Message);
                            }
                        }
                        else
                        {
                            if (File.Exists(fLocalPath))
                            {
                                FileInfo f = new FileInfo(fLocalPath);
                                Log.Write("@&&^$ Not DEleteD");
                                CheckExistingFile(name, s.Key, FullRemPath, FullLocalPath, LocalFileDirParent, f.LastWriteTimeUtc, s.Value, comPath);
                            }
                            else
                            {
                                try
                                {
                                    Log.Write("Log not null but {0} doesnt exist!", s.Key);
                                    ftpbg.SetCurrentDirectory(FullRemPath);
                                    ftpbg.SetLocalDirectory(LocalFileDirParent);
                                    downloading = true;
                                    fswFiles.EnableRaisingEvents = false;
                                    ftpbg.GetFile(name, false);

                                    if (ShowNots() && lasttip != string.Format("File {0} was updated.", name))
                                        tray.ShowBalloonTip(50, "FTPbox", string.Format("File {0} was updated.", name), ToolTipIcon.Info);
                                    lasttip = string.Format("File {0} was updated.", name);

                                    fswFiles.EnableRaisingEvents = true;
                                    Get_Link(comPath, name);
                                    UpdateTheLog(s.Key, s.Value);
                                    downloading = false;
                                }
                                catch (Exception ex)
                                {
                                    Log.Write("[ERROR] -> " + ex.Message);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// check a file that exists both locally and on remote server, by comparing LastWriteTime values to the ones logged
        /// </summary>
        /// <param name="name">name of file</param>
        /// <param name="cPath">common path to file</param>
        /// <param name="FullRemPath">Full remote path to file's folder</param>
        /// <param name="FullLocalPath">full local path to file</param>
        /// <param name="LocalFileParentFolder">full local path to file's folder</param>
        /// <param name="lDT">current LastWriteTime of local file</param>
        /// <param name="rDT">current LastWriteTime of remote file</param>
        public void CheckExistingFile(string name, string cPath, string FullRemPath, string FullLocalPath, string LocalFileParentFolder, DateTime lDT, DateTime rDT, string comPath)
        {
            DateTime lDTlog = GetlDateTime(cPath);
            DateTime rDTlog = GetrDateTime(cPath);
            int rResult = DateTime.Compare(rDT, rDTlog);    //compare remote file's lastwritetime with the value saved in log
            int lResult = DateTime.Compare(lDT, lDTlog);    //compare local file's lastwritetime with the value saved in log
            int bResult = DateTime. Compare(rDT, lDT);       //compare remote file's lastwritetime with local's lastwritetime value

            if (rResult > 0)
            {
                try
                {
                    Log.Write("rResult > 0");
                    Log.Write("fRP: {0} -- lfPF: {1} lDT: {2} -- lDTlog: {3} -- rDT: {4} -- rDTlog {5}", FullRemPath, LocalFileParentFolder, lDT.ToString(), lDTlog.ToString(), rDT.ToString(), rDTlog.ToString());
                    ftp.SetCurrentDirectory(FullRemPath);
                    ftp.SetLocalDirectory(LocalFileParentFolder);
                    downloading = true;
                    fswFiles.EnableRaisingEvents = false;
                    ftp.GetFile(name, false);

                    if (ShowNots() && lasttip != string.Format("File {0} was updated.", name))
                        tray.ShowBalloonTip(50, "FTPbox", string.Format("File {0} was updated.", name), ToolTipIcon.Info);
                    lasttip = string.Format("File {0} was updated.", name);

                    fswFiles.EnableRaisingEvents = true;
                    Get_Link(comPath, name);
                    UpdateTheLog(cPath, rDT);
                    downloading = false;
                }
                catch (Exception ex)
                {
                    Log.Write("[ERROR] -> " + ex.Message);
                }
            }
            else
            {
                if ((lDT.ToString() != lDTlog.ToString()) && bResult < 0)
                {
                    try
                    {
                        Syncing();
                        Log.Write("(lDT.ToString() != lDTlog.ToString()) && bResult < 0");
                        Log.Write("lDT: {0} -- lDTlog: {1} -- rDT: {2} -- rDTlog: {3}", lDT.ToString(), lDTlog.ToString(), rDT.ToString(), rDTlog.ToString());
                        ftpbg.SetCurrentDirectory(FullRemPath);
                        ftpbg.PutFile(FullLocalPath, name);
                        if (ShowNots() && lasttip != string.Format("File {0} was updated.", name))
                            tray.ShowBalloonTip(50, "FTPbox", string.Format("File {0} was updated.", name), ToolTipIcon.Info);
                        lasttip = string.Format("File {0} was updated.", name);
                        UpdateTheLog(cPath, GetLWTof(FullRemPath, name));
                        DoneSyncing();
                    }
                    catch (Exception ex)
                    {
                        Log.Write("[ERROR] -> " + ex.Message);
                        DoneSyncing();
                    }
                }
                else if ((lDT.ToString() != lDTlog.ToString()) && bResult > 0)
                {
                    try
                    {
                        Log.Write("(lDT.ToString() != lDTlog.ToString()) && bResult > 0");
                        Log.Write("lDT: {0} -- lDTlog: {1} -- rDT: {2} -- rDTlog: {3}", lDT.ToString(), lDTlog.ToString(), rDT.ToString(), rDTlog.ToString());
                        ftpbg.SetCurrentDirectory(FullRemPath);
                        ftpbg.SetLocalDirectory(LocalFileParentFolder);
                        downloading = true;
                        fswFiles.EnableRaisingEvents = false;
                        ftpbg.GetFile(name, false);
                        Get_Link(comPath, name);

                        if (ShowNots() && lasttip != string.Format("File {0} was updated.", name))
                            tray.ShowBalloonTip(50, "FTPbox", string.Format("File {0} was updated.", name), ToolTipIcon.Info);
                        lasttip = string.Format("File {0} was updated.", name);

                        fswFiles.EnableRaisingEvents = true;
                        UpdateTheLog(cPath, rDT);
                        downloading = false;
                    }
                    catch (Exception ex)
                    {
                        Log.Write("[ERROR] -> " + ex.Message);
                    }
                }
                else
                {
                    Log.Write("***********");
                    Log.Write(cPath);
                    Log.Write("rDT {0} rDTlog {1} result {2}", rDT.ToString(), rDTlog.ToString(), rResult.ToString());
                    Log.Write("lDT {0} lDTlog {1} result {2}", lDT.ToString(), lDTlog.ToString(), lResult.ToString());
                    Log.Write(bResult.ToString());
                    Log.Write("***********");
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
            string comPath = cPath;
            DateTime lDTlog;

            if (comPath.StartsWith(rPath()))
            {
                comPath = comPath.Substring(rPath().Length, comPath.Length - rPath().Length);
            }
            comPath = noSlashes(comPath.Replace(@"/", @"\"));
            FullLocalPath = FullLocalPath + @"\" + comPath;

            if (cPath.EndsWith(@"/"))
            {
                name = GetParentFolder(cPath);
                //if (cPath != "")
                    //FullLocalPath = FullLocalPath + @"\" + noSlashes(cPath.Replace(@"/", @"\")) + @"\" + name;
                DirectoryInfo dInfo = new DirectoryInfo(FullLocalPath);
                lDTlog = dInfo.LastWriteTimeUtc;
            }
            else
            {
               name = cPath.Substring(cPath.LastIndexOf("/") + 1, cPath.Length - cPath.LastIndexOf("/") - 1);
               /*if (cPath != "")
               {
                   FullLocalPath = FullLocalPath + @"\" + noSlashes(cPath.Replace(@"/", @"\"));
               }*/
               FileInfo fInfo = new FileInfo(FullLocalPath);
               lDTlog = fInfo.LastWriteTimeUtc;
            }

            RemoveFromLog(cPath);
            FTPbox.Properties.Settings.Default.nLog += cPath + "|";
            FTPbox.Properties.Settings.Default.rLog += rDTlog.ToString() + "|";
            FTPbox.Properties.Settings.Default.lLog += lDTlog.ToString() + "|";
            FTPbox.Properties.Settings.Default.Save();

            Log.Write("##########");
            Log.Write("FLP {0} + name: {1} + rDTlog: {2} + lDTlog: {3} + cPath: {4}", FullLocalPath, name, rDTlog.ToString(), lDTlog.ToString(), cPath);
            Log.Write("#########");         
        }

        public void RemoveFromLog(string cPath)
        {
            if (nLog().Contains(cPath))
            {
                List<string> Namelog = new List<string>(FTPbox.Properties.Settings.Default.nLog.Split('|', '|'));
                List<string> remoteDL = new List<string>(FTPbox.Properties.Settings.Default.rLog.Split('|', '|'));
                List<string> localDL = new List<string>(FTPbox.Properties.Settings.Default.lLog.Split('|', '|'));

                while (Namelog.Contains(cPath))
                {
                    int i = Namelog.IndexOf(cPath);
                    Namelog.RemoveAt(i);
                    remoteDL.RemoveAt(i);
                    localDL.RemoveAt(i);
                }
                FTPbox.Properties.Settings.Default.nLog = "";
                FTPbox.Properties.Settings.Default.rLog = "";
                FTPbox.Properties.Settings.Default.lLog = "";
                foreach (string s in Namelog)
                {
                    FTPbox.Properties.Settings.Default.nLog += s + "|";
                }
                foreach (string s in remoteDL)
                {
                    FTPbox.Properties.Settings.Default.rLog += s + "|";
                }
                foreach (string s in localDL)
                {
                    FTPbox.Properties.Settings.Default.lLog += s + "|";
                }
                FTPbox.Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets remote lastwritetime value of a file from log
        /// </summary>
        /// <param name="cPath">common path to file</param>
        /// <returns></returns>
        DateTime GetrDateTime(string cPath)
        {
            DateTime dt;
            List<string> Namelog = new List<string>(FTPbox.Properties.Settings.Default.nLog.Split('|', '|'));
            List<string> remoteDL = new List<string>(FTPbox.Properties.Settings.Default.rLog.Split('|', '|'));

            int i = Namelog.LastIndexOf(cPath);
            dt = DateTime.Parse(remoteDL[i]);

            return dt;
        }

        /// <summary>
        /// Gets local lastwritetime value of a file from log
        /// </summary>
        /// <param name="cPath">common path to file</param>
        /// <returns></returns>
        DateTime GetlDateTime(string cPath)
        {
            DateTime dt;
            List<string> Namelog = new List<string>(FTPbox.Properties.Settings.Default.nLog.Split('|', '|'));
            List<string> localDL = new List<string>(FTPbox.Properties.Settings.Default.lLog.Split('|', '|'));

            int i = Namelog.LastIndexOf(cPath);
            dt = DateTime.Parse(localDL[i]);

            return dt;
        }

        /// <summary>
        /// Get lastwritetime of a remote fily by its path
        /// </summary>
        /// <param name="FullRemPath">path to remote file</param>
        /// <param name="name">name of remote file</param>
        /// <returns></returns>
        DateTime GetLWTof(string FullRemPath, string name)
        {
            DateTime dt = DateTime.UtcNow;
            ftp.SetCurrentDirectory(FullRemPath);
            foreach (FtpFileInfo fi in ftp.GetFiles())
            {
                if (fi.Name == name)
                {
                    dt = fi.LastWriteTimeUtc.Value.AddHours(timedif.Hours);
                }
            }
            Log.Write("========> " + dt.ToString());
            return dt;
        }

        bool PathIsFile(string path)
        {
            FileAttributes attr = File.GetAttributes(path);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                return false;
            else
                return true;
        }

        private void CheckConnection_Tick(object sender, EventArgs e)
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
            }
            catch { }
        }

        public void CheckLocal()
        {
            try
            {
                string[] alllocal = Directory.GetFiles(lPath(), "*", SearchOption.AllDirectories);
                List<string> Namelog = new List<string>(FTPbox.Properties.Settings.Default.nLog.Split('|', '|'));
                foreach (string s in alllocal)
                {
                    string cPath = GetCommonPath(s, false);
                    cPath = cPath.Substring(0, cPath.Length - 1);

                    int i = cPath.LastIndexOf(@"/");
                    string path = cPath.Substring(0, i + 1);
                    string name = cPath.Substring(i + 1, cPath.Length - i - 1);

                    if (name != ".ftpquota")
                    {
                        if (Namelog.Contains(cPath))
                        {
                            Log.Write("++++++++> {0} {1} {2} in {3}", cPath, path, name, s);
                            if (ftp.DirectoryExists(noSlashes(rPath()) + path))
                            {
                                ftp.SetCurrentDirectory(noSlashes(rPath()) + path);
                                if (!ftp.FileExists(name))
                                {
                                    File.Delete(s);
                                }
                            }
                            else
                            {
                                File.Delete(s);
                            }
                        }
                        else
                        {
                            Log.Write("--------> {0} {1} {2} in {3}", cPath, path, name, s);
                            if (!ftp.DirectoryExists(noSlashes(rPath()) + path))
                            {
                                string spath = rPath();
                                ftp.SetCurrentDirectory(rPath());
                                foreach (string p in path.Split('/', '/'))
                                {
                                    if (!ftp.DirectoryExists(p))
                                    {
                                        ftp.CreateDirectory(p);

                                        if (ShowNots() && lasttip != string.Format("Folder {0} was updated.", name))
                                            tray.ShowBalloonTip(50, "FTPbox", string.Format("Folder {0} was updated.", name), ToolTipIcon.Info);
                                        lasttip = string.Format("Folder {0} was updated.", name);
                                        link = null;

                                    }
                                    ftp.SetCurrentDirectory(noSlashes(ftp.GetCurrentDirectory()) + "/" + p);
                                }
                            }

                            ftp.SetCurrentDirectory(noSlashes(rPath()) + path);
                            if (ftp.FileExists(name))
                                ftp.RemoveFile(name);

                            ftp.PutFile(s, name);
                            if (ShowNots() && lasttip != string.Format("File {0} was updated.", name))
                                tray.ShowBalloonTip(50, "FTPbox", string.Format("File {0} was updated.", name), ToolTipIcon.Info);
                            lasttip = string.Format("File {0} was updated.", name);
                            Get_Link(path, name);
                            UpdateTheLog(cPath, GetLWTof(rPath() + path, name));
                        }
                    }
                }
            }
            catch { }
        }

        public void CheckList()
        {
            BackgroundWorker MakeChangesBGW = new BackgroundWorker();
            MakeChangesBGW.DoWork += new DoWorkEventHandler(MakeChangesBGW_DoWork);
            MakeChangesBGW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MakeChangesBGW_RunWorkerCompleted);
            MakeChangesBGW.RunWorkerAsync();
        }

        void MakeChangesBGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Thread.Sleep(5000);
            FullList = new Dictionary<string, DateTime>();
            ListAllFiles();
        }

        void MakeChangesBGW_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!OfflineMode)
            {
                CheckRemoteFiles(FullList);
                Log.Write("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");                
            }
        }

    }
}