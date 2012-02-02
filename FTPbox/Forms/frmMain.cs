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
using Utilities.Encryption;
using ICSharpCode.SharpZipLib.Zip;
using Starksoft.Net.Ftp;

namespace FTPbox
{
    public partial class frmMain : Form
    {
        Dictionary<string, DateTime> FullList = new Dictionary<string, DateTime>();
        //List<string> Deleted_List = new List<string>();
        public bool FilesDeleted = false;
        //FTP connection
        //FtpConnection ftp;
        //FtpConnection ftpbg;
        
        FtpClient ftpc;
        FtpClient ftpcbg;

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

        public bool failedlisting = false;

        Settings AppSettings = new Settings();
        public Translations languages = new Translations();
        string DecryptionPassword = "removed";      //removed for security purposes
        string DecryptionSalt = "removed";          //removed for security purposes

        bool updatewebintpending = false;
        bool addremovewebintpending = false;
        bool containswebintfolder = false;
        bool changedfromcheck = true;

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
                Log.Write(l.Info, "In Log: {0}", s);
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

            //if (AppSettings.Get("Settings/Timedif", "") == "" || AppSettings.Get("Settings/Timedif", "") == null)
            //{
                GetServerTime();
            //}
            //else
            //{
            //    timedif = TimeSpan.Parse(AppSettings.Get("Settings/Timedif", ""));
            //}

            Syncing();

            //CheckLocal();

            //remove this after getting new library to work:
            //ListAllFiles();
            //remove this after getting new library to work:

            /* //Moved to GetServerTime's end
            if (chkWebInt.Checked)
                CheckForWebIntUpdate();
            else
                ListAllFiles();
            
            SetLocalWatcher();

            DoneSyncing();
             */
            if (chkWebInt.Checked)
                CheckForWebIntUpdate();
            else
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
                    ftpc = new FtpClient(ftpHost(), ftpPort());

                    if (FTPS())
                    {
                        if (FTPES())
                            ftpc.SecurityProtocol = FtpSecurityProtocol.Tls1OrSsl3Explicit;
                        else
                            ftpc.SecurityProtocol = FtpSecurityProtocol.Tls1OrSsl3Implicit;
                        ftpc.ServerResponse += new EventHandler<FtpResponseEventArgs>(ftp_gotresponsefromserver);
                        ftpc.ValidateServerCertificate += new EventHandler<ValidateServerCertificateEventArgs>(ftp_ValidateServerCertificate);
                    }
                    Log.Write(l.Info, "Connecting ftpc");
                    ftpc.Open(ftpUser(), ftpPass());
                    Log.Write(l.Info, "Connected ftpc: {0}", ftpc.IsConnected.ToString());
                    
                    //and the background client:

                    ftpcbg = new FtpClient(ftpHost(), ftpPort());

                    if (FTPS())
                    {
                        if (FTPES())
                            ftpcbg.SecurityProtocol = FtpSecurityProtocol.Tls1OrSsl3Explicit;
                        else
                            ftpcbg.SecurityProtocol = FtpSecurityProtocol.Tls1OrSsl3Implicit;
                        ftpcbg.ValidateServerCertificate += new EventHandler<ValidateServerCertificateEventArgs>(ftp_ValidateServerCertificate);
                    }

                    Log.Write(l.Info, "Connecting ftpcbg");
                    ftpcbg.Open(ftpUser(), ftpPass());
                    Log.Write(l.Info, "Connected ftpcbg: " + ftpcbg.IsConnected.ToString());

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
                    Log.Write(l.Info, "FTP");

                    try
                    {
                        ftpc.Close();
                        ftpcbg.Close();
                    }
                    catch (Exception ex)
                    {
                        //Log.Write(l.Error, "Couldnt close connection, msg: {0}", ex.Message);
                    }

                    ftpc = new FtpClient(ftpHost(), ftpPort());

                    if (FTPS())
                    {
                        if (FTPES())
                            ftpc.SecurityProtocol = FtpSecurityProtocol.Tls1OrSsl3Explicit;
                        else
                            ftpc.SecurityProtocol = FtpSecurityProtocol.Tls1OrSsl3Implicit;
                        ftpc.ValidateServerCertificate += new EventHandler<ValidateServerCertificateEventArgs>(ftp_ValidateServerCertificate);
                    }
                    Log.Write(l.Info, "opening ftpc...");
                    ftpc.Open(ftpUser(), ftpPass());

                    //and the background client: 

                    ftpcbg = new FtpClient(ftpHost(), ftpPort());

                    if (FTPS())                     
                    {
                        if (FTPES())
                            ftpcbg.SecurityProtocol = FtpSecurityProtocol.Tls1OrSsl3Explicit;
                        else
                            ftpcbg.SecurityProtocol = FtpSecurityProtocol.Tls1OrSsl3Implicit;
                        ftpcbg.ValidateServerCertificate += new EventHandler<ValidateServerCertificateEventArgs>(ftp_ValidateServerCertificate);
                    }
                    Log.Write(l.Info, "opening ftpcbg...");
                    ftpcbg.Open(ftpUser(), ftpPass());
                    Log.Write(l.Info, "Connected: " + ftpc.IsConnected.ToString());

                    this.ShowInTaskbar = false;
                    this.Hide();
                    this.ShowInTaskbar = true;
                    loggedIn = true;
                }
                else
                {
                    Log.Write(l.Info, "SFTP");
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
                string rpath = rPath();
                if (rpath.StartsWith(@"/") && rpath != @"/")
                    rpath = rpath.Substring(1);

                foreach (FtpItem f in ftpc.GetDirList())
                {
                    Log.Write(l.Debug, "Name {0} FullPath {1}", f.Name, f.FullPath);
                }

                if (rpath == "" || lPath() == "")
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
                else if ((rpath != "/" && !ftpc.Exists(rpath)) || !Directory.Exists(lPath()))
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
                else
                    gotpaths = true;
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
                        Log.Write(l.Debug, rpath + " | " + sftpc.pwd());
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
                        gotpaths = true;
                    }
                    catch (SftpException e)
                    {
                        Log.Write(l.Error, e.Message);
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
            
            if (gotpaths)
                WebIntExists();            

            /*
            if (!chkWebInt.Checked)
                changedfromcheck = false;*/

            if (FTP())
            {
                if (FTPS())
                    lMode.Text = "FTPS";
                else
                    lMode.Text = "FTP";
            }
            else
                lMode.Text = "SFTP";

            AppSettings.Put("Settings/Startup", CheckStartup().ToString());

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
            try
            {
                return AESEncryption.Decrypt(AppSettings.Get("Account/Host", ""), DecryptionPassword, DecryptionSalt, "SHA1", 2, "OFRna73m*aze01xY", 256);
            }
            catch
            {
                return "";
            }             
        }

        public string ftpUser()
        {
            string x = AppSettings.Get("Account/Username", "");
            try
            {
                return AESEncryption.Decrypt(x, DecryptionPassword, DecryptionSalt, "SHA1", 2, "OFRna73m*aze01xY", 256);
            }
            catch
            {
                return "";
            }     
        }

        public string ftpPass()
        {
            string x = AppSettings.Get("Account/Password", "");
            try
            {
                return AESEncryption.Decrypt(x, DecryptionPassword, DecryptionSalt, "SHA1", 2, "OFRna73m*aze01xY", 256);
            }
            catch
            {
                return "";
            } 
        }

