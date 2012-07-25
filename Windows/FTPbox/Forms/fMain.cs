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
using Renci.SshNet;
using Renci.SshNet.Sftp;
using Starksoft.Net.Ftp;
using FTPboxLib;
using FTPbox.Classes;
using Utilities.Encryption;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;

namespace FTPbox.Forms
{
    public partial class fMain : Form
    {
        public FileQueue fQueue = new FileQueue();			//file queue
        public DeletedQueue dQueue = new DeletedQueue();	//Deleted items queue
        public IgnoreList ignoreList = new IgnoreList();	//list of ignored folders

        List<string> localFolders = new List<string>();     //Used to store all the local folders at all times
        List<string> localFiles = new List<string>();       //Used to store all the local files at all times

        public RecentFiles recentFiles = new RecentFiles(); //List of the 5 most recently changed files
        TrayAction _trayAct = TrayAction.OpenLocalFile;     //the tray action to be used for opening files

        Thread rcThread;                                    //remote-check thread
        Thread wiThread;                                    //web interface thread
        Thread wiuThread;                                   //Web interface update thread

        FtpClient ftpc;                                     //Our FTP client        
        SftpClient sftpc;                                   //And our SFTP client;

        Settings AppSettings = new Settings();              //Used to get the application settings from the settings.xml file
        public Translations languages = new Translations(); //Used to grab the translations from the translations.xml file

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

            //FTPbox.Properties.Settings.Default.ftpUsername = "";
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(OnNetworkChange);
            CheckForPreviousInstances();

            LoadLocalFolders();
            LoadLog();
            LoadProfile();
            
            _trayAct = SettingsTrayAction;

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

                SetTray(MessageType.Ready);

