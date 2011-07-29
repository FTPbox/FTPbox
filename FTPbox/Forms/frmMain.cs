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
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using Tamir.SharpSsh.jsch;
using Tamir.SharpSsh.jsch.examples;
using FTPbox.Classes;

namespace FTPbox
{
    public partial class frmMain : Form
    {
        Dictionary<string, DateTime> FullList = new Dictionary<string, DateTime>();

        //FTP connection
        FtpConnection ftp;
        FtpConnection ftpbg;

        //SFTP connection
        ChannelSftp sftpc;
        //ChannelSftp sftpc;

        //Form instances
        NewFTP fNewFtp;
        fNewDir newDir;

        List<List<string>> pending = new List<List<string>>();
        Dictionary<string, string> RenamedList = new Dictionary<string, string>();

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

        WebBrowser wb = new WebBrowser();

        public bool downloading = false;
        public bool listing = false;
        public bool recentlycreated = false;

        Settings AppSettings = new Settings();
        
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            //FTPbox.Properties.Settings.Default.ftpUsername = "";
            KillPrevInstances();

            //ClearLog();

            foreach (string s in nLog().Split('|', '|'))
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

            Get_Language();

            //WatchRemote.Start();
        }

        private void StartUpWork()
        {   
            CheckAccount();

            UpdateDetails();

            CheckPaths();

            UpdateDetails();

            if (AppSettings.Get("Settings/Timedif", "") == "" || AppSettings.Get("Settings/Timedif", "") == null)
            {
                GetServerTime();
            }
            else
            {
                timedif = TimeSpan.Parse(AppSettings.Get("Settings/Timedif", ""));
            }

            Syncing();

            //CheckLocal();

            ListAllFiles();

            SetLocalWatcher();

            DoneSyncing();
        }
        