        public int ftpPort()
        {
            int i = 21;
            if (!FTP())
                i = 22;

            return AppSettings.Get("Account/Port", i);
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

        public bool FTPS()
        {
            return bool.Parse(AppSettings.Get("Account/FTPS", "False"));
        }

        public bool FTPES()
        {
            return bool.Parse(AppSettings.Get("Account/FTPES", "True"));
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
                    Log.Write(l.Debug, "File {0} ;;; {1} ;;; {2} ;;; {3}", cPath, f.Directory.FullName, e.FullPath, name);
                }
                else
                {
                    DirectoryInfo d = new DirectoryInfo(e.FullPath);
                    cPath = GetCommonPath(d.Parent.FullName, false); //GetCommonPath(e.FullPath.Replace(e.Name, ""), false);
                    name = d.Name;
                    Log.Write(l.Debug, "Dir {0} ;;; {1} ;;; {2} ;;; {3}", cPath, d.FullName, e.FullPath, name);                    
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
                Log.Write(l.Debug, "cpath: {0} name: {1}", cPath, name);

            }

            string rFullpath = noSlashes(rPath()) + cPath;

            Log.Write(l.Debug, "====} cPath: {0} rFullPath: {1}", cPath, rFullpath);


            if (!OfflineMode && e.Name != LastChangedFileFromRem && e.Name != LastChangedFolderFromRem && e.Name != LastChangedFolderFromRem)
            {
                Log.Write(l.Debug, "Gonna change {0}", e.Name);
                if (e.ChangeType == WatcherChangeTypes.Created)
                {
                    Log.Write(l.Info, "ooooooooooooooooo Created");
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
                        if (e.Name != "webint")
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
                }
                else if (e.ChangeType == WatcherChangeTypes.Deleted)
                {
                    Log.Write(l.Info, "ooooooooooooooooo Deleted");
                    if (!downloading)
                    {
                        if (source == fswFiles)
                        {
                            Log.Write(l.Info, ">>>>>> Deleted file");
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
                            Log.Write(l.Info, ">>>>>> Deleted Folder");
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
                    Log.Write(l.Info, "ooooooooooooooooo Changed");
                    try
                    {
                        if (source == fswFiles)
                        {
                            try
                            {
                                if (FTP())
                                {
                                    //ChangeRemote(name, cPath, e.FullPath);
                                }
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
                        Log.Write(l.Error, ex.Message);
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
                        try
                        {
                            RenamedList.Add(e.OldName.Replace(@"\", @"/"), e.Name.Replace(@"\", @"/"));
                        }
                        catch { }
                        Log.Write(l.Debug, "Added to list: {0} - {1}", e.OldName.Replace(@"\", @"/"), e.Name.Replace(@"\", @"/"));
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
                        Log.Write(l.Debug, "Added to list: {0} - {1}", e.OldName.Replace(@"\", @"/"), e.Name.Replace(@"\", @"/"));
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
            Log.Write(l.Debug, "About to rename {0} to {1} in path: {2}", oldName, newName, rFullpath);

            string nameOld = oldName.Replace(@"\", @"/");
            string nameNew = newName.Replace(@"\", @"/");

            Log.Write(l.Info, "nameOld: {0} nameNew: {1}", nameOld, nameNew);

            bool exists = false;
            foreach (FtpItem f in ftpc.GetDirList())
                if (f.FullPath.Equals(nameOld) || f.FullPath.Equals("/" + nameOld)) exists = true;

            if (exists)
            {
                Log.Write(l.Debug, "{0} exists", oldName);

                Syncing();
                try
                {
                    ftpc.Rename(nameOld, nameNew);
                }
                catch (FtpException ex)
                {
                    Log.Write(l.Error, "Exception: {0}", ex.Message);
                    DoneSyncing();
                    if (!ftpc.IsConnected)
                    {
                        ftp_reconnect();
                        RenameRemote(oldName, newName, cPath, FullPath, isDir);
                    }
                }

                //Removing the path from the beginning of nameOld and nameNew, leaving just the filename
                if (nameOld.Contains(@"/"))
                    nameOld = nameOld.Substring(oldName.LastIndexOf(@"\"), oldName.Length - oldName.LastIndexOf(@"\"));
                if (nameNew.Contains(@"/"))
                    nameNew = nameNew.Substring(newName.LastIndexOf(@"\"), newName.Length - newName.LastIndexOf(@"\"));
                if (nameOld.StartsWith(@"/"))
                    nameOld = nameOld.Substring(1, nameOld.Length - 1);
                if (nameNew.StartsWith(@"/"))
                    nameNew = nameNew.Substring(1, nameNew.Length - 1);

                if (ShowNots())
                    tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("renamed", true), nameOld, nameNew), ToolTipIcon.Info);
                lasttip = string.Format("{0} was renamed to {1}.", nameOld, nameNew);
                Get_Link("", nameNew);

                string oldLogPath = (@"\" + noSlashes(oldName)).Replace(@"\", @"/");
                string newLogPath = (@"\" + noSlashes(newName)).Replace(@"\", @"/");
                RemoveFromLog(oldLogPath);
                UpdateTheLog(newLogPath, GetLWTof(rFullpath, newName));

                DoneSyncing();
            }
            else
            {
                Log.Write(l.Debug, "{0} doesn't exist in {1}", nameOld, ftpc.CurrentDirectory);
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
            try
            {
                Syncing();
                string rFullPath = noSlashes(rPath()) + cPath;
                string pathtofile = rFullPath + name;
                Log.Write(l.Debug, "&&&&&&&&&& {0} {1} {2} {3}", rFullPath, cPath, name, pathtofile);

                if (isDir)
                {
                    ftpc.MakeDirectory(pathtofile);
                    Log.Write(l.Debug, "????> Created Directory: {0} (remote)", pathtofile);

                    if (ShowNots() && lasttip != string.Format("Folder {0} was created.", name))
                        tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("created", false), name), ToolTipIcon.Info);
                    lasttip = string.Format("Folder {0} was created.", name);
                    link = null;
                }
                else
                {
                    FileInfo fi = new FileInfo(path);
                    ftpc.PutFile(path, pathtofile, getProperFileAction(name));

                    if (ShowNots() && lasttip != string.Format("File {0} was updated.", fi.Name))
                        tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("udpated", true), fi.Name), ToolTipIcon.Info);
                    lasttip = string.Format("File {0} was updated.", fi.Name);
                    Get_Link(cPath, fi.Name);

                    string comPath = noSlashes(cPath) + @"/" + fi.Name;
                    Log.Write(l.Debug, "~~~~~~~~~~~~~> comPath: {0}", comPath);
                    UpdateTheLog(comPath, GetLWTof(rFullPath, fi.Name));
                }

                DoneSyncing();
            }
            catch
            {
                DoneSyncing();
                if (!ftpc.IsConnected)
                {
                    ftp_reconnect();
                    CreateRemote(path, cPath, name, isDir);
                }
            }
        }

        /// <summary>
        /// Delete a folder or file on host. Called only if deleting from remote is allowed
        /// </summary>
        /// <param name="name">name of file/folder to delete</param>
        /// <param name="cPath">common path of local and remote</param>
        /// <param name="isDir">true in case of folder, false in case of file</param>
        private void DeleteRemote(string name, string cPath, bool isDir)
        {
            try
            {
                Syncing();
                string rFullPath = noSlashes(rPath()) + cPath;

                Log.Write(l.Debug, "===+> Gonna delete file, name: {0} cPath {1} rFullPath {2} isDir {3}", name, cPath, rFullPath, isDir.ToString());

                if (isDir)
                {
                    try
                    {
                        string dl_path = rFullPath + name + "/";
                        if (dl_path.StartsWith("/"))
                            dl_path = dl_path.Substring(1);
                        FilesDeleted = true;
                        //Deleted_List.Add(dl_path);
                        Log.Write(l.Info, "***** Added to Deleted_list: {0}", dl_path);
                    }
                    catch (Exception e)
                    {
                        Log.Write(l.Error, "Error!: {0}", e.Message);
                    }

                    DeleteFolderFTP(noSlashes(rFullPath) + "/" + name, true, ref ftpc);
                    
                    if (ShowNots() && lasttip != string.Format("Folder {0} was deleted.", name))
                        tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("deleted", false), name), ToolTipIcon.Info);
                    lasttip = string.Format("Folder {0} was deleted.", name);
                    link = null;
                }
                else
                {
                    string rpath = noSlashes(rFullPath) + "/" + name;

                    ftpc.DeleteFile(rpath);

                    if (ShowNots() && lasttip != string.Format("File {0} was deleted.", name))
                        tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("deleted", true), name), ToolTipIcon.Info);
                    lasttip = string.Format("File {0} was deleted.", name);
                    link = null;

                    string locPath = noSlashes(lPath()) + @"\" + noSlashes(cPath) + @"\" + name;

                    RemoveFromLog(noSlashes(cPath) + @"/" + name);

                }
                DoneSyncing();
            }
            catch
            {
                DoneSyncing();
                if (!ftpc.IsConnected)
                {
                    ftp_reconnect();
                    DeleteRemote(name, cPath, isDir);
                }
            }

            
        }

        /// <summary>
        /// change a file on remote server. Only files change.
        /// </summary>
        /// <param name="name">name of file</param>
        /// <param name="cPath">common path</param>
        private void ChangeRemote(string name, string cPath, string FullPath)
        {
            string rempath = cPath + name;
            Log.Write(l.Info, "{0} exists: {1} in {2}", rempath, ftpc.Exists(rempath), ftpc.CurrentDirectory);
            if (rempath.StartsWith(@"/"))
                rempath = rempath.Substring(1);

            bool exists = false;

            foreach (FtpItem f in ftpc.GetDirList())
                if (f.FullPath.Equals(rempath) || f.FullPath.Equals(cPath + name)) exists = true;
            Log.Write(l.Info, "{0} exists: {1} in {2}", rempath, exists, ftpc.CurrentDirectory);
            
            if (exists)
            {
                try
                {
                    Syncing();
                    string rFullPath = noSlashes(rPath()) + cPath;
                    //if (rFullPath.StartsWith(@"/"))
                    //    rFullPath = rFullPath.Substring(1);

                    Log.Write(l.Debug, "===+> Gonna change file, name: {0} cPath {1} FullPath {2} rFullPath {3}", name, cPath, FullPath, rFullPath);

                    ftpc.PutFile(FullPath, rFullPath + name, getProperFileAction(name));

                    if (ShowNots()) //&& lasttip != string.Format("File {0} was updated.", name))
                        tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("updated", true), name), ToolTipIcon.Info);
                    lasttip = string.Format("File {0} was updated.", name);
                    Get_Link(cPath, name);

                    string comPath = noSlashes(cPath) + @"/" + name;
                    Log.Write(l.Debug, "~~~~~~~~~~~~~> comPath: {0}", comPath);
                    UpdateTheLog(comPath, GetLWTof(rFullPath, name));

                    Get_Link(cPath, name);

                    DoneSyncing();
                }
                catch
                {
                    DoneSyncing();
                    if (!ftpc.IsConnected)
                    {
                        ftp_reconnect();
                        ChangeRemote(name, cPath, FullPath);
                    }
                }
                
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
            tray.Text = languages.Get(lang() + "/tray/syncing", "FTPbox - Syncing");
        }

        /// <summary>
        /// called when syncing ends
        /// </summary>
        private void DoneSyncing()
        {
            fswFiles.EnableRaisingEvents = true;
            fswFolders.EnableRaisingEvents = true;
            tray.Icon = FTPbox.Properties.Resources.AS;
            tray.Text = languages.Get(lang() + "/tray/synced", "FTPbox - All files synced");
            downloading = false;
            
        }

        private void bAddFTP_Click(object sender, EventArgs e)
        {
            AppSettings.Put("Account/Host", "");
            AppSettings.Put("Account/Username", "");
            AppSettings.Put("Account/Password", "");
            AppSettings.Put("Paths/rPath", "");
            AppSettings.Put("Paths/lPath", "");
            ClearLog();
            Application.Restart();
        }

        private void bChangeBox_Click(object sender, EventArgs e)
        {
            AppSettings.Put("Paths/rPath", "");
            AppSettings.Put("Paths/lPath", "");
            ClearLog();
            Application.Restart();
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
                tray.ShowBalloonTip(30, "FTPbox", languages.Get(lang() + "/tray/link_copied", "Link copied to clipboard"), ToolTipIcon.Info);               
                link = null;
            }
        }

        private void tParent_TextChanged(object sender, EventArgs e)
        {
            AppSettings.Put("Paths/Parent", tParent.Text);
            //FTPbox.Properties.Settings.Default.ftpParent = tParent.Text;
            //FTPbox.Properties.Settings.Default.Save();
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

            while (name.StartsWith(@"/"))
                name = name.Substring(1);

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
            Log.Write(l.Debug, "-----------------> link: {0}", link);
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
                    string rpath = rPath();
                    foreach (FtpItem dir in ftpc.GetDirList(rPath()))
                    {
                        if (dir.Name == "public_html")
                        {
                            if (rpath == "/")
                                rpath = "/public_html";
                            else
                                rpath = noSlashes(rpath) + "/public_html";
                        }
                    }                    
                    string fname;

                    if (rpath == "/")
                        fname = rpath + "tempfolder" + RandomString(4);
                    else
                        fname = rpath + "/tempfolder" + RandomString(4);
                    
                    ftpc.MakeDirectory(fname);

                    DateTime now = DateTime.UtcNow;
                    /*
                    DateTime rnow = GetLWTof(rpath, fname);
                    TimeSpan x = now - rnow;
                    timedif = x;
                    Log.Write(l.Debug, "time difference: {0}", timedif.ToString());
                    AppSettings.Put("Settings/Timedif", timedif.ToString());
                    */
                    foreach (FtpItem f in ftpc.GetDirList(rpath))
                    {
                        if (f.FullPath == fname)
                        {
                            DateTime rnow = f.Modified.ToUniversalTime();
                            TimeSpan x = now - rnow;
                            timedif = x;
                            Log.Write(l.Debug, "time difference: {0}", timedif.ToString());

                            AppSettings.Put("Settings/Timedif", timedif.ToString());
                            //FTPbox.Properties.Settings.Default.timedif = timedif.ToString();
                            //FTPbox.Properties.Settings.Default.Save();
                        }
                    }
                    Log.Write(l.Info, "Created");
                    try
                    {
                        ftpc.DeleteDirectory(fname);
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
                        Log.Write(l.Debug, "Remote time: {0} - Utc Time: {1}", rnow.ToString(), DateTime.UtcNow.ToString());
                    }
                    catch
                    {
                        Log.Write(l.Info, "gonna use utcnow");
                        rnow = DateTime.UtcNow;
                    }

                    int result = DateTime.Compare(now, rnow);

                    if (result > 0)
                    {
                        addorremove = true;
                        timedif = now - rnow;
                        Log.Write(l.Debug, "now > rnow");
                    }
                    else
                    {
                        addorremove = false;
                        timedif = now - rnow;
                        Log.Write(l.Debug, "rnow > now");
                    }

                    Log.Write(l.Debug, "Timedif.TotalSeconds: {0}", timedif.TotalSeconds);
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
                Log.Write(l.Error, ex.Message);
                MessageBox.Show("Error creating directory. Make sure your FTP account has permissions to create directories! " + 
                    "If your account has the permissions needed but this error keeps on showing, feel free to contact support@ftpbox.org!" + Environment.NewLine + "Error message: " + ex.Message, "Error", MessageBoxButtons.OK);
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
                FilesDeleted = false;
                //We are using forms, so let's use a background worker.
                //cause NoFate is gay and loves background workers :3
                lRemoteWrk = new BackgroundWorker();
                FullList = new Dictionary<string, DateTime>();
                lRemoteWrk.WorkerSupportsCancellation = true;
                lRemoteWrk.DoWork += new DoWorkEventHandler(lRemoteWrk_DoWork);
                lRemoteWrk.RunWorkerCompleted += new RunWorkerCompletedEventHandler(lRemoteWrk_RunWorkerCompleted);
                Log.Write(l.Info, "@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                listing = true;
                lRemoteWrk.RunWorkerAsync();
                //FullList = new Dictionary<string, DateTime>();
                //FullList = listRemote();

            }
            catch 
            {
                Log.Write(l.Error, "Could not list remote files");
            }
        }

        void lRemoteWrk_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {           
            if (!e.Cancelled)
            {
                listing = false;
                // Retrieve the dictionary from the result
                //Thread.Sleep(10000);
                //Thread.Sleep(5000);
                Dictionary<String, DateTime> fdDict = e.Result as Dictionary<String, DateTime>;

                FullList = new Dictionary<string, DateTime>();
                FullList = fdDict;

                if (FTP())
                {
                    CheckList();
                    //CheckLocal();
                }
                else
                {
                    //CheckPending();
                    CheckLocal();
                    if (failedlisting)
                    {
                        Log.Write(l.Warning, "!!error");
                        sftpc.quit();
                        sftp_login();
                        SftpCDtoRoot();
                        failedlisting = false;
                    }

                    if (updatewebintpending)
                        UploadWebInt();
                    else if (addremovewebintpending)
                        AddRemoveWebInt();
                    else
                        ListAllFiles();
                }
                
                //Log.Write("GONNA LIST LOCAL FILES NOW");
                //CheckLocal();
            }
            else
            {
                Log.Write(l.Warning, "!!! lRemWrk Cancelled !!!");
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
                        // Get the remote directories/files
                        try
                        {
                            Dictionary<String, DateTime> rtDict = listRemote();
                            e.Result = rtDict;
                        }
                        catch (SftpException ex)
                        {
                            if (!FTP())
                            {
                                Log.Write(l.Error, "[Error listing]: {0}", ex.Message);
                                sftpc.quit();
                                sftp_login();
                                SftpCDtoRoot();
                                lRemoteWrk.CancelAsync();
                            }
                        }
                        // And our work is complete!

                    }
                    catch (Exception ex) 
                    { 
                        Log.Write(l.Error, "ERROR!!! " + ex.Message);
                        ftp_reconnect();
                    }
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

            if (FTP())
            {

                foreach (FtpItem f in ftpcbg.GetDirListDeep(rPath()))
                {
                    if (f.ItemType == FtpItemType.File && f.Name != ".ftpquota")
                    {
                        if (!f.FullPath.Contains("webint"))
                        {
                            Log.Write(l.Info, "File: {0}", f.FullPath);
                            fdDict.Add(f.FullPath, GetLWTof(f.ParentPath, f.Name));//f.Modified.ToUniversalTime().Add(timedif));
                        }
                    }
                    else
                    {
                        if (!f.FullPath.Contains("webint") && f.Name != ".ftpquota")
                        {
                            Log.Write(l.Info, "Drct: {0}", f.FullPath);
                            fdDict.Add(f.FullPath + "/", f.Modified.ToUniversalTime().Add(timedif)); //GetLWTof(f.ParentPath, f.Name));
                        }
                    }                    
                }
                #region old_code
                /*
                // Loop through files
                foreach (FtpFileInfo fInfo in ftpbg.GetFiles())
                {
                    if (fInfo.Name != ".ftpquota")
                    {
                        // Got it? Add it!
                        fdDict.Add(noSlashes(String.Format("{0}/{1}", noSlashes(rPath()), fInfo.Name)), fInfo.LastWriteTimeUtc.Value.AddHours(timedif.Hours));
                        Log.Write("{0}/{1}", noSlashes(rPath()), fInfo.Name);
                        string cpath = string.Format("{0}/{1}", noSlashes(rPath()), fInfo.Name);
                        //CheckRemFtpFiles(cpath, fInfo.LastWriteTimeUtc.Value.AddHours(timedif.Hours));
                        //CheckRemoteFile(noSlashes(rPath()), fInfo.Name, fInfo.LastWriteTimeUtc.Value);
                    }
                }

                // And directories
                foreach (FtpDirectoryInfo dInfo in ftpbg.GetDirectories())
                {
                    // You can't trick me!
                    if (dInfo.Name != "." && dInfo.Name != ".." && dInfo.Name != "webint")
                    {
                        getSetDirFiles(dInfo.Name, dInfo.LastWriteTimeUtc.Value.AddHours(timedif.Hours), ref fdDict);
                    }
                }
                 */
                #endregion
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
                    Log.Write(l.Error, "error");
                    Log.Write(l.Error, ex.Message);
                    vv = null;
                    sftpc.quit();
                    sftp_login();
                    SftpCDtoRoot();
                    lRemoteWrk.CancelAsync();
                }
                foreach (ChannelSftp.LsEntry lse in vv)
                {
                    Log.Write(l.Info, "Found: {0}", lse.getFilename());
                    SftpATTRS attrs = lse.getAttrs();
                    if (lse.getFilename() != "webint")
                    {
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
                                Log.Write(l.Debug, "sftp {0}/{1}", noSlashes(rPath()), lse.getFilename());
                                //CheckRemoteFile(noSlashes(rPath()), fInfo.Name, fInfo.LastWriteTimeUtc.Value);
                                CheckRemSftpFiles(lse.getFilename(), "", attrs.getMtimeString().AddSeconds(timedif.TotalSeconds), false);
                            }

                        }
                    }
                }
            }
            return fdDict;
        }

        #region getSetDirFiles (FTP)
        /// <summary>
        /// A nice internal function that loops through the directories and files
        /// </summary>
        /// <param name="path">The path to look into</param>
        /// <param name="lastWriteTime">Last write time of the path</param>
        /// <param name="fdDict">The dictionary in which to put found directories/files</param>
        /*
        private void getSetDirFiles(string path, DateTime lastWriteTime, ref Dictionary<String, DateTime> fdDict)
        {
            // Clean up the path and add it to the dictionary
            String newPath = noSlashes(String.Format("{0}/{1}", noSlashes(rPath()), path)) + @"/";
            fdDict.Add(newPath, lastWriteTime);

            Log.Write("Drct ({0})", newPath);

            //CheckRemFtpFiles(newPath, lastWriteTime);

            if (ftpbg.DirectoryExists(newPath))
            {
                // Set our current directory
                ftpbg.SetCurrentDirectory(newPath);

                // Loop through files
                foreach (FtpFileInfo fInfo in ftpbg.GetFiles())
                {
                    if (fInfo.Name != ".ftpquota")
                    {
                        // Got it? Add it!
                        Log.Write("*FTP* File ({0})", noSlashes(String.Format("{0}{1}", newPath, fInfo.Name)));
                        fdDict.Add(noSlashes(String.Format("{0}{1}", newPath, fInfo.Name)), fInfo.LastWriteTimeUtc.Value.AddHours(timedif.Hours));
                        string cpath = String.Format("{0}{1}", newPath, fInfo.Name);
                        //CheckRemFtpFiles(cpath, fInfo.LastWriteTimeUtc.Value.AddHours(timedif.Hours));
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
        }
        */
        #endregion

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

                Log.Write(l.Debug, "*SFTP* Drct ({0}) in path: ({1}) LWT: {2}", newPath, path, lastWriteTime.ToString());

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
                        Log.Write(l.Debug, "*SFTP* File ({0})", noSlashes(String.Format("{0}{1}", newPath, lse.getFilename())));
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

                        string rpath = s.Key;
                        if (!rpath.StartsWith(rPath()))
                        {
                            if (rpath.StartsWith(@"/"))
                                rpath = noSlashes(rPath()) + rpath;
                            else
                                rpath = noSlashes(rPath()) + @"/" + rpath;
                        }
                        if (rpath.StartsWith("/"))
                            rpath = rpath.Substring(1);
                        rpath = noSlashes(rpath);

                        Log.Write(l.Debug, "remPath:: {0} rpath: {1}", remPath, rpath);

                        if (!Directory.Exists(path) && !FilesDeleted)
                        {
                            if ((FTP() && ftpcbg.Exists(rpath)) || !FTP())
                            {                                
                                Directory.CreateDirectory(path);
                                Log.Write(l.Debug, "????> Created Directory: {0} (local)", path);
                                DirectoryInfo d = new DirectoryInfo(path);
                                if (ShowNots() && lasttip != string.Format("Folder {0} was created.", d.Name))
                                    tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("created", false), d.Name), ToolTipIcon.Info);
                                lasttip = string.Format("Folder {0} was created.", d.Name);
                                link = null;
                            }
                            else
                                Log.Write(l.Debug, "{0} doesnt exist... won't create the local folder: {1}", rpath, ftpcbg.Exists(rpath).ToString());
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
                        Log.Write(l.Debug, "cPath {0} -> name {1} -> fullRP {2} -> Flp {3} -> comPath {4} -> LFDP {5}", cPath, name, FullRemPath, FullLocalPath, comPath, LocalFileDirParent);
                        Log.Write(l.Debug, FullRemPath + " ~ " + LocalFileDirParent);

                        string rfpath = noSlashes(FullRemPath) + "/" + name;
                                
                                if (rfpath.StartsWith("/"))
                                    rfpath = rfpath.Substring(1);

                        if (nLog() == null || nLog() == "" || !nLog().Contains(s.Key))
                        {
                            try
                            {
                                if (ftpcbg.Exists(rfpath))
                                {
                                    Syncing();
                                    Log.Write(l.Debug, "Log is null, gonna get {0}", name);


                                    LastChangedFileFromRem = name;
                                    LastChangedFolderFromRem = GetParentFolder(cPath);
                                    downloading = true;
                                    fswFiles.EnableRaisingEvents = false;

                                    ftpcbg.GetFile(rfpath, FullLocalPath, FileAction.Create);

                                    fswFiles.EnableRaisingEvents = true;

                                    if (ShowNots() && lasttip != string.Format("File {0} was updated.", name))
                                        tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("updated", true), name), ToolTipIcon.Info);
                                    lasttip = string.Format("File {0} was updated.", name);

                                    UpdateTheLog(s.Key, s.Value);
                                    Get_Link(comPath, name);
                                    downloading = false;
                                    DoneSyncing();
                                }
                            }
                            catch (Exception ex)
                            {
                                DoneSyncing();
                                Log.Write(l.Error, "[ERROR] -> " + ex.Message);
                            }
                        }
                        else
                        {
                            if (File.Exists(fLocalPath))
                            {
                                FileInfo f = new FileInfo(fLocalPath);
                                CheckExistingFile(name, s.Key, FullRemPath, FullLocalPath, LocalFileDirParent, f.LastWriteTime, s.Value, comPath);
                            }
                            else
                            {
                                try
                                {
                                    Syncing();
                                    Log.Write(l.Info, "Log not null but {0} doesnt exist!", s.Key);

                                    downloading = true;
                                    fswFiles.EnableRaisingEvents = false;

                                    ftpcbg.GetFile(rfpath, FullLocalPath, FileAction.Create);

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
                                    Log.Write(l.Error, "[ERROR] -> " + ex.Message);
                                }
                            }
                        }
                    }
                }
            }
        }