                if (Profile.SyncingMethod == SyncMethod.Automatic && !_busy)
                    StartRemoteSync();
            }
            else
            {
                OfflineMode = true;
                SetTray(MessageType.Offline);
            }
        }

        #region variables

        public string ftpHost()
        {
            string x = AppSettings.Get("Account/Host", "");
            try
            {
                return AESEncryption.Decrypt(x, Profile.DecryptionPassword, Profile.DecryptionSalt, "SHA1", 2, "OFRna73m*aze01xY", 256);
            }
            catch
            {
                return x;
            }
        }

        public string ftpUser()
        {
            string x = AppSettings.Get("Account/Username", "");
            try
            {
                return AESEncryption.Decrypt(x, Profile.DecryptionPassword, Profile.DecryptionSalt, "SHA1", 2, "OFRna73m*aze01xY", 256);
            }
            catch
            {
                return x;
            }
        }

        public string ftpPass()
        {
            string x = AppSettings.Get("Account/Password", "");
            try
            {
                return AESEncryption.Decrypt(x, Profile.DecryptionPassword, Profile.DecryptionSalt, "SHA1", 2, "OFRna73m*aze01xY", 256);
            }
            catch
            {
                return x;
            }
        }

        public int ftpPort()
        {
            int i = (FTP()) ? 21 : 22;
            return AppSettings.Get("Account/Port", i);
        }

        public string rPath()
        {
            return AppSettings.Get("Paths/rPath", "");            
        }

        public string lPath()
        {
            return AppSettings.Get("Paths/lPath", "");
        }

        bool StartOnStartup()
        {
            return bool.Parse(AppSettings.Get("Settings/Startup", "True"));
        }

        /// <summary>
        /// Show notifications?
        /// </summary>
        /// <returns></returns>
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

        string foLog()
        {
            return AppSettings.Get("Log/folders", "");
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
            return bool.Parse(AppSettings.Get("Account/FTP", "True"));
        }

        public bool FTPS()
        {
            return bool.Parse(AppSettings.Get("Account/FTPS", "False"));
        }

        public bool FTPES()
        {
            return bool.Parse(AppSettings.Get("Account/FTPES", "True"));
        }

        public string HTTPPath()
        {
            return AppSettings.Get("Paths/AccountsPath", ftpHost());
        }

        public int UpLimit()
        {
            return AppSettings.Get("Settings/UpLimit", 0);
        }

        public int DownLimit()
        {
            return AppSettings.Get("Settings/DownLimit", 0);
        }

        public SyncMethod syncMethod()
        {
            return (AppSettings.Get("Settings/SyncMethod", SyncMethod.Automatic.ToString()) == SyncMethod.Automatic.ToString()) ? SyncMethod.Automatic : SyncMethod.Manual;
        }

        public int syncFrequency()
        {
            return AppSettings.Get("Settings/SyncFrequency", 10);
        }

        #endregion

        /// <summary>
        /// Connect the client to the server
        /// </summary>
        public void LoginFTP()
        {
            SetTray(MessageType.Connecting);
            
            if (FTP())
            {
                try
                {
                    ftpc.Close();
                }
                catch { }

                ftpc = new FtpClient(Profile.Host, Profile.Port); //ftpHost(), ftpPort());

                if (FTPS())
                {
                    if (FTPES())
                        ftpc.SecurityProtocol = FtpSecurityProtocol.Tls1OrSsl3Explicit;
                    else
                        ftpc.SecurityProtocol = FtpSecurityProtocol.Tls1OrSsl3Implicit;

                    ftpc.ValidateServerCertificate += new EventHandler<ValidateServerCertificateEventArgs>(ftp_ValidateServerCertificate);
                }
                Log.Write(l.Debug, "Connecting ftpc -> user: {0}", Profile.Username);
                ftpc.Open(Profile.Username, Profile.Password); //ftpUser(), ftpPass());
                
                Log.Write(l.Debug, "Connection opened");
                
                //ftpc.ConnectionClosed += new EventHandler<ConnectionClosedEventArgs>(FTPClosedConnection);
                //ftpc.TransferComplete += new EventHandler<TransferCompleteEventArgs>(FtpTransferComplete);

                if (LimitUpSpeed())
                    nUpLimit.Value = Convert.ToDecimal(UpLimit());
                if (LimitDownSpeed())
                    nDownLimit.Value = Convert.ToDecimal((DownLimit()));
                 
                
                Log.Write(l.Debug, "Connected");
                Log.Write(l.Info, ftpc.IsConnected.ToString());

                Thread.Sleep(5000);
            }
            else
            {
                try
                {
                    sftpc.Disconnect();
                }
                catch { }

                sftpc = new SftpClient(ftpHost(), ftpPort(), ftpUser(), ftpPass());
                Log.Write(l.Info, "Connecting sftpc");
                sftpc.Connect();                

                Log.Write(l.Info, sftpc.IsConnected.ToString());

                SftpHome = sftpc.WorkingDirectory;
                SftpHome = (SftpHome.StartsWith("/")) ? SftpHome.Substring(1) : SftpHome;
                //sftpc.ChangeDirectory(rPath());
                Profile.SftpHome = SftpHome;
            }

            SetTray(MessageType.Ready);
        }

        /// <summary>
        /// checks if account's information used the last time has changed
        /// </summary>
        private void CheckAccount()
        {
            if (ftpUser() == "" || ftpHost() == "" || ftpPass() == "")
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
                    Log.Write(l.Error, "Error: {0}", ex.Message);
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
            string rpath = rPath();
            if (rpath.StartsWith(@"/") && rpath != @"/")
                rpath = rpath.Substring(1);
            
            if (rpath == "" || lPath() == "")
            {
                newDir.ShowDialog();
                
                //Application.Run();
                this.Show();

                if (!gotpaths)
                {
                    Application.Exit();
                }
            }
            else if ((rpath != "/" && !_exists(rpath)) || !Directory.Exists(lPath()))
            {
                newDir.ShowDialog();

                this.Show();

                if (!gotpaths)
                {
                    Application.Exit();
                }
            }
            else
                gotpaths = true;
            
            LoadLocalFolders();
        }

        /// <summary>
        /// Saves data from Profile Class to the XML file
        /// </summary>
        public void SaveProfile()
        {
            Log.Write(l.Debug, "Saving the profile");
            AppSettings.Put("Account/Host", Profile.Host);
            AppSettings.Put("Account/Username", Profile.Username);
            AppSettings.Put("Account/Password", AESEncryption.Encrypt(Profile.Password, Profile.DecryptionPassword, Profile.DecryptionSalt, "SHA1", 2, "OFRna73m*aze01xY", 256));
            AppSettings.Put("Account/Port", Profile.Port);
            AppSettings.Put("Account/FTP", (Profile.Protocol != FtpProtocol.SFTP).ToString());
            AppSettings.Put("Account/FTPS", (Profile.Protocol == FtpProtocol.FTPS).ToString());
            AppSettings.Put("Account/FTPES", (Profile.FtpsInvokeMethod == FtpsMethod.Explicit).ToString());

            AppSettings.Put("Paths/rPath", Profile.RemotePath);
            AppSettings.Put("Paths/lPath", Profile.LocalPath);
            AppSettings.Put("Paths/Parent", Profile.HttpPath);
            Log.Write(l.Debug, "Saved the profile successfully");
        }

        /// <summary>
        /// Updates the form's labels etc
        /// </summary>
        public void UpdateDetails()
        {
            Log.Write(l.Debug, "Updating the form labels and shit");
            AppSettings = new Settings();

            WebIntExists();

            chkStartUp.Checked = CheckStartup();

            lHost.Text = ftpHost();
            lUsername.Text = ftpUser();
            lPort.Text = ftpPort().ToString();
            lMode.Text = (FTP()) ? "FTP" : "SFTP";

            lLocPath.Text = Profile.LocalPath;
            lRemPath.Text = Profile.RemotePath;
            tParent.Text = Profile.HttpPath;

            chkShowNots.Checked = ShowNots();

            if (_trayAct == TrayAction.OpenInBrowser)
                rOpenInBrowser.Checked = true;
            else if (_trayAct == TrayAction.CopyLink)
                rCopy2Clipboard.Checked = true;
            else
                rOpenLocal.Checked = true;

            lVersion.Text = Application.ProductVersion.ToString().Substring(0, 5) + @" Beta";

            if (Profile.SyncingMethod == SyncMethod.Automatic)
                cAuto.Checked = true;
            else
                cManually.Checked = true;

            nSyncFrequency.Value = Convert.ToDecimal(Profile.SyncFrequency);

            if (FTP())
            {
                if (LimitUpSpeed())
                    nUpLimit.Value = Convert.ToDecimal(UpLimit());
                if (LimitDownSpeed())
                    nDownLimit.Value = Convert.ToDecimal(DownLimit());
            }
            else
            {
                gLimits.Visible = false;
            }

            AfterPathsAreSet();
        }

        private string SftpHome = null;
        /// <summary>
        /// Called after the paths are correctly set. In case of SFTP, it gets the SFTP Home Directory.
        /// Also changes the working directory of the client to the remote syncing folder.
        /// </summary>
        public void AfterPathsAreSet()
        {
            try
            {
                if (!rPath().Equals(" ") && !rPath().Equals("/"))
                {
                    if (FTP())
                        ftpc.ChangeDirectory(rPath());
                    else
                    {                        
                        SftpHome = sftpc.WorkingDirectory;
                        SftpHome = (SftpHome.StartsWith("/")) ? SftpHome.Substring(1) : SftpHome;
                        sftpc.ChangeDirectory(rPath());
                        Profile.SftpHome = SftpHome;
                    }
                }
                if (FTP())
                    Log.Write(l.Info, "Changed current directory to {0}", ftpc.CurrentDirectory);
                else
                    Log.Write(l.Info, "Changed current directory to {0}, home is: {1}", sftpc.WorkingDirectory, SftpHome);                                                
            }
            catch (Exception e) { Log.Write(l.Debug, e.Message); }

            SetWatchers();
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
        /// Get lastwritetime of a remote file by its path (for FTP)
        /// </summary>
        /// <param name="FullRemPath">path to remote file</param>
        /// <param name="name">name of remote file</param>
        /// <returns></returns>
        DateTime GetLWTof(string FullRemPath, string name)
        {
            //Log.Write(l.Info, "Getting LWT of {0} in {1}", name, FullRemPath);
            string path;
            if (FullRemPath.EndsWith(name))
                path = FullRemPath;
            else
                path = string.Format("{0}/{1}", noSlashes(FullRemPath), name);

            while (path.StartsWith("/"))
                path = path.Substring(1);

            DateTime dt = DateTime.UtcNow;
            dt = DateTime.MinValue;
            try
            {
                dt = ftpc.GetFileDateTime(path, true);

                Log.Write(l.Debug, "========> {0} ~~ {1}", dt.ToString(), ftpc.GetFileDateTime(path, false));
            }
            catch (Exception ex)
            {
                Log.Write(l.Debug, "========> it's a folder, err: {0}", ex.Message);
            }
            return dt;
        }

        /// <summary>
        /// Returns the LastWriteTime of the specified file/folder
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        DateTime GetLWTof(string path)
        {
            string p = path;
            if (p.StartsWith("/"))
                p = p.Substring(1);

            DateTime dt = DateTime.MinValue;
            try
            {
                dt = (FTP()) ? ftpc.GetFileDateTime(p, true) : sftpc.GetLastWriteTime(p);
            }
            catch (Exception ex)
            {
                Log.Write(l.Debug, "========> it's a folder, err: {0}", ex.Message);
            }

            if (!FTP())
                Log.Write(l.Debug, "Got LWT: {0} UTC: {1}", dt, sftpc.GetLastAccessTimeUtc(p));

            return dt;
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
            AppSettings.Put("Log/nLog", nLog() + cPath + "|");
            AppSettings.Put("Log/rLog", rLog() + rDTlog.ToString() + "|");
            AppSettings.Put("Log/lLog", lLog() + lDTlog.ToString() + "|");

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
            AppSettings.Put("Log/folders", foLog() + cpath + "|");
        }

        /// <summary>
        /// removes an item from the log
        /// </summary>
        /// <param name="cPath">name to remove</param>
        public void RemoveFromLog(string cPath)
        {
            if (fLog.Contains(cPath))
            {
                fLog.Remove(cPath);
                ClearLog();

                foreach (FileLogItem f in fLog.Files)
                {
                    AppSettings.Put("Log/nLog", nLog() + f.CommonPath + "|");
                    AppSettings.Put("Log/lLog", lLog() + f.Local.ToString() + "|");
                    AppSettings.Put("Log/rLog", rLog() + f.Remote.ToString() + "|");
                }
                Log.Write(l.Debug, "*** Removed from Log: {0}", cPath);
            }
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
                ClearFoldersLog();

                foreach (string f in fLog.Folders)
                {
                    AppSettings.Put("Log/folders", foLog() + f + "|");
                }
                Log.Write(l.Debug, "*** Removed from folders Log: {0}", cpath);
            }
        }

        /// <summary>
        /// Updates the specified path in the log
        /// </summary>
        /// <param name="path"></param>
        /// <param name="rem"></param>
        /// <param name="loc"></param>
        public void UpdateLog(string path, DateTime rem, DateTime loc)
        {
            Log.Write(l.Debug, "Updating log: path: {0} rem {1} loc {2}", path, rem.ToString(), loc.ToString());
            fLog.putFile(path, rem, loc);
            WriteRemToLog(path, rem);
            WriteLocToLog(path, loc);
            Log.Write(l.Debug, "################################");
        }

        /// <summary>
        /// Writes a remote change to the log
        /// </summary>
        /// <param name="path"></param>
        /// <param name="rem"></param>
        private void WriteRemToLog(string path, DateTime rem)
        {
            path.Replace(@"/", @"|");
            if (path.EndsWith("|"))
                path = path.Substring(0, path.Length - 1);
            if (path.StartsWith("|"))
                path = path.Substring(1);
            Log.Write(l.Debug, "files/" + path + "/remote");
            AppSettings.Put("files/" + path + "/remote", rem.ToString());            
        }

        /// <summary>
        /// Writes a local change to the log
        /// </summary>
        /// <param name="path"></param>
        /// <param name="loc"></param>
        private void WriteLocToLog(string path, DateTime loc)
        {
            path.Replace(@"/", @"|");
            if (path.EndsWith("|"))
                path.Substring(0, path.Length - 1);
            if (path.StartsWith("|"))
                path = path.Substring(1);

            AppSettings.Put("files/" + path + "/local", loc.ToString());
        }

        /// <summary>
        /// Clears the log
        /// </summary>
        public void ClearLog()
        {
            AppSettings.Put("Log/nLog", "");
            AppSettings.Put("Log/rLog", "");
            AppSettings.Put("Log/lLog", "");
        }

        /// <summary>
        /// clears the folders log
        /// </summary>
        public void ClearFoldersLog()
        {
            AppSettings.Put("Log/folders", "");
        }

        /// <summary>
        /// Clears the paths from the xml
        /// </summary>
        public void ClearPaths()
        {
            AppSettings.Put("Paths/rPath", "");
            AppSettings.Put("Paths/lPath", "");
        }

        /// <summary>
        /// clears the account info from the XML file
        /// </summary>
        public void ClearAccount()
        {
            AppSettings.Put("Account/Host", "");
            AppSettings.Put("Account/Username", "");
            AppSettings.Put("Account/Password", "");
            AppSettings.Put("Paths/rPath", "");
            AppSettings.Put("Paths/lPath", "");
        }

        /// <summary>
        /// stores all items of the localFolders list in the Log file
        /// </summary>
        public void PutLocalFoldersToLog()
        {
            foreach (string s in localFolders)
            {
                string cpath = GetComPath(s, true);
                PutFolderInLog(cpath);
                fLog.putFolder(cpath);
            }
        }

        /// <summary>
        /// Loads the Log from the XML
        /// </summary>
        public void LoadLog()
        {
            List<string> Namelog = new List<string>(nLog().Split('|', '|'));
            List<string> remoteDL = new List<string>(rLog().Split('|', '|'));
            List<string> localDL = new List<string>(lLog().Split('|', '|'));

            List<string> folderLog = new List<string>(foLog().Split('|', '|'));

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
                        Log.Write(l.Error, e.Message);
                    }
                }
            }

            if (foLog() == "")
            {
                foreach (string s in localFolders)
                {
                    string cpath = GetComPath(s, true);
                    PutFolderInLog(cpath);
                    fLog.putFolder(cpath);
                }
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
            Profile.AddAccount(ftpHost(), ftpUser(), ftpPass(), ftpPort());
            Profile.AddPaths(rPath(), lPath(), ftpParent());
            Profile.Protocol = (FTP()) ? (FTPS() ? FtpProtocol.FTPS : FtpProtocol.FTP) : FtpProtocol.SFTP;
            Profile.FtpsInvokeMethod = (FTP()) ? FtpsMethod.None : ((FTPES()) ? FtpsMethod.Explicit : FtpsMethod.Implicit);

            Profile.SyncingMethod = syncMethod();
            Profile.SyncFrequency = syncFrequency();
        }

        /// <summary>
        /// Loads the local folders.
        /// </summary>
        public void LoadLocalFolders()
        {
            localFolders.Clear();
            if (Directory.Exists(lPath()))
            {
                DirectoryInfo d = new DirectoryInfo(lPath());
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

        /// <summary>
        /// Whether the specified path should be synced. Used in selective sync and to avoid syncing the webUI folder, temp files and invalid file/folder-names.
        /// </summary>
        /// <param name='cpath'>
        /// path to check
        /// </param>
        public bool ItemGetsSynced(string name)
        {
            if (name.EndsWith("/"))
                name = name.Substring(0, name.Length - 1);
            string aName = (name.Contains("/")) ? name.Substring(name.LastIndexOf("/")) : name; //the actual name of the file in the given path. (removes the path from the beginning of the given string)
            if (aName.StartsWith("/"))
                aName = aName.Substring(1);

            bool b = !(ignoreList.isIgnored(name)
                || name.Contains("webint") || name.EndsWith(".") || name.EndsWith("..")                                                                                             //web interface, current and parent folders are ignored
                || name.EndsWith("~") || name.StartsWith(".goutputstream") || aName.StartsWith("~")                                                                                 //temporary files are ignored
                || aName == ".ftpquota" || aName == "error_log"                                                                                                                     //server files are ignored
                || aName.Contains('"') || aName.Contains("?") || aName.Contains("*") || aName.Contains(":") || aName.Contains("<") || aName.Contains(">") || aName.Contains("|")    //characters not allowed in windows file/folder names
                );

            Log.Write(l.Debug, "Item {0} gets synced: {1}", name, b);
            return b;
        }

        /// <summary>
        /// whether the file/folder exists in the server.
        /// </summary>
        /// <param name='cpath'>
        /// If set to <c>true</c> cpath.
        /// </param>
        public bool _exists(string cpath)
        {
            try
            {
                if (FTP())
                {
                    bool exists = false;
                    string p = (cpath.Contains("/")) ? cpath.Substring(0, cpath.LastIndexOf("/")) : ".";
                    string name = _name(cpath);
                    foreach (FtpItem f in ftpc.GetDirList(p))
                        if (f.Name.Equals(name))
                            exists = true;
                    return exists;
                }
                else
                    return sftpc.Exists(cpath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// whether a path is in the LocalFolders list. Used to check if deleted items are folders or not.
        /// </summary>
        /// <returns>
        /// True if LocalFolders list contains 'path'
        /// </returns>
        /// <param name='path'>
        /// If set to <c>true</c> path.
        /// </param>
        private bool _isDir(string path)
        {
            return localFolders.Contains(path);
        }

        /// <summary>
        /// Removes the part until the last slash from the provided string.
        /// </summary>
        /// <param name="path">the path to the item</param>
        /// <returns>name of the item</returns>
        private string _name(string path)
        {
            if (path.Contains("/"))
                path = path.Substring(path.LastIndexOf("/"));
            if (path.Contains(@"\"))
                path = path.Substring(path.LastIndexOf(@"\"));
            if (path.StartsWith("/") || path.StartsWith(@"\"))
                path = path.Substring(1);
            return path;
        }

        /// <summary>
        /// returns the current working directory in both SFTP and FTP
        /// </summary>
        private string _currentDirectory
        {
            get { return (FTP()) ? ftpc.CurrentDirectory : sftpc.WorkingDirectory; }
        }

        private void ftp_ValidateServerCertificate(object sender, ValidateServerCertificateEventArgs e)
        {
            // display the certificate to the user and ask the user to either accept or reject the certificate
            //if (MessageBox.Show(e.Certificate.ToString(), "FTPbox - Accept this Certificate?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            //{
            // the user accepted the certicate so we need to inform the FtpClient Component that the certificate is valid
            // be setting the IsCertificateValue property to true
            e.IsCertificateValid = true;
            //}        
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
            fswFiles.Path = lPath();
            fswFolders.Path = lPath();
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
            string cpath = GetComPath(e.FullPath, true);
            if (ItemGetsSynced(cpath) && !FileIsUsed(e.FullPath))
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
                        fQueue.Add(cpath, e.FullPath, TypeOfTransfer.Change);
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
            if (PathIsFolder(e.FullPath) && e.ChangeType == WatcherChangeTypes.Created && !_exists(GetComPath(e.FullPath, true)))
            {
                if (FTP())
                    ftpc.MakeDirectory(GetComPath(e.FullPath, true));
                else
                    sftpc.CreateDirectory(GetComPath(e.FullPath, true));
                fQueue.AddFolder(GetComPath(e.FullPath, true));
                
                fLog.putFolder(GetComPath(e.FullPath, true));
                PutFolderInLog(GetComPath(e.FullPath, true));

                //if (fQueue.CountFolders() == 1)
                //	ShowNotification(e.Name, ChangeAction.created, false);

                GetLink(GetComPath(e.FullPath, true));
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
            if (ItemGetsSynced(GetComPath(e.FullPath, true)))
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
            if (ItemGetsSynced(GetComPath(e.FullPath, true)))
            {
                fswFolders.EnableRaisingEvents = false;
                if (e.FullPath.StartsWith(lPath()))
                {
                    if (FTP())
                    {
                        if (!ftpc.IsConnected)
                        {
                            try
                            {
                                ftpc.Reopen();
                            }
                            catch
                            {
                                Application.Exit();
                            }
                        }
                    }

                    Log.Write(l.Debug, "{0} File {1} ", e.ChangeType.ToString(), e.FullPath);
                    string oldName = GetComPath(e.OldFullPath, true);
                    string newName = GetComPath(e.FullPath, true);
                    Log.Write(l.Debug, "Oldname: {0} Newname: {1}", oldName, newName);

                    if (FTP())
                    {
                        if (oldName == newName || !_exists(oldName))
                        {
                            ftpc.PutFile(e.FullPath, newName, FileAction.CreateNew);
                        }
                        else
                            ftpc.Rename(oldName, newName);
                    }
                    else
                        sftpc.RenameFile(oldName, newName);

                    ShowNotification(e.OldName, ChangeAction.renamed, e.Name);
                    GetLink(GetComPath(e.FullPath, true));

                    if (source == fswFiles)
                    {
                        string cpath = GetComPath(newName, true);
                        RemoveFromLog(GetComPath(oldName, true));
                        FileInfo f = new FileInfo(e.FullPath);
                        UpdateTheLog(cpath, GetLWTof(cpath), f.LastWriteTime);
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
        /// Checks whether a path is a folder or file
        /// </summary>
        /// <returns>
        /// True if the path is a folder
        /// </returns>
        /// <param name='p'>
        /// the path to check
        /// </param>
        public bool PathIsFolder(string p)
        {
            FileAttributes attr = File.GetAttributes(p);

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Gets the common path of both local and remote directories.
        /// </summary>
        /// <returns>
        /// The common path, using forward slashes ( / )
        /// </returns>
        /// <param name='s'>
        /// The full path to be 'shortened'
        /// </param>
        /// <param name='fromLocal'>
        /// True if the given path is in local format.
        /// </param>
        public string GetComPath(string s, bool fromLocal)
        {
            string cp = s;
            //Log.Write(l.Debug, "Getting common path of : {0}", s);

            if (fromLocal)
            {

                if (cp.StartsWith(lPath()))
                {
                    cp = cp.Substring(lPath().Length);
                    cp = cp.Replace(@"\", @"/");
                }
            }
            else
            {
                cp = (cp.StartsWith("/")) ? cp.Substring(1) : cp;
                //Log.Write(l.Debug, "without slash: {0}", cp);
                if (!FTP() && cp.StartsWith(SftpHome) && SftpHome != "")
                    cp = cp.Substring(SftpHome.Length + 1);
                //Log.Write(l.Debug, "without home: {0}", cp);
                if (cp.StartsWith(rPath()))
                {
                    cp = cp.Substring(rPath().Length);
                    //cp = cp.Replace(@"/", @"\");
                }
                //Log.Write(l.Debug, "without remPath: {0}", cp);
            }

            if (cp.StartsWith("/") || cp.StartsWith(@"\"))
                cp = cp.Substring(1);
            if (cp.StartsWith("./"))
                cp = cp.Substring(2);

            if (cp.Equals(null) || cp.Equals(" ") || cp.Equals(""))
                cp = "/";

            return cp;
        }

        /// <summary>
        /// Checks the type of item in the specified path (the item must exist)
        /// </summary>
        /// <param name="p">The path to check</param>
        /// <returns>true if the specified path is a file, false if it's a folder</returns>
        private bool PathIsFile(string p)
        {
            try
            {
                FileAttributes attr = File.GetAttributes(p);
                return !((attr & FileAttributes.Directory) == FileAttributes.Directory);
            }
            catch
            {
                return !localFolders.Contains(p);
            }
        }

        /// <summary>
        /// Delete a remote folder and everything inside it (FTP)
        /// </summary>
        /// <param name="path">path to folder to delete</param>
        /// <param name="RemFromLog">True to also remove deleted stuf from log, false to not.</param>
        public void DeleteFolderFTP(string path)
        {
            CheckConnectionStatus();

            if (_exists(path))
            {
                Log.Write(l.Debug, "About to DeleteFolderFTP: {0}", path);
                
                foreach (FtpItem fi in ftpc.GetDirList(path))
                {
                    if (fi.ItemType == FtpItemType.File)
                    {
                        string fpath = string.Format("{0}/{1}", path, fi.Name);
                        ftpc.DeleteFile(fpath);
                        Log.Write(l.Debug, "Gon'delete: {0}", fpath);
                        RemoveFromLog(GetComPath(fi.FullPath, false));
                    }
                    else if (fi.ItemType == FtpItemType.Directory)
                    {
                        if (fi.Name != "." && fi.Name != "..")
                        {
                            string fpath = string.Format("{0}/{1}", noSlashes(path), fi.Name);
                            Log.Write(l.Debug, "Gon'delete files in: {0}", fpath);
                            RecursiveDeleteFTP(fpath);
                            RemoveFromLog(GetComPath(fi.FullPath, false));
                        }
                    }
                }
                ftpc.DeleteDirectory(path);
                RemoveFromLog(GetComPath(path, false));
            }

            Log.Write(l.Debug, "Deleted {0} - current folder is: {1}", path, ftpc.CurrentDirectory);
        }

        /// <summary>
        /// (recursively) Delete all files and folders inside the specified path. (FTP)
        /// </summary>
        /// <param name="path"></param>
        public void RecursiveDeleteFTP(string path)
        {
            foreach (FtpItem fi in ftpc.GetDirList(path))
            {
                if (fi.ItemType == FtpItemType.File)
                {
                    string fpath = string.Format("{0}/{1}", path, fi.Name);
                    Log.Write(l.Debug, "Gon'delete: {0}", fpath);
                    ftpc.DeleteFile(fpath);
                    RemoveFromLog(GetComPath(fi.FullPath, false));
                }
                else if (fi.ItemType == FtpItemType.Directory)
                {
                    if (fi.Name != "." && fi.Name != "..")
                    {
                        string fpath = string.Format("{0}/{1}", noSlashes(path), fi.Name);
                        RecursiveDeleteFTP(fpath);
                    }
                }
            }

            ftpc.DeleteDirectory(path);
            RemoveFromLog(GetComPath(path, false));
        }

        /// <summary>
        /// Delete a remote folder and everything inside it (SFTP)
        /// </summary>
        /// <param name="path">path to folder to delete</param>
        /// <param name="RemFromLog">True to also remove deleted stuf from log, false to not.</param>
        public void DeleteFolderSFTP(string path)
        {
            if (_exists(path))
            {
                Log.Write(l.Debug, "About to DeleteFolderSFTP: {0}", path);

                foreach (SftpFile f in sftpc.ListDirectory(path)) //"./" + path))
                {
                    string cpath = GetComPath(f.FullName, false);
                    if (f.Name != "." && f.Name != "..")
                    {
                        if (f.IsDirectory)
                        {
                            Log.Write(l.Debug, "Gon'delete files in: {0}", cpath);
                            RecursiveDeleteSFTP(cpath);
                        }
                        else if (f.IsRegularFile)
                        {
                            sftpc.DeleteFile(cpath);
                            RemoveFromLog(cpath);
                        }
                    }
                }
                sftpc.DeleteDirectory(path);
                RemoveFromLog(GetComPath(path, false));
            }
        }

        /// <summary>
        /// (recursively) Delete all files and folders inside the specified path. (SFTP)
        /// </summary>
        /// <param name="path"></param>
        public void RecursiveDeleteSFTP(string path)
        {
            foreach (SftpFile f in sftpc.ListDirectory("./" + path))
            {
                string cpath = GetComPath(f.FullName, false);
                if ((ItemGetsSynced(cpath) || path.StartsWith("webint")) && f.Name != "." && f.Name != "..")
                {
                    if (f.IsDirectory)
                    {
                        Log.Write(l.Debug, "Gon'delete files in: {0}", cpath);
                        RecursiveDeleteSFTP(cpath);
                    }
                    else if (f.IsRegularFile)
                    {
                        sftpc.DeleteFile(cpath);
                        RemoveFromLog(cpath);
                    }
                }
            }
            sftpc.DeleteDirectory(path);
            RemoveFromLog(GetComPath(path, false));
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
        /// Get the HTTP link to a file
        /// </summary>
        /// <param name='cpath'>
        /// The common path to the file/folder.
        /// </param>
        private void GetLink(string cpath)
        {
            Log.Write(l.Debug, "---------------\n Getting link for {0}", cpath);
            string newlink = noSlashes(Profile.HttpPath) + @"/";

            if (!noSlashes(newlink).StartsWith("http://") && !noSlashes(newlink).StartsWith("https://"))
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

        /// <summary>
        /// Checks the connection status and tries to re-login if needed.
        /// </summary>
        private void CheckConnectionStatus()
        {
            Log.Write(l.Debug, "Checking FTP connection...");
            if (!ftpc.IsLoggingOn)
            {
                //System.Timers.Timer t = new System.Timers.Timer();		
                Log.Write(l.Debug, "isConnected: {0}", ftpc.IsConnected);
                try
                {
                    //Log.Write(l.Debug, ftpc.DataTransferMode.ToString());
                    ftpc.DataTransferMode = TransferMode.Passive;
                    //Log.Write(l.Debug, ftpc.DataTransferMode.ToString());
                    FtpItemCollection s = ftpc.GetDirList();
                    Log.Write(l.Debug, "Client is connected!");
                }
                catch (Exception e)
                {
                    Log.Write(l.Error, "Exception m: {0}", e.Message);
                    Log.Write(l.Error, "FTP Client was disconnected, attempting to reconnect");
                    try
                    {                        
                        LoginFTP();
                        if (!rPath().Equals(" ") && !rPath().Equals("/"))
                        {
                            Log.Write(l.Info, ftpc.CurrentDirectory);
                            if (FTP())
                                ftpc.ChangeDirectory(rPath());
                            else
                            {
                                SftpHome = sftpc.WorkingDirectory;
                                SftpHome = (SftpHome.StartsWith("/")) ? SftpHome.Substring(1) : SftpHome;
                                sftpc.ChangeDirectory(rPath());
                            }
                        }
                        if (FTP())
                            Log.Write(l.Info, "Changed current directory to {0}", ftpc.CurrentDirectory);
                        else
                            Log.Write(l.Info, "Changed current directory to {0}, home is: {1}", sftpc.WorkingDirectory, SftpHome);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(l.Info, "Exception MSG: {0}", ex.Message);
                        Application.Exit();
                    }
                }
            }
        }

        #region Queue Operations

        int co = 0;
        private void SyncLocQueueFiles()
        {            
            List<FileQueueItem> fi = new List<FileQueueItem>(fQueue.List());
            string name = null;

            if (FTP())
                CheckConnectionStatus();            

            foreach (FileQueueItem i in fi)
            {
                Log.Write(l.Debug, "FileQueueItem -> cpath: {0} contains: {1} local: {2} current: {3}", i.CommonPath, fLog.Contains(i.CommonPath), fLog.getLocal(i.CommonPath), File.GetLastWriteTime(i.LocalPath));
                if (i.CommonPath.Contains("/"))
                {
                    string cpath = i.CommonPath.Substring(0, i.CommonPath.LastIndexOf("/"));
                    if (_exists(cpath))
                    {
                        
                        Log.Write(l.Debug, "Makin directory: {0} inside {1}", cpath, _currentDirectory);
                        try
                        {
                            if (FTP())
                                ftpc.MakeDirectory(cpath);
                            else
                                sftpc.CreateDirectory(cpath);
                            fLog.putFolder(cpath);
                            PutFolderInLog(cpath);
                        }
                        catch (Exception e)
                        {
                            Log.Write(l.Debug, "Exc Msg: {0}", e.Message);
                        }

                    }
                }
                try
                {                    
                    SetTray(MessageType.Uploading, _name(i.CommonPath));
                    Log.Write(l.Debug, "Gonna put file {0} from queue, current path: {1}", i.CommonPath, _currentDirectory);

                    if (FTP())
                        if (_exists(i.CommonPath))
                            ftpc.DeleteFile(i.CommonPath);

                    if (FTP())
                        ftpc.PutFile(i.LocalPath, i.CommonPath, FileAction.Create);
                    else
                        using (var file = File.OpenRead(i.LocalPath))
                            sftpc.UploadFile(file, i.CommonPath, true);

                    SetTray(MessageType.AllSynced);

                    Log.Write(l.Debug, "Done");

                    name = i.LocalPath;
                    co++;
                    fQueue.Remove(i.CommonPath);
                    UpdateTheLog(i.CommonPath, GetLWTof(i.CommonPath), File.GetLastWriteTime(i.LocalPath));
                    
                    GetLink(i.CommonPath);                    
                }
                catch (Exception ex)
                {
                    SetTray(MessageType.AllSynced);
                    Log.Write(l.Debug, "lError: {0}", ex.Message);
                    try
                    {
                        ftpc.Reopen();
                    }
                    catch
                    {
                        Application.Exit();
                    }
                    /*
                    if (ftpc.Exists(i.CommonPath))
                    {
                        name = i.LocalPath;
                        //c++;
                        fQueue.Remove(i.CommonPath);
                        UpdateTheLog(i.CommonPath, GetLWTof(i.CommonPath), File.GetLastWriteTime(i.LocalPath));	
                    }*/
                }
            }

            if (fQueue.reCheck)
            {
                fQueue.reCheck = false;
                SyncInFolder(lPath(), true);
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
        }

        public void SyncRemQueueFiles()
        {
            int c = 0;
            List<FileQueueItem> fi = new List<FileQueueItem>(fQueue.List());
            string name = null;
            
            foreach (FileQueueItem i in fi)
            {
                if (ItemGetsSynced(i.CommonPath))
                {
                    if (dQueue.Contains(i.LocalPath))
                    {
                        fQueue.Remove(i.CommonPath);
                        continue;
                    }

                    Log.Write(l.Debug, "Gonna get remote file {0} from queue, local path: {1}", i.CommonPath, i.LocalPath);
                    try
                    {
                        SetTray(MessageType.Downloading, _name(i.CommonPath));

                        if (FTP())                    
                            ftpc.GetFile(i.CommonPath, i.LocalPath, FileAction.Create);                                                    
                        else
                            using (FileStream f = new FileStream(i.LocalPath, FileMode.Create, FileAccess.ReadWrite))
                                sftpc.DownloadFile(i.CommonPath, f);
                            //using (var file = File.OpenWrite(i.LocalPath))
                               // sftpc.DownloadFile(i.CommonPath, file);
                        
                        Log.Write(l.Debug, "Done");
                        SetTray(MessageType.AllSynced);                        

                        name = i.LocalPath;
                        c++;
                        fQueue.Remove(i.CommonPath);
                        UpdateTheLog(i.CommonPath, GetLWTof(i.CommonPath), File.GetLastWriteTime(i.LocalPath));
                        GetLink(i.CommonPath);                       
                    }
                    catch (FtpException ex)
                    {
                        SetTray(MessageType.AllSynced);
                        Log.Write(l.Error, "rError: {0}", ex.Message);
                        Log.Write(l.Debug, "current dir: {0}", _currentDirectory);

                        if (!_exists(i.CommonPath))
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
                    ftpc.ChangeDirectory(rPath());
                }
                catch
                {
                    Application.Exit();
                }
                fi = new List<FileQueueItem>(fQueue.List());
                foreach (FileQueueItem i in fi)
                {
                    if (ItemGetsSynced(i.CommonPath))
                    {
                        Log.Write(l.Debug, "Gonna retry to get remote file {0} from queue, local path: {1}", i.CommonPath, i.LocalPath);
                        try
                        {
                            SetTray(MessageType.Downloading, _name(i.CommonPath));

                            if (FTP())
                                ftpc.GetFile(i.CommonPath, i.LocalPath, FileAction.Create);
                            else
                                using (FileStream f = new FileStream(i.LocalPath, FileMode.Create, FileAccess.ReadWrite))
                                    sftpc.DownloadFile(i.CommonPath, f);

                            Log.Write(l.Debug, "Done");
                            SetTray(MessageType.AllSynced);

                            name = i.LocalPath;
                            c++;
                            fQueue.Remove(i.CommonPath);
                            UpdateTheLog(i.CommonPath, GetLWTof(i.CommonPath), File.GetLastWriteTime(i.LocalPath));
                        }
                        catch (FtpException ex)
                        {
                            SetTray(MessageType.AllSynced);
                            Log.Write(l.Debug, "rError: {0}", ex.Message);
                            Log.Write(l.Debug, "current dir: {0}", _currentDirectory);

                            if (!_exists(i.CommonPath))
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
            if (FTP())
                    CheckConnectionStatus();
            Syncing();
            fQueue.Busy = true;
            DirectoryInfo di = new DirectoryInfo(path);

            string cparent = GetComPath(path, true);

            Log.Write(l.Debug, "Syncing local folder: {0} cparent: {1}", path, cparent);
            bool cParentExists = (cparent.Equals("/") || cparent.Equals(@"\") || cparent.Equals("")) ? true : _exists(cparent);

            //foreach (FileInfo f in di.GetFiles())
            //	Console.Write("{0} - ", f.Name);
            //ftpc.Exists(cparent);            
            //try
            //{
             //   if (!cparent.Equals("/") && FTP())
              //      if (!ftpc.Exists(cparent))
               //         ftpc.MakeDirectory(cparent);
            //}
            //catch (Exception e) { Log.Write(l.Debug, "exception msg: {0}", e.Message); }

            foreach (FileInfo fi in di.GetFiles())
            {
                string cpath = GetComPath(fi.FullName, true);

                if (!ItemGetsSynced(cpath))
                    continue;

                if (cParentExists)
                {
                    Log.Write(l.Debug, "~~Cpath: {0} lLWT: {1} LogLWT: {2} in {3}", cpath, fi.LastWriteTime.ToString(), fLog.getLocal(cpath).ToString(), _currentDirectory);
                    
                    if (!fi.FullName.EndsWith("~") && !fi.Name.StartsWith(".goutputstream")) //&& !fQueue.Contains(cpath))
                    {
                        bool ex = false;
                        try
                        {
                            ex = _exists(cpath);
                        }
                        catch { }

                        Log.Write(l.Debug, "Exists: {0} contains: {1}", ex, fLog.Contains(cpath));

                        if (!fLog.Contains(cpath) || !ex)
                        {
                            //Log.Write(l.Debug, "ADDED! cpath: {0} contains: {1} local: {2} current: {3}", cpath, fLog.Contains(cpath), fLog.getLocal(cpath), fi.LastWriteTime.ToString());
                            fQueue.Add(cpath, fi.FullName, TypeOfTransfer.Create);
                        }
                        else if (fLog.getLocal(cpath).ToString() != fi.LastWriteTime.ToString())
                        {
                            //Log.Write(l.Debug, "ADDED cpath: {0} contains: {1} local: {2} current: {3}", cpath, fLog.Contains(cpath), fLog.getLocal(cpath), fi.LastWriteTime.ToString());
                            fQueue.Add(cpath, fi.FullName, TypeOfTransfer.Change);
                        }
                    }
                }
                else
                    fQueue.Add(cpath, fi.FullName, TypeOfTransfer.Create);
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

                string cparent = GetComPath(path, true);
                Log.Write(l.Debug, "Syncing local folder (recursive): {0} cparent: {1}", path, cparent);

                List<string> RemoteFilesList = new List<string>(FullRemoteList);

                bool cParentExists = (cparent.Equals("/") || cparent.Equals(@"\") || cparent.Equals("")) ? true : RemoteFilesList.Contains(cparent);

                SetTray(MessageType.Listing);

                foreach (DirectoryInfo d in di.GetDirectories("*", SearchOption.AllDirectories))
                {
                    string cpath = GetComPath(d.FullName, true);
                    if (!RemoteFilesList.Contains(cpath))
                    {
                        Log.Write(l.Debug, "Making directory: {0}", cpath);
                        if (FTP())
                            ftpc.MakeDirectory(cpath);
                        else
                            sftpc.CreateDirectory(cpath);

                        GetLink(cpath);

                        fLog.putFolder(cpath);
                        PutFolderInLog(cpath);

                        fQueue.AddFolder(cpath);
                    }
                }

                foreach (FileInfo fi in di.GetFiles("*", SearchOption.AllDirectories))
                {
                    string cpath = GetComPath(fi.FullName, true);

                    if (!ItemGetsSynced(cpath))
                        continue;

                    if (cParentExists)
                    {
                        Log.Write(l.Debug, "~~Cpath: {0} lLWT: {1} LogLWT: {2} in {3}", cpath, fi.LastWriteTime.ToString(), fLog.getLocal(cpath).ToString(), _currentDirectory);

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
                                fQueue.Add(cpath, fi.FullName, TypeOfTransfer.Create);
                            }
                            else if (fLog.getLocal(cpath).ToString() != fi.LastWriteTime.ToString())
                            {
                                //Log.Write(l.Debug, "ADDED cpath: {0} contains: {1} local: {2} current: {3}", cpath, fLog.Contains(cpath), fLog.getLocal(cpath), fi.LastWriteTime.ToString());
                                fQueue.Add(cpath, fi.FullName, TypeOfTransfer.Change);
                            }
                        }
                    }
                    else
                        fQueue.Add(cpath, fi.FullName, TypeOfTransfer.Create);
                }

                SetTray(MessageType.AllSynced);

                SyncLocQueueFiles();
                Log.Write(l.Debug, "---------------------");
                DoneSyncing();
            }
            catch (Exception ex)
            {
                Log.Write(l.Error, "Exception: {0}", ex.Message);
                if (FTP())
                    CheckConnectionStatus();
                SyncInFolder(path, true);
            }
        }

        private void DeleteFromQueue()
        {            
            Log.Write(l.Debug, "About to delete {0} item(s) from queue", dQueue.Count);
            if (FTP())
                CheckConnectionStatus();

            while (dQueue.Count > 0 || dQueue.reCheck) 
            {
                Log.Write(l.Debug, "dQueue.Count: {0}", dQueue.Count.ToString());
                dQueue.Busy = true;

                List<string> li = new List<string>(dQueue.List);
                foreach (string s in li)
                {
                    Log.Write(l.Debug, "Found in dQueue: {0}", s);
                    if (!_exists(GetComPath(s, true)))
                        dQueue.Remove(s);
                }
                
                List<string> alreadyChecked = new List<string>();

                foreach (string s in localFiles)
                {
                    if (alreadyChecked.Contains(s) || !ItemGetsSynced(GetComPath(s, true)))
                    {
                        dQueue.Remove(s);
                        continue;
                    }
                    
                    if (!File.Exists(s))
                    {
                        try
                        {
                            Log.Write(l.Debug, "File {0} was deleted, cpath: {1}", s, GetComPath(s, true));
                            if (FTP())
                                ftpc.DeleteFile(GetComPath(s, true));
                            else
                                sftpc.DeleteFile(GetComPath(s, true));
                        }
                        catch { }

                        dQueue.LastItem = new KeyValuePair<string, bool>(_name(s), true);
                        dQueue.Counter++;
                        RemoveFromLog(GetComPath(s, true));
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
                        if (FTP())
                            DeleteFolderFTP(GetComPath(s, true));
                        else
                            DeleteFolderSFTP(GetComPath(s, true));

                        dQueue.LastItem = new KeyValuePair<string, bool>(_name(s), false);
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
            if (ShowNots())
            {
                name = _name(name);

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
            if (ShowNots())
            {
                name = _name(name);
                newname = _name(newname);

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
            if (ShowNots() && i > 0)
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

            if (ShowNots() && ( f > 0 || d > 0))
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
            if (c == ChangeAction.deleted && ShowNots())
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
            if (ShowNots())
            {
                tray.ShowBalloonTip(30, "FTPbox", languages.Get(lang() + "/tray/link_copied", "Link copied to clipboard"), ToolTipIcon.Info);
                link = null;
            }
        }

        /// <summary>
        /// Get the list of recent files and update the tray menu
        /// </summary>
        /// <param name="cpath"></param>
        private void Get_Recent(string cpath)
        {
            string name = _name(cpath);
            string path = Path.Combine(lPath(), cpath);

            locLink = path;
            FileInfo f = new FileInfo(path);
            Log.Write(l.Debug, "LastWriteTime is: {0} for {1} - {2}", f.LastWriteTime.ToString(), path, lPath());
            
            recentFiles.Add(name, link, path, f.LastWriteTime);

            for (int i = 0; i < 5; i++) // recentFiles.count(); i++)
            {
                if (trayMenu.InvokeRequired)
                {
                    trayMenu.Invoke(new MethodInvoker(delegate
                    {
                        recentFilesToolStripMenuItem.DropDownItems[i].Text = (recentFiles.getName(i) == "Not available") ? _(MessageType.NotAvailable) : recentFiles.getName(i);
                        if (recentFilesToolStripMenuItem.DropDownItems[i].Text != _(MessageType.NotAvailable))
                            recentFilesToolStripMenuItem.DropDownItems[i].Enabled = true;

                        if (recentFiles.Count >= i)
                            recentFilesToolStripMenuItem.DropDownItems[i].ToolTipText = recentFiles.getDate(i).ToString("dd MMM HH:mm:ss");
                    }));
                }
                else
                {
                    recentFilesToolStripMenuItem.DropDownItems[i].Text = (recentFiles.getName(i) == "Not available") ? _(MessageType.NotAvailable) : recentFiles.getName(i);
                    if (recentFilesToolStripMenuItem.DropDownItems[i].Text != _(MessageType.NotAvailable))
                        recentFilesToolStripMenuItem.DropDownItems[i].Enabled = true;

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
            string curlan = lang();

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
                else
                    locallang = "English";

                List<string> alllang = new List<string>{ "es", "de", "fr", "nl", "el", "it", "tr", "pt-BR", "fo", "sv", "sq", "ro", "ko", "ru", "ja", "no", "hu", "vi", "zh_HANS", "zh_HANT", "lt", "da", "pl", "hr" };

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

            this.Text = "FTPbox | " + languages.Get(lan + "/main_form/options", "Options");
            //general tab
            tabGeneral.Text = languages.Get(lan + "/main_form/general", "General");
            tabAccount.Text = languages.Get(lan + "/main_form/account", "Account");
            gAccount.Text = "FTP " + languages.Get(lan + "/main_form/account", "Account");
            labHost.Text = languages.Get(lan + "/main_form/host", "Host") + ":";
            labUN.Text = languages.Get(lan + "/main_form/username", "Username") + ":";
            labPort.Text = languages.Get(lan + "/main_form/port", "Port") + ":";
            labMode.Text = languages.Get(lan + "/main_form/mode", "Mode") + ":";
            //bAddFTP.Text = languages.Get(lan + "/main_form/change", "Change");
            gApp.Text = languages.Get(lan + "/main_form/application", "Application");
            gWebInt.Text = languages.Get(lan + "/web_interface/web_int", "Web Interface");
            chkWebInt.Text = languages.Get(lan + "/web_interface/use_webint", "Use the Web Interface");
            labViewInBrowser.Text = languages.Get(lan + "/web_interface/view", "(View in browser)");
            chkShowNots.Text = languages.Get(lan + "/main_form/show_nots", "Show notifications");
            chkStartUp.Text = languages.Get(lan + "/main_form/start_on_startup", "Start on system start-up");            
            //ftpbox tab
            gDetails.Text = languages.Get(lan + "/main_form/details", "Details");
            labRemPath.Text = languages.Get(lan + "/main_form/remote_path", "Remote Path") + ":";
            labLocPath.Text = languages.Get(lan + "/main_form/local_path", "Local Path") + ":";
            bChangeBox.Text = languages.Get(lan + "/main_form/change", "Change");
            gLinks.Text = languages.Get(lan + "/main_form/links", "Links");
            labFullPath.Text = languages.Get(lan + "/main_form/account_full_path", "Account's full path") + ":";
            labLinkClicked.Text = languages.Get(lan + "/main_form/when_not_clicked", "When tray notification or recent file is clicked") + ":";
            rOpenInBrowser.Text = languages.Get(lan + "/main_form/open_in_browser", "Open link in default browser");
            rCopy2Clipboard.Text = languages.Get(lan + "/main_form/copy", "Copy link to clipboard");
            rOpenLocal.Text = languages.Get(lan + "/main_form/open_local", "Open the local file");
            //bandwidth tab
            tabBandwidth.Text = languages.Get(lan + "/main_form/bandwidth", "Bandwidth");
            gSyncing.Text = languages.Get(lan + "/main_form/sync_freq", "Sync Frequency");
            labSyncWhen.Text = languages.Get(lan + "/main_form/sync_when", "Synchronize remote files");
            cAuto.Text = languages.Get(lan + "/main_form/auto", "automatically every");
            labSeconds.Text = languages.Get(lan + "/main_form/seconds", "seconds");
            cManually.Text = languages.Get(lan + "/main_form/manually", "manually");
            gLimits.Text = languages.Get(lan + "/main_form/speed_limits", "Speed Limits");
            labDownSpeed.Text = languages.Get(lan + "/main_form/limit_download", "Limit Download Speed");
            labUpSpeed.Text = languages.Get(lan + "/main_form/limit_upload", "Limit Upload Speed");
            labNoLimits.Text = languages.Get(lan + "/main_form/no_limits", "( set to 0 for no limits )");
            //language tab
            tabLanguage.Text = languages.Get(lan + "/main_form/language", "Language");
            //about tab
            tabAbout.Text = languages.Get(lan + "/main_form/about", "About");
            labCurVersion.Text = languages.Get(lan + "/main_form/current_version", "Current Version") + ":";
            labTeam.Text = languages.Get(lan + "/main_form/team", "The Team") + ":";
            labSite.Text = languages.Get(lan + "/main_form/website", "Official Website") + ":";
            labContact.Text = languages.Get(lan + "/main_form/contact", "Contact") + ":";
            labLangUsed.Text = languages.Get(lan + "/main_form/coded_in", "Coded in") + ":";
            gNotes.Text = languages.Get(lan + "/main_form/notes", "Notes");
            gContribute.Text = languages.Get(lan + "/main_form/contribute", "Contribute");
            labFree.Text = languages.Get(lan + "/main_form/ftpbox_is_free", "- FTPbox is free and open-source");
            labContactMe.Text = languages.Get(lan + "/main_form/contact_me", "- Feel free to contact me for anything.");
            linkLabel1.Text = languages.Get(lan + "/main_form/report_bug", "Report a bug");
            linkLabel2.Text = languages.Get(lan + "/main_form/request_feature", "Request a feature");
            labDonate.Text = languages.Get(lan + "/main_form/donate", "Donate") + ":";
            labSupportMail.Text = "support@ftpbox.org";
            //tray
            optionsToolStripMenuItem.Text = languages.Get(lan + "/main_form/options", "Options");
            recentFilesToolStripMenuItem.Text = languages.Get(lan + "/tray/recent_files", "Recent Files");
            aboutToolStripMenuItem.Text = languages.Get(lan + "/main_form/about", "About");
            SyncToolStripMenuItem.Text = languages.Get(lan + "/tray/start_syncing", "Start Syncing");
            exitToolStripMenuItem.Text = languages.Get(lan + "/tray/exit", "Exit");

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
            
            AppSettings.Put("Settings/Language", lan);            
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
            else //if (not == "updated")
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
                Log.Write(l.Error, "[Error] -> {0} -> {1}", ex.Message);
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
                Log.Write(l.Error, "[Error] -> {0} -> {1}", ex.Message);
            }
        }

        #endregion

        #region Tray Controls' Event Handlers

        private void tray_MouseDoubleClick(object sender, MouseEventArgs e)
        {           
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                Process.Start("explorer.exe", lPath()); 
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

        private TrayAction SettingsTrayAction
        {
            get
            {
                if (AppSettings.Get("Settings/OpenInBrowser", "True") == "True" || AppSettings.Get("Settings/OpenInBrowser", "OpenInBrowser") == "OpenInBrowser")
                    return TrayAction.OpenInBrowser;
                else if (AppSettings.Get("Settings/OpenInBrowser", "True") == "False" || AppSettings.Get("Settings/OpenInBrowser", "OpenInBrowser") == "CopyLink")
                    return TrayAction.CopyLink;
                else
                    return TrayAction.OpenLocalFile;
            }
        }

        #region Form Controls
        private void rOpenInBrowser_CheckedChanged(object sender, EventArgs e)
        {
            if (rOpenInBrowser.Checked)
            {
                _trayAct = TrayAction.OpenInBrowser;
                AppSettings.Put("Settings/OpenInBrowser", "OpenInBrowser");
            }
        }

        private void rCopy2Clipboard_CheckedChanged(object sender, EventArgs e)
        {
            if (rCopy2Clipboard.Checked)
            {
                _trayAct = TrayAction.CopyLink;
                AppSettings.Put("Settings/OpenInBrowser", "CopyLink");
            }
        }

        private void rOpenLocal_CheckedChanged(object sender, EventArgs e)
        {
            if (rOpenLocal.Checked)
            {
                _trayAct = TrayAction.OpenLocalFile;
                AppSettings.Put("Settings/OpenInBrowser", "OpenLocalFile");
            }
        }

        private bool ChangingAccount = false;
        private void bChangeBox_Click(object sender, EventArgs e)
        {
            ChangeAccount();
            //Application.Restart();
        }

        private void tParent_TextChanged(object sender, EventArgs e)
        {
            AppSettings.Put("Paths/Parent", tParent.Text);
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
                        ftpc.Close();
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

        /// <summary>
        /// Starts the thread that checks the remote files.
        /// </summary>
        public void StartRemoteSync()
        {
            if (loggedIn)
            {
                rcThread = new Thread(SyncRemote);
                rcThread.Start();
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
                Log.Write(l.Debug, "Starting remote sync...");
                rcThread = new Thread(SyncRemote);
                rcThread.Start();
            }            
        }

        List<string> allFilesAndFolders;
        /// <summary>
        /// Sync remote files to the local folder
        /// </summary>
        public void SyncRemote()
        {
            allFilesAndFolders = new List<string>();

            fQueue.Busy = true;

            //Syncing();

            if (FTP())
                CheckConnectionStatus();

            Log.Write(l.Debug, "Current directory: {0}", (FTP()) ? ftpc.CurrentDirectory : sftpc.WorkingDirectory);

            SetTray(MessageType.Listing);

            if (FTP())
            {
                foreach (FtpItem f in ftpc.GetDirListDeep("."))
                {
                    string cpath = GetComPath(f.FullPath, false);
                    allFilesAndFolders.Add(GetComPath(f.FullPath, false));

                    if (!ItemGetsSynced(cpath))
                        continue;

                    if (f.ItemType == FtpItemType.File)
                    {
                        string lpath = System.IO.Path.Combine(lPath(), cpath.Replace("/", @"\"));
                        Log.Write(l.Debug, "Found: {0} cpath is: {1} lpath: {2} type: {3}", f.Name, cpath, lpath, f.ItemType.ToString());
                        if (File.Exists(lpath))
                            CheckExistingFile(cpath, f.Modified, lpath);
                        else
                        {
                            fQueue.Add(cpath, lpath, TypeOfTransfer.Create);
                        }
                    }
                    else if (f.ItemType == FtpItemType.Directory)
                    {                        
                        string lpath = System.IO.Path.Combine(lPath(), cpath);

                        if (!Directory.Exists(lpath) && ItemGetsSynced(cpath) && !dQueue.Contains(lpath))
                        {
                            Directory.CreateDirectory(lpath);
                            fQueue.AddFolder(cpath);

                            fLog.putFolder(cpath);
                            PutFolderInLog(cpath);

                            GetLink(cpath);
                        }
                    }
                }
            }
            else
            {
                foreach (SftpFile f in sftpc.ListDirectory("."))
                {
                    allFilesAndFolders.Add(GetComPath(f.FullName, false));
                    string cpath = GetComPath(f.FullName, false);

                    if (!ItemGetsSynced(cpath))
                        continue;

                    if (f.IsDirectory)
                    {
                        Log.Write(l.Debug, "~~~~~~~~~> found directory: {0}", f.FullName);
                        string lpath = System.IO.Path.Combine(lPath(), cpath);
                        if (!Directory.Exists(lpath) && ItemGetsSynced(cpath) && !dQueue.Contains(lpath))
                        {
                            Directory.CreateDirectory(lpath);
                            fQueue.AddFolder(cpath);

                            fLog.putFolder(cpath);
                            PutFolderInLog(cpath);

                            GetLink(cpath);
                        }

                        if (ItemGetsSynced(cpath))
                            SftpRecursiveListing(cpath);
                    }
                    else if (f.IsRegularFile)
                    {
                        string lpath = System.IO.Path.Combine(lPath(), cpath.Replace("/", @"\"));
                        Log.Write(l.Debug, "Found: {0} cpath is: {1} lpath: {2} type: {3}", f.Name, cpath, lpath, (f.IsRegularFile) ? "file" : "folder");
                        if (File.Exists(lpath))
                            CheckExistingFile(cpath, f.LastWriteTime, lpath);
                        else
                        {
                            fQueue.Add(cpath, lpath, TypeOfTransfer.Create);
                        }
                    }
                }                        
            }

            SetTray(MessageType.AllSynced);

            SyncRemQueueFiles();

            DirectoryInfo di = new DirectoryInfo(lPath());
            int count = 0;
            string lastname = null;
            bool lastIsFile = false;
            foreach (FileInfo f in di.GetFiles("*", SearchOption.AllDirectories))
            {
                string cpath = GetComPath(f.FullName, true);
                if (!allFilesAndFolders.Contains(cpath) && fLog.Contains(cpath))
                {
                    count++;
                    lastname = f.Name;
                    lastIsFile = PathIsFile(f.FullName);
                    Log.Write(l.Info, "Deleting local file: {0}", f.FullName);
                    File.Delete(f.FullName);
                    RemoveFromLog(cpath);
                }
            }

            foreach (DirectoryInfo d in di.GetDirectories("*", SearchOption.AllDirectories))
            {
                //Log.Write(l.Debug, "Found local folder: {0}", d.FullName);
                string cpath = GetComPath(d.FullName, true);
                if (!allFilesAndFolders.Contains(cpath) && fLog.Folders.Contains(cpath))
                {
                    count++;
                    lastname = d.Name;
                    lastIsFile = false;
                    Log.Write(l.Info, "Deleting local folder: {0}", d.FullName);
                    try
                    {
                        Directory.Delete(d.FullName, true);
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
                SyncInFolder(lPath(), true);
            if (dQueue.reCheck)
                DeleteFromQueue();

            if (Profile.SyncingMethod == SyncMethod.Automatic)
            {
                tSync = new System.Threading.Timer(new TimerCallback(StartRemoteSync), null, 1000 * Profile.SyncFrequency, 0);
                Log.Write(l.Debug, "Syncing set to start in {0} seconds.", Profile.SyncFrequency);
                Log.Write(l.Debug, "~~~~~~~~~~~~~~~~~");
            }
        }

        private List<string> FoldersWithSpaces = new List<string>();
        /// <summary>
        /// Manually get the list of files and folders inside folders that contain spaces
        /// </summary>
        /// <param name="path">The folder in which to look</param>
        private void FtpRecursiveListing(string path)
        {
            foreach (string f in FoldersWithSpaces)
            {
                ftpc.ChangeDirectoryMultiPath(f);
                foreach (FtpItem fi in ftpc.GetDirListDeep("."))
                {
                    string cpath = GetComPath(fi.FullPath, false);
                    allFilesAndFolders.Add(GetComPath(fi.FullPath, false));

                    if (!ItemGetsSynced(cpath))
                        continue;

                    if (fi.ItemType == FtpItemType.File)
                    {
                        string lpath = System.IO.Path.Combine(lPath(), cpath.Replace("/", @"\"));
                        Log.Write(l.Debug, "Found: {0} cpath is: {1} lpath: {2} type: {3}", fi.Name, cpath, lpath, fi.ItemType.ToString());
                        if (File.Exists(lpath))
                            CheckExistingFile(cpath, fi.Modified, lpath);
                        else
                        {
                            fQueue.Add(cpath, lpath, TypeOfTransfer.Create);
                        }
                    }
                    else if (fi.ItemType == FtpItemType.Directory)
                    {
                        string lpath = System.IO.Path.Combine(lPath(), cpath);

                        if (!Directory.Exists(lpath) && ItemGetsSynced(cpath) && !dQueue.Contains(lpath))
                        {
                            Directory.CreateDirectory(lpath);
                            fQueue.AddFolder(cpath);

                            fLog.putFolder(cpath);
                            PutFolderInLog(cpath);

                            GetLink(cpath);
                        }
                    }
                }
            }
        }

        private void FtpRecursiveFolderListing(string path)
        {

        }

        /// <summary>
        /// Gets the full (recursive) list of files and folders inside the given path
        /// </summary>
        /// <param name="path">The path to search into</param>
        private void SftpRecursiveListing(string path)
        {
            Log.Write(l.Debug, "Listing inside directory: {0}", path);            

            try
            {
                foreach (SftpFile f in sftpc.ListDirectory("./" + path))
                {
                    allFilesAndFolders.Add(GetComPath(f.FullName, false));

                    string cpath = GetComPath(f.FullName, false);
                    string lpath = System.IO.Path.Combine(lPath(), cpath.Replace("/", @"\"));

                    Log.Write(l.Debug, "----- Found folder: {0} local Path: {1} exists: {2}", cpath, lpath, Directory.Exists(lpath));

                    if (!ItemGetsSynced(cpath))
                        continue;

                    if (f.IsDirectory)
                    {
                        Log.Write(l.Debug, "~~~~~~~~~>) found directory: {0}", f.FullName);
                        if (!Directory.Exists(lpath) && ItemGetsSynced(cpath))
                        {
                            Directory.CreateDirectory(lpath);
                            fQueue.AddFolder(cpath);

                            fLog.putFolder(cpath);
                            PutFolderInLog(cpath);

                            GetLink(cpath);
                        }
                        if (ItemGetsSynced(cpath))
                            SftpRecursiveListing(cpath);
                    }
                    else if (f.IsRegularFile)
                    {
                        Log.Write(l.Debug, "Found: {0} cpath is: {1} lpath: {2} type: {3}", f.Name, cpath, lpath, (f.IsRegularFile) ? "file" : "folder");
                        if (File.Exists(lpath))
                            CheckExistingFile(cpath, f.LastWriteTime, lpath);
                        else
                        {
                            fQueue.Add(cpath, lpath, TypeOfTransfer.Create);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write("Error in recursive list: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Checks an existing file for any changes
        /// </summary>
        /// <param name="cpath">the common path to the existing files</param>
        /// <param name="rLWT">the remote LastWriteTime of the file</param>
        /// <param name="lpath">the local path to the file</param>
        private void CheckExistingFile(string cpath, DateTime rLWT, string lpath)
        {
            FileInfo fi = new FileInfo(lpath);
            DateTime lLWT = fi.LastWriteTime;

            if (FTP())
                rLWT = GetLWTof(cpath);

            DateTime lDTLog = fLog.getLocal(cpath);
            DateTime rDTLog = fLog.getRemote(cpath);

            int rResult = DateTime.Compare(rLWT, rDTLog);
            int lResult = DateTime.Compare(lLWT, lDTLog);
            int bResult = DateTime.Compare(rLWT, lLWT);

            Log.Write(l.Debug, "Checking existing file: {0} rem: {1} remLog: {2} loc {3}", cpath, rLWT.ToString(), rDTLog.ToString() , lLWT.ToString());
            Log.Write(l.Debug, "rResult: {0} lResult: {1} bResult: {2}", rResult, lResult, bResult);

            TimeSpan rdif = rLWT - rDTLog;
            TimeSpan ldif = lLWT - lDTLog;

            if (rResult > 0 && lResult > 0 && rdif.TotalSeconds > 1 && ldif.TotalSeconds > 1)
            {
                if (rdif.TotalSeconds > ldif.TotalSeconds)
                {
                    //MessageBox.Show("rdif.TotalSeconds > ldif.TotalSeconds" + Environment.NewLine + string.Format("{0} - {1}", rLWT.ToString(), rDTLog.ToString()) + Environment.NewLine + string.Format("{0} - {1}", lLWT.ToString(), lDTLog.ToString()));
                    fQueue.Add(cpath, lpath, TypeOfTransfer.Change);
                }
                else
                {
                    //MessageBox.Show("rdif.TotalSeconds < ldif.TotalSeconds" + Environment.NewLine + string.Format("{0} - {1}", rLWT.ToString(), rDTLog.ToString()) + Environment.NewLine + string.Format("{0} - {1}", lLWT.ToString(), lDTLog.ToString()));
                    fQueue.reCheck = true;
                }
            }
            else if (rResult > 0 && rdif.TotalSeconds > 1)
            {
                fQueue.Add(cpath, lpath, TypeOfTransfer.Change);
            }
            else if (lResult > 0 && ldif.TotalSeconds > 1)
            {
                Log.Write(l.Debug, "lResult > 0 because lWT: {0} & lWTLog: {1}", lLWT, lDTLog);
                fQueue.reCheck = true;
                //doesnt seem to be required now
            }
        }	

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
                    return languages.Get(Profile.Language + "/tray/changed", "{0} was changed.");
                case MessageType.ItemCreated:
                    return languages.Get(Profile.Language + "/tray/created", "{0} was created.");
                case MessageType.ItemDeleted:
                    return languages.Get(Profile.Language + "/tray/deleted", "{0} was deleted.");
                case MessageType.ItemRenamed:
                    return languages.Get(Profile.Language + "/tray/renamed", "{0} was renamed to {1}.");
                case MessageType.ItemUpdated:
                    return languages.Get(Profile.Language + "/tray/updated", "{0} was updated.");
                case MessageType.FilesOrFoldersUpdated:
                    return languages.Get(Profile.Language + "/tray/FilesOrFoldersUpdated", "{0} {1} have been updated");
                case MessageType.FilesOrFoldersCreated:
                    return languages.Get(Profile.Language + "/tray/FilesOrFoldersCreated", "{0} {1} have been created");
                case MessageType.FilesAndFoldersChanged:
                    return languages.Get(Profile.Language + "/tray/FilesAndFoldersChanged", "{0} {1} and {2} {3} have been updated");
                case MessageType.ItemsDeleted:
                    return languages.Get(Profile.Language + "/tray/ItemsDeleted", "{0} items have been deleted.");
                case MessageType.File:
                    return languages.Get(Profile.Language + "/tray/file", "File");
                case MessageType.Files:
                    return languages.Get(Profile.Language + "/tray/files", "Files");
                case MessageType.Folder:
                    return languages.Get(Profile.Language + "/tray/folder", "Folder");
                case MessageType.Folders:
                    return languages.Get(Profile.Language + "/tray/folders", "Folders");
                case MessageType.LinkCopied:
                    return languages.Get(Profile.Language + "/tray/link_copied", "Link copied to clipboard");                                
                case MessageType.Connecting:
                    return languages.Get(Profile.Language + "/tray/connecting", "FTPbox - Connecting...");
                case MessageType.Disconnected:
                    return languages.Get(Profile.Language + "/tray/disconnected", "FTPbox - Disconnected");
                case MessageType.Reconnecting:
                    return languages.Get(Profile.Language + "/tray/reconnecting", "FTPbox - Re-Connecting...");
                case MessageType.Listing:
                    return languages.Get(Profile.Language + "/tray/listing", "FTPbox - Listing...");
                case MessageType.Uploading:
                    return "FTPbox" + Environment.NewLine + languages.Get(Profile.Language + "/tray/uploading", "Uploading {0}");
                case MessageType.Downloading:
                    return "FTPbox" + Environment.NewLine + languages.Get(Profile.Language + "/tray/downloading", "Downloading {0}");
                case MessageType.Syncing:
                    return languages.Get(Profile.Language + "/tray/syncing", "FTPbox - Syncing");
                case MessageType.AllSynced:
                    return languages.Get(Profile.Language + "/tray/synced", "FTPbox - All files synced");
                case MessageType.Offline:
                    return languages.Get(Profile.Language + "/tray/offline", "FTPbox - Offline");
                case MessageType.Ready:
                    return languages.Get(Profile.Language + "/tray/ready", "FTPbox - Ready");
                case MessageType.Nothing:
                    return "FTPbox";
                case MessageType.NotAvailable:
                    return languages.Get(Profile.Language + "/tray/not_available", "Not Available");
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
                Log.Write(l.Error, "error setting tray: {0}", ex.Message);
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
                Log.Write(l.Error, "error setting tray: {0}", ex.Message);
            }
        }

        #endregion

        #region Update System
        WebBrowser br = new WebBrowser();
        /// <summary>
        /// checks for an update
        /// called on each start-up of FTPbox.
        /// </summary>
        private void CheckForUpdate()
        {
            try
            {
                br.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(browser_DocumentCompleted);
                br.Navigate(@"http://ftpbox.org/latestversion.txt");
            }
            catch (Exception ex)
            {
                Log.Write(l.Debug, "Error with version checking: {0}", ex.Message);
            }
        }

        private void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                string version = br.Document.Body.InnerText;
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
            catch
            {
                Log.Write(l.Error, "Server down");
            }
        }

        #endregion

        private void SyncToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_busy)
                StartRemoteSync();
            else
                Log.Write(l.Debug, "How about you wait until the current synchronization finishes?");
        }

        #region Get the full remote file-folder listing

        private List<string> FullRemList;
        /// <summary>
        /// Fills FullRemList with a fully recursive list of the remote server (both files and folders)
        /// </summary>
        private List<string> FullRemoteList
        {
            get
            {
                FullRemList = new List<String>();
                if (FTP())
                {
                    foreach (FtpItem f in ftpc.GetDirListDeep("."))
                        if (ItemGetsSynced(GetComPath(f.FullPath, false)))
                        {
                            FullRemList.Add(GetComPath(f.FullPath, false));
                        }
                }
                else
                {
                    foreach (SftpFile s in sftpc.ListDirectory("."))
                    {                 
                        if (ItemGetsSynced(GetComPath(s.FullName, false)))
                            if (!s.IsDirectory)
                                FullRemList.Add(GetComPath(s.FullName, false));
                            else
                                FullRemoteRecursiveList(GetComPath(s.FullName, false));
                    }
                }

                return FullRemList;
            }
        }

        /// <summary>
        /// Gets the list of files & folders inside the specified folder
        /// </summary>
        /// <param name="path">the folder to look into</param>
        private void FullRemoteRecursiveList(string path)
        {
            FullRemList.Add(GetComPath(path, false));
            Log.Write(l.Debug, "Listing inside: {0}", path);
            foreach (SftpFile f in sftpc.ListDirectory(path))
            {
                if (ItemGetsSynced(GetComPath(f.FullName, false)))
                    if (!f.IsDirectory)
                        FullRemList.Add(GetComPath(f.FullName, false));
                    else
                        FullRemoteRecursiveList(GetComPath(f.FullName, false));
            }
        }

        #endregion

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
            string rpath = rPath();
            if (rpath != "/")
                rpath = noSlashes(rpath) + "/webint";
            else
                rpath = "webint";

            Log.Write(l.Info, "Searching webint folder in path: {0} ({1}) : {2}", rPath(), rpath, changedfromcheck.ToString());

            Log.Write(l.Info, "Webint folder exists: {0}", _exists(rpath));

            if (_exists(rpath))
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
                return languages.Get(lang() + "/web_interface/downloading", "The Web Interface will be downloaded.")
                    + Environment.NewLine + languages.Get(lang() + "/web_interface/in_a_minute", "This will take a minute.");
            else if (not == "removing")
                return languages.Get(lang() + "/web_interface/removing", "Removing the Web Interface...");
            else if (not == "updated")
                return languages.Get(lang() + "/web_interface/updated", "Web Interface has been updated.")
                    + Environment.NewLine + languages.Get(lang() + "/web_interface/setup", "Click here to view and set it up!");
            else if (not == "updating")
                return languages.Get(lang() + "/web_interface/updating", "Updating the web interface...");
            else // if (not == "removed")
                return languages.Get(lang() + "/web_interface/removed", "Web interface has been removed.");
        }

        public void AddRemoveWebInt()
        {
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
                    Log.Write(l.Error, "Could not download web interface with error: {0}", ex.Message);
                }
            }
            else
            {
                DeleteWebInt(false);
                //ListAllFiles();

                link = "";
                if (ShowNots())
                    tray.ShowBalloonTip(50, "FTPbox", get_webint_message("removed"), ToolTipIcon.Info);
            }

            addremovewebintpending = false;
        }

        public void GetWebInt()
        {
            CheckForFiles();
            link = null;
            if (ShowNots())
                tray.ShowBalloonTip(100, "FTPbox", get_webint_message("downloading"), ToolTipIcon.Info);

            string dllink = "http://ftpbox.org/webint.zip";
            string path = noSlashes(lPath());
            //DeleteWebInt();
            WebClient wc = new WebClient();
            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(WebIntDownloaded);
            wc.DownloadFileAsync(new Uri(dllink), Application.StartupPath + @"\webint.zip");
        }

        private void WebIntDownloaded(object sender, AsyncCompletedEventArgs e)
        {
            Log.Write(l.Debug, "path: {0} | lpath: {1}", Application.StartupPath, Application.StartupPath + @"\WebInterface");

            string data = @"|\webint\layout\css|\webint\layout\images\fancybox|\webint\layout\templates|\webint\system\classes|\webint\system\config|\webint\system\js|\webint\system\logs|\webint\system\savant\Savant3\resources|";
            MakeWebIntFolders(data);

            unZip(Application.StartupPath + @"\webint.zip", Application.StartupPath + @"\WebInterface");
            Log.Write(l.Info, "unzipped");
            updatewebintpending = true;
            try
            {
                UploadWebInt();
            }
            catch (Exception ex)
            {
                Log.Write(l.Error, "Error: {0}", ex.Message);
                if (!FTP())
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
                if (ShowNots())
                    tray.ShowBalloonTip(100, "FTPbox", get_webint_message("updating"), ToolTipIcon.Info);
            }
            else
            {
                if (ShowNots())
                    tray.ShowBalloonTip(100, "FTPbox", get_webint_message("removing"), ToolTipIcon.Info);
            }

            if (FTP())
            {
                DeleteFolderFTP("webint");
            }
            else
            {
                try
                {
                    DeleteFolderSFTP("webint");
                }
                catch (Exception e) { Log.Write(l.Error, "Error:: {0}", e.Message); }
            }
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
            string lpath = Application.StartupPath;
            if (FTP())
            {
                string rpath = rPath();
                if (rpath == "/")
                    rpath = "/webint/version.ini";
                else
                    rpath = noSlashes(rpath) + "/webint/version.ini";
                lpath = noSlashes(lpath) + @"\version.ini";
                Log.Write(l.Debug, "rpath {0} lpath {1}", rpath, lpath);
                ftpc.GetFile(rpath, lpath, FileAction.Create);
                Log.Write(l.Debug, "7");
            }
            else
            {
                using (FileStream f = new FileStream(lpath, FileMode.Create, FileAccess.ReadWrite))
                    sftpc.DownloadFile("webint/version.ini", f);

                lpath = noSlashes(lpath) + @"\version.ini";
            }

            string inipath = lpath;
            IniFile ini = new IniFile(inipath);
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
                    string path = Application.StartupPath + @"\WebInterface" + s;
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
            string path = Application.StartupPath + @"\WebInterface";

            foreach (string d in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
            {
                Log.Write(l.Debug, "dir: {0}", d);
                string fname = d.Substring(path.Length, d.Length - path.Length);
                fname = noSlashes(fname);
                fname = fname.Replace(@"\", @"/");
                Log.Write(l.Debug, "fname: {0}", fname);

                if (FTP())
                {
                    ftpc.MakeDirectory(fname);
                }
                else
                {
                    try
                    {
                        sftpc.CreateDirectory(fname);
                    }
                    catch { }
                }
            }

            foreach (string f in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                Log.Write(l.Debug, "file: {0}", f);

                FileAttributes attr = File.GetAttributes(f);
                if ((attr & FileAttributes.Directory) != FileAttributes.Directory)
                {
                    FileInfo fi = new FileInfo(f);
                    string fname = f.Substring(path.Length, f.Length - path.Length);
                    fname = noSlashes(fname);
                    fname = fname.Replace(@"\", @"/");
                    string cpath = fname.Substring(0, fname.Length - fi.Name.Length);

                    if (cpath.EndsWith("/"))
                        cpath = cpath.Substring(0, cpath.Length - 1);

                    Log.Write(l.Debug, "fname: {0} | cpath: {1}", fname, cpath);

                    if (FTP())
                    {
                        Log.Write(l.Debug, "f: {0}", f);
                        ftpc.PutFile(f, fname, FileAction.Create);
                    }
                    else
                    {
                        using (var file = File.OpenRead(f))
                            sftpc.UploadFile(file, fname, true);
                    }
                }
            }

            link = ftpParent();
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

            if (ShowNots())
                tray.ShowBalloonTip(50, "FTPbox", get_webint_message("updated"), ToolTipIcon.Info);

            Directory.Delete(Application.StartupPath + @"\WebInterface", true);
            File.Delete(Application.StartupPath + @"\webint.zip");
            try
            {
                Directory.Delete(Application.StartupPath + @"\webint", true);
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
            string p = Application.StartupPath;
            if (File.Exists(p + @"\webint.zip"))
                File.Delete(p + @"\webint.zip");
            if (Directory.Exists(p + @"\webint"))
                Directory.Delete(p + @"\webint", true);
            if (Directory.Exists(p + @"\WebInterface"))
                Directory.Delete(p + @"\WebInterface", true);
            if (File.Exists(p + @"\version.ini"))
                File.Delete(p + @"\version.ini");
        }        

        private void chkWebInt_CheckedChanged(object sender, EventArgs e)
        {
            if (!changedfromcheck)
            {
                addremovewebintpending = true;

                while (_busy)
                {
                    Thread.Sleep(50);
                }
                wiThread = new Thread(AddRemoveWebInt);
                wiThread.Start();
            }
                                    
            changedfromcheck = false;
        }

        private void labViewInBrowser_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string thelink = ftpParent();
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
            catch { }
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

        /// <summary>
        /// returns true if FTPbox is set to start on windows startup
        /// </summary>
        /// <returns></returns>
        private bool CheckStartup()
        {
            string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            Microsoft.Win32.RegistryKey startupKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(runKey);

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
            return UpLimit() > 0;
        }

        private bool LimitDownSpeed()
        {
            return DownLimit() > 0;
        }

        private void nDownLimit_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                AppSettings.Put("Settings/DownLimit", Convert.ToInt32(nDownLimit.Value));
                ftpc.MaxDownloadSpeed = Convert.ToInt32(nDownLimit.Value);
            }
            catch { }
        }

        private void nUpLimit_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                AppSettings.Put("Settings/UpLimit", Convert.ToInt32(nUpLimit.Value));
                ftpc.MaxUploadSpeed = Convert.ToInt32(nUpLimit.Value);
            }
            catch { }
        }
        #endregion

        #region Sync Frequency

        private void cManually_CheckedChanged(object sender, EventArgs e)
        {
            SyncToolStripMenuItem.Enabled = cManually.Checked;
            Profile.SyncingMethod = (cManually.Checked) ? SyncMethod.Manual : SyncMethod.Automatic;            
            AppSettings.Put("Settings/SyncMethod", Profile.SyncingMethod.ToString());

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
            SyncToolStripMenuItem.Enabled = !cAuto.Checked;
            Profile.SyncingMethod = (!cAuto.Checked) ? SyncMethod.Manual : SyncMethod.Automatic;
            AppSettings.Put("Settings/SyncMethod", Profile.SyncingMethod.ToString());

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
            AppSettings.Put("Settings/SyncFrequency", Profile.SyncFrequency);
        }

        #endregion        

        /// <summary>
        /// Kills the current process. Called from the tray menu.
        /// </summary>
        public void KillTheProcess()
        {
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
            AppSettings.Put("Settings/ShowNots", chkShowNots.Checked.ToString());
        }

        /// <summary>
        /// Clears the account info from the settings file and restarts the application
        /// </summary>
        private void ChangeAccount()
        {
            ClearAccount();
            ClearPaths();
            ClearLog();

            /*
            fswFiles.EnableRaisingEvents = false;
            fswFolders.EnableRaisingEvents = false;
            
            SetTray(MessageType.Ready);

            if (rcThread.IsAlive)
                rcThread.Abort();
            if (wiThread.IsAlive)
                wiThread.Abort();

            Profile.Clear();

            //do the start-up tests
            LoadLocalFolders();
            StartUpWork(); 

            //Application.Restart();*/

            Process.Start(Application.ExecutablePath);

            KillTheProcess();
        }

        private void bTranslate_Click(object sender, EventArgs e)
        {
            ftranslate.ShowDialog();
        }

        /// <summary>
        /// Checks if a file is still being used (hasn't been completely transfered to the folder)
        /// </summary>
        /// <param name="path">The file to check</param>
        /// <returns><c>True</c> if the file is being used, <c>False</c> if now</returns>
        private bool FileIsUsed(string path)
        {
            FileStream stream = null;
            
            string name = null;

            try
            {
                FileInfo fi = new FileInfo(path);
                name = fi.Name;
                stream = fi.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);                
            }
            catch
            {
                if (name != null)
                    Log.Write(l.Debug, "File {0} is locked: True", name);
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            if (name != null)
                Log.Write(l.Debug, "File {0} is locked: False", name);
            return false;
        }
    }
}
