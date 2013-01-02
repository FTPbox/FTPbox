/* License
 * This file is part of FTPbox - Copyright (C) 2012 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* fMain.cs
 * The main form of the application (options form)
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
using System.IO;
using FTPboxLib;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using System.IO.Pipes;

namespace FTPbox.Forms
{
    public partial class fMain : Form
    {
        public FileQueue fQueue = new FileQueue();			//file queue
        public DeletedQueue dQueue = new DeletedQueue();	//Deleted items queue        

        List<string> localFolders = new List<string>();     //Used to store all the local folders at all times
        List<string> localFiles = new List<string>();       //Used to store all the local files at all times

        public RecentFiles recentFiles = new RecentFiles(); //List of the 5 most recently changed files
        TrayAction _trayAct = TrayAction.OpenLocalFile;     //the tray action to be used for opening files

        Thread rcThread;                                    //remote-check thread
        Thread wiThread;                                    //web interface thread
        Thread wiuThread;                                   //Web interface update thread                  

        public bool loggedIn = false;                       //if the client has been connected
        public bool gotpaths = false;                       //if the paths have been set or checked    

        FileLog fLog = new FileLog();                       //the file log

        //Form instances
        Account fNewFtp;
        Paths newDir;
        Translate ftranslate;

        //Links
        public string link = null;                          //The web link of the last-changed file
        public string locLink = null;                       //The local path to the last-changed file

        private System.Threading.Timer tSync;               //Timer used to schedule syncing according to user's preferences

        public fMain()
        {
            InitializeComponent();
        }

        private void fMain_Load(object sender, EventArgs e)
        {
            rcThread = new Thread(SyncRemote);
            wiThread = new Thread(AddRemoveWebInt);
            wiuThread = new Thread(UpdateWebIntThreadStart);

            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(OnNetworkChange);
            CheckForPreviousInstances();

            Settings.Load();
            LoadLocalFolders();
            LoadLog();
            LoadProfile();
            
            _trayAct = Settings.SettingsTrayAction;

            fNewFtp = new Account();
            fNewFtp.Tag = this;
            newDir = new Paths();
            newDir.Tag = this;
            ftranslate = new Translate();
            ftranslate.Tag = this;

            Get_Language();
            
            StartUpWork();

            CheckForUpdate();
        }

        /// <summary>
        /// Work done at the application startup. 
        /// Checks the saved account info, updates the form controlls and starts syncing if syncing is automatic.
        /// If there's no internet connection, puts the program to offline mode.
        /// </summary>
        private void StartUpWork()
        {
            Log.Write(l.Debug, ConnectedToInternet().ToString());
            if (ConnectedToInternet())
            {
                CheckAccount();
                //UpdateDetails();
                Log.Write(l.Debug, "Account: OK");
                CheckPaths();
                Log.Write(l.Debug, "Paths: OK");

                UpdateDetails();

                if (!Profile.IsNoMenusMode)
                {
                    AddContextMenu();
                    RunServer();
                }

                SetTray(MessageType.Ready);

                RefreshListing();

                if (Profile.SyncingMethod == SyncMethod.Automatic && !_busy)
                    StartRemoteSync(".");
            }
            else
            {
                OfflineMode = true;
                SetTray(MessageType.Offline);
            }

            Log.Write(l.Client, Process.GetCurrentProcess().ProcessName);
        }       

        /// <summary>
        /// Connect the client to the server
        /// </summary>
        public void LoginFTP()
        {
            SetTray(MessageType.Connecting);

            Log.Write(l.Debug, "Connecting ftpc -> user: {0} protocol: {1}", Profile.Username, Profile.SecurityProtocol.ToString());
            Client.Connect();
            Log.Write(l.Debug, "Connection opened");

            if (Profile.Protocol != FtpProtocol.SFTP)
            {
                if (LimitUpSpeed())
                    nUpLimit.Value = Convert.ToDecimal(Settings.UpLimit);
                if (LimitDownSpeed())
                    nDownLimit.Value = Convert.ToDecimal((Settings.DownLimit));
            }
            else
            {
                Profile.HomePath = Client.WorkingDirectory;
                Profile.HomePath = (Profile.HomePath.StartsWith("/")) ? Profile.HomePath.Substring(1) : Profile.HomePath;
            }

            SetTray(MessageType.Ready);
        }

        public void RetryConnection()
        {
            try
            {
                LoginFTP();
                AfterPathsAreSet();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
                KillTheProcess();
            }
        }

        /// <summary>
        /// checks if account's information used the last time has changed
        /// </summary>
        private void CheckAccount()
        {
            if (Profile.Username == "" || Profile.Host == "" || Profile.Password == "")
            {
                Log.Write(l.Info, "Will open New FTP form.");

                fNewFtp.ShowDialog();

                Log.Write(l.Info, "Done");

                this.Show();
            }
            else
            {
                try
                {
                    LoginFTP();

                    this.ShowInTaskbar = false;
                    this.Hide();
                    this.ShowInTaskbar = true;                    
                }
                catch (Exception ex)
                {
                    Log.Write(l.Info, "Will open New FTP form");
                    Common.LogError(ex);
                    fNewFtp.ShowDialog();
                    Log.Write(l.Info, "Done");

                    this.Show();
                }
            }

            loggedIn = true;
        }
        
        /// <summary>
        /// checks if paths used the last time still exist
        /// </summary>
        public void CheckPaths()
        {
            string rpath = Profile.RemotePath;
            if (rpath.StartsWith(@"/") && rpath != @"/")
                rpath = rpath.Substring(1);

            Log.Write(l.Debug, "rpath: {0} lPath: {1}", rpath, Profile.LocalPath);

            if (rpath == "" || Profile.LocalPath == "")
            {
                Log.Write(l.Debug, "Case 1");
                newDir.ShowDialog();
                
                //Application.Run();
                this.Show();

                if (!gotpaths)
                {
                    Log.Write(l.Debug, "shutting down");
                    KillTheProcess();
                }
            }
            else if ((rpath != "/" && !Client.Exists(rpath)) || !Directory.Exists(Profile.LocalPath))
            {
                Log.Write(l.Debug, "Case 2");
                newDir.ShowDialog();

                this.Show();

                if (!gotpaths)
                {
                    KillTheProcess();
                }
            }
            else
                gotpaths = true;
            
            LoadLocalFolders();
        }
        
        /// <summary>
        /// Updates the form's labels etc
        /// </summary>
        public void UpdateDetails()
        {
            Log.Write(l.Debug, "Updating the form labels and shit");
            //Settings.Load();

            WebIntExists();

            chkStartUp.Checked = CheckStartup();

            lHost.Text = Profile.Host;
            lUsername.Text = Profile.Username;
            lPort.Text = Profile.Port.ToString();
            lMode.Text = (Profile.Protocol != FtpProtocol.SFTP) ? "FTP" : "SFTP";

            lLocPath.Text = Profile.LocalPath;
            lRemPath.Text = Profile.RemotePath;
            tParent.Text = Profile.HttpPath;

            chkShowNots.Checked = Settings.ShowNots;

            if (_trayAct == TrayAction.OpenInBrowser)
                rOpenInBrowser.Checked = true;
            else if (_trayAct == TrayAction.CopyLink)
                rCopy2Clipboard.Checked = true;
            else
                rOpenLocal.Checked = true;

            lVersion.Text = Application.ProductVersion.ToString().Substring(0, 5) + @" Beta";

            //   Filters Tab    //

            cIgnoreDotfiles.Checked = Common.IgnoreList.IgnoreDotFiles;
            cIgnoreTempFiles.Checked = Common.IgnoreList.IgnoreTempFiles;
            lIgnoredExtensions.Clear();
            foreach (string s in Common.IgnoreList.ExtensionList) 
                if (!string.IsNullOrWhiteSpace(s))  lIgnoredExtensions.Items.Add(new ListViewItem(s));

            //  Bandwidth tab   //

            if (Profile.SyncingMethod == SyncMethod.Automatic)
                cAuto.Checked = true;
            else
                cManually.Checked = true;

            nSyncFrequency.Value = Convert.ToDecimal(Settings.syncFrequency);

            if (Profile.Protocol != FtpProtocol.SFTP)
            {
                if (LimitUpSpeed())
                    nUpLimit.Value = Convert.ToDecimal(Settings.UpLimit);
                if (LimitDownSpeed())
                    nDownLimit.Value = Convert.ToDecimal(Settings.DownLimit);
            }
            else
            {
                gLimits.Visible = false;
            }

            AfterPathsAreSet();
            SetWatchers();
        }

        /// <summary>
        /// Called after the paths are correctly set. In case of SFTP, it gets the SFTP Home Directory.
        /// Also changes the working directory of the client to the remote syncing folder.
        /// </summary>
        public void AfterPathsAreSet()
        {
            try
            {
                if (!Profile.RemotePath.Equals(" ") && !Profile.RemotePath.Equals("/"))
                {
                    Client.WorkingDirectory = Profile.RemotePath;
                }
                Log.Write(l.Info, "Changed current directory to {0}", Client.WorkingDirectory);                                          
            }
            catch (Exception e) { Common.LogError(e); }            
        }

        /// <summary>
        /// Called when syncing is about to start.
        /// </summary>        
        private void Syncing()
        {
            fswFiles.EnableRaisingEvents = false;
            fswFolders.EnableRaisingEvents = false;
            SetTray(MessageType.Syncing);
        }

        /// <summary>
        /// Called when syncing is finished.
        /// </summary>
        private void DoneSyncing()
        {
            fswFiles.EnableRaisingEvents = true;
            fswFolders.EnableRaisingEvents = true;
            SetTray(MessageType.AllSynced);
            LoadLocalFolders();
        }

        /// <summary>
        /// Kill if instances of FTPbox are already running
        /// </summary>
        private void CheckForPreviousInstances()
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
                            p.WaitForExit(3000);
                            if (!p.HasExited)
                            {
                                MessageBox.Show("Another instance of FTPbox is already running.", "FTPbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                Process.GetCurrentProcess().Kill();
                            }
                        }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// possible types of file change
        /// </summary>
        private enum ChangeAction
        {
            changed = 0,
            created = 1,
            deleted = 2,
            renamed = 3,
            updated = 4
        }

        #region Put/Remove items from the Log file

        /// <summary>
        /// Adds the given item to the log file
        /// </summary>
        /// <param name="cPath">the common path to the item</param>
        /// <param name="rDTlog">the remote LastWriteTime of the item</param>
        /// <param name="lDTlog">the local LastWriteTime of the item</param>
        public void UpdateTheLog(string cPath, DateTime rDTlog, DateTime lDTlog)
        {
            string name = "";
            name = cPath.Substring(cPath.LastIndexOf("/") + 1, cPath.Length - cPath.LastIndexOf("/") - 1);

            RemoveFromLog(cPath);
            fLog.putFile(cPath, rDTlog, lDTlog);

            Settings.SaveLog(cPath, rDTlog.ToString(), lDTlog.ToString()); 
            
            Log.Write(l.Debug, "##########");
            Log.Write(l.Debug, "FLP {0} + name: {1} + rDTlog: {2} + lDTlog: {3} + cPath: {4}", "", name, rDTlog.ToString(), lDTlog.ToString(), cPath);
            Log.Write(l.Debug, "#########");
        }

        /// <summary>
        /// puts the specified folder in log
        /// </summary>
        /// <param name="cpath"></param>
        public void PutFolderInLog(string cpath)
        {
            Settings.SaveFolder(cpath);
            //AppSettings.Put("Log/folders", foLog() + cpath + "|");
        }

        /// <summary>
        /// removes an item from the log
        /// </summary>
        /// <param name="cPath">name to remove</param>
        public void RemoveFromLog(string cPath)
        {
            Common.RemoveFromLog(cPath);
            Delete_Recent(cPath);
        }

        /// <summary>
        /// removes the specified folder from log
        /// </summary>
        /// <param name="cpath"></param>
        public void RemoveFolderFromLog(string cpath)
        {
            if (fLog.Folders.Contains(cpath))
            {
                fLog.removeFolder(cpath);
                Settings.ClearFolders();

                foreach (string f in fLog.Folders)
                {
                    Settings.SaveFolder(f);     //AppSettings.Put("Log/folders", foLog() + f + "|");
                }
                Log.Write(l.Debug, "*** Removed from folders Log: {0}", cpath);
            }
        }

        /// <summary>
        /// Loads the Log from the XML
        /// </summary>
        public void LoadLog()
        {
            List<string> Namelog = new List<string>(Settings.nLog.Split('|', '|'));
            List<string> remoteDL = new List<string>(Settings.rLog.Split('|', '|'));
            List<string> localDL = new List<string>(Settings.lLog.Split('|', '|'));

            List<string> folderLog = new List<string>(Settings.foLog.Split('|', '|'));

            if (Namelog.Count == 0)
                return;

            for (int i = 0; i < Namelog.Count; i++)
            {
                if (Namelog[i] != null && Namelog[i] != "")
                {
                    Log.Write(l.Debug, "Found in log: {0} in position {1} rDT: {2} lDT: {3}", Namelog[i], i, remoteDL[i], localDL[i]);
                    try
                    {
                        fLog.putFile(Namelog[i], Convert.ToDateTime(remoteDL[i]), Convert.ToDateTime(localDL[i]));
                    }
                    catch (Exception e)
                    {
                        Common.LogError(e);
                    }
                }
            }

            if (Settings.foLog == "")
            {
                /*
                foreach (string s in localFolders)
                {
                    string cpath = Common.GetComPath(s, true);
                    PutFolderInLog(cpath);
                    fLog.putFolder(cpath);
                }*/
            }
            else
            {
                for (int i = 0; i < folderLog.Count; i++)
                {
                    if (folderLog[i] != null && folderLog[i] != "")
                    {
                        Log.Write(l.Debug, "Found in folders log: {0} in position {1}", folderLog[i], i);
                        fLog.putFolder(folderLog[i]);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Loads the profile class from the XML file.
        /// </summary>
        public void LoadProfile()
        {
            Profile.AddAccount(Settings.Host, Settings.User, Settings.Pass, Settings.Port);
            Profile.AddPaths(Settings.rPath, Settings.lPath, Settings.ftpParent);
            Profile.Protocol = (Settings.FTP) ? (Settings.FTPS ? FtpProtocol.FTPS : FtpProtocol.FTP) : FtpProtocol.SFTP;
            Profile.FtpsInvokeMethod = (Profile.Protocol == FtpProtocol.FTP) ? FtpsMethod.None : ((Settings.FTPES) ? FtpsMethod.Explicit : FtpsMethod.Implicit);

            Profile.SecurityProtocol = (Profile.Protocol == FtpProtocol.FTP) ? Starksoft.Net.Ftp.FtpSecurityProtocol.None : ((Settings.FTPES) ? Starksoft.Net.Ftp.FtpSecurityProtocol.Tls1OrSsl3Explicit : Starksoft.Net.Ftp.FtpSecurityProtocol.Tls1OrSsl3Implicit);

            if (Settings.FtpsSecProtocol != "Default")
                Profile.SecurityProtocol = (Starksoft.Net.Ftp.FtpSecurityProtocol)Enum.Parse(typeof(Starksoft.Net.Ftp.FtpSecurityProtocol), Settings.FtpsSecProtocol);

            Profile.SyncingMethod = Settings.syncMethod;
            Profile.SyncFrequency = Settings.syncFrequency;
        }

        /// <summary>
        /// Loads the local folders.
        /// </summary>
        public void LoadLocalFolders()
        {
            localFolders.Clear();
            if (Directory.Exists(Settings.lPath))
            {
                DirectoryInfo d = new DirectoryInfo(Settings.lPath);
                foreach (DirectoryInfo di in d.GetDirectories("*", SearchOption.AllDirectories))
                {
                    //Log.Write(l.Info, "Found local folder: {0}", di.FullName);
                    localFolders.Add(di.FullName);
                }

                foreach (FileInfo fi in d.GetFiles("*", SearchOption.AllDirectories))
                {
                    localFiles.Add(fi.FullName);
                }
            }
            Log.Write(l.Info, "Loaded {0} local directories and {1} files", localFolders.Count, localFiles.Count);
        }

        #region File/Folder Watchers and Event Handlers

        /// <summary>
        /// Sets the file watchers for the local directory.
        /// </summary>
        public void SetWatchers()
        {
            Log.Write(l.Debug, "Setting the file system watchers");

            fswFiles = new FileSystemWatcher();
            fswFolders = new FileSystemWatcher();
            fswFiles.Path = Profile.LocalPath;
            fswFolders.Path = Profile.LocalPath;
            fswFiles.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite; //NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
            fswFolders.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.DirectoryName;

            fswFiles.Filter = "*";
            fswFolders.Filter = "*";

            fswFiles.IncludeSubdirectories = true;
            fswFolders.IncludeSubdirectories = true;

            //add event handlers for files:
            fswFiles.Changed += new FileSystemEventHandler(FileChanged);
            fswFiles.Created += new FileSystemEventHandler(FileChanged); //olderChanged);
            fswFiles.Deleted += new FileSystemEventHandler(onDeleted);
            fswFiles.Renamed += new RenamedEventHandler(OnRenamed);
            //and for folders:
            //fswFolders.Changed += new FileSystemEventHandler(FolderChanged);
            fswFolders.Created += new FileSystemEventHandler(FolderChanged);
            fswFolders.Deleted += new FileSystemEventHandler(onDeleted);
            fswFolders.Renamed += new RenamedEventHandler(OnRenamed);

            fswFiles.EnableRaisingEvents = true;
            fswFolders.EnableRaisingEvents = true;

            Log.Write(l.Debug, "Done!");
        }

        /// <summary>
        /// Raised when a file was changed
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void FileChanged(object source, FileSystemEventArgs e)
        {
            string cpath = Common.GetComPath(e.FullPath, true);
            if (Common.ItemGetsSynced(cpath) && !Common.FileIsUsed(e.FullPath))
            {
                if (_busy)
                {
                    fQueue.reCheck = true;
                    Log.Write(l.Debug, "Will recheck, later");
                }
                else
                {
                    Log.Write(l.Debug, "File {0} was changed, type: {1}", e.Name, e.ChangeType.ToString());

                    if (e.ChangeType == WatcherChangeTypes.Changed)
                    {
                        FileInfo fli = new FileInfo(e.FullPath);
                        fQueue.Add(cpath, e.FullPath, fli.Length, TypeOfTransfer.Change);
                        SyncLocQueueFiles();
                    }
                    else
                    {
                        if (File.Exists(e.FullPath) || Directory.Exists(e.FullPath))
                            SyncInFolder(Directory.GetParent(e.FullPath).FullName, true);
                    }
                }
            }
        }

        /// <summary>
        /// Raised when a folder was changed
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void FolderChanged(object source, FileSystemEventArgs e)
        {
            string cpath = Common.GetComPath(e.FullPath, true);
            if (!Common.ItemGetsSynced(cpath)) return;

            if (Common.PathIsFolder(e.FullPath) && e.ChangeType == WatcherChangeTypes.Created && !Client.Exists(cpath))
            {
                Client.MakeFolder(cpath);
                fQueue.AddFolder(cpath);

                fLog.putFolder(cpath);
                PutFolderInLog(cpath);

                //if (fQueue.CountFolders() == 1)
                //	ShowNotification(e.Name, ChangeAction.created, false);

                GetLink(cpath);
                if (_busy)
                {
                    fQueue.reCheck = true;
                    Log.Write(l.Debug, "Will recheck, later");
                }
                else
                    SyncInFolder(e.FullPath, true);
            }
            if (!e.FullPath.EndsWith("~") && !e.Name.StartsWith(".goutputstream"))
            {
                if (_busy)
                {
                    fQueue.reCheck = true;
                    Log.Write(l.Debug, "Will recheck, later");
                    //wait a bit
                    //Console.Write("Waiting...");
                }
                else
                {
                    SyncInFolder(Directory.GetParent(e.FullPath).FullName, true);
                }
            }
        }

        /// <summary>
        /// Raised when either a file or a folder was deleted.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void onDeleted(object source, FileSystemEventArgs e)
        {            
            if (Common.ItemGetsSynced(Common.GetComPath(e.FullPath, true)))
            {                
                dQueue.Add(e.FullPath);
                if (!_busy)
                    DeleteFromQueue();
                else
                    dQueue.reCheck = true;
            }
        }

        /// <summary>
        /// Raised when file/folder is renamed
        /// </summary>
        /// <param name='source'>
        /// Source.
        /// </param>
        /// <param name='e'>
        /// E.
        /// </param>
        private void OnRenamed(object source, RenamedEventArgs e)
        {
            Log.Write(l.Debug, "Item {0} was renamed", e.OldName);
            if (Common.ItemGetsSynced(Common.GetComPath(e.FullPath, true)) && Common.ItemGetsSynced(Common.GetComPath(e.OldFullPath, true)))
            {
                fswFolders.EnableRaisingEvents = false;
                if (e.FullPath.StartsWith(Profile.LocalPath))
                {
                    if (!Client.CheckConnectionStatus())
                        RetryConnection();

                    Log.Write(l.Debug, "{0} File {1} ", e.ChangeType.ToString(), e.FullPath);
                    string oldName = Common.GetComPath(e.OldFullPath, true);
                    string newName = Common.GetComPath(e.FullPath, true);
                    Log.Write(l.Debug, "Oldname: {0} Newname: {1}", oldName, newName);

                    Client.Rename(oldName, newName);
                    
                    ShowNotification(e.OldName, ChangeAction.renamed, e.Name);
                    GetLink(Common.GetComPath(e.FullPath, true));

                    if (source == fswFiles)
                    {
                        string cpath = Common.GetComPath(newName, true);
                        RemoveFromLog(Common.GetComPath(oldName, true));
                        FileInfo f = new FileInfo(e.FullPath);
                        UpdateTheLog(cpath, Client.GetLWTof(cpath), f.LastWriteTime);
                        //fLog.putFile(cpath, GetLWTof(cpath), f.LastWriteTime);
                    }
                }
                fswFolders.EnableRaisingEvents = true;
            }
            else
            {
                if (!_busy)
                    SyncInFolder(Directory.GetParent(e.FullPath).FullName, true);
            }
        }

        #endregion

        /// <summary>
        /// Get the HTTP link to a file
        /// </summary>
        /// <param name='cpath'>
        /// The common path to the file/folder.
        /// </param>
        private void GetLink(string cpath)
        {
            Log.Write(l.Debug, "---------------\n Getting link for {0}", cpath);
            string newlink = Common.noSlashes(Profile.HttpPath) + @"/";

            if (!Common.noSlashes(newlink).StartsWith("http://") && !Common.noSlashes(newlink).StartsWith("https://"))
            {
                newlink = @"http://" + newlink;
            }

            if (newlink.EndsWith("/"))
                newlink = newlink.Substring(0, newlink.Length - 1);

            if (cpath.StartsWith("/"))
                cpath = cpath.Substring(1);

            newlink = string.Format("{0}/{1}", newlink, cpath);
            newlink = newlink.Replace(@" ", @"%20");

            link = newlink.Replace(" ", "%20");
            Log.Write(l.Debug, "-----------------> link: {0}", link);
            Get_Recent(cpath);

            Log.Write(l.Debug, "**************");
            Log.Write(l.Debug, "HTTP Link is: " + newlink);
            Log.Write(l.Debug, "**************");
        }

        #region Queue Operations

        int co = 0;
        private void SyncLocQueueFiles()
        {            
            List<FileQueueItem> fi = new List<FileQueueItem>(fQueue.List());
            string name = null;

            if (!Client.CheckConnectionStatus())
                RetryConnection();

            foreach (FileQueueItem i in fi)
            {
                Log.Write(l.Debug, "FileQueueItem -> cpath: {0} contains: {1} local: {2} current: {3}", i.CommonPath, fLog.Contains(i.CommonPath), fLog.getLocal(i.CommonPath), File.GetLastWriteTime(i.LocalPath));
                if (i.CommonPath.Contains("/"))
                {
                    string cpath = i.CommonPath.Substring(0, i.CommonPath.LastIndexOf("/"));
                    if (!Client.Exists(cpath))
                    {
                        Log.Write(l.Debug, "Makin directory: {0} inside {1}", cpath, Client.WorkingDirectory);
                        try
                        {
                            Client.MakeFolder(cpath);
                            fLog.putFolder(cpath);
                            PutFolderInLog(cpath);
                        }
                        catch (Exception e)
                        {
                            Common.LogError(e);
                        }
                    }
                }
                try
                {                    
                    SetTray(MessageType.Uploading, Common._name(i.CommonPath));
                    Log.Write(l.Debug, "Gonna put file {0} from queue, current path: {1}", i.CommonPath, Client.WorkingDirectory);

                    Client.Upload(i);

                    //check if the file was fully transfered. If yes, replace the old file with the temp one...
                    if (i.Size == Client.SizeOf(Common._tempName(i.CommonPath)))
                    {
                        if (Client.Exists(i.CommonPath))
                            Client.Remove(i.CommonPath);

                        Client.Rename(Common._tempName(i.CommonPath), i.CommonPath);
                    }
                    else
                    {
                        Client.Remove(Common._tempName(i.CommonPath));                        
                        continue;
                    }

                    SetTray(MessageType.AllSynced);

                    Log.Write(l.Debug, "Done");

                    name = i.LocalPath;
                    co++;
                    fQueue.Remove(i.CommonPath);
                    UpdateTheLog(i.CommonPath, Client.GetLWTof(i.CommonPath), File.GetLastWriteTime(i.LocalPath));
                    
                    GetLink(i.CommonPath);                    
                }
                catch (Exception ex)
                {
                    SetTray(MessageType.AllSynced);
                    Common.LogError(ex);
                }
            }

            if (fQueue.reCheck)
            {
                fQueue.reCheck = false;
                SyncInFolder(Profile.LocalPath, true);
            }
            else if (fQueue.Count() > 0)
            {
                Log.Write(l.Debug, "{0} have not been removed from the (local) file queue... Clearing them out...", fQueue.Count());
                SyncLocQueueFiles();
                fQueue.Clear();
            }            

            fQueue.Busy = false;

            Log.Write(l.Debug, "#Folders: {0} #Files: {1}", fQueue.CountFolders(), co);

            if (fQueue.CountFolders() > 0 && co > 0)
                ShowNotification(co, fQueue.CountFolders());
            else if (fQueue.CountFolders() == 1 && co == 0)
                ShowNotification(fQueue.LastFolder(), ChangeAction.created, false);
            else if (fQueue.CountFolders() > 0 && co == 0)
                ShowNotification(fQueue.CountFolders(), false);
            else if (fQueue.CountFolders() == 0 && co == 1)
                ShowNotification(name, ChangeAction.changed, true);
            else if (fQueue.CountFolders() == 0 && co > 1)
                ShowNotification(co, true);

            co = 0;
            fQueue.ClearCounter();

            if (dQueue.reCheck)
                DeleteFromQueue();
            if (isWebUIPending)
                wiThread.Start();
        }

        public void SyncRemQueueFiles()
        {
            int c = 0;
            List<FileQueueItem> fi = new List<FileQueueItem>(fQueue.List());
            string name = null;
            
            foreach (FileQueueItem i in fi)
            {
                if (Common.ItemGetsSynced(i.CommonPath))
                {
                    if (dQueue.Contains(i.LocalPath))
                    {
                        fQueue.Remove(i.CommonPath);
                        continue;
                    }

                    Log.Write(l.Debug, "Gonna get remote file {0} from queue, local path: {1}", i.CommonPath, i.LocalPath);
                    try
                    {
                        SetTray(MessageType.Downloading, Common._name(i.CommonPath));

                        Client.Download(i);
                        
                        //verify it was downloaded succesfully
                        FileInfo iFile = new FileInfo(Common._tempLocal(i.LocalPath));
                        if (i.Size == iFile.Length)
                        {
                            if (File.Exists(i.LocalPath))
                            {
                                fswFiles.EnableRaisingEvents = false;
                                fswFolders.EnableRaisingEvents = false;
                                Log.Write(l.Debug, "Deleting {0}", i.LocalPath);
                                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(i.LocalPath, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                                fswFiles.EnableRaisingEvents = true;
                                fswFolders.EnableRaisingEvents = true;
                            }
                            Log.Write(l.Debug, "Moving {0} to {1}", Common._tempLocal(i.LocalPath), i.LocalPath);
                            File.Move(Common._tempLocal(i.LocalPath), i.LocalPath);                            
                        }
                        else
                        {
                            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(Common._tempLocal(i.LocalPath), Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                            continue;
                        }
                        
                        Log.Write(l.Debug, "Done");
                        SetTray(MessageType.AllSynced);                        

                        name = i.LocalPath;
                        c++;
                        fQueue.Remove(i.CommonPath);
                        UpdateTheLog(i.CommonPath, Client.GetLWTof(i.CommonPath), File.GetLastWriteTime(i.LocalPath));
                        GetLink(i.CommonPath);                       
                    }
                    catch (Exception ex)
                    {
                        SetTray(MessageType.AllSynced);
                        Common.LogError(ex);
                        Log.Write(l.Debug, "current dir: {0}", Client.WorkingDirectory);

                        if (!Client.Exists(i.CommonPath))
                            fQueue.Remove(i.CommonPath);                        
                    }
                }
            }

            #region retry
            if (fQueue.hasMore())
            {
                Log.Write(l.Debug, "Retrying...");
                try
                {
                    LoginFTP();
                    Client.WorkingDirectory = Profile.RemotePath;
                }
                catch
                {
                    KillTheProcess();
                }
                fi = new List<FileQueueItem>(fQueue.List());
                foreach (FileQueueItem i in fi)
                {
                    if (Common.ItemGetsSynced(i.CommonPath))
                    {
                        Log.Write(l.Debug, "Gonna retry to get remote file {0} from queue, local path: {1}", i.CommonPath, i.LocalPath);
                        try
                        {
                            Client.Download(i);

                            //verify it was downloaded succesfully
                            FileInfo iFile = new FileInfo(Common._tempLocal(i.LocalPath));
                            if (i.Size == iFile.Length)
                            {
                                if (File.Exists(i.LocalPath))
                                {
                                    fswFiles.EnableRaisingEvents = false;
                                    fswFolders.EnableRaisingEvents = false;
                                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(i.LocalPath, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                                    fswFiles.EnableRaisingEvents = true;
                                    fswFolders.EnableRaisingEvents = true;
                                }

                                File.Move(Common._tempLocal(i.LocalPath), i.LocalPath);
                            }
                            else
                            {
                                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(Common._tempLocal(i.LocalPath), Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                                continue;
                            }

                            Log.Write(l.Debug, "Done");
                            SetTray(MessageType.AllSynced);

                            name = i.LocalPath;
                            c++;
                            fQueue.Remove(i.CommonPath);
                            UpdateTheLog(i.CommonPath, Client.GetLWTof(i.CommonPath), File.GetLastWriteTime(i.LocalPath));
                        }
                        catch (Exception ex)
                        {
                            SetTray(MessageType.AllSynced);
                            Common.LogError(ex);
                            Log.Write(l.Debug, "current dir: {0}", Client.WorkingDirectory);

                            if (!Client.Exists(i.CommonPath))
                                fQueue.Remove(i.CommonPath);
                        }
                    }
                }
            }
            #endregion

            //fQueue.ClearCounter();
            if (fQueue.Count() > 0)
            {
                Log.Write(l.Debug, "{0} have not been removed from the (remote) file queue... Clearing them out...", fQueue.Count());
                SyncRemQueueFiles();
                fQueue.Clear();
            }

            fQueue.Busy = false;

            Log.Write(l.Debug, "File counter: {0} folder counter: {1}", c, fQueue.CountFolders());

            if (fQueue.CountFolders() == 0 && c == 1)
                ShowNotification(name, ChangeAction.changed, true);
            else if (fQueue.CountFolders() == 1 && c == 0)
                ShowNotification(fQueue.LastFolder(), ChangeAction.created, false);
            else if (fQueue.CountFolders() > 0 && c > 0)
                ShowNotification(c, fQueue.CountFolders());
            else if (fQueue.CountFolders() > 0 && c == 0)
                ShowNotification(fQueue.CountFolders(), false);
            else if (fQueue.CountFolders() == 0 && c > 1)
                ShowNotification(c, true);
            fQueue.ClearCounter();
            /*if (c == 1)
                ShowNotification(name, ChangeAction.changed, true);
            else if (c > 1)
                ShowNotification(c, true);	*/            
        }

        /// <summary>
        /// Check the files inside the specified folder for changes and, if found, 
        /// put the changed item in the file queue.
        /// </summary>
        /// <param name='path'>
        /// the local path to search inside.
        /// </param>
        private void SyncInFolder(string path)
        {
            if (!Client.CheckConnectionStatus())
                RetryConnection();

            Syncing();
            fQueue.Busy = true;
            DirectoryInfo di = new DirectoryInfo(path);

            string cparent = Common.GetComPath(path, true);

            Log.Write(l.Debug, "Syncing local folder: {0} cparent: {1}", path, cparent);
            bool cParentExists = (cparent.Equals("/") || cparent.Equals(@"\") || cparent.Equals("")) ? true : Client.Exists(cparent);

            foreach (FileInfo fi in di.GetFiles())
            {
                string cpath = Common.GetComPath(fi.FullName, true);

                if (!Common.ItemGetsSynced(cpath))
                    continue;

                if (cParentExists)
                {
                    Log.Write(l.Debug, "~~Cpath: {0} lLWT: {1} LogLWT: {2} in {3}", cpath, fi.LastWriteTime.ToString(), fLog.getLocal(cpath).ToString(), Client.WorkingDirectory);
                    
                    if (!fi.FullName.EndsWith("~") && !fi.Name.StartsWith(".goutputstream")) //&& !fQueue.Contains(cpath))
                    {
                        bool ex = false;
                        try
                        {
                            ex = Client.Exists(cpath);
                        }
                        catch { }

                        Log.Write(l.Debug, "Exists: {0} contains: {1}", ex, fLog.Contains(cpath));

                        if (!fLog.Contains(cpath) || !ex)
                        {
                            //Log.Write(l.Debug, "ADDED! cpath: {0} contains: {1} local: {2} current: {3}", cpath, fLog.Contains(cpath), fLog.getLocal(cpath), fi.LastWriteTime.ToString());
                            fQueue.Add(cpath, fi.FullName, fi.Length, TypeOfTransfer.Create);
                        }
                        else if (fLog.getLocal(cpath).ToString() != fi.LastWriteTime.ToString())
                        {
                            //Log.Write(l.Debug, "ADDED cpath: {0} contains: {1} local: {2} current: {3}", cpath, fLog.Contains(cpath), fLog.getLocal(cpath), fi.LastWriteTime.ToString());
                            fQueue.Add(cpath, fi.FullName, fi.Length, TypeOfTransfer.Change);
                        }
                    }
                }
                else
                    fQueue.Add(cpath, fi.FullName, fi.Length, TypeOfTransfer.Create);
            }

            SyncLocQueueFiles();
            Log.Write(l.Debug, "---------------------");
            DoneSyncing();
        }

        private void SyncInFolder(string path, bool recursive)
        {
            if (fQueue.reCheck)
                fQueue.reCheck = false;

            try
            {
                Syncing();
                fQueue.Busy = true;

                DirectoryInfo di = new DirectoryInfo(path);

                string cparent = Common.GetComPath(path, true);
                Log.Write(l.Debug, "Syncing local folder (recursive): {0} cparent: {1}", path, cparent);

                string cp = (path == Profile.LocalPath) ? "." : cparent;
                List<string> RemoteFilesList = new List<string>(Client.FullRemoteListInside(cp));

                bool cParentExists = (cparent.Equals("/") || cparent.Equals(@"\") || cparent.Equals("")) ? true : RemoteFilesList.Contains(cparent);

                SetTray(MessageType.Listing);

                foreach (DirectoryInfo d in di.GetDirectories("*", SearchOption.AllDirectories))
                {
                    string cpath = Common.GetComPath(d.FullName, true);
                    if (!RemoteFilesList.Contains(cpath))
                    {
                        Log.Write(l.Debug, "Making directory: {0}", cpath);
                        Client.MakeFolder(cpath);

                        GetLink(cpath);

                        fLog.putFolder(cpath);
                        PutFolderInLog(cpath);

                        fQueue.AddFolder(cpath);
                    }
                }

                foreach (FileInfo fi in di.GetFiles("*", SearchOption.AllDirectories))
                {
                    string cpath = Common.GetComPath(fi.FullName, true);

                    if (!Common.ItemGetsSynced(cpath))
                        continue;

                    if (cParentExists)
                    {
                        Log.Write(l.Debug, "~~Cpath: {0} lLWT: {1} LogLWT: {2} in {3}", cpath, fi.LastWriteTime.ToString(), fLog.getLocal(cpath).ToString(), Client.WorkingDirectory);

                        if (!fi.FullName.EndsWith("~") && !fi.Name.StartsWith(".goutputstream")) //&& !fQueue.Contains(cpath))
                        {
                            bool ex = false;
                            try
                            {
                                ex = RemoteFilesList.Contains(cpath);
                            }
                            catch { }

                            Log.Write(l.Debug, "Exists: {0} contains: {1}", ex, fLog.Contains(cpath));

                            if (!fLog.Contains(cpath) || !ex)
                            {
                                //Log.Write(l.Debug, "ADDED! cpath: {0} contains: {1} local: {2} current: {3}", cpath, fLog.Contains(cpath), fLog.getLocal(cpath), fi.LastWriteTime.ToString());
                                fQueue.Add(cpath, fi.FullName, fi.Length, TypeOfTransfer.Create);
                            }
                            else if (fLog.getLocal(cpath).ToString() != fi.LastWriteTime.ToString())
                            {
                                //Log.Write(l.Debug, "ADDED cpath: {0} contains: {1} local: {2} current: {3}", cpath, fLog.Contains(cpath), fLog.getLocal(cpath), fi.LastWriteTime.ToString());
                                fQueue.Add(cpath, fi.FullName, fi.Length, TypeOfTransfer.Change);
                            }
                        }
                    }
                    else
                        fQueue.Add(cpath, fi.FullName, fi.Length, TypeOfTransfer.Create);
                }

                SetTray(MessageType.AllSynced);

                SyncLocQueueFiles();
                Log.Write(l.Debug, "---------------------");
                DoneSyncing();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
                if (!Client.CheckConnectionStatus())
                    RetryConnection();
                SyncInFolder(path, true);
            }
        }

        private void DeleteFromQueue()
        {            
            Log.Write(l.Debug, "About to delete {0} item(s) from queue", dQueue.Count);
            if (!Client.CheckConnectionStatus())
                RetryConnection();

            while (dQueue.Count > 0 || dQueue.reCheck) 
            {
                Log.Write(l.Debug, "dQueue.Count: {0}", dQueue.Count.ToString());
                dQueue.Busy = true;

                List<string> li = new List<string>(dQueue.List);
                foreach (string s in li)
                {
                    Log.Write(l.Debug, "Found in dQueue: {0}", s);
                    if (!Client.Exists(Common.GetComPath(s, true)))
                        dQueue.Remove(s);
                }
                
                List<string> alreadyChecked = new List<string>();

                foreach (string s in localFiles)
                {
                    if (alreadyChecked.Contains(s) || !Common.ItemGetsSynced(Common.GetComPath(s, true)))
                    {
                        dQueue.Remove(s);
                        continue;
                    }
                    
                    if (!File.Exists(s))
                    {
                        try
                        {
                            Log.Write(l.Debug, "File {0} was deleted, cpath: {1}", s, Common.GetComPath(s, true));
                            Client.Remove(Common.GetComPath(s, true));
                        }
                        catch { }

                        dQueue.LastItem = new KeyValuePair<string, bool>(Common._name(s), true);
                        dQueue.Counter++;
                        RemoveFromLog(Common.GetComPath(s, true));
                        dQueue.Remove(s);
                        alreadyChecked.Add(s);
                    }
                }

                foreach (string s in localFolders)
                {
                    if (alreadyChecked.Contains(s))
                        continue;

                    if (!Directory.Exists(s))
                    {
                        Log.Write(l.Debug, "Folder {0} was deleted", s);
                        Client.RemoveFolder(Common.GetComPath(s, true));

                        dQueue.LastItem = new KeyValuePair<string, bool>(Common._name(s), false);
                        dQueue.Counter++;
                        dQueue.Remove(s);
                        alreadyChecked.Add(s);
                    }
                }

                foreach (string s in alreadyChecked)
                    if (localFiles.Contains(s))
                        localFiles.Remove(s);

                if (dQueue.Count <= 0)
                    dQueue.reCheck = false;
            }
            if (dQueue.Count <= 0)
            {
                dQueue.reCheck = false;
                dQueue.Busy = false;
                if (dQueue.Counter == 1)
                    ShowNotification(dQueue.LastItem.Key, ChangeAction.deleted, dQueue.LastItem.Value);
                else if (dQueue.Counter > 1)
                    ShowNotification(dQueue.Counter, ChangeAction.deleted);
                dQueue.Counter = 0;
                dQueue.Clear();
            }
            
        }

        #endregion

        #region Notifications

        /// <summary>
        /// Shows a notification regarding an action on one file OR folder
        /// </summary>
        /// <param name="name">The name of the file or folder</param>
        /// <param name="ca">The ChangeAction</param>
        /// <param name="file">True if file, False if Folder</param>
        private void ShowNotification(string name, ChangeAction ca, bool file)
        {
            if (Settings.ShowNots)
            {
                name = Common._name(name);

                string b = string.Format(Get_Message(ca, file), name);

                tray.ShowBalloonTip(100, "FTPbox", b, ToolTipIcon.Info);
            }
        }

        /// <summary>
        /// Shows a notification that a file or folder was renamed.
        /// </summary>
        /// <param name="name">The old name of the file/folder</param>
        /// <param name="ca">file/folder ChangeAction, should be ChangeAction.renamed</param>
        /// <param name="newname">The new name of the file/folder</param>
        private void ShowNotification(string name, ChangeAction ca, string newname)
        {
            if (Settings.ShowNots)
            {
                name = Common._name(name);
                newname = Common._name(newname);

                string body = string.Format(Get_Message(ChangeAction.renamed, true), name, newname);

                string b = string.Format(Get_Message(ChangeAction.renamed, true), name, newname);
                tray.ShowBalloonTip(100, "FTPbox", b, ToolTipIcon.Info);
            }
        }
        
        /// <summary>
        /// Shows a notification of how many files OR folders were updated
        /// </summary>
        /// <param name="i"># of files or folders</param>
        /// <param name="file">True if files, False if folders</param>
        private void ShowNotification(int i, bool file)
        {
            if (Settings.ShowNots && i > 0)
            {
                string type = (file) ? _(MessageType.Files) : _(MessageType.Folders);
                string change = (file) ? _(MessageType.FilesOrFoldersUpdated) : _(MessageType.FilesOrFoldersCreated);

                //string b = String.Format(Get_Message(name, file), name);

                string body = string.Format(change, i.ToString(), type);
                tray.ShowBalloonTip(100, "FTPbox", body, ToolTipIcon.Info);
            }
        }

        /// <summary>
        /// Shows a notifications of how many files and how many folders were updated.
        /// </summary>
        /// <param name="f"># of files</param>
        /// <param name="d"># of folders</param>
        private void ShowNotification(int f, int d)
        {
            string fType = (f != 1) ? _(MessageType.Files) : _(MessageType.File);
            string dType = (d != 1) ? _(MessageType.Folders) : _(MessageType.Folder);

            if (Settings.ShowNots && ( f > 0 || d > 0))
            {
                string body = string.Format(_(MessageType.FilesAndFoldersChanged), d.ToString(), dType, f.ToString(), fType);
                tray.ShowBalloonTip(100, "FTPbox", body, ToolTipIcon.Info);
            }
        }

        /// <summary>
        /// Shows a notification of how many items were deleted.
        /// </summary>
        /// <param name="n"># of deleted items</param>
        /// <param name="c">ChangeAction, should be ChangeAction.deleted</param>
        private void ShowNotification(int n, ChangeAction c)
        {
            if (c == ChangeAction.deleted && Settings.ShowNots)
            {
                string body = string.Format(_(MessageType.ItemsDeleted), n);
                tray.ShowBalloonTip(100, "FTPbox", body, ToolTipIcon.Info);
            }
        }

        #endregion

        /// <summary>
        /// Show (?) the appropriate message when a link is copied
        /// </summary>
        private void LinkCopied()
        {
            if (Settings.ShowNots)
            {
                tray.ShowBalloonTip(30, "FTPbox", _(MessageType.LinkCopied), ToolTipIcon.Info);
                link = null;
            }
        }

        /// <summary>
        /// Get the list of recent files and update the tray menu
        /// </summary>
        /// <param name="cpath"></param>
        private void Get_Recent(string cpath)
        {
            string name = Common._name(cpath);
            string path = Path.Combine(Profile.LocalPath, cpath);

            locLink = path;
            FileInfo f = new FileInfo(path);
            Log.Write(l.Debug, "LastWriteTime is: {0} for {1} - {2}", f.LastWriteTime.ToString(), path, Profile.LocalPath);
            
            recentFiles.Add(name, link, path, f.LastWriteTime);

            Load_Recent();
        }

        /// <summary>
        /// Removes the specified item from the recent list and from the items in the tray menu
        /// Called when the item has been deleted.
        /// </summary>
        /// <param name="cpath">The common path to the item that will be removed</param>
        private void Delete_Recent(string cpath)
        {
            try
            {
                List<RecentFileItem> oldRecentList = new List<RecentFileItem>(recentFiles.RecentList);                
                foreach (RecentFileItem f in oldRecentList)
                {
                    if (f.Name == "Not available") continue;
                    Console.WriteLine("checkin to delete recent, name: {0} path: {1} link: {2}", f.Name, f.Path, f.Link);
                    if (Common.GetComPath(f.Path, true) == cpath)
                    {
                        int ind = oldRecentList.IndexOf(f);

                        recentFiles.RecentList[ind].Name = "Not available";

                        for (int i = oldRecentList.Count - 1; i >= 0; i--)
                        {
                            if (oldRecentList[i].Name == "Not available")
                                oldRecentList.RemoveAt(i);
                        }
                        recentFiles = new RecentFiles();

                        for (int i = 0; i < oldRecentList.Count; i++)
                        {
                            recentFiles.Add(oldRecentList[i].Name, oldRecentList[i].Link, oldRecentList[i].Path, oldRecentList[i].LastWriteTime);
                        }

                        break;
                    }
                }

                Load_Recent();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        private void Load_Recent()
        {
            for (int i = 0; i < 5; i++) // recentFiles.count(); i++)
            {
                if (trayMenu.InvokeRequired)
                {
                    trayMenu.Invoke(new MethodInvoker(delegate
                    {
                        recentFilesToolStripMenuItem.DropDownItems[i].Text = (recentFiles.getName(i) == "Not available") ? _(MessageType.NotAvailable) : recentFiles.getName(i);
                        recentFilesToolStripMenuItem.DropDownItems[i].Enabled = recentFilesToolStripMenuItem.DropDownItems[i].Text != _(MessageType.NotAvailable);

                        if (recentFiles.Count >= i)
                            recentFilesToolStripMenuItem.DropDownItems[i].ToolTipText = recentFiles.getDate(i).ToString("dd MMM HH:mm:ss");
                    }));
                }
                else
                {
                    recentFilesToolStripMenuItem.DropDownItems[i].Text = (recentFiles.getName(i) == "Not available") ? _(MessageType.NotAvailable) : recentFiles.getName(i);
                    recentFilesToolStripMenuItem.DropDownItems[i].Enabled = recentFilesToolStripMenuItem.DropDownItems[i].Text != _(MessageType.NotAvailable);

                    if (recentFiles.Count >= i)
                        recentFilesToolStripMenuItem.DropDownItems[i].ToolTipText = recentFiles.getDate(i).ToString("dd MMM HH:mm:ss");
                }
            }
        }

        #region translations

        /// <summary>
        /// Checks the computer's language and offers to switch to it, if available.
        /// Finally, calls Set_Language to set the form's language
        /// </summary>
        private void Get_Language()
        {
            string curlan = Settings.lang;

            if (curlan == "" || curlan == null)
            {
                string locallangtwoletter = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                string locallang;

                if (locallangtwoletter == "es")
                    locallang = "Spanish";
                else if (locallangtwoletter == "de")
                    locallang = "German";
                else if (locallangtwoletter == "fr")
                    locallang = "French";
                else if (locallangtwoletter == "nl")
                    locallang = "Dutch";
                else if (locallangtwoletter == "el")
                    locallang = "Greek";
                else if (locallangtwoletter == "it")
                    locallang = "Italian";
                else if (locallangtwoletter == "tr")
                    locallang = "Turkish";
                else if (locallangtwoletter == "pt-BR")
                    locallang = "Brazilian Portuguese";
                else if (locallangtwoletter == "fo")
                    locallang = "Faroese";
                else if (locallangtwoletter == "sv")
                    locallang = "Swedish";
                else if (locallangtwoletter == "sq")
                    locallang = "Albanian";
                else if (locallangtwoletter == "ro")
                    locallang = "Romanian";
                else if (locallangtwoletter == "ko")
                    locallang = "Korean";
                else if (locallangtwoletter == "ru")
                    locallang = "Russian";
                else if (locallangtwoletter == "vi")
                    locallang = "Vietnamese";
                else if (locallangtwoletter == "ja")
                    locallang = "Japanese";
                else if (locallangtwoletter == "hu")
                    locallang = "Hungarian";
                else if (locallangtwoletter == "no")
                    locallang = "Norwegian";
                else if (locallangtwoletter == "zh_HANS")
                    locallang = "Simplified Chinese";
                else if (locallangtwoletter == "zh_HANT")
                    locallang = "Traditional Chinese";
                else if (locallangtwoletter == "lt")
                    locallang = "Lithuanian";
                else if (locallangtwoletter == "da")
                    locallang = "Dansk";
                else if (locallangtwoletter == "pl")
                    locallang = "Polish";
                else if (locallangtwoletter == "hr")
                    locallang = "Croatian";
                else if (locallangtwoletter == "sk")
                    locallang = "Slovak";
                else if (locallangtwoletter == "pt")
                    locallang = "Portuguese";
                else if (locallangtwoletter == "gl")
                    locallang = "Galego";
                else if (locallangtwoletter == "th")
                    locallang = "Thai";
                else if (locallangtwoletter == "sl")
                    locallang = "Slovenian";
				else if (locallangtwoletter == "cs")
                    locallang = "Czech";
                else
                    locallang = "English";

                List<string> alllang = new List<string>{ "es", "de", "fr", "nl", "el", "it", "tr", "pt-BR", "fo", "sv", "sq", "ro", "ko", "ru", "ja", "no", "hu", "vi", "zh_HANS", "zh_HANT", "lt", "da", "pl", "hr", "sk", "pt", "gl", "th", "sl", "cs" };

                if (alllang.Contains(locallangtwoletter))
                {
                    string msg = string.Format("FTPbox detected that you use {0} as your computer language. Do you want to use {0} as the language of FTPbox as well?", locallang);
                    DialogResult x = MessageBox.Show(msg, "FTPbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (x == DialogResult.Yes)
                    {
                        Set_Language(locallangtwoletter);
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

        /// <summary>
        /// Translate all controls and stuff to the given language.
        /// </summary>
        /// <param name="lan">The language to translate to in 2-letter format</param>
        private void Set_Language(string lan)
        {
            Profile.Language = lan;
            Log.Write(l.Debug, "Changing language to: {0}", lan);

            this.Text = "FTPbox | " + Common.Languages.Get(lan + "/main_form/options", "Options");
            //general tab
            tabGeneral.Text = Common.Languages.Get(lan + "/main_form/general", "General");
            tabAccount.Text = Common.Languages.Get(lan + "/main_form/account", "Account");
            gAccount.Text = "FTP " + Common.Languages.Get(lan + "/main_form/account", "Account");
            labHost.Text = Common.Languages.Get(lan + "/main_form/host", "Host") + ":";
            labUN.Text = Common.Languages.Get(lan + "/main_form/username", "Username") + ":";
            labPort.Text = Common.Languages.Get(lan + "/main_form/port", "Port") + ":";
            labMode.Text = Common.Languages.Get(lan + "/main_form/mode", "Mode") + ":";
            //bAddFTP.Text = languages.Get(lan + "/main_form/change", "Change");
            gApp.Text = Common.Languages.Get(lan + "/main_form/application", "Application");
            gWebInt.Text = Common.Languages.Get(lan + "/web_interface/web_int", "Web Interface");
            chkWebInt.Text = Common.Languages.Get(lan + "/web_interface/use_webint", "Use the Web Interface");
            labViewInBrowser.Text = Common.Languages.Get(lan + "/web_interface/view", "(View in browser)");
            chkShowNots.Text = Common.Languages.Get(lan + "/main_form/show_nots", "Show notifications");
            chkStartUp.Text = Common.Languages.Get(lan + "/main_form/start_on_startup", "Start on system start-up");            
            //ftpbox tab
            gDetails.Text = Common.Languages.Get(lan + "/main_form/details", "Details");
            labRemPath.Text = Common.Languages.Get(lan + "/main_form/remote_path", "Remote Path") + ":";
            labLocPath.Text = Common.Languages.Get(lan + "/main_form/local_path", "Local Path") + ":";
            bChangeBox.Text = Common.Languages.Get(lan + "/main_form/change", "Change");
            gLinks.Text = Common.Languages.Get(lan + "/main_form/links", "Links");
            labFullPath.Text = Common.Languages.Get(lan + "/main_form/account_full_path", "Account's full path") + ":";
            labLinkClicked.Text = Common.Languages.Get(lan + "/main_form/when_not_clicked", "When tray notification or recent file is clicked") + ":";
            rOpenInBrowser.Text = Common.Languages.Get(lan + "/main_form/open_in_browser", "Open link in default browser");
            rCopy2Clipboard.Text = Common.Languages.Get(lan + "/main_form/copy", "Copy link to clipboard");
            rOpenLocal.Text = Common.Languages.Get(lan + "/main_form/open_local", "Open the local file");
            //filters
            tabFilters.Text = Common.Languages.Get(lan + "/main_form/file_filters", "Filters");
            gSelectiveSync.Text = Common.Languages.Get(lan + "/main_form/selective", "Selective Sync");
            labSelectFolders.Text = Common.Languages.Get(lan + "/main_form/selective_info", "Uncheck the items you don't want to sync") + ":";
            bRefresh.Text = Common.Languages.Get(lan + "/main_form/refresh", "Refresh");
            gFileFilters.Text = Common.Languages.Get(lan + "/main_form/file_filters", "Filters");
            labSelectExtensions.Text = Common.Languages.Get(lan + "/main_form/ignored_extensions", "Ignored Extensions") + ":";
            bAddExt.Text = Common.Languages.Get(lan + "/new_account/add", "Add");
            bRemoveExt.Text = Common.Languages.Get(lan + "/main_form/remove", "Remove");
            labAlsoIgnore.Text = Common.Languages.Get(lan + "/main_form/also_ignore", "Also ignore") + ":";
            cIgnoreDotfiles.Text = Common.Languages.Get(lan + "/main_form/dotfiles", "dotfiles");
            cIgnoreTempFiles.Text = Common.Languages.Get(lan + "/main_form/temp_files", "Temporary Files");
            cIgnoreOldFiles.Text = Common.Languages.Get(lan + "/main_form/old_files", "Files modified before") + ":";
            //bandwidth tab
            tabBandwidth.Text = Common.Languages.Get(lan + "/main_form/bandwidth", "Bandwidth");
            gSyncing.Text = Common.Languages.Get(lan + "/main_form/sync_freq", "Sync Frequency");
            labSyncWhen.Text = Common.Languages.Get(lan + "/main_form/sync_when", "Synchronize remote files");
            cAuto.Text = Common.Languages.Get(lan + "/main_form/auto", "automatically every");
            labSeconds.Text = Common.Languages.Get(lan + "/main_form/seconds", "seconds");
            cManually.Text = Common.Languages.Get(lan + "/main_form/manually", "manually");
            gLimits.Text = Common.Languages.Get(lan + "/main_form/speed_limits", "Speed Limits");
            labDownSpeed.Text = Common.Languages.Get(lan + "/main_form/limit_download", "Limit Download Speed");
            labUpSpeed.Text = Common.Languages.Get(lan + "/main_form/limit_upload", "Limit Upload Speed");
            labNoLimits.Text = Common.Languages.Get(lan + "/main_form/no_limits", "( set to 0 for no limits )");
            //language tab
            tabLanguage.Text = Common.Languages.Get(lan + "/main_form/language", "Language");
            //about tab
            tabAbout.Text = Common.Languages.Get(lan + "/main_form/about", "About");
            labCurVersion.Text = Common.Languages.Get(lan + "/main_form/current_version", "Current Version") + ":";
            labTeam.Text = Common.Languages.Get(lan + "/main_form/team", "The Team") + ":";
            labSite.Text = Common.Languages.Get(lan + "/main_form/website", "Official Website") + ":";
            labContact.Text = Common.Languages.Get(lan + "/main_form/contact", "Contact") + ":";
            labLangUsed.Text = Common.Languages.Get(lan + "/main_form/coded_in", "Coded in") + ":";
            gNotes.Text = Common.Languages.Get(lan + "/main_form/notes", "Notes");
            gContribute.Text = Common.Languages.Get(lan + "/main_form/contribute", "Contribute");
            labFree.Text = Common.Languages.Get(lan + "/main_form/ftpbox_is_free", "- FTPbox is free and open-source");
            labContactMe.Text = Common.Languages.Get(lan + "/main_form/contact_me", "- Feel free to contact me for anything.");
            linkLabel1.Text = Common.Languages.Get(lan + "/main_form/report_bug", "Report a bug");
            linkLabel2.Text = Common.Languages.Get(lan + "/main_form/request_feature", "Request a feature");
            labDonate.Text = Common.Languages.Get(lan + "/main_form/donate", "Donate") + ":";
            labSupportMail.Text = "support@ftpbox.org";
            //tray
            optionsToolStripMenuItem.Text = Common.Languages.Get(lan + "/main_form/options", "Options");
            recentFilesToolStripMenuItem.Text = Common.Languages.Get(lan + "/tray/recent_files", "Recent Files");
            aboutToolStripMenuItem.Text = Common.Languages.Get(lan + "/main_form/about", "About");
            SyncToolStripMenuItem.Text = Common.Languages.Get(lan + "/tray/start_syncing", "Start Syncing");
            exitToolStripMenuItem.Text = Common.Languages.Get(lan + "/tray/exit", "Exit");

            for (int i = 0; i < 5; i++)
            {
                if (trayMenu.InvokeRequired)
                {
                    trayMenu.Invoke(new MethodInvoker(delegate
                    {
                        foreach (ToolStripItem t in recentFilesToolStripMenuItem.DropDownItems)
                            if (!t.Enabled)
                                t.Text = _(MessageType.NotAvailable);
                    }));
                }
                else
                {
                    foreach (ToolStripItem t in recentFilesToolStripMenuItem.DropDownItems)
                        if (!t.Enabled)
                            t.Text = _(MessageType.NotAvailable);
                    /*
                    bool e = !recentFilesToolStripMenuItem.DropDownItems[i].Enabled;
                    recentFilesToolStripMenuItem.DropDownItems[i].Text = (e) ? _(MessageType.NotAvailable) : recentFiles.getName(i); */
                }
            }

            SetTray(_lastTrayStatus);

            Settings.lang = lan;         
        }

        private string Get_Message(ChangeAction ca, bool file)
        {
            string fileorfolder = (file) ? _(MessageType.File) : _(MessageType.Folder);
            
            if (ca == ChangeAction.created)
            {
                return fileorfolder + " " + _(MessageType.ItemCreated);
            }
            else if (ca == ChangeAction.deleted)
            {
                return fileorfolder + " " + _(MessageType.ItemDeleted);
            }
            else if (ca == ChangeAction.renamed)
            {
                return _(MessageType.ItemRenamed);
            }
            else if (ca == ChangeAction.changed)
            {
                return fileorfolder + " " + _(MessageType.ItemChanged);
            }
            else //if "updated"
            {
                return fileorfolder + " " + _(MessageType.ItemUpdated);
            }
        }           

        private void cmbLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            string lan = cmbLang.Text.Substring(cmbLang.Text.IndexOf("(") + 1);
            lan = lan.Substring(0, lan.Length - 1);
            try
            {
                Set_Language(lan);
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        /// <summary>
        /// When the user changes to another language, translate every label etc to that language.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string text = "(en)";
            foreach (ListViewItem l in listView1.Items)
                if (listView1.SelectedItems.Contains(l))
                {
                    text = l.Text;
                    break;
                }
            string lan = text.Substring(text.IndexOf("(") + 1);
            lan = lan.Substring(0, lan.Length - 1);
            try
            {
                Set_Language(lan);
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        #endregion

        #region Tray Controls' Event Handlers

        private void tray_MouseDoubleClick(object sender, MouseEventArgs e)
        {           
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                Process.Start("explorer.exe", Profile.LocalPath); 
        }

        public bool ExitedFromTray = false;
        private void fMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ExitedFromTray && e.CloseReason != CloseReason.WindowsShutDown)
            {
                e.Cancel = true;
                this.Hide();
            }
        }
        
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KillTheProcess();
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

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int ind = 0;
            if (_trayAct == TrayAction.OpenInBrowser)
            {
                try
                {
                    Process.Start(recentFiles.getLink(ind));
                }
                catch { }

            }
            else if (_trayAct == TrayAction.CopyLink)
            {
                try
                {
                    Clipboard.SetText(recentFiles.getLink(ind));
                    LinkCopied();
                }
                catch { }
            }
            else
            {
                Log.Write(l.Debug, "Opening local file: {0}", recentFiles.getPath(ind));
                Process.Start(recentFiles.getPath(ind));
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            int ind = 1;
            if (_trayAct == TrayAction.OpenInBrowser)
            {
                try
                {
                    Process.Start(recentFiles.getLink(ind));
                }
                catch { }

            }
            else if (_trayAct == TrayAction.CopyLink)
            {
                try
                {
                    Clipboard.SetText(recentFiles.getLink(ind));
                    LinkCopied();
                }
                catch { }
            }
            else
            {
                Log.Write(l.Debug, "Opening local file: {0}", recentFiles.getPath(ind));
                Process.Start(recentFiles.getPath(ind));
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            int ind = 2;
            if (_trayAct == TrayAction.OpenInBrowser)
            {
                try
                {
                    Process.Start(recentFiles.getLink(ind));
                }
                catch { }

            }
            else if (_trayAct == TrayAction.CopyLink)
            {
                try
                {
                    Clipboard.SetText(recentFiles.getLink(ind));
                    LinkCopied();
                }
                catch { }
            }
            else
            {
                Log.Write(l.Debug, "Opening local file: {0}", recentFiles.getPath(ind));
                Process.Start(recentFiles.getPath(ind));
            }
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            int ind = 3;
            if (_trayAct == TrayAction.OpenInBrowser)
            {
                try
                {
                    Process.Start(recentFiles.getLink(ind));
                }
                catch { }

            }
            else if (_trayAct == TrayAction.CopyLink)
            {
                try
                {
                    Clipboard.SetText(recentFiles.getLink(ind));
                    LinkCopied();
                }
                catch { }
            }
            else
            {
                Log.Write(l.Debug, "Opening local file: {0}", recentFiles.getPath(ind));
                Process.Start(recentFiles.getPath(ind));
            }
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            int ind = 4;
            if (_trayAct == TrayAction.OpenInBrowser)
            {
                try
                {
                    Process.Start(recentFiles.getLink(ind));
                }
                catch { }

            }
            else if (_trayAct == TrayAction.CopyLink)
            {
                try
                {
                    Clipboard.SetText(recentFiles.getLink(ind));
                    LinkCopied();
                }
                catch { }
            }
            else
            {
                Log.Write(l.Debug, "Opening local file: {0}", recentFiles.getPath(ind));
                Process.Start(recentFiles.getPath(ind));
            }
        }        

        private void tray_BalloonTipClicked(object sender, EventArgs e)
        {
            if (LinkHasWebint())
                Process.Start(link);
            else
            {
                if ((Control.MouseButtons & MouseButtons.Right) != MouseButtons.Right)
                {
                    if (_trayAct == TrayAction.OpenInBrowser)
                    {
                        if (link != null && link != "")
                        {
                            try
                            {
                                Process.Start(link);
                            }
                            catch
                            {
                                //Gotta catch 'em all 
                            }
                        }
                    }
                    else if (_trayAct == TrayAction.CopyLink)
                    {
                        if (link != null && link != "")
                        {
                            try
                            {
                                Clipboard.SetText(link);
                            }
                            catch
                            {
                                //Gotta catch 'em all 
                            }

                            LinkCopied();
                        }
                    }
                    else
                    {
                        if (locLink != null && locLink != "")
                        {
                            try
                            {
                                Process.Start(locLink);
                            }
                            catch
                            {
                                //Gotta catch 'em all
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the link is a link to the WebUI or not. 
        /// If it is, the app should open the link in browser regardless of the chosen options.
        /// </summary>
        /// <returns></returns>
        private bool LinkHasWebint()
        {
            if (link == null)
                return false;

            if (link.EndsWith("webint"))
                return true;
            
            return false;
        }

        #endregion        

        #region Form Controls
        private void rOpenInBrowser_CheckedChanged(object sender, EventArgs e)
        {
            if (rOpenInBrowser.Checked)
            {
                _trayAct = TrayAction.OpenInBrowser;
                Settings.SaveTrayAction(TrayAction.OpenInBrowser);
            }
        }

        private void rCopy2Clipboard_CheckedChanged(object sender, EventArgs e)
        {
            if (rCopy2Clipboard.Checked)
            {
                _trayAct = TrayAction.CopyLink;
                Settings.SaveTrayAction(TrayAction.CopyLink);
            }
        }

        private void rOpenLocal_CheckedChanged(object sender, EventArgs e)
        {
            if (rOpenLocal.Checked)
            {
                _trayAct = TrayAction.OpenLocalFile;
                Settings.SaveTrayAction(TrayAction.OpenLocalFile);
            }
        }

        private void bChangeBox_Click(object sender, EventArgs e)
        {
            ChangeAccount();
            //Application.Restart();
        }

        private void tParent_TextChanged(object sender, EventArgs e)
        {
            Settings.ftpParent = tParent.Text;
            Profile.HttpPath = tParent.Text;
        }

        #endregion

        #region check internet connection
        private bool OfflineMode = false;
        [DllImport("wininet.dll")]        
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        public void OnNetworkChange(object sender, EventArgs e)
        {
            try
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    if (OfflineMode)
                    {
                        while (!ConnectedToInternet())
                            Thread.Sleep(5000);
                        StartUpWork();
                    }
                    OfflineMode = false;
                }
                else
                {
                    if (!OfflineMode)
                    {
                        Client.Disconnect();
                        fswFiles.Dispose();
                        fswFolders.Dispose();
                    }
                    OfflineMode = true;
                    SetTray(MessageType.Offline);
                }
            }
            catch { }
        }

        /// <summary>
        /// Check if internet connection is available
        /// </summary>
        /// <returns></returns>
        public static bool ConnectedToInternet()
        {
            int Desc;
            return InternetGetConnectedState(out Desc, 0);
        }

        #endregion

        #region Remote Sync

        /// <summary>
        /// Starts the thread that checks the remote files.
        /// </summary>
        public void StartRemoteSync(string path)
        {
            if (loggedIn)
            {
                if (Profile.SyncingMethod == SyncMethod.Automatic) SyncToolStripMenuItem.Enabled = false;
                rcThread = new Thread(SyncRemote);
                rcThread.Start(path);
            }
        }

        /// <summary>
        /// Starts the thread that checks the remote files.
        /// Called from the timer, when remote syncing is automatic.
        /// </summary>
        /// <param name="state"></param>
        public void StartRemoteSync(object state)
        {
            if (loggedIn && !_busy)
            {
                if (Profile.SyncingMethod == SyncMethod.Automatic) SyncToolStripMenuItem.Enabled = false;
                Log.Write(l.Debug, "Starting remote sync...");
                rcThread = new Thread(SyncRemote);
                rcThread.Start(".");
            }            
        }

        List<string> allFilesAndFolders;
        /// <summary>
        /// Sync remote files to the local folder
        /// </summary>
        public void SyncRemote(object path)
        {
            allFilesAndFolders = new List<string>();
            
            fQueue.Busy = true;

            //Syncing();

            if (!Client.CheckConnectionStatus())
                RetryConnection();

            Log.Write(l.Debug, "Current directory: {0}", Client.WorkingDirectory);

            SetTray(MessageType.Listing);
            
            foreach (ClientItem f in Client.ListRecursive((string)path))
            {
	            allFilesAndFolders.Add(Common.GetComPath(f.FullPath, false));
	            string cpath = Common.GetComPath(f.FullPath, false);

	            if (!Common.ItemGetsSynced(cpath))
		            continue;

	            if (f.Type == ClientItemType.Folder)
	            {
		            Log.Write(l.Debug, "~~~~~~~~~> found directory: {0}", f.FullPath);
		            string lpath = System.IO.Path.Combine(Profile.LocalPath, cpath);
		            if (!Directory.Exists(lpath) && !dQueue.Contains(lpath))
		            {
                        fswFiles.EnableRaisingEvents = false;
                        fswFolders.EnableRaisingEvents = false;
			            Directory.CreateDirectory(lpath);
                        fswFiles.EnableRaisingEvents = true;
                        fswFolders.EnableRaisingEvents = true;

			            fQueue.AddFolder(cpath);

			            fLog.putFolder(cpath);
			            PutFolderInLog(cpath);

			            GetLink(cpath);
		            }
	            }
	            else if (f.Type == ClientItemType.File)
	            {
		            string lpath = System.IO.Path.Combine(Profile.LocalPath, cpath.Replace("/", @"\"));
		            Log.Write(l.Debug, "Found: {0} cpath is: {1} lpath: {2} type: {3}", f.Name, cpath, lpath, f.Type.ToString());
		            if (File.Exists(lpath))
			            CheckExistingFile(cpath, f.LastWriteTime, lpath, f.Size);
		            else
		            {
			            fQueue.Add(cpath, lpath, f.Size, TypeOfTransfer.Create);
		            }
	            }
            }

            SetTray(MessageType.AllSynced);

            SyncRemQueueFiles();

            string locpath = ((string)path == ".") ? Profile.LocalPath : Path.Combine(Profile.LocalPath, (string)path);

            DirectoryInfo di = new DirectoryInfo(locpath);
            int count = 0;
            string lastname = null;
            bool lastIsFile = false;

            foreach (FileInfo f in di.GetFiles("*", SearchOption.AllDirectories))
            {
                string cpath = Common.GetComPath(f.FullName, true);
                if (!Common.ItemGetsSynced(cpath)) continue;
                if (Common.ParentFolderHasSpace(cpath) && Profile.Protocol != FtpProtocol.SFTP) continue;

                if (!allFilesAndFolders.Contains(cpath) && fLog.Contains(cpath))
                {
                    count++;
                    lastname = f.Name;
                    lastIsFile = Common.PathIsFile(f.FullName);
                    Log.Write(l.Info, "Deleting local file: {0}", f.FullName);
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(f.FullName, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin); //File.Delete(f.FullName);                    
                    RemoveFromLog(cpath);                    
                }
            }

            foreach (DirectoryInfo d in di.GetDirectories("*", SearchOption.AllDirectories))
            {
                string cpath = Common.GetComPath(d.FullName, true);
                if (!Common.ItemGetsSynced(cpath)) continue;
                if (Common.ParentFolderHasSpace(cpath) && Profile.Protocol != FtpProtocol.SFTP) continue;

                if (!allFilesAndFolders.Contains(cpath) && fLog.Folders.Contains(cpath))
                {
                    count++;
                    lastname = d.Name;
                    lastIsFile = false;
                    Log.Write(l.Info, "Deleting local folder: {0}", d.FullName);
                    try
                    {
                        Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(d.FullName, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);  
                        //Directory.Delete(d.FullName, true);
                        RemoveFolderFromLog(cpath);
                    }
                    catch { }
                    //RemoveFromLog(cpath);
                }
            }

            if (count == 1)
                ShowNotification(lastname, ChangeAction.deleted, lastIsFile);
            if (count > 1)
                ShowNotification(count, ChangeAction.deleted);

            //DoneSyncing();

            Log.Write(l.Debug, "~~~~~~~~~~~~~~~~~");

            if (fQueue.reCheck)
                SyncInFolder(Profile.LocalPath, true);
            if (dQueue.reCheck)
                DeleteFromQueue();
            if (isWebUIPending)
                wiThread.Start();

            arEvent.Set();

            arEvent.WaitOne();
            Log.Write(l.Debug, "Checking menu sync done!");

            if (Profile.SyncingMethod == SyncMethod.Automatic)
            {
                tSync = new System.Threading.Timer(new TimerCallback(StartRemoteSync), null, 1000 * Profile.SyncFrequency, 0);
                Log.Write(l.Debug, "Syncing set to start in {0} seconds.", Profile.SyncFrequency);
                Log.Write(l.Debug, "~~~~~~~~~~~~~~~~~");
            }
        }

        /// <summary>
        /// Checks an existing file for any changes
        /// </summary>
        /// <param name="cpath">the common path to the existing files</param>
        /// <param name="rLWT">the remote LastWriteTime of the file</param>
        /// <param name="lpath">the local path to the file</param>
        private void CheckExistingFile(string cpath, DateTime rLWT, string lpath, long size)
        {
            FileInfo fi = new FileInfo(lpath);
            DateTime lLWT = fi.LastWriteTime;

            if (Profile.Protocol != FtpProtocol.SFTP)
                rLWT = Client.GetLWTof(cpath);

            DateTime lDTLog = fLog.getLocal(cpath);
            DateTime rDTLog = fLog.getRemote(cpath);

            int rResult = DateTime.Compare(rLWT, rDTLog);
            int lResult = DateTime.Compare(lLWT, lDTLog);
            int bResult = DateTime.Compare(rLWT, lLWT);

            Log.Write(l.Debug, "Checking existing file: {0} rem: {1} remLog: {2} loc {3}", cpath, rLWT.ToString(), rDTLog.ToString(), lLWT.ToString());
            Log.Write(l.Debug, "rResult: {0} lResult: {1} bResult: {2}", rResult, lResult, bResult);

            TimeSpan rdif = rLWT - rDTLog;
            TimeSpan ldif = lLWT - lDTLog;

            if (rResult > 0 && lResult > 0 && rdif.TotalSeconds > 1 && ldif.TotalSeconds > 1)
            {
                if (rdif.TotalSeconds > ldif.TotalSeconds)
                {
                    //MessageBox.Show("rdif.TotalSeconds > ldif.TotalSeconds" + Environment.NewLine + string.Format("{0} - {1}", rLWT.ToString(), rDTLog.ToString()) + Environment.NewLine + string.Format("{0} - {1}", lLWT.ToString(), lDTLog.ToString()));
                    fQueue.Add(cpath, lpath, size, TypeOfTransfer.Change);
                }
                else
                {
                    //MessageBox.Show("rdif.TotalSeconds < ldif.TotalSeconds" + Environment.NewLine + string.Format("{0} - {1}", rLWT.ToString(), rDTLog.ToString()) + Environment.NewLine + string.Format("{0} - {1}", lLWT.ToString(), lDTLog.ToString()));
                    fQueue.reCheck = true;
                }
            }
            else if (rResult > 0 && rdif.TotalSeconds > 1)
            {
                fQueue.Add(cpath, lpath, size, TypeOfTransfer.Change);
            }
            else if (lResult > 0 && ldif.TotalSeconds > 1)
            {
                Log.Write(l.Debug, "lResult > 0 because lWT: {0} & lWTLog: {1}", lLWT, lDTLog);
                fQueue.reCheck = true;
                //doesnt seem to be required now
            }
        }

        #endregion

        #region Messages for notifications and tray text

        /// <summary>
        /// All types of messages that are shown to the user       
        /// </summary>
        public enum MessageType
        {
            ItemChanged, ItemCreated, ItemDeleted, ItemRenamed, ItemUpdated, FilesAndFoldersChanged, FilesOrFoldersUpdated, FilesOrFoldersCreated, ItemsDeleted, File, Files, Folder, Folders, LinkCopied, 
            Connecting, Disconnected, Reconnecting, Listing, Uploading, Downloading, Syncing, AllSynced, Offline, Ready, Nothing, NotAvailable
        }

        /// <summary>
        /// get translated text related to the given message type.
        /// </summary>
        /// <param name="t">the type of message to translate</param>
        /// <returns></returns>
        public string _(MessageType t)
        {
            switch (t)
            {                                        
                default:
                    return null;               
                case MessageType.ItemChanged:
                    return Common.Languages.Get(Profile.Language + "/tray/changed", "{0} was changed.");
                case MessageType.ItemCreated:
                    return Common.Languages.Get(Profile.Language + "/tray/created", "{0} was created.");
                case MessageType.ItemDeleted:
                    return Common.Languages.Get(Profile.Language + "/tray/deleted", "{0} was deleted.");
                case MessageType.ItemRenamed:
                    return Common.Languages.Get(Profile.Language + "/tray/renamed", "{0} was renamed to {1}.");
                case MessageType.ItemUpdated:
                    return Common.Languages.Get(Profile.Language + "/tray/updated", "{0} was updated.");
                case MessageType.FilesOrFoldersUpdated:
                    return Common.Languages.Get(Profile.Language + "/tray/FilesOrFoldersUpdated", "{0} {1} have been updated");
                case MessageType.FilesOrFoldersCreated:
                    return Common.Languages.Get(Profile.Language + "/tray/FilesOrFoldersCreated", "{0} {1} have been created");
                case MessageType.FilesAndFoldersChanged:
                    return Common.Languages.Get(Profile.Language + "/tray/FilesAndFoldersChanged", "{0} {1} and {2} {3} have been updated");
                case MessageType.ItemsDeleted:
                    return Common.Languages.Get(Profile.Language + "/tray/ItemsDeleted", "{0} items have been deleted.");
                case MessageType.File:
                    return Common.Languages.Get(Profile.Language + "/tray/file", "File");
                case MessageType.Files:
                    return Common.Languages.Get(Profile.Language + "/tray/files", "Files");
                case MessageType.Folder:
                    return Common.Languages.Get(Profile.Language + "/tray/folder", "Folder");
                case MessageType.Folders:
                    return Common.Languages.Get(Profile.Language + "/tray/folders", "Folders");
                case MessageType.LinkCopied:
                    return Common.Languages.Get(Profile.Language + "/tray/link_copied", "Link copied to clipboard");                                
                case MessageType.Connecting:
                    return Common.Languages.Get(Profile.Language + "/tray/connecting", "FTPbox - Connecting...");
                case MessageType.Disconnected:
                    return Common.Languages.Get(Profile.Language + "/tray/disconnected", "FTPbox - Disconnected");
                case MessageType.Reconnecting:
                    return Common.Languages.Get(Profile.Language + "/tray/reconnecting", "FTPbox - Re-Connecting...");
                case MessageType.Listing:
                    return Common.Languages.Get(Profile.Language + "/tray/listing", "FTPbox - Listing...");
                case MessageType.Uploading:
                    return "FTPbox" + Environment.NewLine + Common.Languages.Get(Profile.Language + "/tray/uploading", "Uploading {0}");
                case MessageType.Downloading:
                    return "FTPbox" + Environment.NewLine + Common.Languages.Get(Profile.Language + "/tray/downloading", "Downloading {0}");
                case MessageType.Syncing:
                    return Common.Languages.Get(Profile.Language + "/tray/syncing", "FTPbox - Syncing");
                case MessageType.AllSynced:
                    return Common.Languages.Get(Profile.Language + "/tray/synced", "FTPbox - All files synced");
                case MessageType.Offline:
                    return Common.Languages.Get(Profile.Language + "/tray/offline", "FTPbox - Offline");
                case MessageType.Ready:
                    return Common.Languages.Get(Profile.Language + "/tray/ready", "FTPbox - Ready");
                case MessageType.Nothing:
                    return "FTPbox";
                case MessageType.NotAvailable:
                    return Common.Languages.Get(Profile.Language + "/tray/not_available", "Not Available");
            }
        }

        MessageType _lastTrayStatus = MessageType.AllSynced;    //the last type of message set as tray-text. Used when changing the language.
        /// <summary>
        /// Set the tray icon and text according to the given message type
        /// </summary>
        /// <param name="m">The MessageType that defines what kind of icon/text should be shown</param>
        public void SetTray(MessageType m)
        {
            try
            {
                switch (m)
                {
                    case MessageType.AllSynced:
                        tray.Icon = FTPbox.Properties.Resources.AS;
                        tray.Text = _(MessageType.AllSynced);
                        _lastTrayStatus = MessageType.AllSynced;
                        break;
                    case MessageType.Syncing:
                        tray.Icon = FTPbox.Properties.Resources.syncing;
                        tray.Text = _(MessageType.Syncing);
                        _lastTrayStatus = MessageType.Syncing;
                        break;
                    case MessageType.Offline:
                        tray.Icon = FTPbox.Properties.Resources.offline1;
                        tray.Text = _(MessageType.Offline);
                        _lastTrayStatus = MessageType.Offline;
                        break;
                    case MessageType.Listing:
                        tray.Icon = FTPbox.Properties.Resources.AS;
                        tray.Text = (Profile.SyncingMethod == SyncMethod.Automatic) ? _(MessageType.AllSynced) : _(MessageType.Listing);
                        _lastTrayStatus = MessageType.Listing;
                        break;
                    case MessageType.Connecting:
                        tray.Icon = FTPbox.Properties.Resources.syncing;
                        tray.Text = _(MessageType.Connecting);
                        _lastTrayStatus = MessageType.Connecting;
                        break;
                    case MessageType.Disconnected:
                        tray.Icon = FTPbox.Properties.Resources.syncing;
                        tray.Text = _(MessageType.Disconnected);
                        _lastTrayStatus = MessageType.Disconnected;
                        break;
                    case MessageType.Reconnecting:
                        tray.Icon = FTPbox.Properties.Resources.syncing;
                        tray.Text = _(MessageType.Reconnecting);
                        _lastTrayStatus = MessageType.Reconnecting;
                        break;
                    case MessageType.Ready:
                        tray.Icon = FTPbox.Properties.Resources.AS;
                        tray.Text = _(MessageType.Ready);
                        _lastTrayStatus = MessageType.Ready;
                        break;
                    case MessageType.Nothing:
                        tray.Icon = FTPbox.Properties.Resources.ftpboxnew;
                        tray.Text = _(MessageType.Nothing);
                        _lastTrayStatus = MessageType.Nothing;
                        break;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        /// <summary>
        /// Set the tray icon and text according to the given message type.
        /// Used for Uploading and Downloading MessageTypes
        /// </summary>
        /// <param name="m">either Uploading or Downloading</param>
        /// <param name="name">Name of the file that is being downloaded/uploaded</param>
        public void SetTray(MessageType m, string name)
        {
            try
            {
                _lastTrayStatus = MessageType.Syncing;

                string msg = (m == MessageType.Uploading) ? string.Format(_(MessageType.Uploading), name) : string.Format(_(MessageType.Downloading), name);

                if (msg.Length > 64)
                    msg = msg.Substring(0, 54) + "..." + msg.Substring(msg.Length - 5);

                switch (m)
                {
                    case MessageType.Uploading:
                        tray.Icon = FTPbox.Properties.Resources.syncing;
                        tray.Text = msg;
                        break;
                    case MessageType.Downloading:
                        tray.Icon = FTPbox.Properties.Resources.syncing;
                        tray.Text = msg;
                        break;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        #endregion

        #region Update System
        /// <summary>
        /// checks for an update
        /// called on each start-up of FTPbox.
        /// </summary>
        private void CheckForUpdate()
        {
            try
            {
                WebClient wc = new WebClient();
                string lfile = Path.Combine(Profile.AppdataFolder, "latestversion.txt");
                wc.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadVersionFileComplete);
                wc.DownloadFileAsync(new Uri(@"http://ftpbox.org/latestversion.txt"), lfile);
            }
            catch (Exception ex)
            {
                Log.Write(l.Debug, "Error with version checking");
                Common.LogError(ex);
            }
        }

        private void DownloadVersionFileComplete(object sender, AsyncCompletedEventArgs e)
        {
            string path = Path.Combine(Profile.AppdataFolder, "latestversion.txt");
            if (!File.Exists(path)) return;

            string version = File.ReadAllText(path);
            if (version.Length == 7)
            {
                Log.Write(l.Debug, "Current Version: {0} Installed Version: {1}", version, Application.ProductVersion);

                if (version != Application.ProductVersion)
                {
                    newversion nvform = new newversion(version);
                    nvform.Tag = this;
                    nvform.ShowDialog();
                    this.Show();
                    // show dialog box for  download now, learn more and remind me next time
                }
            }
        }

        #endregion

        private void SyncToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_busy)
                StartRemoteSync(".");            
            else
                Log.Write(l.Debug, "How about you wait until the current synchronization finishes?");
        }

        /// <summary>
        /// Returns true if any operation is currently running
        /// </summary>
        private bool _busy
        {
            get
            {
                return (rcThread.IsAlive || wiThread.IsAlive || dQueue.Busy || fQueue.Busy);
            }
        }

        #region WebUI

        bool updatewebintpending = false;
        bool addremovewebintpending = false;
        bool containswebintfolder = false;
        bool changedfromcheck = true;
        public void WebIntExists()
        {
            string rpath = Profile.RemotePath;
            if (rpath != "/")
                rpath = Common.noSlashes(rpath) + "/webint";
            else
                rpath = "webint";

            Log.Write(l.Info, "Searching webint folder in path: {0} ({1}) : {2}", Profile.RemotePath, rpath, changedfromcheck.ToString());

            Log.Write(l.Info, "Webint folder exists: {0}", Client.Exists(rpath));

            if (Client.Exists(rpath))
            {
                chkWebInt.Checked = true;
                labViewInBrowser.Enabled = true;

                CheckForWebIntUpdate();
            }
            else
            {
                chkWebInt.Checked = false;
                labViewInBrowser.Enabled = false;
            }

            changedfromcheck = false;
        }

        public string get_webint_message(string not)
        {
            if (not == "downloading")
                return Common.Languages.Get(Profile.Language + "/web_interface/downloading", "The Web Interface will be downloaded.")
                    + Environment.NewLine + Common.Languages.Get(Profile.Language + "/web_interface/in_a_minute", "This will take a minute.");
            else if (not == "removing")
                return Common.Languages.Get(Profile.Language + "/web_interface/removing", "Removing the Web Interface...");
            else if (not == "updated")
                return Common.Languages.Get(Profile.Language + "/web_interface/updated", "Web Interface has been updated.")
                    + Environment.NewLine + Common.Languages.Get(Profile.Language + "/web_interface/setup", "Click here to view and set it up!");
            else if (not == "updating")
                return Common.Languages.Get(Profile.Language + "/web_interface/updating", "Updating the web interface...");
            else // if (not == "removed")
                return Common.Languages.Get(Profile.Language + "/web_interface/removed", "Web interface has been removed.");
        }

        public void AddRemoveWebInt()
        {
            isWebUIPending = false;
            if (chkWebInt.Checked)
            {
                try
                {
                    Log.Write(l.Debug, "containswebintfolder: " + containswebintfolder.ToString());
                    if (!containswebintfolder)
                        GetWebInt();
                    else
                    {
                        Log.Write(l.Debug, "Contains web int folder");
                        updatewebintpending = false;
                        DoneSyncing();
                        //ListAllFiles();
                    }
                }
                catch (Exception ex)
                {
                    chkWebInt.Checked = false;
                    Log.Write(l.Error, "Could not download web interface");
                    Common.LogError(ex);
                }
            }
            else
            {
                DeleteWebInt(false);
                //ListAllFiles();

                link = "";
                if (Settings.ShowNots)
                    tray.ShowBalloonTip(50, "FTPbox", get_webint_message("removed"), ToolTipIcon.Info);
            }

            addremovewebintpending = false;
        }

        string webui_path = null;
        public void GetWebInt()
        {
            CheckForFiles();
            link = null;
            if (Settings.ShowNots)
                tray.ShowBalloonTip(100, "FTPbox", get_webint_message("downloading"), ToolTipIcon.Info);

            string dllink = "http://ftpbox.org/webint.zip";
            webui_path = Path.Combine(Profile.AppdataFolder, "webint.zip");
            //DeleteWebInt();
            WebClient wc = new WebClient();
            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(WebIntDownloaded);
            wc.DownloadFileAsync(new Uri(dllink), webui_path);
        }

        private void WebIntDownloaded(object sender, AsyncCompletedEventArgs e)
        {
            string data = @"|\webint\layout\css|\webint\layout\images\fancybox|\webint\layout\templates|\webint\system\classes|\webint\system\config|\webint\system\js|\webint\system\logs|\webint\system\savant\Savant3\resources|";
            MakeWebIntFolders(data);

            unZip(webui_path, Path.Combine(Profile.AppdataFolder, @"WebInterface"));
            Log.Write(l.Info, "unzipped");
            updatewebintpending = true;
            try
            {
                UploadWebInt();
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
                if (Profile.Protocol != FtpProtocol.FTP)
                {   
                    //check if this is ok <-----------------------------------------
                    LoginFTP();
                    UploadWebInt();
                }
            }
            //File.Delete(Application.StartupPath + @"\webint.zip");
        }

        public void DeleteWebInt(bool updating)
        {
            Log.Write(l.Info, "gonna remove web interface");

            if (updating)
            {
                if (Settings.ShowNots)
                    tray.ShowBalloonTip(100, "FTPbox", get_webint_message("updating"), ToolTipIcon.Info);
            }
            else
            {
                if (Settings.ShowNots)
                    tray.ShowBalloonTip(100, "FTPbox", get_webint_message("removing"), ToolTipIcon.Info);
            }

            Client.RemoveFolder("webint");
            if (labViewInBrowser.InvokeRequired)
                labViewInBrowser.Invoke(new MethodInvoker(delegate
                {
                    labViewInBrowser.Enabled = false;
                }));
            else
                labViewInBrowser.Enabled = false;

            if (Profile.SyncingMethod == SyncMethod.Automatic)
            {
                tSync = new System.Threading.Timer(new TimerCallback(StartRemoteSync), null, 1000 * Profile.SyncFrequency, 0);
                Log.Write(l.Debug, "Syncing set to start in {0} seconds.", Profile.SyncFrequency);
                Log.Write(l.Debug, "~~~~~~~~~~~~~~~~~");
            }
        }

        WebBrowser webintwb;
        public void CheckForWebIntUpdate()
        {
            Log.Write(l.Debug, "Gon'check for web interface");
            try
            {
                SetTray(MessageType.Syncing);
                webintwb = new WebBrowser();
                webintwb.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webintwb_DocumentCompleted);
                webintwb.Navigate("http://ftpbox.org/webintversion.txt");
            }
            catch
            {
                SetTray(MessageType.Ready);
                //ListAllFiles();
            }
        }

        public void webintwb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                string lpath = Path.Combine(Profile.AppdataFolder, @"version.ini");
                Log.Write(l.Debug, "lpath: {0}", lpath);

                Client.Download("webint/version.ini", lpath);

                string inipath = lpath;
                Classes.IniFile ini = new Classes.IniFile(inipath);
                string currentversion = ini.ReadValue("Version", "latest");
                Log.Write(l.Info, "currentversion is: {0} when newest is: {1}", currentversion, webintwb.Document.Body.InnerText);

                if (currentversion != webintwb.Document.Body.InnerText)
                {
                    string msg = "A new version of the web interface is available, do you want to upgrade to it?";
                    if (MessageBox.Show(msg, "FTPbox - WebUI Update", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.Yes)
                    {
                        wiuThread.Start();
                    }
                }
                else
                {
                    SetTray(MessageType.AllSynced);
                    //ListAllFiles();
                }
                File.Delete(inipath);
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        private void UpdateWebIntThreadStart()
        {
            SetTray(MessageType.Syncing);
            string data = @"|\webint\layout\css|\webint\layout\images\fancybox|\webint\layout\templates|\webint\system\classes|\webint\system\config|\webint\system\js|\webint\system\logs|\webint\system\savant\Savant3\resources|";
            MakeWebIntFolders(data);
            DeleteWebInt(true);
            GetWebInt();
        }

        public void MakeWebIntFolders(string data)
        {
            List<string> all = new List<string>(data.Split('|', '|'));
            foreach (string s in all)
            {
                try
                {
                    string path = Profile.AppdataFolder + @"\WebInterface" + s;
                    Directory.CreateDirectory(path);
                    Log.Write(l.Info, "making folder: {0}", path);
                }
                catch { }
            }
        }

        static void unZip(string fi, string dir)
        {
            using (ZipInputStream s = new ZipInputStream(File.OpenRead(fi)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    Log.Write(l.Debug, theEntry.Name);

                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);

                    if (directoryName.Length > 0) { Directory.CreateDirectory(directoryName); }

                    if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }

                    if (fileName != String.Empty)
                    {
                        //Log.Write("dir: " + dir);
                        //Log.Write("filename " + fileName);
                        using (FileStream streamWriter = File.Create(String.Format(@"{0}\{1}", dir, theEntry.Name)))
                        {
                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void UploadWebInt()
        {
            Log.Write(l.Info, "Gonna upload webint");

            Syncing();
            string path = Profile.AppdataFolder + @"\WebInterface";

            foreach (string d in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
            {
                Log.Write(l.Debug, "dir: {0}", d);
                string fname = d.Substring(path.Length, d.Length - path.Length);
                fname = Common.noSlashes(fname);
                fname = fname.Replace(@"\", @"/");
                Log.Write(l.Debug, "fname: {0}", fname);

                Client.MakeFolder(fname);
            }

            foreach (string f in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                Log.Write(l.Debug, "file: {0}", f);

                FileAttributes attr = File.GetAttributes(f);
                if ((attr & FileAttributes.Directory) != FileAttributes.Directory)
                {
                    FileInfo fi = new FileInfo(f);
                    string fname = f.Substring(path.Length, f.Length - path.Length);
                    fname = Common.noSlashes(fname);
                    fname = fname.Replace(@"\", @"/");
                    string cpath = fname.Substring(0, fname.Length - fi.Name.Length);

                    if (cpath.EndsWith("/"))
                        cpath = cpath.Substring(0, cpath.Length - 1);

                    Log.Write(l.Debug, "fname: {0} | cpath: {1}", fname, cpath);

                    Client.Upload(f, fname);
                }
            }

            link = Profile.HttpPath;
            if (!link.StartsWith("http://") || !link.StartsWith("https://"))
                link = "http://" + link;
            if (link.EndsWith("/"))
                link = link + "webint";
            else
                link = link + "/webint";
            //open link in browser

            if (labViewInBrowser.InvokeRequired)
                labViewInBrowser.Invoke(new MethodInvoker(delegate
                {
                    labViewInBrowser.Enabled = true;
                }));
            else
                labViewInBrowser.Enabled = true;

            if (Settings.ShowNots)
                tray.ShowBalloonTip(50, "FTPbox", get_webint_message("updated"), ToolTipIcon.Info);

            Directory.Delete(Profile.AppdataFolder + @"\WebInterface", true);
            File.Delete(Profile.AppdataFolder + @"\webint.zip");
            try
            {
                Directory.Delete(Profile.AppdataFolder + @"\webint", true);
            }
            catch { }

            updatewebintpending = false;
            DoneSyncing();

            if (Profile.SyncingMethod == SyncMethod.Automatic)
            {
                tSync = new System.Threading.Timer(new TimerCallback(StartRemoteSync), null, 1000 * Profile.SyncFrequency, 0);
                Log.Write(l.Debug, "Syncing set to start in {0} seconds.", Profile.SyncFrequency);
                Log.Write(l.Debug, "~~~~~~~~~~~~~~~~~");
            }

            //ListAllFiles();
        }

        /// <summary>
        /// deletes any existing webint files and folders from the installation folder.
        /// These files/folders would exist if previous webint installing wasn't successful
        /// </summary>
        public void CheckForFiles()
        {
            string p = Profile.AppdataFolder;
            if (File.Exists(p + @"\webint.zip"))
                File.Delete(p + @"\webint.zip");
            if (Directory.Exists(p + @"\webint"))
                Directory.Delete(p + @"\webint", true);
            if (Directory.Exists(p + @"\WebInterface"))
                Directory.Delete(p + @"\WebInterface", true);
            if (File.Exists(p + @"\version.ini"))
                File.Delete(p + @"\version.ini");
        }

        private bool isWebUIPending = false;
        private void chkWebInt_CheckedChanged(object sender, EventArgs e)
        {
            if (!changedfromcheck)
            {
                wiThread = new Thread(AddRemoveWebInt);
                addremovewebintpending = true;

                if (_busy)
                {
                    isWebUIPending = true;
                    return;
                }

                wiThread.Start();
            }
            changedfromcheck = false;
        }

        private void labViewInBrowser_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string thelink = Profile.HttpPath;
            if (!thelink.StartsWith("http://") || !thelink.StartsWith("https://"))
                thelink = "http://" + thelink;
            if (thelink.EndsWith("/"))
                thelink = thelink + "webint";
            else
                thelink = thelink + "/webint";

            Process.Start(thelink);
        }

        #endregion

        #region about tab

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

        #region Start on Windows Start-Up

        private void chkStartUp_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                SetStartup(chkStartUp.Checked);
            }
            catch (Exception ex) { Common.LogError(ex); }
        }

        /// <summary>
        /// run FTPbox on windows startup
        /// <param name="enable"><c>true</c> to add it to system startup, <c>false</c> to remove it</param>
        /// </summary>
        private void SetStartup(bool enable)
        {
            string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(runKey);

            if (enable)
            {
                if (startupKey.GetValue("FTPbox") == null)
                {
                    startupKey = Registry.CurrentUser.OpenSubKey(runKey, true);
                    startupKey.SetValue("FTPbox", Application.ExecutablePath.ToString());
                    startupKey.Close();
                }
            }
            else
            {
                // remove startup
                startupKey = Registry.CurrentUser.OpenSubKey(runKey, true);
                startupKey.DeleteValue("FTPbox", false);
                startupKey.Close();
            }
        }

        /// <summary>
        /// returns true if FTPbox is set to start on windows startup
        /// </summary>
        /// <returns></returns>
        private bool CheckStartup()
        {
            string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            Microsoft.Win32.RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(runKey);

            if (startupKey.GetValue("FTPbox") == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        
        #endregion

        #region Speed Limits

        private bool LimitUpSpeed()
        {
            return Settings.UpLimit > 0;
        }

        private bool LimitDownSpeed()
        {
            return Settings.DownLimit > 0;
        }

        private void nDownLimit_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                Settings.DownLimit = Convert.ToInt32(nDownLimit.Value);
                Client.SetMaxDownloadSpeed(Convert.ToInt32(nDownLimit.Value));
            }
            catch { }
        }

        private void nUpLimit_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                Settings.UpLimit = Convert.ToInt32(nUpLimit.Value);
                Client.SetMaxUploadSpeed(Convert.ToInt32(nUpLimit.Value));
            }
            catch { }
        }
        #endregion

        #region Sync Frequency

        private void cManually_CheckedChanged(object sender, EventArgs e)
        {
            SyncToolStripMenuItem.Enabled = cManually.Checked || !rcThread.IsAlive;
            Profile.SyncingMethod = (cManually.Checked) ? SyncMethod.Manual : SyncMethod.Automatic;

            Settings.syncMethod = Profile.SyncingMethod;

            if (Profile.SyncingMethod == SyncMethod.Automatic)
            {
                Profile.SyncFrequency = Convert.ToInt32(nSyncFrequency.Value);
                nSyncFrequency.Enabled = true;
            }
            else
            {
                nSyncFrequency.Enabled = false;
                try{
                    tSync.Dispose();
                }
                catch
                {
                    //gotta catch em all
                }
            }
        }

        private void cAuto_CheckedChanged(object sender, EventArgs e)
        {
            SyncToolStripMenuItem.Enabled = !cAuto.Checked || !rcThread.IsAlive;
            Profile.SyncingMethod = (!cAuto.Checked) ? SyncMethod.Manual : SyncMethod.Automatic;

            Settings.syncMethod = Profile.SyncingMethod;

            if (Profile.SyncingMethod == SyncMethod.Automatic)
            {
                Profile.SyncFrequency = Convert.ToInt32(nSyncFrequency.Value);
                nSyncFrequency.Enabled = true;
            }
            else
            {
                nSyncFrequency.Enabled = false;
                try
                {
                    tSync.Dispose();
                }
                catch {
                    //gotta catch em all
                }
            }
        }

        private void nSyncFrequency_ValueChanged(object sender, EventArgs e)
        {
            Profile.SyncFrequency = Convert.ToInt32(nSyncFrequency.Value);
            Settings.syncFrequency = Profile.SyncFrequency;
        }

        #endregion        

        /// <summary>
        /// Kills the current process. Called from the tray menu.
        /// </summary>
        public void KillTheProcess()
        {
            if (!Profile.IsNoMenusMode)
                RemoveFTPboxMenu();

            ExitedFromTray = true;
            Log.Write(l.Info, "Killing the process...");

            try
            {
                tray.Visible = false;
                Process p = Process.GetCurrentProcess();
                p.Kill();
            }
            catch
            {
                Application.Exit();
            }
        }

        private void chkShowNots_CheckedChanged(object sender, EventArgs e)
        {            
            Settings.ShowNots = chkShowNots.Checked;
        }

        /// <summary>
        /// Clears the account info from the settings file and restarts the application
        /// </summary>
        private void ChangeAccount()
        {
            Settings.ClearAccount();
            Settings.ClearPaths();
            Settings.ClearLog();

            Process.Start(Application.ExecutablePath);

            KillTheProcess();
        }

        private void bTranslate_Click(object sender, EventArgs e)
        {
            ftranslate.ShowDialog();
        }

        #region context menus

        private void AddContextMenu()
        {
            Log.Write(l.Info, "Adding registry keys for context menus");
            string reg_path = "Software\\Classes\\*\\Shell\\FTPbox";
            RegistryKey key = Registry.CurrentUser;
            key.CreateSubKey(reg_path);
            key = Registry.CurrentUser.OpenSubKey(reg_path, true);
            string icon_path = string.Format("\"{0}\"", Path.Combine(Application.StartupPath, "ftpboxnew.ico"));
            string applies_to = getAppliesTo(false);
            string command;

            //RegistryKey sub_key = key.OpenSubKey("MUIVerb", RegistryKeyPermissionCheck.ReadWriteSubTree);  

            //Add the parent menu
            key.SetValue("MUIVerb", "FTPbox");
            key.SetValue("Icon", icon_path);
            key.SetValue("SubCommands", "");

            //The 'Copy link' child item
            reg_path = "Software\\Classes\\*\\Shell\\FTPbox\\Shell\\Copy";
            Registry.CurrentUser.CreateSubKey(reg_path);
            key = Registry.CurrentUser.OpenSubKey(reg_path, true);
            key.SetValue("MUIVerb", "Copy HTTP link");
            key.SetValue("AppliesTo", applies_to);
            key.CreateSubKey("Command");
            reg_path += "\\Command";
            key = Registry.CurrentUser.OpenSubKey(reg_path, true);
            command = string.Format("\"{0}\" \"%1\" \"{1}\"", Application.ExecutablePath, "copy");
            key.SetValue("", command);

            //the 'Open in browser' child item
            reg_path = "Software\\Classes\\*\\Shell\\FTPbox\\Shell\\Open";
            Registry.CurrentUser.CreateSubKey(reg_path);
            key = Registry.CurrentUser.OpenSubKey(reg_path, true);
            key.SetValue("MUIVerb", "Open file in browser");
            key.SetValue("AppliesTo", applies_to);
            key.CreateSubKey("Command");
            reg_path += "\\Command";
            key = Registry.CurrentUser.OpenSubKey(reg_path, true);
            command = string.Format("\"{0}\" \"%1\" \"{1}\"", Application.ExecutablePath, "open");
            key.SetValue("", command);

            //the 'Synchronize this file' child item
            reg_path = "Software\\Classes\\*\\Shell\\FTPbox\\Shell\\Sync";
            Registry.CurrentUser.CreateSubKey(reg_path);
            key = Registry.CurrentUser.OpenSubKey(reg_path, true);
            key.SetValue("MUIVerb", "Synchronize this file");
            key.SetValue("AppliesTo", applies_to);
            key.CreateSubKey("Command");
            reg_path += "\\Command";
            key = Registry.CurrentUser.OpenSubKey(reg_path, true);
            command = string.Format("\"{0}\" \"%1\" \"{1}\"", Application.ExecutablePath, "sync");
            key.SetValue("", command);

            //the 'Move to FTPbox folder' child item
            reg_path = "Software\\Classes\\*\\Shell\\FTPbox\\Shell\\Move";
            Registry.CurrentUser.CreateSubKey(reg_path);
            key = Registry.CurrentUser.OpenSubKey(reg_path, true);
            key.SetValue("MUIVerb", "Move to FTPbox folder");
            key.SetValue("AppliesTo", getAppliesTo(true));
            key.CreateSubKey("Command");
            reg_path += "\\Command";
            key = Registry.CurrentUser.OpenSubKey(reg_path, true);
            command = string.Format("\"{0}\" \"%1\" \"{1}\"", Application.ExecutablePath, "move");
            key.SetValue("", command);

            #region same keys for the Folder menus
            reg_path = "Software\\Classes\\Directory\\Shell\\FTPbox";
            key = Registry.CurrentUser;
            key.CreateSubKey(reg_path);
            key = Registry.CurrentUser.OpenSubKey(reg_path, true);

            //Add the parent menu
            key.SetValue("MUIVerb", "FTPbox");
            key.SetValue("Icon", icon_path);
            key.SetValue("SubCommands", "");

            //The 'Copy link' child item
            reg_path = "Software\\Classes\\Directory\\Shell\\FTPbox\\Shell\\Copy";
            Registry.CurrentUser.CreateSubKey(reg_path);
            key = Registry.CurrentUser.OpenSubKey(reg_path, true);
            key.SetValue("MUIVerb", "Copy HTTP link");
            key.SetValue("AppliesTo", applies_to);
            key.CreateSubKey("Command");
            reg_path += "\\Command";
            key = Registry.CurrentUser.OpenSubKey(reg_path, true);
            command = string.Format("\"{0}\" \"%1\" \"{1}\"", Application.ExecutablePath, "copy");
            key.SetValue("", command);

            //the 'Open in browser' child item
            reg_path = "Software\\Classes\\Directory\\Shell\\FTPbox\\Shell\\Open";
            Registry.CurrentUser.CreateSubKey(reg_path);
            key = Registry.CurrentUser.OpenSubKey(reg_path, true);
            key.SetValue("MUIVerb", "Open folder in browser");
            key.SetValue("AppliesTo", applies_to);
            key.CreateSubKey("Command");
            reg_path += "\\Command";
            key = Registry.CurrentUser.OpenSubKey(reg_path, true);
            command = string.Format("\"{0}\" \"%1\" \"{1}\"", Application.ExecutablePath, "open");
            key.SetValue("", command);

            //the 'Synchronize this folder' child item
            reg_path = "Software\\Classes\\Directory\\Shell\\FTPbox\\Shell\\Sync";
            Registry.CurrentUser.CreateSubKey(reg_path);
            key = Registry.CurrentUser.OpenSubKey(reg_path, true);
            key.SetValue("MUIVerb", "Synchronize this folder");
            key.SetValue("AppliesTo", applies_to);
            key.CreateSubKey("Command");
            reg_path += "\\Command";
            key = Registry.CurrentUser.OpenSubKey(reg_path, true);
            command = string.Format("\"{0}\" \"%1\" \"{1}\"", Application.ExecutablePath, "sync");
            key.SetValue("", command);

            //the 'Move to FTPbox folder' child item
            reg_path = "Software\\Classes\\Directory\\Shell\\FTPbox\\Shell\\Move";
            Registry.CurrentUser.CreateSubKey(reg_path);
            key = Registry.CurrentUser.OpenSubKey(reg_path, true);
            key.SetValue("MUIVerb", "Move to FTPbox folder");
            key.SetValue("AppliesTo", "NOT " + applies_to);
            key.CreateSubKey("Command");
            reg_path += "\\Command";
            key = Registry.CurrentUser.OpenSubKey(reg_path, true);
            command = string.Format("\"{0}\" \"%1\" \"{1}\"", Application.ExecutablePath, "move");
            key.SetValue("", command);

            #endregion

            key.Close();
        }

        private void RemoveFTPboxMenu()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Classes\\*\\Shell\\", true);            
            key.DeleteSubKeyTree("FTPbox", false);
            key.Close();

            key = Registry.CurrentUser.OpenSubKey("Software\\Classes\\Directory\\Shell\\", true);
            key.DeleteSubKeyTree("FTPbox", false);
            key.Close();
        }

        /// <summary>
        /// Gets the value of the AppliesTo String Value that will be put to registry and determine on which files' right-click menus each FTPbox menu item will show.
        /// If the local path is inside a library folder, it has to check for another path (short_path), because System.ItemFolderPathDisplay will, for example, return 
        /// Documents\FTPbox instead of C:\Users\Username\Documents\FTPbox
        /// </summary>
        /// <param name="isForMoveItem">If the AppliesTo value is for the Move-to-FTPbox item, it adds 'NOT' to make sure it shows anywhere but in the local syncing folder.</param>
        /// <returns></returns>
        private string getAppliesTo(bool isForMoveItem)
        {
            string path = Profile.LocalPath;
            string applies_to = (isForMoveItem) ? string.Format("NOT System.ItemFolderPathDisplay:~< \"{0}\"", path) : string.Format("System.ItemFolderPathDisplay:~< \"{0}\"", path);
            string short_path = null;
            Environment.SpecialFolder[] Libraries = new[] { Environment.SpecialFolder.MyDocuments, Environment.SpecialFolder.MyMusic, Environment.SpecialFolder.MyPictures, Environment.SpecialFolder.MyVideos };
            string userpath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\";

            if (path.StartsWith(userpath))
                foreach (Environment.SpecialFolder s in Libraries)            
                    if (path.StartsWith(Environment.GetFolderPath(s)))
                        if (s != Environment.SpecialFolder.UserProfile) //TODO: is this ok?
                            short_path = path.Substring(userpath.Length);            

            if (short_path == null) return applies_to;

            applies_to += (isForMoveItem) ? string.Format(" AND NOT System.ItemFolderPathDisplay: \"*{0}*\"", short_path) : string.Format(" OR System.ItemFolderPathDisplay: \"*{0}*\"", short_path);

            return applies_to;
        }

        private void RunServer()
        {
            Thread _tServer = new Thread(RunServerThread);
            _tServer.SetApartmentState(ApartmentState.STA);
            _tServer.Start();            
        }

        private void RunServerThread()
        {
            int i = 1;
            Thread server;
            Log.Write(l.Client, "Started the named-pipe server, waiting for clients (if any)");

            server = new Thread(ServerThread);
            server.SetApartmentState(ApartmentState.STA);
            server.Start();

            Thread.Sleep(250);

            while (i > 0)
            {
                if (server != null)
                {
                    if (server.Join(250))
                    {
                        Log.Write(l.Client, "named-pipe server thread finished");
                        server = null;
                        i--;
                    }
                }
            }
            Log.Write(l.Client, "named-pipe server thread exiting...");

            RunServer();
        }

        public void ServerThread()
        {
            NamedPipeServerStream pipeServer = new NamedPipeServerStream("FTPbox Server", PipeDirection.InOut, 5);
            int threadID = Thread.CurrentThread.ManagedThreadId;

            pipeServer.WaitForConnection();
            
            Log.Write(l.Client, "Client connected, id: {0}", threadID);

            try
            {
                StreamString ss = new StreamString(pipeServer);

                ss.WriteString("ftpbox");
                string args = ss.ReadString();

                ReadMessageSent fReader = new ReadMessageSent(ss, "All done!");

                Log.Write(l.Client, "Reading file: \n {0} \non thread [{1}] as user {2}.", args, threadID, pipeServer.GetImpersonationUserName());

                List<string> li = new List<string>();
                li = ReadCombinedParameters(args);

                CheckClientArgs(li.ToArray());

                pipeServer.RunAsClient(fReader.Start);
            }
            catch (IOException e)
            {
                Common.LogError(e);
            }
            pipeServer.Close();
        }

        private List<string> ReadCombinedParameters(string args)
        {
            List<string> r = new List<string>(args.Split('"'));
            while(r.Contains(""))
                r.Remove("");

            return r;
        }

        private void CheckClientArgs(string[] args)
        {
            List<string> list = new List<string>(args);
            string param = list[0];
            list.RemoveAt(0);

            switch (param)
            {
                case "copy":
                    CopyArgLinks(list.ToArray());
                    break;
                case "sync":
                    SyncArgItems(list.ToArray());
                    break;
                case "open":
                    OpenArgItemsInBrowser(list.ToArray());
                    break;
                case "move":
                    MoveArgItems(list.ToArray());
                    break;
            }
        }

        private DateTime dtLastContextAction = DateTime.Now;

        private void CopyArgLinks(string[] args)
        {
            string c = null;
            int i = 0;
            foreach (string s in args)
            {
                if (!s.StartsWith(Profile.LocalPath))
                {
                    MessageBox.Show("You cannot use this for files that are not inside the FTPbox folder.", "FTPbox - Invalid file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }

                i++;
                //if (File.Exists(s))
                c += Common.GetHttpLink(s);
                if (i<args.Count())
                    c += Environment.NewLine;
            }

            if (c == null) return;

            try
            {
                if ((DateTime.Now - dtLastContextAction).TotalSeconds < 2)
                    Clipboard.SetText(Clipboard.GetText() + Environment.NewLine + c);
                else                    
                    Clipboard.SetText(c);
                //LinkCopied();
            }
            catch (Exception e)
            {
                Common.LogError(e);
            }
            dtLastContextAction = DateTime.Now;
        }

        AutoResetEvent arEvent = new AutoResetEvent(false);
        private void SyncArgItems(string[] args)
        {
            int valid_items = 0;
            foreach (string s in args)
            {
                string cpath = Common.GetComPath(s, true);

                if (!s.StartsWith(Profile.LocalPath))
                {
                    MessageBox.Show("You cannot use this for files that are not inside the FTPbox folder.", "FTPbox - Invalid file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }

                if (Common.PathIsFile(s) && File.Exists(s))
                {
                    //FileInfo fli = new FileInfo(s);
                    FileQueueItem fi = new FileQueueItem(cpath, s, Client.SizeOf(cpath), TypeOfTransfer.Change);
                    fQueue.MenuFiles.Add(fi);
                    valid_items++;
                }
                else if (!Common.PathIsFile(s) && Directory.Exists(s))
                {
                    fQueue.MenuFolders.Add(s);
                    valid_items++;
                }
            }

            if (valid_items == 0) return;

            if (fQueue.Busy)
            {
                arEvent.WaitOne();
            }

            SetTray(MessageType.Syncing);
            fQueue.Busy = true;
            foreach (FileQueueItem fqi in fQueue.MenuFiles)
            {
                DateTime rDT = Client.GetLWTof(fqi.CommonPath);
                CheckExistingFile(fqi.CommonPath, rDT, fqi.LocalPath, fqi.Size);

                SyncRemQueueFiles();
            }
            foreach (string dqi in fQueue.MenuFolders)
                StartRemoteSync(Common.GetComPath(dqi, true));
            /*
                foreach (string fi in Directory.GetFiles(dqi, "*", SearchOption.AllDirectories))
                {
                    string cpath = Common.GetComPath(fi, true);
                    DateTime rDT = GetLWTof(cpath);
                    FileInfo iFile = new FileInfo(fi);
                    CheckExistingFile(cpath, rDT, fi, iFile.Length);
                }*/

            SetTray(MessageType.AllSynced);
            

            arEvent.Set();
        }

        private void OpenArgItemsInBrowser(string[] args)
        {
            foreach (string s in args)
            {
                if (!s.StartsWith(Profile.LocalPath))
                {
                    MessageBox.Show("You cannot use this for files that are not inside the FTPbox folder.", "FTPbox - Invalid file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }

                string link = Common.GetHttpLink(s);
                try
                {
                    Process.Start(link);
                }
                catch (Exception e)
                {
                    Common.LogError(e);
                }
            }
            
            dtLastContextAction = DateTime.Now;
        }

        private void MoveArgItems(string[] args)
        {
            foreach (string s in args)
            {
                if (!s.StartsWith(Profile.LocalPath))
                {
                    if (File.Exists(s))
                    {
                        FileInfo fi = new FileInfo(s);
                        File.Copy(s, Path.Combine(Profile.LocalPath, fi.Name));
                    }
                    else if (Directory.Exists(s))
                    {
                        foreach (string dir in Directory.GetDirectories(s, "*", SearchOption.AllDirectories))
                        {
                            string name = dir.Substring(s.Length);
                            Directory.CreateDirectory(Path.Combine(Profile.LocalPath, name));
                        }
                        foreach (string file in Directory.GetFiles(s, "*", SearchOption.AllDirectories))
                        {
                            string name = file.Substring(s.Length);
                            File.Copy(file, Path.Combine(Profile.LocalPath, name));
                        }
                    }
                }
            }
        }

        #endregion

        #region Filters

        #region Form control handlers

        private void cIgnoreTempFiles_CheckedChanged(object sender, EventArgs e)
        {
            Common.IgnoreList.IgnoreTempFiles = cIgnoreTempFiles.Checked;
            Common.IgnoreList.Save();
        }

        private void cIgnoreDotfiles_CheckedChanged(object sender, EventArgs e)
        {
            Common.IgnoreList.IgnoreDotFiles = cIgnoreDotfiles.Checked;
            Common.IgnoreList.Save();
        }

        private void bAddExt_Click(object sender, EventArgs e)
        {
            string newext = tNewExt.Text;
            if (newext.StartsWith(".")) newext = newext.Substring(1);

            if (!Common.IgnoreList.ExtensionList.Contains(newext))
                Common.IgnoreList.ExtensionList.Add(newext);
            Common.IgnoreList.Save();
            
            tNewExt.Text = string.Empty;
            //refresh the list
            lIgnoredExtensions.Clear();
            foreach (string s in Common.IgnoreList.ExtensionList)
                if (!string.IsNullOrWhiteSpace(s)) lIgnoredExtensions.Items.Add(new ListViewItem(s));
        }

        private void tNewExt_TextChanged(object sender, EventArgs e)
        {
            bAddExt.Enabled = !string.IsNullOrWhiteSpace(tNewExt.Text);

            this.AcceptButton = (string.IsNullOrWhiteSpace(tNewExt.Text)) ? null : bAddExt;
        }

        private void bRemoveExt_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem li in lIgnoredExtensions.SelectedItems)
                if (!string.IsNullOrWhiteSpace(li.Text))
                    Common.IgnoreList.ExtensionList.Remove(li.Text);
            Common.IgnoreList.Save();

            //refresh the list
            lIgnoredExtensions.Clear();
            foreach (string s in Common.IgnoreList.ExtensionList)
                if (!string.IsNullOrWhiteSpace(s)) lIgnoredExtensions.Items.Add(new ListViewItem(s));
        }

        private void lIgnoredExtensions_SelectedIndexChanged(object sender, EventArgs e)
        {
            bRemoveExt.Enabled = lIgnoredExtensions.SelectedItems.Count > 0;
            this.AcceptButton = (lIgnoredExtensions.SelectedItems.Count > 0) ? bRemoveExt : null;
        }

        private void cIgnoreOldFiles_CheckedChanged(object sender, EventArgs e)
        {            
            dtpLastModTime.Enabled = cIgnoreOldFiles.Checked;
            Common.IgnoreList.IgnoreOldFiles = cIgnoreOldFiles.Checked;
            Common.IgnoreList.LastModifiedMinimum = (cIgnoreOldFiles.Checked) ? dtpLastModTime.Value : DateTime.MinValue;
            Common.IgnoreList.Save();            
        }

        private void dtpLastModTime_ValueChanged(object sender, EventArgs e)
        {
            Common.IgnoreList.IgnoreOldFiles = cIgnoreOldFiles.Checked;
            Common.IgnoreList.LastModifiedMinimum = (cIgnoreOldFiles.Checked) ? dtpLastModTime.Value : DateTime.MinValue;
            Common.IgnoreList.Save();
        }

        private void lSelectiveSync_AfterExpand(object sender, TreeViewEventArgs e)
        {
            string path = e.Node.FullPath;

            if (e.Node.Nodes.Count > 0)
            {
                int i = e.Node.Index;

                foreach (TreeNode tn in e.Node.Nodes)
                {
                    try
                    {
                        lSelectiveSync.Nodes[i].Nodes.Remove(tn);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(l.Debug, ex.Message);
                    }
                }
            }

            Thread tExpandItem = new Thread(() =>
            {
                List<ClientItem> li = new List<ClientItem>();
                try
                {
                    li = Client.List(path);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex);
                    return;
                }

                foreach (ClientItem d in li)
                    if (d.Type == ClientItemType.Folder)
                        this.Invoke(new MethodInvoker(delegate
                        {
                            TreeNode parent = new TreeNode(d.Name);
                            e.Node.Nodes.Add(parent);
                            parent.Nodes.Add(new TreeNode("!tempnode"));
                        }));

                foreach (ClientItem f in li)
                    if (f.Type == ClientItemType.File)
                        this.Invoke(new MethodInvoker(delegate
                        {
                            e.Node.Nodes.Add(new TreeNode(f.Name));
                        }));

                //EditNodeCheckboxes();
                foreach (TreeNode tn in e.Node.Nodes)
                    this.Invoke(new MethodInvoker(delegate
                    {
                        tn.Checked = !Common.IgnoreList.isInIgnoredFolders(tn.FullPath);
                    }));
            });
            tExpandItem.Start();
        }        

        private void lSelectiveSync_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (checking_nodes || e.Node.Text == "!tempnode!") return;

            string cpath = Common.GetComPath(e.Node.FullPath, false);
            Log.Write(l.Debug, "{0} is ignored: {1} already in list: {2}", cpath, !e.Node.Checked, Common.IgnoreList.FolderList.Contains(cpath));

            if (e.Node.Checked && Common.IgnoreList.FolderList.Contains(cpath))
                Common.IgnoreList.FolderList.Remove(cpath);
            else if (!e.Node.Checked && !Common.IgnoreList.FolderList.Contains(cpath))
                Common.IgnoreList.FolderList.Add(cpath);
            Common.IgnoreList.Save();
            
            checking_nodes = true;
            CheckUncheckChildNodes(e.Node, e.Node.Checked);
            
            if (e.Node.Checked && e.Node.Parent != null)
                if (!e.Node.Parent.Checked)
                {
                    e.Node.Parent.Checked = true;
                    if (Common.IgnoreList.FolderList.Contains(e.Node.Parent.FullPath))
                        Common.IgnoreList.FolderList.Remove(e.Node.Parent.FullPath);
                    CheckSingleRoute(e.Node.Parent);
                }
            Common.IgnoreList.Save();
            checking_nodes = false;
        }

        private void lSelectiveSync_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            e.Node.Nodes.Clear();
            e.Node.Nodes.Add(e.Node.Name);
        }
        
        private void bRefresh_Click(object sender, EventArgs e)
        {
            RefreshListing();
        }

        #endregion 

        #region Manage filter options

        private void RefreshListing()
        {
            Thread tRefresh = new Thread(() =>
            {
                if (!Client.CheckConnectionStatus())
                    RetryConnection();

                List<ClientItem> li = new List<ClientItem>();
                try
                {
                    li = Client.List(".");
                }
                catch(Exception ex)
                {
                    Common.LogError(ex);
                    return;
                }

                this.Invoke(new MethodInvoker(delegate
                {
                    lSelectiveSync.Nodes.Clear();
                }));

                foreach (ClientItem d in li)
                    if (d.Type == ClientItemType.Folder)
                    {
                        if (d.Name == "webint") continue;

                        TreeNode parent = new TreeNode(d.Name);
                        this.Invoke(new MethodInvoker(delegate
                        {
                            lSelectiveSync.Nodes.Add(parent);
                            parent.Nodes.Add(new TreeNode("!tempnode!"));
                        }));
                        
                    }
                foreach (ClientItem f in li)
                    if (f.Type == ClientItemType.File)
                        this.Invoke(new MethodInvoker(delegate
                        {
                            lSelectiveSync.Nodes.Add(new TreeNode(f.Name));
                        }));

                this.Invoke(new MethodInvoker(delegate
                {
                    EditNodeCheckboxes();
                }));
            });
            tRefresh.Start();
        }

        private void CheckSingleRoute(TreeNode tn)
        {
            if (tn.Checked && tn.Parent != null)
                if (!tn.Parent.Checked)
                {
                    tn.Parent.Checked = true;
                    if (Common.IgnoreList.FolderList.Contains(tn.Parent.FullPath))
                        Common.IgnoreList.FolderList.Remove(tn.Parent.FullPath);
                    CheckSingleRoute(tn.Parent);
                }
        }

        private void EditNodeCheckboxes()
        {
            foreach (TreeNode t in lSelectiveSync.Nodes)
            {
                if (!Common.IgnoreList.isInIgnoredFolders(t.FullPath)) t.Checked = true;
                if (t.Parent != null)
                    if (!t.Parent.Checked) t.Checked = false;

                foreach (TreeNode tn in t.Nodes)
                    EditNodeCheckboxesRecursive(tn);
            }
        }

        private void EditNodeCheckboxesRecursive(TreeNode t)
        {
            t.Checked = Common.IgnoreList.isInIgnoredFolders(t.FullPath);
            if (t.Parent != null)
                if (!t.Parent.Checked) t.Checked = false;

            Log.Write(l.Debug, "Node {0} is checked {1}", t.FullPath, t.Checked);

            foreach (TreeNode tn in t.Nodes)
                EditNodeCheckboxesRecursive(tn);
        }

        private bool checking_nodes = false;
        private void CheckUncheckChildNodes(TreeNode t, bool c)
        {
            t.Checked = c;
            foreach (TreeNode tn in t.Nodes)
                CheckUncheckChildNodes(tn, c);
        }

        private bool isParentNodeUnchecked(TreeNode t)
        {
            return !t.Parent.Checked;
        }

        #endregion

        #endregion
    }
}