        #region CheckRemFtpFiles - will be added later
        /*
        public void CheckRemFtpFiles(string cpath, DateTime lwt)
        {
            DateTime rDT = lwt;

            if (cpath.EndsWith(@"/"))
            {
                //means it's a folder
                string path = lPath();
                string remPath = cpath;
                if (remPath.StartsWith(rPath()))
                {
                    remPath = remPath.Substring(rPath().Length);
                }
                path = path + @"\" + noSlashes(remPath.Replace(@"/", @"\"));

                if (!Directory.Exists(path))
                {
                    if (DeletedList.Contains(cpath))
                    {
                        DeleteFolderFTP(cpath, true, ref ftpbg);
                        DeletedList.Remove(cpath);

                        string name = noSlashes(cpath).Substring(noSlashes(cpath).LastIndexOf("/") + 1);

                        if (ShowNots() && lasttip != string.Format("Folder {0} was deleted.", name))
                            tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("deleted", false), name), ToolTipIcon.Info);
                        lasttip = string.Format("Folder {0} was deleted.", name);
                        link = null;
                    }
                    else
                    {
                        List<string> Namelog = new List<string>(nLog().Split('|', '|'));
                        if (!RenamedList.ContainsKey(cpath))
                        {
                            Directory.CreateDirectory(path);
                            Log.Write("????> Created Directory: {0} (local)", path);
                            DirectoryInfo d = new DirectoryInfo(path);
                            if (ShowNots() && lasttip != string.Format("Folder {0} was created.", d.Name))
                                tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("created", false), d.Name), ToolTipIcon.Info);
                            lasttip = string.Format("Folder {0} was created.", d.Name);
                            link = null;
                        }
                        else
                        {
                            Log.Write("namelog doesnt contain: {0}", cpath);
                            //RenamedList.Remove(cpath);
                        }
                    }
                }
            }
            else
            {
                //means it's a file
                int i = cpath.LastIndexOf(@"/");
                string cPath = cpath.Substring(0, i);
                string name = cpath.Substring(i + 1, cpath.Length - i - 1);
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
                    FullLocalPath = noSlashes(FullLocalPath) + @"\" + name;
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

                if (nLog() == null || nLog() == "" || !nLog().Contains(cpath))
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

                        UpdateTheLog(cpath, lwt);
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
                        CheckExistingFile(name, cpath, FullRemPath, FullLocalPath, LocalFileDirParent, f.LastWriteTimeUtc, lwt, comPath);
                    }
                    else
                    {
                        try
                        {
                            if (DeletedList.Contains(cpath))
                            {
                                ftpbg.SetCurrentDirectory(FullRemPath);
                                ftpbg.RemoveFile(name);
                                DeletedList.Remove(cpath);

                                if (ShowNots() && lasttip != string.Format("File {0} was deleted.", name))
                                    tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("deleted", true), name), ToolTipIcon.Info);
                                lasttip = string.Format("File {0} was deleted.", name);
                                link = null;

                                string logpath = "/" + cPath.Substring(rPath().Length);
                                RemoveFromLog(logpath);
                            }
                            else
                            {
                                Syncing();
                                Log.Write("Log not null but {0} doesnt exist!", cpath);
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
                                UpdateTheLog(cpath, lwt);
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
        */
        #endregion

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