        /// <summary>
        /// checks if account's information used the last time has changed
        /// </summary>
        private void CheckAccount()
        {
            try
            {
                if (FTP())
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
                else
                {
                    sftp_login();
                    this.ShowInTaskbar = false;
                    this.Hide();
                    this.ShowInTaskbar = true;
                    loggedIn = true;
                }
                
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

                if (FTP())
                {
                    Log.Write("FTP");
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
                else
                {
                    Log.Write("SFTP");
                    sftp_login();
                    this.ShowInTaskbar = false;
                    this.Hide();
                    this.ShowInTaskbar = true;
                    loggedIn = true;
                }
                newDir.ShowDialog();
            }
        }

        /// <summary>
        /// checks if paths used the last time still exist
        /// </summary>
        private void CheckPaths()
        {
            if (FTP())
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
            else
            {
                string rpath = rPath();
                if (rpath != "/")
                {
                    if (rpath.StartsWith("/"))
                        rpath = rpath.Substring(1);
                    try
                    {
                        SftpCDtoRoot();
                        Log.Write(rpath + " | " + sftpc.pwd());
                        //sftpc.cd(rpath);
                        if (!Directory.Exists(lPath()))
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
                    catch (SftpException e)
                    {
                        Log.Write(e.Message);
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
            chkStartUp.Checked = CheckStartup();

            if (FTP())
                lMode.Text = "FTP";
            else
                lMode.Text = "SFTP";

            AppSettings.Put("Settings/Startup", CheckStartup().ToString());
            //FTPbox.Properties.Settings.Default.startup = CheckStartup();

            chkShowNots.Checked = ShowNots();
            tParent.Text = ftpParent();

            lVersion.Text = Application.ProductVersion.ToString().Substring(0, 5) + @" Beta";

            //FTPbox Tab
            lRemPath.Text = rPath();
            lLocPath.Text = lPath();
            tParent.Text = ftpParent();

            if (OpenInBrowser())
                rOpenInBrowser.Checked = true;
            else
                rCopy2Clipboard.Checked = true;
        }

        #region variables

        public string ftpHost()
        {
            return AppSettings.Get("Account/Host", "");
        }

        public string ftpUser()
        {
            return AppSettings.Get("Account/Username", "");
        }

        public string ftpPass()
        {
            return AppSettings.Get("Account/Password", "");
        }

        public int ftpPort()
        {
            return AppSettings.Get("Account/Port", 21);
        }

        public string rPath()
        {
            if (FTP())
                return AppSettings.Get("Paths/rPath", "");
            else
            {
                string rpath = AppSettings.Get("Paths/rPath", "");
                if (rpath == "/")
                    rpath = "";
                else if (rpath.StartsWith("/"))
                    rpath = rpath.Substring(1);
                return rpath;
            }
        }

        public string lPath()
        {
            return AppSettings.Get("Paths/lPath", "");
        }

        bool StartOnStartup()
        {
            return bool.Parse(AppSettings.Get("Settings/Startup", "True"));
        }

        bool ShowNots()
        {
            return bool.Parse(AppSettings.Get("Settings/ShowNots", "True"));
        }

        bool OpenInBrowser()
        {
            return bool.Parse(AppSettings.Get("Settings/OpenInBrowser", "True"));
        }

        public string ftpParent()
        {
            return AppSettings.Get("Paths/Parent", ftpHost());
        }

        string nLog()
        {
            return AppSettings.Get("Log/nLog", "");
        }

        string rLog()
        {
            return AppSettings.Get("Log/rLog", "");
        }

        string lLog()
        {
            return AppSettings.Get("Log/lLog", "");
        }

        public string lang()
        {
            return AppSettings.Get("Settings/Language", "");
        }

        public bool FTP()
        {
            return bool.Parse(AppSettings.Get("Account/FTPorSFTP", "True"));
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
                    recentlycreated = true;
                    if (source == fswFiles)
                    {
                        try
                        {
                            if (FTP())
                                CreateRemote(e.FullPath, cPath, name, false);
                            else
                            {
                                List<string> ls = new List<string>();
                                ls.Add("cr");
                                ls.Add(e.FullPath);
                                ls.Add(cPath);
                                ls.Add(name);
                                ls.Add("false");
                                pending.Add(ls);
                            }
                                //SftpCreate(e.FullPath, cPath, name, false);
                        }
                        catch { DoneSyncing(); }
                    }
                    else
                    {
                        try
                        {
                            if (FTP())
                                CreateRemote(e.FullPath, cPath, name, true);
                            else
                            {
                                List<string> ls = new List<string>();
                                ls.Add("cr");
                                ls.Add(e.FullPath);
                                ls.Add(cPath);
                                ls.Add(name);
                                ls.Add("true");
                                pending.Add(ls);
                            }
                                //SftpCreate(e.FullPath, cPath, name, true);
                        }
                        catch { DoneSyncing(); }
                    }
                }
                else if (e.ChangeType == WatcherChangeTypes.Deleted)
                {
                    Log.Write("ooooooooooooooooo Deleted");
                    if (!downloading)
                    {
                        if (source == fswFiles)
                        {
                            Log.Write(">>>>>> Deleted file");
                            try
                            {
                                if (FTP())
                                    DeleteRemote(name, cPath, false);
                                else
                                {
                                    List<string> ls = new List<string>();
                                    ls.Add("d");
                                    ls.Add(name);
                                    ls.Add(cPath);
                                    ls.Add("false");
                                    pending.Add(ls);
                                }
                                    //SftpDelete(name, cPath, false);
                            }
                            catch { DoneSyncing(); }
                            RemoveFromLog(cPath  + name);
                        }
                        else
                        {
                            Log.Write(">>>>>> Deleted Folder");
                            try
                            {
                                if (FTP())
                                    DeleteRemote(name, cPath, true);
                                else
                                {
                                    List<string> ls = new List<string>();
                                    ls.Add("d");
                                    ls.Add(name);
                                    ls.Add(cPath);                                    
                                    ls.Add("True");
                                    pending.Add(ls);
                                }
                                    //SftpDelete(name, cPath, true);
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
                                if (FTP())
                                    ChangeRemote(name, cPath, e.FullPath);
                                else
                                {
                                    List<string> ls = new List<string>();
                                    ls.Add("ch");
                                    ls.Add(e.FullPath);
                                    ls.Add(name);
                                    ls.Add(cPath);
                                    pending.Add(ls);
                                }
                                    //SftpChange(e.FullPath, name, cPath);
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
                    if (FTP())
                        RenameRemote(e.OldName, e.Name, cPath, e.FullPath, false);
                    else
                    {
                        List<string> ls = new List<string>();
                        ls.Add("r");
                        ls.Add(e.OldName);
                        ls.Add(e.Name);
                        ls.Add(cPath);
                        ls.Add(e.FullPath);
                        ls.Add("false");
                        pending.Add(ls);
                        RenamedList.Add(e.OldName.Replace(@"\", @"/"), e.Name.Replace(@"\", @"/"));
                        Log.Write("Added to list: {0} - {1}", e.OldName.Replace(@"\", @"/"), e.Name.Replace(@"\", @"/"));
                        SftpRename(e.OldName, e.Name, cPath, e.FullPath, false);
                    }
                        //SftpRename(e.OldName, e.Name, cPath, e.FullPath, false);
                }
                else
                {
                    if (FTP())
                        RenameRemote(e.OldName, e.Name, cPath, e.FullPath, true);
                    else
                    {
                        List<string> ls = new List<string>();
                        ls.Add("r");
                        ls.Add(e.OldName);
                        ls.Add(e.Name);
                        ls.Add(cPath);
                        ls.Add(e.FullPath);
                        ls.Add("true");
                        pending.Add(ls);
                        RenamedList.Add(e.OldName.Replace(@"\", @"/"), e.Name.Replace(@"\", @"/"));
                        Log.Write("Added to list: {0} - {1}", e.OldName.Replace(@"\", @"/"), e.Name.Replace(@"\", @"/"));
                        SftpRename(e.OldName, e.Name, cPath, e.FullPath, true);
                    }
                        //SftpRename(e.OldName, e.Name, cPath, e.FullPath, true);
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
                    tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("renamed", true), nameOld, nameNew), ToolTipIcon.Info);
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
                    tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("created", false), name), ToolTipIcon.Info);
                lasttip = string.Format("Folder {0} was created.", name);
                link = null;
            }
            else 
            {
                FileInfo fi = new FileInfo(path);
                ftp.PutFile(path, fi.Name);

                if (ShowNots() && lasttip != string.Format("File {0} was updated.", fi.Name))
                    tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("udpated", true), fi.Name), ToolTipIcon.Info);
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
                    tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("deleted", false), name), ToolTipIcon.Info);
                lasttip = string.Format("Folder {0} was deleted.", name);
                link = null;
            }
            else 
            {
                ftp.RemoveFile(name);

                if (ShowNots() && lasttip != string.Format("File {0} was deleted.", name))
                    tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("deleted", true), name), ToolTipIcon.Info);
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
                    tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("updated", true), name), ToolTipIcon.Info);
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
            if (lang() == "es")
            {
                tray.Text = "FTPbox - Sincronizando";
            }
            else if (lang() == "de")
            {
                tray.Text = "FTPbox - Aktuallisieren...";
            }
            else
            {
                tray.Text = "FTPbox - Syncing...";
            }
        }

        /// <summary>
        /// called when syncing ends
        /// </summary>
        private void DoneSyncing()
        {
            fswFiles.EnableRaisingEvents = true;
            fswFolders.EnableRaisingEvents = true;
            tray.Icon = FTPbox.Properties.Resources.AS;
            if (lang() == "es")
            {
                tray.Text = "FTPbox - Todos los archivos sincronizados";
            }
            else if (lang() == "de")
            {
                tray.Text = "FTPbox - Alle Daten sind aktuell";
            }
            else
            {
                tray.Text = "FTPbox - All files synced";
            }
            downloading = false;
            
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
            AppSettings.Put("Settings/OpenInBrowser", rOpenInBrowser.Checked.ToString());
            //FTPbox.Properties.Settings.Default.openinbrowser = rOpenInBrowser.Checked;
            //FTPbox.Properties.Settings.Default.Save();
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
                    
                    LinkCopied();
                }
            }
        }

        private void LinkCopied()
        {
            if (ShowNots())
            {
                if (lang() == "es")
                {
                    tray.ShowBalloonTip(30, "FTPbox", "Vinculo copiado al portapapeles.", ToolTipIcon.Info);
                }
                else if (lang() == "de")
                {
                    tray.ShowBalloonTip(30, "FTPbox", "Link wurde in die Zwischenablage koppiert", ToolTipIcon.Info);
                }
                else
                {
                    tray.ShowBalloonTip(30, "FTPbox", "Link copied to clipboard", ToolTipIcon.Info);
                }
                link = null;
            }
        }

        private void tParent_TextChanged(object sender, EventArgs e)
        {
            AppSettings.Put("Paths/Parent", tParent.Text);
            //FTPbox.Properties.Settings.Default.ftpParent = tParent.Text;
            //FTPbox.Properties.Settings.Default.Save();
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
                        tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("updated", true), rf.Name), ToolTipIcon.Info);
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
                        tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("updated", true), fi.Name), ToolTipIcon.Info);
                        lasttip = string.Format("File {0} was updated.", fi.Name);
                    }
                }
            }
        } //not used

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
                    tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("updated", true), name), ToolTipIcon.Info);
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
                        tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("updated", true), name), ToolTipIcon.Info);
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
                            tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("updated", true), name), ToolTipIcon.Info);
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
                            tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("updated", true), name), ToolTipIcon.Info);
                        lasttip = string.Format("File {0} was updated.", name);
                        //UpdateLog(path);
                    }
                }
            }
        } //not used

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

            AppSettings.Put("Settings/Startup", chkStartUp.Checked.ToString());
            //FTPbox.Properties.Settings.Default.startup = chkStartUp.Checked;
            //FTPbox.Properties.Settings.Default.Save();
        }

        private void chkShowNots_CheckedChanged(object sender, EventArgs e)
        {
            AppSettings.Put("Settings/ShowNots", chkShowNots.Checked.ToString());
            //FTPbox.Properties.Settings.Default.shownots = chkShowNots.Checked;
            //FTPbox.Properties.Settings.Default.Save();
        }

        /// <summary>
        /// run FTPbox on windows startup
        /// <param name="enable"><c>true</c> to add it to system startup, <c>false</c> to remove it</param>
        /// </summary>
        private void SetStartup(bool enable)
        {
            string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            Microsoft.Win32.RegistryKey startupKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(runKey);

            if (enable)
            {
                if (startupKey.GetValue("FTPbox") == null)
                {
                    startupKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(runKey, true);
                    startupKey.SetValue("FTPbox", Application.ExecutablePath.ToString());
                    startupKey.Close();
                }
            }
            else
            {
                // remove startup
                startupKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(runKey, true);
                startupKey.DeleteValue("FTPbox", false);
                startupKey.Close();
            }
        }

        private bool CheckStartup()
        {
            string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            Microsoft.Win32.RegistryKey startupKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(runKey);

            if (startupKey.GetValue("MyApp") == null)
            {
                return false;
            }
            else
            {
                return true;
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
                browser.Navigate(@"http://ftpbox.org/latestversion.txt");
            }
            catch { }
        }

        private void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                string version = browser.Document.Body.InnerText;

                if (version != Application.ProductVersion)
                {
                    newversion nvform = new newversion(version);
                    nvform.ShowDialog();
                    this.Show();
                    // show dialog box for  download now, learn more and remind me next time
                }
            }
            catch
            {
                Log.Write("Server down");
            }
        }

        #endregion

        #region About Tab
        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"http://ftpbox.org/about");
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
            if (!ExitedFromTray && e.CloseReason != CloseReason.WindowsShutDown)
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
            tabControl1.SelectedTab = tabAbout;
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
            /*
            if (noSlashes(rPath()) != null)
            {
                if ((rPath() == @"/"))
                {
                    //do nothing...
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
            */
            string spath = subpath;
            if (spath == "/")
                spath = "";
            else if (spath.StartsWith("/"))
                spath = spath.Substring(1);

            if (spath != null && spath != "" && noSlashes(spath) != null && noSlashes(spath) != "")
            {
                newlink = newlink + noSlashes(spath) + @"/";
            }
            newlink = newlink + name;
            link = newlink.Replace(" ", "%20");
            Log.Write("-----------------> link: {0}", link);
            Get_Recent(name);
        }

        public void ClearLog()
        {
            AppSettings.Put("Log/nLog", "");
            AppSettings.Put("Log/rLog", "");
            AppSettings.Put("Log/lLog", "");
            //FTPbox.Properties.Settings.Default.nLog = "";
            //FTPbox.Properties.Settings.Default.lLog = "";
            //FTPbox.Properties.Settings.Default.rLog = "";
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
                        LinkCopied();
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
                        LinkCopied();
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
                        LinkCopied();
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
                        LinkCopied();
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
                        LinkCopied();
                    }
                    catch { }
                } 
            }
        }

        #endregion

        public bool addorremove;
        public void GetServerTime()
        {
            try
            {
                if (FTP())
                {
                    ftp.SetCurrentDirectory(rPath());

                    foreach (FtpDirectoryInfo dir in ftp.GetDirectories())
                    {
                        if (dir.Name == "public_html")
                        {
                            ftp.SetCurrentDirectory(noSlashes(rPath()) + @"/public_html");
                        }
                    }
                    string fname = "tempfolder" + RandomString(4);

                    ftp.CreateDirectory(fname);
                    DateTime now = DateTime.UtcNow;
                    foreach (FtpDirectoryInfo f in ftp.GetDirectories())
                    {
                        if (f.Name == fname)
                        {
                            DateTime rnow = f.LastWriteTimeUtc.Value;
                            TimeSpan x = now - rnow;
                            timedif = x;

                            AppSettings.Put("Settings/Timedif", timedif.ToString());
                            //FTPbox.Properties.Settings.Default.timedif = timedif.ToString();
                            //FTPbox.Properties.Settings.Default.Save();
                        }
                    }
                    Log.Write("Created");
                    try
                    {
                        ftp.RemoveDirectory(fname);
                    }
                    catch
                    {
                        MessageBox.Show("Seems like your FTP account doesn't have permissions to delete." + Environment.NewLine + "You can still use FTPbox, but it won't be able to delete any files."
                            + Environment.NewLine + "Also, you'll have to delete the directory made manually.", "FTPbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    SftpCDtoRoot();

                    string fname = "tempfolder" + RandomString(4);

                    foreach (ChannelSftp.LsEntry lse in sftpc.ls("."))
                    {
                        if (lse.getFilename() == "public_html")
                        {
                            sftpc.cd("public_html");
                        }
                    }
                    
                    sftpc.mkdir(fname);
                    DateTime now = DateTime.UtcNow;
                    DateTime rnow;
                    try
                    {
                        SftpATTRS attrs = sftpc.stat(fname);
                        rnow = attrs.getMtimeString();
                        Log.Write("Remote time: {0} - Utc Time: {1}", rnow.ToString(), DateTime.UtcNow.ToString());
                    }
                    catch
                    {
                        Log.Write("gonna use utcnow");
                        rnow = DateTime.UtcNow;
                    }

                    int result = DateTime.Compare(now, rnow);

                    if (result > 0)
                    {
                        addorremove = true;
                        timedif = now - rnow;
                        Log.Write("now > rnow");
                    }
                    else
                    {
                        addorremove = false;
                        timedif = now - rnow;
                        Log.Write("rnow > now");
                    }

                    Log.Write("Timedif.TotalSeconds: {0}", timedif.TotalSeconds);
                    AppSettings.Put("Settings/Timedif", timedif.ToString());
                    //FTPbox.Properties.Settings.Default.timedif = timedif.ToString();
                    //FTPbox.Properties.Settings.Default.Save();
                    try
                    {
                        sftpc.rmdir(fname);
                    }
                    catch
                    {
                        MessageBox.Show("Seems like your FTP account doesn't have permissions to delete." + Environment.NewLine + "You can still use FTPbox, but it won't be able to delete any files."
                            + Environment.NewLine + "Also, you'll have to delete the directory made manually.", "FTPbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
                MessageBox.Show("Error creating directory. Make sure your FTP account has permissions to create directories! " + 
                    "If your account has the permissions needed but this error keeps on showing, feel free to contact me!", "Error", MessageBoxButtons.OK);
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
                listing = true;
                lRemoteWrk.RunWorkerAsync();
                //FullList = new Dictionary<string, DateTime>();
                //FullList = listRemote();

            }
            catch 
            {
                Log.Write("Could not list remote files");
            }
        }

        void lRemoteWrk_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {           
            if (!e.Cancelled)
            {
                listing = false;
                // Retrieve the dictionary from the result
                //Thread.Sleep(10000);
                Dictionary<String, DateTime> fdDict = e.Result as Dictionary<String, DateTime>;

                FullList = new Dictionary<string, DateTime>();
                FullList = fdDict;

                if (FTP())
                    CheckList();
                else
                {
                    //CheckPending();
                    CheckLocal();
                    ListAllFiles();
                }
                
                //Log.Write("GONNA LIST LOCAL FILES NOW");
                //CheckLocal();
            }
            else
            {
                Log.Write("!!! lRemWrk Cancelled !!!");
                //ListAllFiles();
            }
        }

        void lRemoteWrk_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!downloading)
            {
                if (lRemoteWrk.CancellationPending)
                {
                    e.Cancel = true;
                }
                else
                {
                    try
                    {
                        if (FTP())
                            ftpbg.SetCurrentDirectory(rPath());
                        else
                        {
                            //SftpCDtoRoot();
                            Log.Write("*SFTPbg PWD: ");// + sftpc.pwd());
                        }
                        // Get the remote directories/files
                        try
                        {
                            Dictionary<String, DateTime> rtDict = listRemote();
                            e.Result = rtDict;
                        }
                        catch (SftpException ex)
                        {
                            Log.Write("[Error listing]: {0}", ex.Message);
                            sftpc.quit();
                            sftp_login();
                            SftpCDtoRoot();
                            lRemoteWrk.CancelAsync();
                        }
                        // And our work is complete!

                    }
                    catch { }
                }
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
            if (FTP())
                ftpbg.SetCurrentDirectory(rPath());

            if (FTP())
            {
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
            }
            else
            {
                //SftpCDtoRoot();
                ArrayList vv;
                try
                {
                    vv = sftpc.ls(".");
                }
                catch (SftpException ex)
                {
                    Log.Write("error");
                    Log.Write(ex.Message);
                    vv = null;
                    sftpc.quit();
                    sftp_login();
                    SftpCDtoRoot();
                    lRemoteWrk.CancelAsync();
                }
                foreach (ChannelSftp.LsEntry lse in vv)
                {
                    Log.Write("Found: {0}", lse.getFilename());
                    SftpATTRS attrs = lse.getAttrs();
                    if (attrs.isDir())
                    {
                        // Loop through directories
                        if (lse.getFilename() != "." && lse.getFilename() != "..")
                        {
                            getSetDirFilesSFTP(lse.getFilename(), attrs.getMtimeString().AddSeconds(timedif.TotalSeconds), ref fdDict);
                        }
                    }
                    else
                    {
                        // Loop through files
                        if (lse.getFilename() != ".ftpquota")
                        {
                            // Got it? Add it!
                            fdDict.Add(noSlashes(String.Format("{0}/{1}", noSlashes(rPath()), lse.getFilename())), attrs.getMtimeString().AddSeconds(timedif.TotalSeconds));
                            Log.Write("sftp {0}/{1}", noSlashes(rPath()), lse.getFilename());
                            //CheckRemoteFile(noSlashes(rPath()), fInfo.Name, fInfo.LastWriteTimeUtc.Value);
                            CheckRemSftpFiles(lse.getFilename(), "", attrs.getMtimeString().AddSeconds(timedif.TotalSeconds), false);
                        }
                        
                    }
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
                    Log.Write("*SFTP* File ({0})", noSlashes(String.Format("{0}{1}", newPath, fInfo.Name)));
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

        private void getSetDirFilesSFTP(string path, DateTime lastWriteTime, ref Dictionary<String, DateTime> fdDict)
        {
            if (!downloading)
            {
                String newPath = noSlashes(String.Format("{0}/{1}", noSlashes(rPath()), path)) + @"/";
                fdDict.Add(newPath, lastWriteTime);

                if (newPath.Equals("/"))
                    newPath = ".";
                else if (newPath.StartsWith("/"))
                    newPath = newPath.Substring(1);

                CheckRemSftpFiles(path, "", lastWriteTime, true);

                Log.Write("*SFTP* Drct ({0}) in path: ({1}) LWT: {2}", newPath, path, lastWriteTime.ToString());

                ArrayList vv = sftpc.ls(path);
                foreach (ChannelSftp.LsEntry lse in vv)
                {
                    SftpATTRS attrs = lse.getAttrs();
                    if (attrs.isDir())
                    {
                        if (lse.getFilename() != "." && lse.getFilename() != "..")
                        {
                            getSetDirFilesSFTP(noSlashes(String.Format("{0}/{1}", path, lse.getFilename())), attrs.getMtimeString().AddSeconds(timedif.TotalSeconds), ref fdDict);
                        }
                    }
                    else
                    {
                        Log.Write("*SFTP* File ({0})", noSlashes(String.Format("{0}{1}", newPath, lse.getFilename())));
                        fdDict.Add(noSlashes(String.Format("{0}{1}", newPath, lse.getFilename())), attrs.getMtimeString().AddSeconds(timedif.TotalSeconds));
                        CheckRemSftpFiles(lse.getFilename(), path, attrs.getMtimeString().AddSeconds(timedif.TotalSeconds), false);
                    }
                }
            }
        }

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
                                tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("created", false), d.Name), ToolTipIcon.Info);
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
                                Syncing();
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
                                    tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("updated", true), name), ToolTipIcon.Info);
                                lasttip = string.Format("File {0} was updated.", name);

                                UpdateTheLog(s.Key, s.Value);
                                Get_Link(comPath, name);
                                downloading = false;
                                DoneSyncing();
                            }
                            catch (Exception ex)
                            {
                                DoneSyncing();
                                Log.Write("[ERROR] -> " + ex.Message);
                            }
                        }
                        else
                        {
                            if (File.Exists(fLocalPath))
                            {
                                FileInfo f = new FileInfo(fLocalPath);
                                CheckExistingFile(name, s.Key, FullRemPath, FullLocalPath, LocalFileDirParent, f.LastWriteTimeUtc, s.Value, comPath);
                            }
                            else
                            {
                                try
                                {
                                    Syncing();
                                    Log.Write("Log not null but {0} doesnt exist!", s.Key);
                                    ftpbg.SetCurrentDirectory(FullRemPath);
                                    ftpbg.SetLocalDirectory(LocalFileDirParent);
                                    downloading = true;
                                    fswFiles.EnableRaisingEvents = false;
                                    ftpbg.GetFile(name, false);

                                    if (ShowNots() && lasttip != string.Format("File {0} was updated.", name))
                                        tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("updated", true), name), ToolTipIcon.Info);
                                    lasttip = string.Format("File {0} was updated.", name);

                                    fswFiles.EnableRaisingEvents = true;
                                    Get_Link(comPath, name);
                                    UpdateTheLog(s.Key, s.Value);
                                    downloading = false;
                                    DoneSyncing();
                                }
                                catch (Exception ex)
                                {
                                    DoneSyncing();
                                    Log.Write("[ERROR] -> " + ex.Message);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void CheckRemSftpFiles(string name, string fRemPath, DateTime lastwritetime, bool isDir)
        {
            if (!downloading)
            {
                List<string> Namelog = new List<string>(nLog().Split('|', '|'));

                string comPath = noSlashes(fRemPath) + "/" + name;

                if (comPath.StartsWith("/"))
                    comPath = comPath.Substring(1);

                string LocalFileDirParent = lPath();
                LocalFileDirParent = LocalFileDirParent + @"\" + noSlashes(fRemPath.Replace("/", @"\"));
                string lFullPath = LocalFileDirParent + @"\" + name;

                Log.Write("*CHECKING*: comPath: {0}, name: {1}, fRemPath: {2} LWT: {3}", comPath, name, fRemPath, lastwritetime.ToString());

                if (isDir)
                {
                    //means it's a folder
                    string path = lPath();
                    string remPath = fRemPath;
                    if (remPath.StartsWith(rPath()))
                    {
                        remPath = remPath.Substring(rPath().Length, remPath.Length - rPath().Length);
                    }

                    Log.Write("path: {0} | remPath: {1}", path, remPath);
                    path = path + @"\" + name.Replace(@"/", @"\"); //noSlashes(remPath.Replace(@"/", @"\"));

                    Log.Write("path: {0}", path);

                    if (!Directory.Exists(path))
                    {
                        if (Namelog.Contains(comPath))
                        {
                            int i = name.LastIndexOf("/");
                            string cPath = comPath.Substring(0, i + 1);
                            string thename = name.Substring(i + 1, name.Length - i - 1);
                            Log.Write("*SFTP* Gonna delete remote file, name: {0} cPath: {1}", thename, cPath);
                            SftpDelete(thename, cPath, true);
                            RemoveFromLog(comPath);
                        }
                        else
                        {
                            fswFolders.EnableRaisingEvents = false;
                            Directory.CreateDirectory(path);
                            Log.Write("????> Created Directory: {0} (local)", path);
                            DirectoryInfo d = new DirectoryInfo(path);
                            if (ShowNots() && lasttip != string.Format("Folder {0} was created.", d.Name))
                                tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("created", false), d.Name), ToolTipIcon.Info);
                            lasttip = string.Format("Folder {0} was created.", d.Name);
                            link = null;
                            fswFolders.EnableRaisingEvents = true;
                            UpdateTheLog(comPath, lastwritetime);
                        }
                    }
                }
                else
                {
                    //means it's a file
                    if (nLog() == null || nLog() == "" || !Namelog.Contains(comPath))
                    {
                        try
                        {
                            Syncing();
                            Log.Write("(SFTP) Log is null, gonna get {0}", name);
                            Log.Write("(SFTP) LocalFileDirParent: {0}", LocalFileDirParent);
                            sftpc.lcd(LocalFileDirParent);
                            Log.Write("LFDP: {0} check", LocalFileDirParent);
                            LastChangedFileFromRem = name;
                            LastChangedFolderFromRem = ""; // GetParentFolder(fRemPath);
                            downloading = true;
                            fswFiles.EnableRaisingEvents = false;

                            Log.Write("*SFTP* Downloading...");
                            SftpDownloadFile(comPath);
                            Log.Write("*SFTP* Downloaded...");

                            fswFiles.EnableRaisingEvents = true;

                            if (RenamedList.ContainsKey(comPath))
                            {
                                string nameOld = comPath;
                                string nameNew;
                                RenamedList.TryGetValue(comPath, out nameNew);

                                if (ShowNots())
                                    tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("renamed", true), nameOld, nameNew), ToolTipIcon.Info);
                                Get_Link("", nameNew);

                                RemoveFromLog(nameOld);
                                UpdateTheLog(nameNew, DateTime.UtcNow);

                                DoneSyncing();
                                RenamedList.Remove(nameOld);
                                RemoveFromLog(nameOld);
                            }
                            else
                            {
                                if (ShowNots())
                                    tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("updated", true), name), ToolTipIcon.Info);

                                UpdateTheLog(comPath, lastwritetime);
                                Get_Link(fRemPath, name);
                                downloading = false;
                                DoneSyncing();
                            }
                        }
                        catch (Exception ex)
                        {
                            DoneSyncing();
                            Log.Write("[ERROR] -> " + ex.Message);
                        }
                    }
                    else
                    {
                        if (File.Exists(lFullPath))
                        {
                            FileInfo f = new FileInfo(lFullPath);
                            Log.Write("Shit exists: {0}", f.FullName);
                            CheckExistingFile(name, comPath, fRemPath, lFullPath, LocalFileDirParent, f.LastWriteTimeUtc, lastwritetime, comPath);
                        }
                        else
                        {
                            if (Namelog.Contains(comPath))
                            {
                                string cPath = comPath.Substring(0, comPath.LastIndexOf(name));
                                Log.Write("*SFTP* Gonna delete remote file, name: {0} cPath: {1}", name, cPath);
                               SftpDelete(name, cPath, false);
                             
                            }
                            else
                            {
                                try
                                {
                                    Syncing();
                                    Log.Write("Log not null but {0} doesnt exist!", comPath);
                                    sftpc.lcd(LocalFileDirParent);
                                    downloading = true;
                                    fswFiles.EnableRaisingEvents = false;
                                    SftpDownloadFile(comPath);

                                    if (RenamedList.ContainsKey(comPath))
                                    {
                                        string nameOld = comPath;
                                        string nameNew;
                                        RenamedList.TryGetValue(comPath, out nameNew);

                                        if (ShowNots())
                                            tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("renamed", true), nameOld, nameNew), ToolTipIcon.Info);
                                        Get_Link("", nameNew);

                                        RemoveFromLog(nameOld);
                                        UpdateTheLog(nameNew, DateTime.UtcNow);

                                        DoneSyncing();
                                        RenamedList.Remove(nameOld);
                                        RemoveFromLog(nameOld);
                                    }
                                    else
                                    {
                                        if (ShowNots())
                                            tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("updated", true), name), ToolTipIcon.Info);

                                        fswFiles.EnableRaisingEvents = true;
                                        Get_Link(fRemPath, name);
                                        UpdateTheLog(comPath, lastwritetime);
                                        downloading = false;
                                        DoneSyncing();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    DoneSyncing();
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
            string rPathToCD = cPath;
            if (rPathToCD == "/")
            {
                rPathToCD = ".";
            }
            else if (rPathToCD.StartsWith("/"))
            {
                rPathToCD = rPathToCD.Substring(1);
            }

            TimeSpan dif = rDT - rDTlog;
            if (rResult > 0 && dif.TotalSeconds > 1)
            {                
                if (dif.TotalSeconds > 1)
                {
                    Log.Write("Total Milliseconds of difference: {0} Seconds: {1}", dif.TotalMilliseconds.ToString(), dif.TotalSeconds.ToString());
                    try
                    {
                        Syncing();
                        Log.Write("rResult > 0");
                        Log.Write("fRP: {0} -- lfPF: {1} lDT: {2} -- lDTlog: {3} -- rDT: {4} -- rDTlog {5}", FullRemPath, LocalFileParentFolder, lDT.ToString(),
                            lDTlog.ToString(), rDT.ToString(), rDTlog.ToString());
                        if (FTP())
                        {
                            ftpbg.SetCurrentDirectory(FullRemPath);
                            ftpbg.SetLocalDirectory(LocalFileParentFolder);
                            downloading = true;
                            fswFiles.EnableRaisingEvents = false;
                            ftpbg.GetFile(name, false);
                        }
                        else
                        {
                            string dlPath = noSlashes(FullRemPath) + "/" + name;
                            if (dlPath.StartsWith("/"))
                                dlPath = dlPath.Substring(1);
                            SftpCDtoRoot();
                            //sftpc.cd(FullRemPath);
                            sftpc.lcd(LocalFileParentFolder);
                            downloading = true;
                            fswFiles.EnableRaisingEvents = false;
                            SftpDownloadFile(dlPath);
                        }

                        if (RenamedList.ContainsKey(comPath))
                        {
                            string nameOld = comPath;
                            string nameNew;
                            RenamedList.TryGetValue(comPath, out nameNew);

                            if (ShowNots())
                                tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("renamed", true), nameOld, nameNew), ToolTipIcon.Info);
                            Get_Link("", nameNew);

                            RemoveFromLog(nameOld);
                            UpdateTheLog(nameNew, DateTime.UtcNow);

                            DoneSyncing();
                            RenamedList.Remove(nameOld);
                            RemoveFromLog(nameOld);
                        }
                        else
                        {
                            if (ShowNots())
                                tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("updated", true), name), ToolTipIcon.Info);

                            fswFiles.EnableRaisingEvents = true;
                            Get_Link(comPath, name);
                            UpdateTheLog(cPath, rDT);
                            downloading = false;
                            DoneSyncing();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write("[ERROR] -> " + ex.Message);
                        DoneSyncing();
                    }
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
                        if (FTP())
                        {
                            ftpbg.SetCurrentDirectory(FullRemPath);
                            ftpbg.PutFile(FullLocalPath, name);
                        }
                        else
                        {
                            SftpCDtoRoot();
                            string ulPath = noSlashes(FullRemPath) + "/" + name;
                            if (ulPath.StartsWith("/"))
                                ulPath = ulPath.Substring(1);
                            //sftpc.cd(FullRemPath);
                            //SftpAppend(FullLocalPath, ulPath);
                            SftpProgressMonitor monitor = new MyProgressMonitor();
                            sftpc.put(FullLocalPath, rPathToCD, monitor, ChannelSftp.OVERWRITE);
                        }

                        if (RenamedList.ContainsKey(comPath))
                        {
                            string nameOld = comPath;
                            string nameNew;
                            RenamedList.TryGetValue(comPath, out nameNew);

                            if (ShowNots())
                                tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("renamed", true), nameOld, nameNew), ToolTipIcon.Info);
                            Get_Link("", nameNew);

                            RemoveFromLog(nameOld);
                            UpdateTheLog(nameNew, DateTime.UtcNow);

                            DoneSyncing();
                            RenamedList.Remove(nameOld);
                            RemoveFromLog(nameOld);
                        }
                        else
                        {
                            if (ShowNots() && lasttip != string.Format("File {0} was updated.", name))
                                tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("updated", true), name), ToolTipIcon.Info);
                            lasttip = string.Format("File {0} was updated.", name);
                            UpdateTheLog(cPath, GetLWTof(FullRemPath, name));
                            DoneSyncing();
                        }

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
                        Syncing();
                        Log.Write("(lDT.ToString() != lDTlog.ToString()) && bResult > 0");
                        Log.Write("lDT: {0} -- lDTlog: {1} -- rDT: {2} -- rDTlog: {3}", lDT.ToString(), lDTlog.ToString(), rDT.ToString(), rDTlog.ToString());
                        Log.Write("Timedif: {0}", timedif.Hours.ToString());

                        if (FTP())
                        {
                            ftpbg.SetCurrentDirectory(FullRemPath);
                            ftpbg.SetLocalDirectory(LocalFileParentFolder);
                            downloading = true;
                            fswFiles.EnableRaisingEvents = false;
                            ftpbg.GetFile(name, false);
                            fswFiles.EnableRaisingEvents = true;
                        }
                        else
                        {
                            SftpCDtoRoot();

                            string dlPath = noSlashes(FullRemPath) + "/" + name;
                            if (dlPath.StartsWith("/"))
                                dlPath = dlPath.Substring(1);

                            //sftpc.cd(FullRemPath);
                            sftpc.lcd(LocalFileParentFolder);
                            downloading = true;
                            fswFiles.EnableRaisingEvents = true;
                            SftpDownloadFile(dlPath);
                        }


                        Get_Link(comPath, name);

                        if (RenamedList.ContainsKey(comPath))
                        {
                            string nameOld = comPath;
                            string nameNew;
                            RenamedList.TryGetValue(comPath, out nameNew);                         

                            if (ShowNots())
                                tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("renamed", true), nameOld, nameNew), ToolTipIcon.Info);
                            Get_Link("", nameNew);

                            RemoveFromLog(nameOld);
                            UpdateTheLog(nameNew, DateTime.UtcNow);

                            DoneSyncing();
                            RenamedList.Remove(nameOld);
                            RemoveFromLog(nameOld);
                        }
                        else
                        {
                            if (ShowNots())
                                tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("updated", true), name), ToolTipIcon.Info);

                            fswFiles.EnableRaisingEvents = true;
                            UpdateTheLog(cPath, SftpGetLastWriteTime(cPath));// DateTime.UtcNow);
                            downloading = false;
                            DoneSyncing();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write("[ERROR] -> " + ex.Message);
                        DoneSyncing();
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
            AppSettings.Put("Log/nLog", nLog() + cPath + "|");
            AppSettings.Put("Log/rLog", rLog() + rDTlog.ToString() + "|");
            AppSettings.Put("Log/lLog", lLog() + lDTlog.ToString() + "|");
            //FTPbox.Properties.Settings.Default.nLog += cPath + "|";
            //FTPbox.Properties.Settings.Default.rLog += rDTlog.ToString() + "|";
            //FTPbox.Properties.Settings.Default.lLog += lDTlog.ToString() + "|";
            //FTPbox.Properties.Settings.Default.Save();

            Log.Write("##########");
            Log.Write("FLP {0} + name: {1} + rDTlog: {2} + lDTlog: {3} + cPath: {4}", FullLocalPath, name, rDTlog.ToString(), lDTlog.ToString(), cPath);
            Log.Write("#########");         
        }

        /// <summary>
        /// removes an item from the log
        /// </summary>
        /// <param name="cPath">name to remove</param>
        public void RemoveFromLog(string cPath)
        {
            if (nLog().Contains(cPath))
            {
                List<string> Namelog = new List<string>(nLog().Split('|', '|'));
                List<string> remoteDL = new List<string>(rLog().Split('|', '|'));
                List<string> localDL = new List<string>(lLog().Split('|', '|'));

                while (Namelog.Contains(cPath))
                {
                    int i = Namelog.IndexOf(cPath);
                    Namelog.RemoveAt(i);
                    remoteDL.RemoveAt(i);
                    localDL.RemoveAt(i);
                }
                ClearLog();

                foreach (string s in Namelog)
                {
                    AppSettings.Put("Log/nLog", nLog() + s + "|");
                    //FTPbox.Properties.Settings.Default.nLog += s + "|";
                }
                foreach (string s in remoteDL)
                {
                    AppSettings.Put("Log/rLog", rLog() + s + "|");
                    //FTPbox.Properties.Settings.Default.rLog += s + "|";
                }
                foreach (string s in localDL)
                {
                    AppSettings.Put("Log/lLog", lLog() + s + "|");
                    //FTPbox.Properties.Settings.Default.lLog += s + "|";
                }
                //FTPbox.Properties.Settings.Default.Save();
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
            List<string> Namelog = new List<string>(nLog().Split('|', '|'));
            List<string> remoteDL = new List<string>(rLog().Split('|', '|'));

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
            List<string> Namelog = new List<string>(nLog().Split('|', '|'));
            List<string> localDL = new List<string>(lLog().Split('|', '|'));

            int i = Namelog.LastIndexOf(cPath);
            dt = DateTime.Parse(localDL[i]);

            return dt;
        }

        /// <summary>
        /// Get lastwritetime of a remote file by its path (for FTP)
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
                    if (lang() == "es")
                    {
                        tray.Text = "FTPbox - Sin conexión";
                    }
                    else if (lang() == "de")
                    {
                        tray.Text = "FTPbox - Offline";
                    }
                    else
                    {
                        tray.Text = "FTPbox - Offline";
                    }
                }
            }
            catch { }
        }

        public void CheckLocal()
        {
            Log.Write("$$$$$$$$$$$$ Checkin Local $$$$$$$$$$$$");
            try
            {
                List<string> alllocal = new List<string>(Directory.GetDirectories(lPath(), "*", SearchOption.AllDirectories));
                alllocal.AddRange(Directory.GetFiles(lPath(), "*", SearchOption.AllDirectories));
                
                List<string> Namelog = new List<string>(nLog().Split('|', '|'));
                
                foreach (string s in alllocal)
                {
                    Log.Write("Checking local: {0}", s);
                    bool isDir;
                    FileAttributes attr = File.GetAttributes(s);
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        isDir = true;
                    else
                        isDir = false;

                    if (!recentlycreated)
                    {
                        string cPath = GetCommonPath(s, false);
                        cPath = cPath.Substring(0, cPath.Length - 1);

                        int i = cPath.LastIndexOf(@"/");
                        string path = cPath.Substring(0, i + 1);
                        string name = cPath.Substring(i + 1, cPath.Length - i - 1);

                        string rPathToCD = path;
                        if (!FTP() && path == "/")
                        {
                            path = "";
                            rPathToCD = ".";
                        }
                        else if (!FTP() && path.StartsWith("/"))
                        {
                            path = path.Substring(1);
                            rPathToCD = rPathToCD.Substring(1);
                        }

                        if (!FTP() && cPath.StartsWith("/"))
                            cPath = cPath.Substring(1);

                        string rempath = rPath();

                        if (rempath.Equals("/"))
                            rempath = "";
                        else if (rempath.StartsWith("/"))
                            rempath = rempath.Substring(1);

                        string compath = noSlashes(rempath) + "/" + cPath;

                        if (isDir)
                            compath = compath + "/";

                        if (name != ".ftpquota")
                        {
                            if (Namelog.Contains(cPath))
                            {
                                Log.Write("++++++++> {0} {1} {2} in {3}", compath, path, name, s);
                                if (FTP())
                                {
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
                                    string rpath = rPath();
                                    if (rpath == "/")
                                        rpath = path + name;
                                    else
                                    {
                                        if (rpath.StartsWith("/"))
                                            rpath = rpath.Substring(1);
                                        rpath = noSlashes(rpath) + "/" + path + name;
                                    }
                                    if (isDir && !rpath.EndsWith("/"))
                                        rpath = rpath + "/";

                                    Log.Write("rpath: {0}", rpath);
                                    if (!FullList.ContainsKey(rpath))
                                    {
                                        Log.Write("Case 1");
                                        Log.Write("SFTP > sorry, gotta delete {0}", path + name);
                                        if (isDir)
                                        {
                                            Directory.Delete(s);

                                            if (ShowNots())
                                                tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("deleted", false), name), ToolTipIcon.Info);
                                            lasttip = string.Format("Folder {0} was deleted.", name);
                                            link = null;
                                        }
                                        else
                                        {
                                            File.Delete(s);

                                            if (ShowNots())
                                                tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("deleted", true), name), ToolTipIcon.Info);
                                            lasttip = string.Format("File {0} was deleted.", name);
                                            link = null;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Log.Write("--------> {0} {1} {2} in {3}", cPath, path, name, s);
                                if (FTP())
                                {
                                    if (!ftp.DirectoryExists(noSlashes(rPath()) + path))
                                    {
                                        ftp.SetCurrentDirectory(rPath());
                                        foreach (string p in path.Split('/', '/'))
                                        {
                                            if (!ftp.DirectoryExists(p))
                                            {
                                                ftp.CreateDirectory(p);

                                                if (ShowNots() && lasttip != string.Format("Folder {0} was created.", name))
                                                    tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("created", false), name), ToolTipIcon.Info);
                                                lasttip = string.Format("Folder {0} was created.", name);
                                                link = null;

                                            }
                                            ftp.SetCurrentDirectory(noSlashes(ftp.GetCurrentDirectory()) + "/" + p);
                                        }
                                    }

                                    ftp.SetCurrentDirectory(noSlashes(rPath()) + path);
                                    if (ftp.FileExists(name))
                                        ftp.RemoveFile(name);

                                    ftp.PutFile(s, name);
                                }
                                else
                                {
                                    if (isDir)
                                    {
                                        Log.Write("comPath: {0}", compath);
                                        if (!FullList.ContainsKey(compath))
                                        {
                                            Log.Write("Gonna make folder {0} to remote server in pwd: {1}", cPath, sftpc.pwd());
                                            sftpc.mkdir(cPath);
                                            Log.Write("success");
                                            if (ShowNots() && lasttip != string.Format("Folder {0} was created.", name))
                                                tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("created", false), name), ToolTipIcon.Info);
                                            lasttip = string.Format("Folder {0} was created.", name);
                                        }
                                    }
                                    else
                                    {
                                        Log.Write("Case 3");
                                        SftpProgressMonitor monitor = new MyProgressMonitor();
                                        sftpc.put(s, rPathToCD, monitor, ChannelSftp.OVERWRITE);
                                        if (ShowNots() && lasttip != string.Format("File {0} was created.", name))
                                            tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("created", true), name), ToolTipIcon.Info);
                                        lasttip = string.Format("File {0} was created.", name);
                                    }
                                    

                                }
                                Get_Link(path, name);
                                UpdateTheLog(cPath, DateTime.UtcNow);
                            }
                        }
                    }
                    else
                        Log.Write("recentlycreated is true");
                }
            }
            catch { }
            recentlycreated = false;
            Log.Write("$$$$$$$$$$$$ Done Checkin Local $$$$$$$$$$$$");
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
            //ListAllFiles();
        }

        void MakeChangesBGW_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!OfflineMode)
            {
                if (FTP())
                    CheckRemoteFiles(FullList);
                Log.Write("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");                
            }
        }

        private void Get_Language()
        {
            string curlan = lang();

            if (curlan == "" || curlan == null)
            {
                string locallang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

                if (locallang != "en" && (locallang == "es" || locallang == "de"))
                {
                    DialogResult x = MessageBox.Show("FTPbox detected that you use {0} as your computer language. Do you want to use {1} as the language of FTPbox as well?", 
                        "FTPbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (x == DialogResult.Yes)
                    {
                        Set_Language(locallang);
                    }
                    else
                    {
                        Set_Language("en");
                    }
                }
                else
                {
                    Set_Language("en");
                }
            }
            else
            {
                Set_Language(curlan);
            }

        }

        private void Set_Language(string lan)
        {
            if (lan == "en")
            {
                this.Text = "FTPbox | Options";
                //general tab
                tabGeneral.Text = "General";
                gAccount.Text = "FTP Account";
                labHost.Text = "Host:";
                labUN.Text = "Username:";
                labPort.Text = "Port:";
                labMode.Text = "Mode:";
                bAddFTP.Text = "Change";
                gApp.Text = "Application";
                chkShowNots.Text = "Show notifications";
                chkStartUp.Text = "Start on system start-up";
                labLang.Text = "Language:";
                //ftpbox tab
                gDetails.Text = "Details";
                labRemPath.Text = "Remote Path:";
                labLocPath.Text = "Local Path:";
                bChangeBox.Text = "Change";
                gLinks.Text = "Links";
                labFullPath.Text = "Account's full path:";
                labLinkClicked.Text = "When tray notification or recent file is clicked:";
                rOpenInBrowser.Text = "Open link in default browser";
                rCopy2Clipboard.Text = "Copy link to clipboard";
                //about tab
                tabAbout.Text = "About";
                labCurVersion.Text = "Current Version:";
                labTeam.Text = "The Team:";
                labSite.Text = "Official Website:";
                labContact.Text = "Contact:";
                labLangUsed.Text = "Coded in:";
                gNotes.Text = "Notes";
                gContribute.Text = "Contribute";
                labFree.Text = "- FTPbox is free and open-srouce";
                labContactMe.Text = "- Feel free to contact me for anything.";
                linkLabel1.Text = "Report a bug";
                linkLabel2.Text = "Request a feature";
                labDonate.Text = "Donate:";
                cmbLang.SelectedIndex = 0;
            }
            else if (lan == "es")
            {
                this.Text = "FTPbox | Opciones";
                //general tab
                tabGeneral.Text = "General";
                gAccount.Text = "Cuenta FTP";
                labHost.Text = "Host:";
                labUN.Text = "Usuario:";
                labPort.Text = "Puerto:";
                labMode.Text = "Modo:";
                bAddFTP.Text = "Cambiar";
                gApp.Text = "Opciones de la aplicación";
                chkShowNots.Text = "Mostrar notificaciones";
                chkStartUp.Text = "Iniciar al arranque del sistema";
                labLang.Text = "Idioma:";
                //ftpbox tab
                gDetails.Text = "Carpetas a sincronizar";
                labRemPath.Text = "Carpeta remota:";
                labLocPath.Text = "Carpeta local:";
                bChangeBox.Text = "Cambiar";
                gLinks.Text = "Creación de vinculos";
                labFullPath.Text = "Dirección completa de la cuenta:";
                labLinkClicked.Text = "Al clickear una notificación o un archivo reciente:";
                rOpenInBrowser.Text = "Abrir vinculo en el explorador predeterminado";
                rCopy2Clipboard.Text = "Copiar vinculo";
                //about tab
                tabAbout.Text = "Acerca";
                labCurVersion.Text = "Versión actual:";
                labTeam.Text = "El equipo:";
                labSite.Text = "Sitio Web oficial:";
                labContact.Text = "Contacto:";
                labLangUsed.Text = "Escrito en:";
                gNotes.Text = "Notas";
                gContribute.Text = "Contribuye";
                labFree.Text = "- FTPbox es gratuito y de código abierto";
                labContactMe.Text = "- No dudes en contactarme sobre lo que sea";
                linkLabel1.Text = "Reporta un error";
                linkLabel2.Text = "Pide una función";
                labDonate.Text = "Dona:";
                cmbLang.SelectedIndex = 1;
            }
            else if (lan == "de")
            {
                this.Text = "FTPbox | Optionen";
                //general tab
                tabGeneral.Text = "Allgemein";
                gAccount.Text = "FTP Account";
                labHost.Text = "Host:";
                labUN.Text = "Benutzername:";
                labPort.Text = "Port:";
                labMode.Text = "Modus:";
                bAddFTP.Text = "Aendern";
                gApp.Text = "Programm";
                chkShowNots.Text = "Benachrichtigungen anzeigen";
                chkStartUp.Text = "Bei Systemstart automatisch starten";
                labLang.Text = "Sprache:";
                //ftpbox tab
                gDetails.Text = "Details";
                labRemPath.Text = "Pfad zum entfernten Server:";
                labLocPath.Text = "Pfad zum lokalen Verzeichnis:";
                bChangeBox.Text = "Ändern";
                gLinks.Text = "Links";
                labFullPath.Text = "Vollstaendiger Kontopfad:";
                labLinkClicked.Text = "Wenn eine Benachrichtigung oder aktuelle Datei angeklickt wurde:";
                rOpenInBrowser.Text = "Link im Standardbrowser öffnen";
                rCopy2Clipboard.Text = "Link in Zwischenablage koppieren";
                //about tab
                tabAbout.Text = "Über";
                labCurVersion.Text = "Aktuelle Version:";
                labTeam.Text = "Das Team:";
                labSite.Text = "Offizielle Webseite:";
                labContact.Text = "Kontakt:";
                labLangUsed.Text = "Programmiert in:";
                gNotes.Text = "Notizen";
                gContribute.Text = "Mitwirken";
                labFree.Text = "- FTPBox ist kostenlos und open-source";
                labContactMe.Text = "- Kontaktieren sie mich.";
                linkLabel1.Text = "Einen Fehler melden";
                linkLabel2.Text = "Ein Feature vorschlagen";
                labDonate.Text = "Spenden:";
                cmbLang.SelectedIndex = 2;
            }
            AppSettings.Put("Settings/Language", lan);
            //FTPbox.Properties.Settings.Default.lan = lan;
            //FTPbox.Properties.Settings.Default.Save();
        }

        private void cmbLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Set_Language(cmbLang.Text.Substring(0, 2).ToLower());
            }
            catch (Exception ex)
            {
                Log.Write("[Error] -> {0} -> {1}", ex.Message);
            }
        }

        public string Get_Message(string not, bool file)
        {
            if (not == "created")
            {
                if (file)
                {
                    if (lang() == "de")
                        return "Datei {0} wurde erstellt.";
                    else if (lang() == "es")
                        return "El archivo {0} fue creado";
                    else
                        return "File {0} was created.";
                }
                else
                {
                    if (lang() == "de")
                        return "Ordner {0} wurde erstellt.";
                    else if (lang() == "es")
                        return "La carpeta {0} fue creada";
                    else
                        return "Folder {0} was created.";
                }
            }
            else if (not == "deleted")
            {
                if (file)
                {
                    if (lang() == "de")
                        return "Datei {0} wurde gelöscht.";
                    else if (lang() == "es")
                        return "El archivo {0} fue borrado.";
                    else
                        return "File {0} was deleted.";
                }
                else
                {
                    if (lang() == "de")
                        return "Ordner {0} wurde gelöscht.";
                    else if (lang() == "es")
                        return "La carpeta {0} fue borrada.";
                    else
                        return "Folder {0} was deleted.";
                }
                
            }
            else if (not == "renamed")
            {
                if (lang() == "de")
                    return "{0} wurde umbenannt in {1}";
                else if (lang() == "es")
                    return "El archivo {0} fue renombrado a {1}";
                else
                    return "{0} was renamed to {1}.";
            }
            else if (not == "changed")
            {
                if (file)
                {
                    if (lang() == "de")
                        return "Datei {0} wurde geändert.";
                    else if (lang() == "es")
                        return "El archivo {0} fue cambiado.";
                    else
                        return "File {0} was created.";
                }
                else
                {
                    if (lang() == "de")
                        return "Ordner {0} wurde geändert.";
                    else if (lang() == "es")
                        return "La carpeta {0} fue cambiada.";
                    else
                        return "Folder {0} was created.";
                }
            }
            else //if (not == "updated")
            {
                if (file)
                {
                    if (lang() == "de")
                        return "Datei {0} wurde aktuallisiert.";
                    else if (lang() == "es")
                        return "El archivo {0} fue actualizado.";
                    else
                        return "File {0} was updated.";
                }
                else
                {
                    if (lang() == "de")
                        return "Ordner {0} wurde aktuallisiert.";
                    else if (lang() == "es")
                        return "La carpeta {0} fue actualizada.";
                    else
                        return "Folder {0} was updated.";
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Extract_WebInt();
            //CheckWebInt();
            //Log.Write("*SFTP* PWD: {0} | Root is: {1}", sftpc.pwd(), "/home/" + ftpUser());
            ListAllFiles();
            //FullList = new Dictionary<string, DateTime>();
            //FullList = listRemote();
        }

        //check if web interface is up to date
        public void CheckWebInt()
        {
            string path = noSlashes(lPath()) + @"\ftpbox";
            string fpath = path + "\version.txt";

            string rpath = noSlashes(ftpParent()) + @"/";

            if (!noSlashes(rpath).StartsWith("http://") && !noSlashes(rpath).StartsWith("https://"))
            {
                rpath = @"http://" + rpath;
            }
            if (noSlashes(rPath()) != null)
            {
                if ((rPath() == @"/"))
                {
                    //do nothing...
                }
                else if (rPath().StartsWith(@"/public_html/"))
                {
                    rpath = rpath + noSlashes(rPath().Substring(@"/public_html/".Length + 1, rPath().Length - @"/public_html/".Length - 1)) + @"/";
                }
                else if (rPath().StartsWith(@"/"))
                {
                    rpath = rpath + noSlashes(rPath()).Substring(1, noSlashes(rPath()).Length - 1) + @"/";
                }
                else
                {
                    rpath = rpath + noSlashes(rPath()) + @"/";
                }
            }
            rpath = rpath + @"ftpbox/version.txt";
            
            wb.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(VersionBrowser_DocumentCompleted);
            wb.Navigate(@"http://ftpbox.org/webintversion.txt");
        }

        private void VersionBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            string source = wb.Document.Body.InnerText;
            CheckWebIntVersion(source);
            //MessageBox.Show(e.Url + Environment.NewLine + source);
        }

        private void CheckWebIntVersion(string version)
        {
            string curversion;
            TextReader tr = new StreamReader(noSlashes(lPath()) + @"\ftpbox\version.txt");
            curversion = tr.ReadLine();
            tr.Close();

            if (!curversion.Equals(version))
            {
                DownloadWebInt();
            }
        }

        private void DownloadWebInt()
        {
            string path = noSlashes(lPath()) + @"\ftpbox";
            string fpath = path + "\version.txt";

            string rpath = noSlashes(ftpParent()) + @"/";

            if (!noSlashes(rpath).StartsWith("http://") && !noSlashes(rpath).StartsWith("https://"))
            {
                rpath = @"http://" + rpath;
            }
            if (noSlashes(rPath()) != null)
            {
                if ((rPath() == @"/"))
                {
                    //do nothing...
                }
                else if (rPath().StartsWith(@"/public_html/"))
                {
                    rpath = rpath + noSlashes(rPath().Substring(@"/public_html/".Length + 1, rPath().Length - @"/public_html/".Length - 1)) + @"/";
                }
                else if (rPath().StartsWith(@"/"))
                {
                    rpath = rpath + noSlashes(rPath()).Substring(1, noSlashes(rPath()).Length - 1) + @"/";
                }
                else
                {
                    rpath = rpath + noSlashes(rPath()) + @"/";
                }
            }
            rpath = rpath + @"ftpbox/version.txt";

            WebClient wc = new WebClient();
            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompleted);
            wc.DownloadFile(rpath, Application.StartupPath);
        }

        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {

        }

        public void ExtractWebInt()
        {
            
        }

        public void DeleteWebDivContent()
        {
            string path = noSlashes(lPath()) + "\ftpbox";
            Directory.Delete(path);
        }

        ///SFTP Code Starting here
        ///hell yeah

        private void sftp_login()
        {
            JSch jsch = new JSch();

            String host = ftpHost();
            String user = ftpUser();

            Session session = jsch.getSession(user, host, 22);

            // username and password will be given via UserInfo interface.
            UserInfo ui = new MyUserInfo();

            session.setUserInfo(ui);

            session.connect();

            Channel channel = session.openChannel("sftp");
            channel.connect();
            sftpc = (ChannelSftp)channel;
            //sftpc = (ChannelSftp)channel;
        }

        

        /// <summary>
        /// Get LastWriteTime of remote file or folder in current directory
        /// </summary>
        /// <param name="fullpath">File or folder to use</param>
        /// <returns></returns>
        private DateTime SftpGetLastWriteTime(string path)
        {
            SftpATTRS attrs = null;

            try
            {
                Log.Write("(SFTP) Gonna get LWT of: {0} in path: {1}", path, sftpc.pwd());
                attrs = sftpc.stat(path);
            }
            catch (SftpException e)
            {
                Console.WriteLine(e.message);
            }

            if (attrs != null)
                return attrs.getMtimeString().AddSeconds(timedif.TotalSeconds);
            else
                return DateTime.UtcNow;
        }

        private void SftpCreate(string path, string cPath, string name, bool isDir)
        {
            try
            {
                Syncing();
                downloading = true;
                SftpCDtoRoot();
                string rPathToCD = cPath;
                string rFullPath = noSlashes(cPath) + "/";
                if (rFullPath == "/")
                {
                    rFullPath = "";
                    rPathToCD = ".";
                }
                else if (rFullPath.StartsWith("/"))
                {
                    rFullPath = rFullPath.Substring(1);
                    rPathToCD = rPathToCD.Substring(1);
                }
                //sftpc.cd(rFullPath);
                Log.Write("&&&&&&&&&& {0} {1} {2}", rFullPath, cPath, name, rPathToCD);

                if (isDir)
                {
                    //DirectoryInfo di = new DirectoryInfo(path);
                    sftpc.mkdir(rFullPath + name);
                    Log.Write("????> Created Directory: {0} (remote)", path);

                    if (ShowNots() && lasttip != string.Format("Folder {0} was created.", name))
                        tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("created", false), name), ToolTipIcon.Info);
                    lasttip = string.Format("Folder {0} was created.", name);
                    link = null;
                }
                else
                {
                    FileInfo fi = new FileInfo(path);
                    string comPath = rFullPath + fi.Name;
                    Log.Write("comPath: {0}", comPath);

                    Log.Write("DirectoryName: {0} from path: {1}", fi.DirectoryName, path);
                    //sftpc.lcd(fi.DirectoryName);
                    Log.Write("LCD: check");
                    SftpProgressMonitor monitor = new MyProgressMonitor();
                    sftpc.put(path, rPathToCD, monitor, ChannelSftp.OVERWRITE);

                    Log.Write("********** {0} **********", fi.DirectoryName);

                    if (ShowNots() && lasttip != string.Format("File {0} was created.", fi.Name))
                        tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("created", true), fi.Name), ToolTipIcon.Info);
                    lasttip = string.Format("File {0} was created.", fi.Name);
                    Get_Link(cPath, fi.Name);

                    //ftp.SetCurrentDirectory(rPath());

                    
                    Log.Write("~~~~~~~~~~~~~> comPath: {0}", comPath);
                    UpdateTheLog(comPath, SftpGetLastWriteTime(comPath));
                }

                DoneSyncing();
            }
            catch (SftpException e)
            {
                Log.Write(e.message);
            }
        }

        private void SftpChange(string path, string name, string cPath)
        {
            try
            {
                Syncing();
                downloading = true;
                SftpCDtoRoot();
                string rPathToCD = cPath;
                string rFullPath = noSlashes(cPath) + "/";
                if (rFullPath == "/")
                {
                    rFullPath = "";
                    rPathToCD = ".";
                }
                else if (rFullPath.StartsWith("/"))
                {
                    rFullPath = rFullPath.Substring(1);
                    rPathToCD = rPathToCD.Substring(1);
                }
                //sftpc.cd(rFullPath);
                Log.Write("&&&&&&&&&& {0} {1} {2}", rFullPath, cPath, name, rPathToCD);
                FileInfo fi = new FileInfo(path);
                string comPath = rFullPath + fi.Name;
                Log.Write("comPath: {0}", comPath);

                Log.Write("DirectoryName: {0} from path: {1}", fi.DirectoryName, path);
                //sftpc.lcd(fi.DirectoryName);
                Log.Write("LCD: check");
                SftpProgressMonitor monitor = new MyProgressMonitor();
                sftpc.put(path, rPathToCD, monitor, ChannelSftp.OVERWRITE);

                Log.Write("********** {0} **********", fi.DirectoryName);

                if (ShowNots() && lasttip != string.Format("File {0} was updated.", fi.Name))
                    tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("udpated", true), fi.Name), ToolTipIcon.Info);
                lasttip = string.Format("File {0} was updated.", fi.Name);
                Get_Link(cPath, fi.Name);

                //ftp.SetCurrentDirectory(rPath());


                Log.Write("~~~~~~~~~~~~~> comPath: {0}", comPath);
                UpdateTheLog(comPath, SftpGetLastWriteTime(comPath));
                
                DoneSyncing();
            }
            catch (SftpException e)
            {
                Log.Write(e.message);
            }
        }

        private void SftpDelete(string name, string cPath, bool isDir)
        {
            Syncing();
            downloading = true;
            SftpCDtoRoot();
            string comPath = cPath;
            if (comPath == "/")
                comPath = "";
            else if (comPath.StartsWith("/"))
                comPath = comPath.Substring(1);

            Log.Write("*SFTP* ===+> Gonna delete file, name: {0} cPath {1}", name, cPath);
            Log.Write("*SFTP* ==> comPath: {0}", comPath);
            string logpath = noSlashes(cPath) + @"/" + name;
            if (logpath.StartsWith("/"))
                logpath = logpath.Substring(1);

            if (isDir)
            {
                try
                {
                    sftpc.rmdir(comPath + name);
                }
                catch (SftpException e)
                {
                    Log.Write("[Error sftp] -> {0}", e.Message);
                }
                if (ShowNots() && lasttip != string.Format("Folder {0} was deleted.", name))
                    tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("deleted", false), name), ToolTipIcon.Info);
                lasttip = string.Format("Folder {0} was deleted.", name);
                link = null;
                RemoveFromLog(logpath);
            }
            else
            {
                try
                {
                    sftpc.rm(comPath + name);
                }
                catch (SftpException e)
                {
                    Log.Write("[Error sftp] -> {0}", e.Message);
                }

                if (ShowNots() && lasttip != string.Format("File {0} was deleted.", name))
                    tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("deleted", true), name), ToolTipIcon.Info);
                lasttip = string.Format("File {0} was deleted.", name);
                link = null;

                string locPath = noSlashes(lPath()) + @"\" + noSlashes(cPath) + @"\" + name;

                RemoveFromLog(logpath);

            }
            DoneSyncing();

        }

        private void SftpDownloadFile(string path)
        {
            Syncing();
            downloading = true;
            Log.Write("GONNA DOWNLOAD: {0} to path {1}", path, sftpc.lpwd());
            SftpProgressMonitor monitor = new MyProgressMonitor();
            int mode = ChannelSftp.OVERWRITE;
            sftpc.get(path, ".", monitor, mode);
            DoneSyncing();
        }

        private void SftpRename(string oldName, string newName, string cPath, string FullPath, bool isDir)
        {
            Syncing();
            downloading = true;

            string rFullpath = cPath;
            if (rFullpath == "/")
                rFullpath = "";
            else if (rFullpath.StartsWith("/"))
                rFullpath = rFullpath.Substring(1);
            Log.Write("*SFTP* About to rename {0} to {1} in path: {2}", oldName, newName, rFullpath);

            SftpCDtoRoot();
            /*
            try
            {
                sftpc.cd(rFullpath);
            }
            catch (SftpException e)
            {
                Log.Write("[ERROR setting dir] -> {0} MSG: {1}", rFullpath, e.Message);
            } */

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

            bool fileexists = true;
            string rFullOld = noSlashes(rFullpath) + "/" + nameOld;
            string rFullNew = noSlashes(rFullpath) + "/" + nameNew;
            if (rFullNew.StartsWith("/"))
            {
                rFullOld = rFullOld.Substring(1);
                rFullNew = rFullNew.Substring(1);
            }
            
            Log.Write("rFullOld: {0} | rFullNew: {1}", rFullOld, rFullNew);

            Syncing();
            try
            {
                //sftpc.rename(rFullOld, rFullNew);
            }
            catch { DoneSyncing(); }

            if (ShowNots())
                tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("renamed", true), nameOld, nameNew), ToolTipIcon.Info);
            Get_Link("", rFullNew);

            if (FTP())
            {
                if (!isDir)
                {
                    string oldLogPath = (@"\" + noSlashes(oldName)).Replace(@"\", @"/");
                    string newLogPath = (@"\" + noSlashes(newName)).Replace(@"\", @"/");
                    RemoveFromLog(oldLogPath);
                    UpdateTheLog(newLogPath, DateTime.UtcNow);//DateTime.UtcNow);
                }
            }
            else
            {
                //RemoveFromLog(rFullOld);
                //UpdateTheLog(rFullNew, DateTime.UtcNow);
            }
                    
            DoneSyncing();
            DoneSyncing();
        }

        public void SftpCDtoRoot()
        {
            try
            {
                string rpath = rPath();
                Log.Write("1. " + rpath);
                string home = "/home/" + ftpUser();
                Log.Write("2. " + home);
                if (rpath == "/")
                    rpath = "";
                else if (rpath.StartsWith("/"))
                {
                    rpath = rpath.Substring(1);
                }
                Log.Write("3. " + rpath);

                if (rpath != "")
                    home = home + "/" + rpath;

                Log.Write("Home: {0}", home);
                
                while (!sftpc.pwd().Equals(home)) 
                {
                    Log.Write("*SFTP* Going up one level from {0} to get to {1}", sftpc.pwd(), home);
                    if (sftpc.pwd() == "/")
                        sftpc.cd(home.Substring(1));
                    else
                        sftpc.cd("..");                    
                }
                if (sftpc.pwd() == "/")
                {
                    sftpc.cd(home.Substring(1));
                }
            }
            catch (SftpException e)
            {
                Log.Write("*SFTP* [ERROR]: {0}", e.Message);
            }
            
            Log.Write("--> Changed to root {0}", sftpc.pwd());
        }

        public void sftpcDtoRoot()
        {
            try
            {
                string rpath = rPath();
                if (rpath == "/")
                    rpath = "";
                else if (rpath.StartsWith("/"))
                    rpath = rpath.Substring(1);

                string home = "/home/" + ftpUser() + "/" + rpath;
                while (!sftpc.pwd().Equals(home))
                {
                    Log.Write("*SFTP* Going up one level from {0}", sftpc.pwd());
                    if (sftpc.pwd() == "/")
                        sftpc.cd(home.Substring(1));
                    else
                        sftpc.cd("..");
                }
            }
            catch (SftpException e)
            {
                Log.Write("*SFTP* [ERROR]: {0}", e.Message);
            }
            Log.Write("--> Changed to root {0}", sftpc.pwd());
        }

        public class MyUserInfo : UserInfo
        {
            FTPbox.Classes.Settings sets = new FTPbox.Classes.Settings();
            public String getPassword() { return sets.Get("Account/Password", ""); }
            public bool promptYesNo(String str)
            {
                DialogResult returnVal = MessageBox.Show(
                    str,
                    "SharpSSH",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                return (returnVal == DialogResult.Yes);
            }
                        
            public String getPassphrase() { return null; }
            public bool promptPassphrase(String message) { return true; }
            public bool promptPassword(String message) { return true; }

            public void showMessage(String message)
            {
                MessageBox.Show(
                    message,
                    "SharpSSH",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
            }
        }

        public class MyProgressMonitor : SftpProgressMonitor
        {
            private ConsoleProgressBar bar;
            private long c = 0;
            private long max = 0;
            private long percent = -1;
            int elapsed = -1;

            System.Timers.Timer timer;

            public override void init(int op, String src, String dest, long max)
            {
                bar = new ConsoleProgressBar();
                this.max = max;
                elapsed = 0;
                timer = new System.Timers.Timer(1000);
                timer.Start();
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            }
            public override bool count(long c)
            {
                this.c += c;
                if (percent >= this.c * 100 / max) { return true; }
                percent = this.c * 100 / max;

                string note = ("Transfering... [Elapsed time: " + elapsed + "]");

                bar.Update((int)this.c, (int)max, note);
                return true;
            }
            public override void end()
            {
                timer.Stop();
                timer.Dispose();
                string note = ("Done in " + elapsed + " seconds!");
                bar.Update((int)this.c, (int)max, note);
                bar = null;
            }

            private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                this.elapsed++;
            }
        }

        public static string RandomString(int size)
        {
            System.Random x = new System.Random();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                //26 letters in the alfabet, ascii + 65 for the capital letters
                builder.Append(Convert.ToChar(Convert.ToInt32(Math.Floor(26 * x.NextDouble() + 65))));
            }
            return builder.ToString().ToLower();
        }

        public void CheckPending()
        {
            Log.Write("Gonna check pending actions");
            foreach (List<string> ls in pending)
            {
                if (ls[0] == "cr")
                {
                    SftpCreate(ls[1], ls[2], ls[3], bool.Parse(ls[4]));
                }
                else if (ls[0] == "d")
                {
                    SftpDelete(ls[1], ls[2], bool.Parse(ls[3]));
                }
                else if (ls[0] == "r")
                {
                    SftpRename(ls[1], ls[2], ls[3], ls[4], bool.Parse(ls[5]));
                }
                else if (ls[0] == "ch")
                {
                    try
                    {
                        SftpChange(ls[1], ls[2], ls[3]);
                    }
                    catch { }
                }
            }
            pending.Clear();
            Log.Write("Done checking pending actions");
        }

        public void UpdateAccountInfo(string host, string username, string password, int port, string timedif, bool ftp)
        {
            AppSettings.Put("Account/Host", host);
            AppSettings.Put("Account/Port", port);
            AppSettings.Put("Account/Username", username);
            AppSettings.Put("Account/Password", password);
            AppSettings.Put("Account/FTPorSFTP", ftp.ToString());
            AppSettings.Put("Settings/Timedif", "");
        }

        public void UpdatePaths(string rpath, string lpath, string parent)
        {
            AppSettings.Put("Paths/rPath", rpath);
            AppSettings.Put("Paths/lPath", lpath);
            AppSettings.Put("Paths/Parent", parent);
        }

        public void SetPass(string pass)
        {
            AppSettings.Put("Account/Password", pass);
        }

        public void SetParent(string parent)
        {
            AppSettings.Put("Paths/Parent", parent);
        }

    }
}