                Log.Write(l.Debug, "*CHECKING*: comPath: {0}, name: {1}, fRemPath: {2} LWT: {3}", comPath, name, fRemPath, lastwritetime.ToString());

                if (isDir)
                {
                    //means it's a folder
                    string path = lPath();
                    string remPath = fRemPath;
                    if (remPath.StartsWith(rPath()))
                    {
                        remPath = remPath.Substring(rPath().Length, remPath.Length - rPath().Length);
                    }

                    Log.Write(l.Debug, "path: {0} | remPath: {1}", path, remPath);
                    path = path + @"\" + name.Replace(@"/", @"\"); //noSlashes(remPath.Replace(@"/", @"\"));

                    Log.Write(l.Debug, "path: {0}", path);

                    if (!Directory.Exists(path))
                    {
                        if (Namelog.Contains(comPath))
                        {
                            int i = name.LastIndexOf("/");
                            string cPath = comPath.Substring(0, i + 1);
                            string thename = name.Substring(i + 1, name.Length - i - 1);
                            Log.Write(l.Debug, "*SFTP* Gonna delete remote file, name: {0} cPath: {1}", thename, cPath);
                            SftpDelete(thename, cPath, true);
                            RemoveFromLog(comPath);
                        }
                        else
                        {
                            fswFolders.EnableRaisingEvents = false;
                            Directory.CreateDirectory(path);
                            Log.Write(l.Debug, "????> Created Directory: {0} (local)", path);
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
                            Log.Write(l.Debug, "(SFTP) Log is null, gonna get {0}", name);
                            Log.Write(l.Debug, "(SFTP) LocalFileDirParent: {0}", LocalFileDirParent);
                            sftpc.lcd(LocalFileDirParent);
                            Log.Write(l.Debug, "LFDP: {0} check", LocalFileDirParent);
                            LastChangedFileFromRem = name;
                            LastChangedFolderFromRem = ""; // GetParentFolder(fRemPath);
                            downloading = true;
                            fswFiles.EnableRaisingEvents = false;

                            Log.Write(l.Debug, "*SFTP* Downloading...");
                            SftpDownloadFile(comPath);
                            Log.Write(l.Debug, "*SFTP* Downloaded...");

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
                            Log.Write(l.Error, "[ERROR] -> " + ex.Message);
                        }
                    }
                    else
                    {
                        if (File.Exists(lFullPath))
                        {
                            FileInfo f = new FileInfo(lFullPath);
                            Log.Write(l.Info, "Shit exists: {0}", f.FullName);
                            CheckExistingFile(name, comPath, fRemPath, lFullPath, LocalFileDirParent, f.LastWriteTimeUtc, lastwritetime, comPath);
                        }
                        else
                        {
                            if (Namelog.Contains(comPath))
                            {
                                string cPath = comPath.Substring(0, comPath.LastIndexOf(name));
                                Log.Write(l.Debug, "*SFTP* Gonna delete remote file, name: {0} cPath: {1}", name, cPath);
                               SftpDelete(name, cPath, false);
                             
                            }
                            else
                            {
                                try
                                {
                                    Syncing();
                                    Log.Write(l.Debug, "Log not null but {0} doesnt exist!", comPath);
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
                                    Log.Write(l.Error, "[ERROR] -> " + ex.Message);
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
            
            string rfpath = noSlashes(FullRemPath) + "/" + name;

            TimeSpan dif = rDT - rDTlog;
            if (rResult > 0 && dif.TotalSeconds > 1)
            {                
                if (dif.TotalSeconds > 1)
                {
                    Log.Write(l.Info, "Total Milliseconds of difference: {0} Seconds: {1}", dif.TotalMilliseconds.ToString(), dif.TotalSeconds.ToString());
                    try
                    {
                        Syncing();
                        Log.Write(l.Debug, "rResult > 0");
                        Log.Write(l.Debug, "fRP: {0} -- lfPF: {1} lDT: {2} -- lDTlog: {3} -- rDT: {4} -- rDTlog {5}", FullRemPath, LocalFileParentFolder, lDT.ToString(),
                            lDTlog.ToString(), rDT.ToString(), rDTlog.ToString());
                        if (FTP())
                        {                           
                            downloading = true;
                            fswFiles.EnableRaisingEvents = false;
                            Log.Write(l.Debug, "~Connected~ {0} ~busy~ {1}", ftpcbg.IsConnected.ToString(), ftpcbg.IsBusy.ToString());
                            ftpcbg.GetFile(rfpath, FullLocalPath, FileAction.Create);
                            Log.Write(l.Debug, "~~~~~~~~~~~");
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
                            if (FTP())
                                Get_Link(comPath, name);
                            else
                                Get_Link("", comPath);
                            UpdateTheLog(cPath, rDT);
                            downloading = false;
                            DoneSyncing();
                        }
                   }
                   catch (Exception ex)
                   {
                       Log.Write(l.Error, "[ERROR] -> " + ex.Message);
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
                        Log.Write(l.Debug, "(lDT.ToString() != lDTlog.ToString()) && bResult < 0");
                        Log.Write(l.Debug, "lDT: {0} -- lDTlog: {1} -- rDT: {2} -- rDTlog: {3}", lDT.ToString(), lDTlog.ToString(), rDT.ToString(), rDTlog.ToString());
                        if (FTP())
                        {
                            ftpcbg.PutFile(FullLocalPath, rfpath, getProperFileAction(name));
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
                            Get_Link("", cPath);

                            UpdateTheLog(cPath, GetLWTof(FullRemPath, name));

                            DoneSyncing();
                        }

                    }
                    catch (Exception ex)
                    {
                        Log.Write(l.Error, "[ERROR] -> " + ex.Message);
                        DoneSyncing();
                    }
                }
                else if ((lDT.ToString() != lDTlog.ToString()) && bResult > 0)
                {
                    try
                    {
                        Syncing();
                        Log.Write(l.Debug, "(lDT.ToString() != lDTlog.ToString()) && bResult > 0");
                        Log.Write(l.Debug, "lDT: {0} -- lDTlog: {1} -- rDT: {2} -- rDTlog: {3}", lDT.ToString(), lDTlog.ToString(), rDT.ToString(), rDTlog.ToString());
                        Log.Write(l.Debug, "Dif: h: {0} m: {1} sec: {2} ms {3}", dif.Hours.ToString(), dif.Minutes.ToString(), dif.Seconds.ToString(), dif.Milliseconds.ToString());

                        if (FTP())
                        {
                            downloading = true;
                            fswFiles.EnableRaisingEvents = false;
                            ftpcbg.GetFile(rfpath, FullLocalPath, FileAction.Create);
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
                            //problem seems to be here:
                            if (FTP())
                                UpdateTheLog(cPath, GetLWTof(rfpath, name));
                            else
                            UpdateTheLog(cPath, SftpGetLastWriteTime(cPath));// DateTime.UtcNow);
                            downloading = false;
                            DoneSyncing();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write(l.Error, "[ERROR] -> " + ex.Message);
                        DoneSyncing();
                    }
                }
                else
                {
                    Log.Write(l.Info, "***********");
                    Log.Write(l.Debug, cPath);
                    Log.Write(l.Debug, "rDT {0} rDTlog {1} result {2}", rDT.ToString(), rDTlog.ToString(), rResult.ToString());
                    Log.Write(l.Debug, "lDT {0} lDTlog {1} result {2}", lDT.ToString(), lDTlog.ToString(), lResult.ToString());
                    Log.Write(l.Debug, bResult.ToString());
                    Log.Write(l.Info, "***********");
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
                if (FTP())
                    lDTlog = dInfo.LastWriteTime;
                else
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
               if (FTP())
                   lDTlog = fInfo.LastWriteTime;
               else
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

            Log.Write(l.Info, "##########");
            Log.Write(l.Debug, "FLP {0} + name: {1} + rDTlog: {2} + lDTlog: {3} + cPath: {4}", FullLocalPath, name, rDTlog.ToString(), lDTlog.ToString(), cPath);
            Log.Write(l.Info, "#########");         
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
            if (remoteDL.Count >= i && i >= 0)
                DateTime.TryParse(remoteDL[i], out dt);
            else
            {
                dt = DateTime.MinValue;
                Log.Write(l.Warning, "Problem with indexes... (remote)");
            }
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
            if (localDL.Count >= i && i>=0)
                DateTime.TryParse(localDL[i], out dt);
            else
            {
                Log.Write(l.Warning, "Problem with indexes... (local)");
                dt = DateTime.MinValue;   
            }

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
            Log.Write(l.Info, "Getting LWT of {0} in {1}", name, FullRemPath);
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
                Log.Write(l.Error, "========> it's a folder, err: {0}", ex.Message);
            }
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
            lasttip = null;
            if (chkWebInt.Checked)
            {
                labViewInBrowser.Enabled = true;
            }
            else
            {
                labViewInBrowser.Enabled = false;
            } 
            
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
                        ftpc.Close();
                        fswFiles.Dispose();
                        fswFolders.Dispose();
                    }
                    OfflineMode = true;
                    tray.Icon = FTPbox.Properties.Resources.offline1;
                    tray.Text = languages.Get(lang() + "/tray/offline", "FTPbox - Offline");        
                }
            }
            catch { }
        }

        public void CheckLocal()
        {
            Log.Write(l.Info, "$$$$$$$$$$$$ Checkin Local $$$$$$$$$$$$");
            try
            {
                List<string> alllocal = new List<string>(Directory.GetDirectories(lPath(), "*", SearchOption.AllDirectories));
                alllocal.AddRange(Directory.GetFiles(lPath(), "*", SearchOption.AllDirectories));
                
                List<string> Namelog = new List<string>(nLog().Split('|', '|'));
                
                foreach (string s in alllocal)
                {
                    Log.Write(l.Debug, @"Checking local: {0}", s);
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
                                Log.Write(l.Debug, "++++++++> {0} {1} {2} in {3}", compath, path, name, s);
                                if (FTP())
                                {
                                    if (!FullList.ContainsKey(compath))
                                    {
                                        if (!RenamedList.ContainsValue(compath))
                                        {
                                            Log.Write(l.Debug, "Case 1");
                                            Log.Write(l.Debug, "FTP > sorry, gotta delete {0}", compath);
                                            if (isDir)
                                            {
                                                Directory.Delete(s, true);

                                                if (ShowNots())
                                                    tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("deleted", false), name), ToolTipIcon.Info);
                                                lasttip = string.Format("Folder {0} was deleted.", name);
                                                link = null;
                                                RemoveFromLog(cPath);
                                            }
                                            else
                                            {
                                                File.Delete(s);

                                                if (ShowNots())
                                                    tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("deleted", true), name), ToolTipIcon.Info);
                                                lasttip = string.Format("File {0} was deleted.", name);
                                                link = null;
                                                RemoveFromLog(cPath);
                                            }
                                        }
                                        else
                                        {
                                            Log.Write(l.Debug, "renamedlist contains value: {0}", compath);
                                            foreach (KeyValuePair<string, string> k in RenamedList)
                                            {
                                                if (k.Value == compath)
                                                    RenamedList.Remove(k.Key);
                                            }
                                        }
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

                                    Log.Write(l.Debug, "rpath: {0}", rpath);
                                    if (!FullList.ContainsKey(rpath))
                                    {
                                        Log.Write(l.Debug, "Case 1");
                                        Log.Write(l.Debug, "SFTP > sorry, gotta delete {0}", path + name);
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
                                Log.Write(l.Debug, "--------> {0} {1} {2} in {3}", cPath, path, name, s);
                                if (FTP())
                                {
                                    /*
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
                                    */
                                }
                                else
                                {
                                    if (isDir)
                                    {
                                        Log.Write(l.Debug, "comPath: {0}", compath);
                                        if (!FullList.ContainsKey(compath))
                                        {
                                            Log.Write(l.Debug, "Gonna make folder {0} to remote server in pwd: {1}", cPath, sftpc.pwd());
                                            sftpc.mkdir(cPath);
                                            Log.Write(l.Info, "success");
                                            if (ShowNots() && lasttip != string.Format("Folder {0} was created.", name))
                                                tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("created", false), name), ToolTipIcon.Info);
                                            lasttip = string.Format("Folder {0} was created.", name);
                                        }
                                    }
                                    else
                                    {
                                        Log.Write(l.Debug, "Case 3");
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
                        Log.Write(l.Info, "recentlycreated is true");
                }
            }
            catch (Exception ex) 
            { 
                Log.Write(l.Error, "Error::: " + ex.Message);
                failedlisting = true;
            }
            recentlycreated = false;
            Log.Write(l.Info, "$$$$$$$$$$$$ Done Checkin Local $$$$$$$$$$$$");
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
            Thread.Sleep(3000);
            FullList = new Dictionary<string, DateTime>();

            if (updatewebintpending)
                UploadWebInt();
            else if (addremovewebintpending)
                AddRemoveWebInt();
            else
                ListAllFiles();
        }

        void MakeChangesBGW_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!OfflineMode)
            {
                if (FTP())
                    CheckRemoteFiles(FullList);
                Log.Write(l.Info, "@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");                
            }
        }

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
                else
                    locallang = "English";

                if (locallangtwoletter != "en" && (locallangtwoletter == "es" || locallangtwoletter == "de" || locallangtwoletter == "fr" || locallangtwoletter == "nl" || locallangtwoletter == "el" || locallangtwoletter == "it" || locallangtwoletter == "tr" || locallangtwoletter == "pt-BR"))
                {
                    string msg = string.Format("FTPbox detected that you use {0} as your computer language. Do you want to use {1} as the language of FTPbox as well?", locallang, locallang);
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

        private void Set_Language(string lan)
        {
            Log.Write(l.Debug, "Changing language to: {0}", lan);
            this.Text = "FTPbox | " + languages.Get(lan + "/main_form/options", "Options");
            //general tab
            tabGeneral.Text = languages.Get(lan + "/main_form/general", "General");
            gAccount.Text = "FTP " + languages.Get(lan + "/main_form/account", "Account");
            labHost.Text = languages.Get(lan + "/main_form/host", "Host") + ":";
            labUN.Text = languages.Get(lan + "/main_form/username", "Username") + ":";
            labPort.Text = languages.Get(lan + "/main_form/port", "Port") + ":";
            labMode.Text = languages.Get(lan + "/main_form/mode", "Mode") + ":";
            bAddFTP.Text = languages.Get(lan + "/main_form/change", "Change");
            gApp.Text = languages.Get(lan + "/main_form/application", "Application");
            gWebInt.Text = languages.Get(lan + "/web_interface/web_int", "Web Interface");
            chkWebInt.Text = languages.Get(lan + "/web_interface/use_webint", "Use the Web Interface");
            labViewInBrowser.Text = languages.Get(lan + "/web_interface/view", "(View in browser)");
            chkShowNots.Text = languages.Get(lan + "/main_form/show_nots", "Show notifications");
            chkStartUp.Text = languages.Get(lan + "/main_form/start_on_startup", "Start on system start-up");
            labLang.Text = languages.Get(lan + "/main_form/language", "Language") + ":";
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
            exitToolStripMenuItem.Text = languages.Get(lan + "/tray/exit", "Exit");

            if (lan == "en")
                cmbLang.SelectedIndex = 0;
            else if (lan == "es")
                cmbLang.SelectedIndex = 1;
            else if (lan == "de")
                cmbLang.SelectedIndex = 2;
            else if (lan == "fr")
                cmbLang.SelectedIndex = 3;
            else if (lan == "nl")
                cmbLang.SelectedIndex = 4;
            else if (lan == "el")
                cmbLang.SelectedIndex = 5;
            else if (lan == "it")
                cmbLang.SelectedIndex = 6;
            else if (lan == "tr")
                cmbLang.SelectedIndex = 7;
            else if (lan == "pt-BR")
                cmbLang.SelectedIndex = 8;

            AppSettings.Put("Settings/Language", lan);
        }

        private void cmbLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            string lan = cmbLang.Text.Substring(cmbLang.Text.Length - 3, 2);
            try
            {
                Set_Language(lan);
            }
            catch (Exception ex)
            {
                Log.Write(l.Error, "[Error] -> {0} -> {1}", ex.Message);
            }
        }

        public string Get_Message(string not, bool file)
        {
            string fileorfolder;
            if (file)
                fileorfolder = languages.Get(lang() + "/tray/file", "File");
            else
                fileorfolder = languages.Get(lang() + "/tray/folder", "Folder");

            if (not == "created")
            {
                return fileorfolder + " " + languages.Get(lang() + "/tray/created", "{0} was created.");
            }
            else if (not == "deleted")
            {
                return fileorfolder + " " + languages.Get(lang() + "/tray/deleted", "{0} was deleted.");
            }
            else if (not == "renamed")
            {
                return languages.Get(lang() + "/tray/renamed", "{0} was reamed to {1}");
            }
            else if (not == "changed")
            {
                return fileorfolder + " " + languages.Get(lang() + "/tray/changed", "{0} was changed.");
            }
            else //if (not == "updated")
            {
                return fileorfolder + " " + languages.Get(lang() + "/tray/updated", "{0} was updated.");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            foreach (FtpItem f in ftpc.GetDirList())
            {
                if (f.Name == "index.html")
                    Log.Write(l.Warning, f.Modified.ToString());
                Log.Write(l.Warning, "{0} {1} {2} {3}", f.Modified.Kind.ToString(), f.Modified.ToLocalTime(), ftpc.GetFileDateTime("index.html", false), ftpc.GetFileDateTime("index.html", true));
            }
            //ftpc.CharacterEncoding = UTF8Encoding.UTF8;
            //ftpc.MakeDirectory(@"/folder/αβγδεζηθικλμνξοπρστυφχψω");
            //Extract_WebInt();
            //CheckWebInt();
            //Log.Write("*SFTP* PWD: {0} | Root is: {1}", sftpc.pwd(), "/home/" + ftpUser());
            //ListAllFiles();
            //FullList = new Dictionary<string, DateTime>();
            //FullList = listRemote();
        }        

        ///SFTP Code Starting here
        ///hell yeah

        private void sftp_login()
        {
            JSch jsch = new JSch();

            String host = ftpHost();
            String user = ftpUser();

            Session session = jsch.getSession(user, host, 22);

            /*
            ProxyHTTP p = new ProxyHTTP("", 8080);
            p.setUserPasswd("", "");
            session.setProxy(p); */

            // username and password will be given via UserInfo interface.
            UserInfo ui = new MyUserInfo();

            session.setUserInfo(ui);            

            session.setPort(ftpPort());

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
                Log.Write(l.Debug, "(SFTP) Gonna get LWT of: {0} in path: {1}", path, sftpc.pwd());
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
                Log.Write(l.Debug, "&&&&&&&&&& {0} {1} {2}", rFullPath, cPath, name, rPathToCD);

                if (isDir)
                {
                    //DirectoryInfo di = new DirectoryInfo(path);
                    sftpc.mkdir(rFullPath + name);
                    Log.Write(l.Debug, "????> Created Directory: {0} (remote)", path);

                    if (ShowNots() && lasttip != string.Format("Folder {0} was created.", name))
                        tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("created", false), name), ToolTipIcon.Info);
                    lasttip = string.Format("Folder {0} was created.", name);
                    link = null;
                }
                else
                {
                    FileInfo fi = new FileInfo(path);
                    string comPath = rFullPath + fi.Name;
                    Log.Write(l.Debug, "comPath: {0}", comPath);

                    Log.Write(l.Debug, "DirectoryName: {0} from path: {1}", fi.DirectoryName, path);
                    //sftpc.lcd(fi.DirectoryName);
                    Log.Write(l.Debug, "LCD: check");
                    SftpProgressMonitor monitor = new MyProgressMonitor();
                    sftpc.put(path, rPathToCD, monitor, ChannelSftp.OVERWRITE);

                    Log.Write(l.Debug, "********** {0} **********", fi.DirectoryName);

                    if (ShowNots() && lasttip != string.Format("File {0} was created.", fi.Name))
                        tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("created", true), fi.Name), ToolTipIcon.Info);
                    lasttip = string.Format("File {0} was created.", fi.Name);
                    Get_Link(cPath, fi.Name);

                    //ftp.SetCurrentDirectory(rPath());


                    Log.Write(l.Debug, "~~~~~~~~~~~~~> comPath: {0}", comPath);
                    UpdateTheLog(comPath, SftpGetLastWriteTime(comPath));
                }

                DoneSyncing();
            }
            catch (SftpException e)
            {
                Log.Write(l.Error, e.message);
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
                Log.Write(l.Debug, "&&&&&&&&&& {0} {1} {2}", rFullPath, cPath, name, rPathToCD);
                FileInfo fi = new FileInfo(path);
                string comPath = rFullPath + fi.Name;
                Log.Write(l.Debug, "comPath: {0}", comPath);

                Log.Write(l.Debug, "DirectoryName: {0} from path: {1}", fi.DirectoryName, path);
                //sftpc.lcd(fi.DirectoryName);
                Log.Write(l.Debug, "LCD: check");
                SftpProgressMonitor monitor = new MyProgressMonitor();
                sftpc.put(path, rPathToCD, monitor, ChannelSftp.OVERWRITE);

                Log.Write(l.Debug, "********** {0} **********", fi.DirectoryName);

                if (ShowNots() && lasttip != string.Format("File {0} was updated.", fi.Name))
                    tray.ShowBalloonTip(50, "FTPbox", string.Format(Get_Message("udpated", true), fi.Name), ToolTipIcon.Info);
                lasttip = string.Format("File {0} was updated.", fi.Name);
                Get_Link(cPath, fi.Name);

                //ftp.SetCurrentDirectory(rPath());


                Log.Write(l.Debug, "~~~~~~~~~~~~~> comPath: {0}", comPath);
                UpdateTheLog(comPath, SftpGetLastWriteTime(comPath));
                
                DoneSyncing();
            }
            catch (SftpException e)
            {
                Log.Write(l.Error, e.message);
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

            Log.Write(l.Debug, "*SFTP* ===+> Gonna delete file, name: {0} cPath {1}", name, cPath);
            Log.Write(l.Debug, "*SFTP* ==> comPath: {0}", comPath);
            string logpath = noSlashes(cPath) + @"/" + name;
            if (logpath.StartsWith("/"))
                logpath = logpath.Substring(1);

            if (isDir)
            {
                try
                {
                    //sftpc.rmdir(comPath + name);
                    DeleteFolderSFTP(comPath + name, true);
                }
                catch (SftpException e)
                {
                    Log.Write(l.Error, "[Error sftp] -> {0}", e.Message);
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
                    Log.Write(l.Error, "[Error sftp] -> {0}", e.Message);
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
            Log.Write(l.Debug, "GONNA DOWNLOAD: {0} to path {1}", path, sftpc.lpwd());
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
            Log.Write(l.Debug, "*SFTP* About to rename {0} to {1} in path: {2}", oldName, newName, rFullpath);

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
            Log.Write(l.Debug, "nameOld: {0} nameNew: {1}", nameOld, nameNew);

            string rFullOld = noSlashes(rFullpath) + "/" + nameOld;
            string rFullNew = noSlashes(rFullpath) + "/" + nameNew;
            if (rFullNew.StartsWith("/"))
            {
                rFullOld = rFullOld.Substring(1);
                rFullNew = rFullNew.Substring(1);
            }

            Log.Write(l.Debug, "rFullOld: {0} | rFullNew: {1}", rFullOld, rFullNew);

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
                if (rpath == "/")
                    rpath = "";
                else if (rpath.StartsWith("/"))
                {
                    rpath = rpath.Substring(1);
                }

                string home = sftpc.getHome();

                if (rpath != "")
                    home = noSlashes(home) + "/" + rpath;

                Log.Write(l.Debug, "Home: {0}", home);

                while (!sftpc.pwd().Equals(home))
                {
                    try
                    {
                        Log.Write(l.Debug, "*SFTP* Going up one level from {0} to get to {1} when sftpc.gethome() is: {2}", sftpc.pwd(), home, sftpc.getHome());
                        if (sftpc.pwd() == "/")
                            sftpc.cd(home.Substring(1));
                        else if (sftpc.pwd() == sftpc.getHome())
                        {
                            sftpc.cd(rpath);
                            Log.Write(l.Debug, "Changed to rpath: {0}", rpath);
                        }
                        else
                        {
                            try
                            {
                                sftpc.cd("..");
                            }
                            catch (Exception ex)
                            {
                                sftpc.quit();
                                sftp_login();
                                Log.Write(l.Error, "ERRoR> " + ex.Message);
                            }
                        }
                    }
                    catch
                    {
                        sftpc.quit();
                        sftp_login();
                    }
                }
                if (sftpc.pwd() == "/")
                {
                    sftpc.cd(home.Substring(1));
                }
            }
            catch (SftpException e)
            {
                Log.Write(l.Error, "*SFTP* [ERROR]: {0}", e.Message);
            }

            Log.Write(l.Debug, "--> Changed to root {0}", sftpc.pwd());
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
                    Log.Write(l.Debug, "*SFTP* Going up one level from {0}", sftpc.pwd());
                    if (sftpc.pwd() == "/")
                        sftpc.cd(home.Substring(1));
                    else
                        sftpc.cd("..");
                }
            }
            catch (SftpException e)
            {
                Log.Write(l.Error, "*SFTP* [ERROR]: {0}", e.Message);
            }
            Log.Write(l.Debug, "--> Changed to root {0}", sftpc.pwd());
        }

        public class MyUserInfo : UserInfo
        {
            FTPbox.Classes.Settings sets = new FTPbox.Classes.Settings();
            
            public String getPassword() { 
                return Decrypt(sets.Get("Account/Password", ""),
                "removed",
                "removed",
                "SHA1", 2, "OFRna73m*aze01xY", 256);
            }
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

            #region Decrypt Method
            public static string Decrypt(string CipherText, string Password,
              string Salt, string HashAlgorithm,
              int PasswordIterations = 2, string InitialVector = "OFRna73m*aze01xY",
              int KeySize = 256)
            {
                if (string.IsNullOrEmpty(CipherText))
                    return "";
                byte[] InitialVectorBytes = Encoding.ASCII.GetBytes(InitialVector);
                byte[] SaltValueBytes = Encoding.ASCII.GetBytes(Salt);
                byte[] CipherTextBytes = Convert.FromBase64String(CipherText);
                System.Security.Cryptography.PasswordDeriveBytes DerivedPassword = new System.Security.Cryptography.PasswordDeriveBytes(Password, SaltValueBytes, HashAlgorithm, PasswordIterations);
                byte[] KeyBytes = DerivedPassword.GetBytes(KeySize / 8);
                System.Security.Cryptography.RijndaelManaged SymmetricKey = new System.Security.Cryptography.RijndaelManaged();
                SymmetricKey.Mode = System.Security.Cryptography.CipherMode.CBC;
                byte[] PlainTextBytes = new byte[CipherTextBytes.Length];
                int ByteCount = 0;
                using (System.Security.Cryptography.ICryptoTransform Decryptor = SymmetricKey.CreateDecryptor(KeyBytes, InitialVectorBytes))
                {
                    using (MemoryStream MemStream = new MemoryStream(CipherTextBytes))
                    {
                        using (System.Security.Cryptography.CryptoStream CryptoStream = new System.Security.Cryptography.CryptoStream(MemStream, Decryptor, System.Security.Cryptography.CryptoStreamMode.Read))
                        {

                            ByteCount = CryptoStream.Read(PlainTextBytes, 0, PlainTextBytes.Length);
                            MemStream.Close();
                            CryptoStream.Close();
                        }
                    }
                }
                SymmetricKey.Clear();
                return Encoding.UTF8.GetString(PlainTextBytes, 0, ByteCount);
            }
            #endregion
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
            Log.Write(l.Info, "Gonna check pending actions");
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
            Log.Write(l.Info, "Done checking pending actions");
        }

        public void UpdateAccountInfo(string host, string username, string password, int port, string timedif, bool ftp, bool ftps, bool ftpes)
        {
            AppSettings.Put("Account/Host", host);
            AppSettings.Put("Account/Port", port);
            AppSettings.Put("Account/Username", username);
            AppSettings.Put("Account/Password", password);
            AppSettings.Put("Account/FTPorSFTP", ftp.ToString());
            AppSettings.Put("Account/FTPS", ftps.ToString());
            AppSettings.Put("Account/FTPES", ftpes.ToString());
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

        /// <summary>
        /// Delete a remote folder and everything inside it (FTP)
        /// </summary>
        /// <param name="path">path to folder to delete</param>
        /// <param name="RemFromLog">True to also remove deleted stuf from log, false to not.</param>
        public void DeleteFolderFTP(string path, bool RemFromLog, ref FtpClient ftpclient)
        {
            foreach (FtpItem fi in ftpclient.GetDirList(path))
            {
                if (fi.ItemType == FtpItemType.File)
                {
                    string fpath = string.Format("{0}/{1}", path, fi.Name);
                    ftpclient.DeleteFile(fpath);
                    Log.Write(l.Info, "Gon'delete: {0}", fpath);
                    if (RemFromLog)
                        RemoveFromLog(fpath);
                }
                else if (fi.ItemType == FtpItemType.Directory)
                {
                    if (fi.Name != "." && fi.Name != "..")
                    {
                        string fpath = string.Format("{0}/{1}", noSlashes(path), fi.Name);
                        Log.Write(l.Info, "Gon'delete files in: {0}", fpath);
                        RecursiveDeleteFTP(fpath, RemFromLog, ref ftpclient);
                    }
                }
            }

            ftpclient.DeleteDirectory(path);
            if (RemFromLog)
                RemoveFromLog(path);
        }

        public void RecursiveDeleteFTP(string path, bool RemFromLog, ref FtpClient ftpclient)
        {
            foreach (FtpItem fi in ftpclient.GetDirList(path))
            {
                if (fi.ItemType == FtpItemType.File)
                {
                    string fpath = string.Format("{0}/{1}", path, fi.Name);
                    Log.Write(l.Info, "Gon'delete: {0}", fpath);
                    ftpclient.DeleteFile(fpath);                    
                    if (RemFromLog)
                        RemoveFromLog(fpath);
                }
                else if (fi.ItemType == FtpItemType.Directory)
                {
                    if (fi.Name != "." && fi.Name != "..")
                    {
                        string fpath = string.Format("{0}/{1}", noSlashes(path), fi.Name);
                        RecursiveDeleteFTP(fpath, RemFromLog, ref ftpclient);
                    }
                }
            }

            ftpclient.DeleteDirectory(path);
            if (RemFromLog)
                RemoveFromLog(path);
        }

        /// <summary>
        /// Delete a remote folder and everything inside it (SFTP)
        /// </summary>
        /// <param name="path">path to folder to delete</param>
        /// <param name="RemFromLog">True to also remove deleted stuf from log, false to not.</param>
        public void DeleteFolderSFTP(string path, bool RemFromLog)
        {
            foreach (ChannelSftp.LsEntry lse in sftpc.ls(path))
            {
                SftpATTRS attrs = lse.getAttrs();

                if (lse.getFilename() != "." && lse.getFilename() != "..")
                {
                    if (attrs.isDir())
                    {
                        string fpath = string.Format("{0}/{1}", path, lse.getFilename());
                        Log.Write(l.Info, "Gon'delete files in: {0}", fpath);
                        RecursiveDeleteSFTP(fpath, RemFromLog);
                    }
                    else
                    {
                        string fpath = string.Format("{0}/{1}", path, lse.getFilename());
                        sftpc.rm(fpath);
                        Log.Write(l.Info, "Gon'delete: {0}", fpath);
                        if (RemFromLog)
                            RemoveFromLog(fpath);
                    }
                }
            }
            sftpc.rmdir(path);
            if (RemFromLog)
                RemoveFromLog(path);
        }

        public void RecursiveDeleteSFTP(string path, bool RemFromLog)
        {
            try
            {

                foreach (ChannelSftp.LsEntry lse in sftpc.ls(path))
                {
                    SftpATTRS attrs = lse.getAttrs();
                    if (lse.getFilename() != "." && lse.getFilename() != "..")
                    {
                        if (attrs.isDir())
                        {
                            string fpath = string.Format("{0}/{1}", path, lse.getFilename());
                            Log.Write(l.Info, "Gon'delete files in: {0}", fpath);
                            RecursiveDeleteSFTP(fpath, RemFromLog);
                        }
                        else
                        {
                            string fpath = string.Format("{0}/{1}", path, lse.getFilename());
                            sftpc.rm(fpath);
                            Log.Write(l.Info, "Gon'delete: {0}", fpath);
                            if (RemFromLog)
                                RemoveFromLog(fpath);
                        }
                    }
                }
                sftpc.rmdir(path);
                if (RemFromLog)
                    RemoveFromLog(path);
            }
            catch (Exception ex)
            {
                Log.Write(l.Error, "ErrorRDSFTP: " + ex.Message);

                sftpc.quit();
                sftp_login();
                SftpCDtoRoot();
                
                RecursiveDeleteSFTP(path, RemFromLog);
            }
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
                        ListAllFiles();
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
                ListAllFiles();

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
                    sftpc.quit();
                    sftp_login();
                    SftpCDtoRoot();
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
                string rpath = rPath();
                if (rpath == "/")
                    rpath = "/webint";
                else
                    rpath = noSlashes(rpath) + "/webint";
                DeleteFolderFTP(rpath, false, ref ftpcbg);
            }
            else
            {
                SftpCDtoRoot();
                try
                {
                    DeleteFolderSFTP("webint", false);
                }
                catch (SftpException e) { Log.Write(l.Error, "Error:: {0}", e.Message); }
            }
        }

        WebBrowser webintwb;
        public void CheckForWebIntUpdate()
        {
            Log.Write(l.Debug, "Gon'check for web interface");
            try
            {
                Syncing();
                Log.Write(l.Debug, "1");
                webintwb = new WebBrowser();
                Log.Write(l.Debug, "2");
                Log.Write(l.Debug, "3"); 
                webintwb.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webintwb_DocumentCompleted);
                Log.Write(l.Debug, "4");
                webintwb.Navigate("http://ftpbox.org/webintversion.txt");
                Log.Write(l.Debug, "5");
            }
            catch
            {
                DoneSyncing();
                ListAllFiles();
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
                SftpCDtoRoot();
                sftpc.lcd(lpath);

                SftpProgressMonitor monitor = new MyProgressMonitor();
                int mode = ChannelSftp.OVERWRITE;

                sftpc.get("webint/version.ini", ".", monitor, mode);
                lpath = noSlashes(lpath) + @"\version.ini";
            }

            string inipath = lpath;
            IniFile ini = new IniFile(inipath);
            string currentversion = ini.ReadValue("Version", "latest");
            Log.Write(l.Info, "currentversion is: {0} when newest is: {1}", currentversion, webintwb.Document.Body.InnerText);
            
            if (currentversion != webintwb.Document.Body.InnerText)
            {
                Syncing();
                string data = @"|\webint\layout\css|\webint\layout\images\fancybox|\webint\layout\templates|\webint\system\classes|\webint\system\config|\webint\system\js|\webint\system\logs|\webint\system\savant\Savant3\resources|";
                MakeWebIntFolders(data);              
                DeleteWebInt(true);
                GetWebInt();
            }
            else
            {
                DoneSyncing();
                ListAllFiles();
            }
            File.Delete(inipath);
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
                    Console.WriteLine(theEntry.Name);

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

            string rpath = rPath();
            if (rpath != "/")
                rpath = noSlashes(rpath) + "/";

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
                    ftpc.MakeDirectory(rpath + fname);
                    //ftpbg.CreateDirectory(noSlashes(rPath()) + "/" + fname);
                }
                else
                {
                    SftpCDtoRoot();
                    try
                    {
                        sftpc.mkdir(fname);
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

                    Log.Write(l.Debug, "fname: {0} | cpath: {1}", fname, cpath);

                    if (FTP())
                    {
                        Log.Write(l.Debug, "f: {0}", f);
                        ftpcbg.PutFile(f, rpath + fname, FileAction.Create);
                    }
                    else
                    {
                        SftpProgressMonitor monitor = new MyProgressMonitor();
                        sftpc.put(f, cpath, monitor, ChannelSftp.OVERWRITE);
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
            if (!FTP())
            {
                //sftpc.quit();
                //sftp_login();
            }
            ListAllFiles();
        }

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
                addremovewebintpending = true;

            changedfromcheck = false;
            //AppSettings.Put("Settings/WebInterface", chkWebInt.Checked.ToString());
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

        public void WebIntExists()
        {
            if (FTP())
            {
                string rpath = rPath();
                if (rpath != "/")
                    rpath = noSlashes(rpath) + "/webint";
                else
                    rpath = "webint";

                Log.Write(l.Info, "Searching webint folder in path: {0} ({1}) : {2}", rPath(), rpath, changedfromcheck.ToString());

                Log.Write(l.Info, "Webint folder exists: {0}", ftpcbg.Exists(rpath).ToString());

                if (ftpcbg.Exists(rpath))
                    chkWebInt.Checked = true;
                else
                    chkWebInt.Checked = false;
            }
            else
            {
                SftpCDtoRoot();
                bool hasit = false;
                foreach (ChannelSftp.LsEntry ls in sftpc.ls("."))
                {
                    if (ls.getFilename() == "webint")
                        hasit = true;
                }
                chkWebInt.Checked = hasit;
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

        public void ftp_reconnect()
        {
            try
            {
                ftpc.Close();
            }
            catch { }
            try
            {
                ftpcbg.Close();
            }
            catch { }

            try
            {
                ftpc = new FtpClient(ftpHost(), ftpPort());
                
                if (FTPS())
                {
                    if (FTPES())
                        ftpc.SecurityProtocol = FtpSecurityProtocol.Tls1OrSsl3Explicit;
                    else
                        ftpc.SecurityProtocol = FtpSecurityProtocol.Tls1OrSsl3Implicit;
                    ftpc.ValidateServerCertificate += new EventHandler<ValidateServerCertificateEventArgs>(ftp_ValidateServerCertificate);
                }
                ftpc.Open(ftpUser(), ftpPass());               
                //and the background client:

                if (!FTPS())
                    ftpcbg = new FtpClient(ftpHost(), ftpPort());
                else
                {
                    ftpcbg = new FtpClient(ftpHost(), ftpPort());
                    ftpcbg.ValidateServerCertificate += new EventHandler<ValidateServerCertificateEventArgs>(ftp_ValidateServerCertificate);
                }

                ftpcbg.Open(ftpUser(), ftpPass());
            }
            catch { CheckAccount(); }
        }

        private void ftp_ValidateServerCertificate(object sender, ValidateServerCertificateEventArgs e)
        {
            e.IsCertificateValid = true;
        }

        private void ftp_gotresponsefromserver(object sender, FtpResponseEventArgs e)
        {
            Log.Write(l.Error, e.Response.ToString());
        }

        FileAction getProperFileAction(string name)
        {
            if (name.EndsWith("txt") || name.EndsWith("html") || name.EndsWith("php") || name.EndsWith("css"))
            {
                Log.Write(l.Warning, "Will try to append");
                return FileAction.Create;
            }
            else
            {
                Log.Write(l.Warning, "Will Create");
                return FileAction.Create;
            }
        }
    }
    
}