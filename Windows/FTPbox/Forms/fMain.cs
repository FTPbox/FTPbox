/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.Win32;
using System.IO.Pipes;
using FTPboxLib;

namespace FTPbox.Forms
{
    public partial class fMain : Form
    {
        public bool gotpaths = false;                       //if the paths have been set or checked
        private bool changedfromcheck = true;

        //Form instances
        private Setup fSetup;
        private Translate ftranslate;
        private fSelectiveSync fSelective;

        private TrayTextNotificationArgs _lastTrayStatus = new TrayTextNotificationArgs
            {AssossiatedFile = null, MessageType = MessageType.AllSynced};
        private Thread tRefresh;

        //Links
        public string link = string.Empty;                  //The web link of the last-changed file
        public string locLink = string.Empty;               //The local path to the last-changed file

        public fMain()
        {
            InitializeComponent();
            PopulateLanguages();
        }

        private void fMain_Load(object sender, EventArgs e)
        {
            NetworkChange.NetworkAddressChanged += OnNetworkChange;            
            
            //TODO: Should this stay?
            Program.Account.LoadLocalFolders();
            Load_Recent();

            if (!Log.DebugEnabled && Settings.General.EnableLogging)
                Log.DebugEnabled = true;

            Notifications.NotificationReady += (o, n) =>
                {
                    link = Program.Account.LinkToRecent();
                    tray.ShowBalloonTip(100, "FTPbox", n.Text, ToolTipIcon.Info);
                };


            Program.Account.FileLog.FileLogChanged += (o, n) => Load_Recent();

            Program.Account.Client.ConnectionClosed += (o, n) => Log.Write(l.Warning, "Connection closed: {0}", n.Text ?? string.Empty);

            Program.Account.Client.ReconnectingFailed += (o, n) => Log.Write(l.Warning, "Reconnecting failed"); //TODO: Use this...

            Program.Account.Client.ValidateCertificate += CheckCertificate;

            Program.Account.WebInterface.UpdateFound += (o, n) =>
                {
                    const string msg = "A new version of the web interface is available, do you want to upgrade to it?";
                    if (MessageBox.Show(msg, "FTPbox - WebUI Update", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        Program.Account.WebInterface.UpdatePending = true;
                        Program.Account.WebInterface.Update();
                    }
                };
            Program.Account.WebInterface.InterfaceRemoved += (o, n) =>
                {
                    chkWebInt.Enabled = true;
                    labViewInBrowser.Enabled = false;
                    link = string.Empty;
                };
            Program.Account.WebInterface.InterfaceUploaded += (o, n) =>
                {
                    chkWebInt.Enabled = true;
                    labViewInBrowser.Enabled = true;
                    link = Program.Account.WebInterfaceLink;
                };

            Notifications.TrayTextNotification += (o,n) => this.Invoke(new MethodInvoker(() => SetTray(o,n)));

            Program.Account.Client.TransferProgress += (o, n) =>
            {
                // Only when Downloading/Uploading.
                if (string.IsNullOrWhiteSpace(_lastTrayStatus.AssossiatedFile)) return;
                // Update tray text.
                this.Invoke(new MethodInvoker(() =>
                    {
                        SetTray(null, _lastTrayStatus);
                        // Append progress details in a new line
                        tray.Text += string.Format("\n{0,3}% - {1}", n.Progress, n.Rate);
                    }));               
            };
            fSetup = new Setup {Tag = this};
            ftranslate = new Translate {Tag = this};
            fSelective = new fSelectiveSync();
            
            if (!string.IsNullOrEmpty(Settings.General.Language))
                Set_Language(Settings.General.Language);
            
            StartUpWork();

            CheckForUpdate();
        }

        /// <summary>
        /// Work done at the application startup. 
        /// Checks the saved account info, updates the form controls and starts syncing if syncing is automatic.
        /// If there's no internet connection, puts the program to offline mode.
        /// </summary>
        private void StartUpWork()
        {
            Log.Write(l.Debug, "Internet connection available: {0}", ConnectedToInternet().ToString());
            if (ConnectedToInternet())
            {
                CheckAccount();
                //UpdateDetails();
                Log.Write(l.Debug, "Account: OK");
                CheckPaths();
                Log.Write(l.Debug, "Paths: OK");

                UpdateDetails();

                if (!Settings.IsNoMenusMode)
                {
                    AddContextMenu();
                    RunServer();
                }
            }
            else
            {
                OfflineMode = true;
                SetTray(null, new TrayTextNotificationArgs { MessageType = MessageType.Offline });
            }
        }

        /// <summary>
        /// checks if account's information used the last time has changed
        /// </summary>
        private void CheckAccount()
        {
            if (!Program.Account.isAccountSet || Program.Account.isPasswordRequired)
            {
                Log.Write(l.Info, "Will open New FTP form.");
                Setup.JustPassword = Program.Account.isPasswordRequired;
                
                fSetup.ShowDialog();

                Log.Write(l.Info, "Done");

                this.Show();
            }
            else if (Program.Account.isAccountSet)
                try
                {
                    Program.Account.Client.Connect();

                    this.ShowInTaskbar = false;
                    this.Hide();
                    this.ShowInTaskbar = true;                    
                }
                catch (Exception ex)
                {
                    Log.Write(l.Info, "Will open New FTP form");
                    Common.LogError(ex);
                    fSetup.ShowDialog();
                    Log.Write(l.Info, "Done");

                    this.Show();
                }
        }
        
        /// <summary>
        /// checks if paths used the last time still exist
        /// </summary>
        public void CheckPaths()
        {
            if (!Program.Account.isPathsSet)
            {
                fSetup.ShowDialog();       
                this.Show();

                if (!gotpaths)
                {
                    Log.Write(l.Debug, "bb cruel world");
                    KillTheProcess();
                }
            }
            else
                gotpaths = true;
            
            Program.Account.LoadLocalFolders();
        }
        
        /// <summary>
        /// Updates the form's labels etc
        /// </summary>
        public void UpdateDetails()
        {
            Log.Write(l.Debug, "Updating the form details");

            bool e = Program.Account.WebInterface.Exists;
            chkWebInt.Checked = e;
            labViewInBrowser.Enabled = e;
            changedfromcheck = false;

            chkStartUp.Checked = CheckStartup();

            lHost.Text = Program.Account.Account.Host;
            lUsername.Text = Program.Account.Account.Username;
            lPort.Text = Program.Account.Account.Port.ToString();
            lMode.Text = (Program.Account.Account.Protocol != FtpProtocol.SFTP) ? "FTP" : "SFTP";

            lLocPath.Text = Program.Account.Paths.Local;
            lRemPath.Text = Program.Account.Paths.Remote;
            tParent.Text = Program.Account.Paths.Parent;

            chkShowNots.Checked = Settings.General.Notifications;
            chkEnableLogging.Checked = Settings.General.EnableLogging;

            if (Settings.General.TrayAction == TrayAction.OpenInBrowser)
                rOpenInBrowser.Checked = true;
            else if (Settings.General.TrayAction == TrayAction.CopyLink)
                rCopy2Clipboard.Checked = true;
            else
                rOpenLocal.Checked = true;

            cProfiles.Items.AddRange(Settings.ProfileTitles);
            cProfiles.SelectedIndex = Settings.General.DefaultProfile;

            lVersion.Text = Application.ProductVersion.Substring(0, 5) + @" Beta";

            //   Filters Tab    //
            
            cIgnoreDotfiles.Checked = Program.Account.IgnoreList.IgnoreDotFiles;
            cIgnoreTempFiles.Checked = Program.Account.IgnoreList.IgnoreTempFiles;

            //  Bandwidth tab   //

            if (Program.Account.Account.SyncMethod == SyncMethod.Automatic)
                cAuto.Checked = true;
            else
                cManually.Checked = true;

            nSyncFrequency.Value = Convert.ToDecimal(Settings.DefaultProfile.Account.SyncFrequency);
            if (nSyncFrequency.Value == 0) nSyncFrequency.Value = 10;

            if (Program.Account.Account.Protocol != FtpProtocol.SFTP)
            {
                if (LimitUpSpeed())
                    nUpLimit.Value = Convert.ToDecimal(Settings.General.UploadLimit);
                if (LimitDownSpeed())
                    nDownLimit.Value = Convert.ToDecimal(Settings.General.DownloadLimit);
            }
            else
                gLimits.Visible = false;

            Set_Language(Settings.General.Language);

            Program.Account.FolderWatcher.Setup();

            // in a separate thread...
            new Thread(() =>
            {
                // ...check local folder for changes
                string cpath = Program.Account.GetCommonPath(Program.Account.Paths.Local, true);
                Program.Account.SyncQueue.Add(new SyncQueueItem (Program.Account)
                    {
                        Item = new ClientItem
                            {
                                FullPath = Program.Account.Paths.Local,
                                Name = Common._name(cpath),
                                Type = ClientItemType.Folder,
                                Size = 0x0,
                                LastWriteTime = DateTime.MinValue
                            },
                        ActionType = ChangeAction.changed,
                        SyncTo = SyncTo.Remote
                    });
            }).Start();
        }

        /// <summary>
        /// Fill the combo-box of available translations.
        /// </summary>
        private void PopulateLanguages()
        {
            cLanguages.Items.Clear();
            cLanguages.Items.AddRange(Common.FormattedLanguageList);
            // Default to English
            cLanguages.SelectedIndex = Common.SelectedLanguageIndex;

            cLanguages.SelectedIndexChanged += cLanguages_SelectedIndexChanged;
        }

        /// <summary>
        /// Kills the current process. Called from the tray menu.
        /// </summary>
        public void KillTheProcess()
        {
            if (!Settings.IsNoMenusMode)
                RemoveFTPboxMenu();

            ExitedFromTray = true;
            Log.Write(l.Info, "Killing the process...");

            try
            {
                tray.Visible = false;
                Process.GetCurrentProcess().Kill();                
            }
            catch
            {
                Application.Exit();
            }
        }

        #region Recent Files

        /// <summary>
        /// Load the recent items in the tray menu
        /// </summary>
        private void Load_Recent()
        {
            var list = new List<FileLogItem>(Program.Account.RecentList);
            int lim = list.Count > 5 ? 5 : list.Count;
            
            for (int i = 0; i < 5; i++)
            {
                if (i >= lim)
                {
                    recentFilesToolStripMenuItem.DropDownItems[i].Text = Common.Languages[MessageType.NotAvailable];
                    recentFilesToolStripMenuItem.DropDownItems[i].Enabled = false;
                    recentFilesToolStripMenuItem.DropDownItems[i].ToolTipText = string.Empty;
                    continue;
                }

                this.Invoke( new MethodInvoker(() =>
                    {
                        recentFilesToolStripMenuItem.DropDownItems[i].Text = Common._name(list[i].CommonPath);
                        recentFilesToolStripMenuItem.DropDownItems[i].Enabled = true;
                        var lastchange = DateTime.Compare(list[i].Local, list[i].Remote) < 0
                                             ? list[i].Local
                                             : list[i].Remote;
                        recentFilesToolStripMenuItem.DropDownItems[i].ToolTipText = lastchange.FormatDate();
                    }));
            }
        }

        #endregion

        #region translations

        /// <summary>
        /// Translate all controls and stuff to the given language.
        /// </summary>
        /// <param name="lan">The language to translate to in 2-letter format</param>
        private void Set_Language(string lan)
        {
            Settings.General.Language = lan;
            Log.Write(l.Debug, "Changing language to: {0}", lan);

            this.Text = "FTPbox | " + Common.Languages[UiControl.Options];
            //general tab
            tabGeneral.Text = Common.Languages[UiControl.General];
            tabAccount.Text = Common.Languages[UiControl.Account];
            gAccount.Text = "FTP " + Common.Languages[UiControl.Account];
            labHost.Text = Common.Languages[UiControl.Host];
            labUN.Text = Common.Languages[UiControl.Username];
            labPort.Text = Common.Languages[UiControl.Port];
            labMode.Text = Common.Languages[UiControl.Protocol];
            gApp.Text = Common.Languages[UiControl.Application];
            gWebInt.Text = Common.Languages[UiControl.WebUi];
            chkWebInt.Text = Common.Languages[UiControl.UseWebUi];
            labViewInBrowser.Text = Common.Languages[UiControl.ViewInBrowser];
            chkShowNots.Text = Common.Languages[UiControl.ShowNotifications];
            chkStartUp.Text = Common.Languages[UiControl.StartOnStartup];
            chkEnableLogging.Text = Common.Languages[UiControl.EnableLogging];
            bBrowseLogs.Text = Common.Languages[UiControl.ViewLog];
            //account tab
            lProfile.Text = Common.Languages[UiControl.Profile];
            gDetails.Text = Common.Languages[UiControl.Details];
            labRemPath.Text = Common.Languages[UiControl.RemotePath] + ":";
            labLocPath.Text = Common.Languages[UiControl.LocalFolder] + ":";
            bAddAccount.Text = Common.Languages[UiControl.Add];
            bRemoveAccount.Text = Common.Languages[UiControl.Remove];
            gLinks.Text = Common.Languages[UiControl.Links];
            labFullPath.Text = Common.Languages[UiControl.FullAccountPath];
            labLinkClicked.Text = Common.Languages[UiControl.WhenRecentFileClicked];
            rOpenInBrowser.Text = Common.Languages[UiControl.OpenUrl];
            rCopy2Clipboard.Text = Common.Languages[UiControl.CopyUrl];
            rOpenLocal.Text = Common.Languages[UiControl.OpenLocal];
            //filters tab
            tabFilters.Text = Common.Languages[UiControl.Filters];            
            gFileFilters.Text = Common.Languages[UiControl.Filters];
            bConfigureSelectiveSync.Text = Common.Languages[UiControl.Configure];
            bConfigureExtensions.Text = Common.Languages[UiControl.Configure];
            labSelectiveSync.Text = Common.Languages[UiControl.SelectiveSync];
            labSelectExtensions.Text = Common.Languages[UiControl.IgnoredExtensions];
            labAlsoIgnore.Text = Common.Languages[UiControl.AlsoIgnore];
            cIgnoreDotfiles.Text = Common.Languages[UiControl.Dotfiles];
            cIgnoreTempFiles.Text = Common.Languages[UiControl.TempFiles];
            cIgnoreOldFiles.Text = Common.Languages[UiControl.FilesModifiedBefore];
            //bandwidth tab
            tabBandwidth.Text = Common.Languages[UiControl.Bandwidth];
            gSyncing.Text = Common.Languages[UiControl.SyncFrequency];
            labSyncWhen.Text = Common.Languages[UiControl.StartSync] + ":";
            cAuto.Text = Common.Languages[UiControl.AutoEvery];
            labSeconds.Text = Common.Languages[UiControl.Seconds];
            cManually.Text = Common.Languages[UiControl.Manually];
            gLimits.Text = Common.Languages[UiControl.SpeedLimits];
            labDownSpeed.Text = Common.Languages[UiControl.DownLimit];
            labUpSpeed.Text = Common.Languages[UiControl.UpLimit];
            labNoLimits.Text = Common.Languages[UiControl.NoLimits];
            //language tab
            gLanguage.Text = Common.Languages[UiControl.Language];
            //about tab
            tabAbout.Text = Common.Languages[UiControl.About];
            labCurVersion.Text = Common.Languages[UiControl.CurrentVersion] + ":";
            labTeam.Text = Common.Languages[UiControl.TheTeam];
            labSite.Text = Common.Languages[UiControl.Website];
            labContact.Text = Common.Languages[UiControl.Contact];
            labLangUsed.Text = Common.Languages[UiControl.CodedIn];
            gNotes.Text = Common.Languages[UiControl.Notes];
            gContribute.Text = Common.Languages[UiControl.Contribute];
            labFree.Text = Common.Languages[UiControl.FreeAndAll];
            labContactMe.Text = Common.Languages[UiControl.GetInTouch];
            linkLabel1.Text = Common.Languages[UiControl.ReportBug];
            linkLabel2.Text = Common.Languages[UiControl.RequestFeature];
            labDonate.Text = Common.Languages[UiControl.Donate];
            labSupportMail.Text = "support@ftpbox.org";
            //tray
            optionsToolStripMenuItem.Text = Common.Languages[UiControl.Options];
            recentFilesToolStripMenuItem.Text = Common.Languages[UiControl.RecentFiles];
            aboutToolStripMenuItem.Text = Common.Languages[UiControl.About];
            SyncToolStripMenuItem.Text = Common.Languages[UiControl.StartSync];
            exitToolStripMenuItem.Text = Common.Languages[UiControl.Exit];

            for (int i = 0; i < 5; i++)
            {
                if (trayMenu.InvokeRequired)
                {
                    trayMenu.Invoke(new MethodInvoker(delegate
                    {
                        foreach (ToolStripItem t in recentFilesToolStripMenuItem.DropDownItems)
                            if (!t.Enabled)
                                t.Text = Common.Languages[MessageType.NotAvailable];
                    }));
                }
                else
                {
                    foreach (ToolStripItem t in recentFilesToolStripMenuItem.DropDownItems)
                        if (!t.Enabled)
                            t.Text = Common.Languages[MessageType.NotAvailable];
                }
            }

            SetTray(null, _lastTrayStatus);

            // Is this a right-to-left language?
            RightToLeftLayout = new[] { "he" }.Contains(lan);

            // Save
            Settings.General.Language = lan;
            Settings.SaveGeneral();
        }

        /// <summary>
        /// When the user changes to another language, translate every label etc to that language.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cLanguages_SelectedIndexChanged(object sender, EventArgs e)
        {
            string lan = cLanguages.SelectedItem.ToString().Substring(cLanguages.SelectedItem.ToString().IndexOf("(") + 1);
            lan = lan.Substring(0, lan.Length - 1);
            try
            {
                Set_Language(lan);
            }
            catch { }
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
                        Program.Account.Client.Disconnect();
                        fswFiles.Dispose();
                        fswFolders.Dispose();
                    }
                    OfflineMode = true;
                    SetTray(null, new TrayTextNotificationArgs { MessageType = MessageType.Offline });
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
                wc.DownloadStringCompleted += (o, e) =>
                {
                    if (e.Cancelled || e.Error != null) return;

                    var json = (Dictionary<string, string>) Newtonsoft.Json.JsonConvert.DeserializeObject(e.Result, typeof (Dictionary<string, string>));
                    string version = json["NewVersion"];

                    //  Check that the downloaded file has the correct version format, using regex.
                    if (System.Text.RegularExpressions.Regex.IsMatch(version, @"[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+"))
                    {
                        Log.Write(l.Debug, "Current Version: {0} Installed Version: {1}", version, Application.ProductVersion);

                        if (version == Application.ProductVersion) return;

                        // show dialog box for  download now, learn more and remind me next time
                        newversion nvform = new newversion() {Tag = this};
                        newversion.newvers = json["NewVersion"];
                        newversion.downLink = json["DownloadLink"];
                        nvform.ShowDialog();
                        this.Show();                
                    }
                };
                // Find out what the latest version is
                wc.DownloadStringAsync(new Uri(@"http://ftpbox.org/winversion.json"));
            }
            catch (Exception ex)
            {
                Log.Write(l.Debug, "Error with version checking");
                Common.LogError(ex);
            }
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
            const string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(runKey);

            if (enable)
            {
                if (startupKey.GetValue("FTPbox") == null)
                {
                    startupKey = Registry.CurrentUser.OpenSubKey(runKey, true);
                    startupKey.SetValue("FTPbox", Application.ExecutablePath);
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
            const string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(runKey);

            return startupKey.GetValue("FTPbox") != null;
        }
        
        #endregion

        #region Speed Limits

        private bool LimitUpSpeed()
        {
            return Settings.General.UploadLimit > 0;
        }

        private bool LimitDownSpeed()
        {
            return Settings.General.DownloadLimit > 0;
        }
        
        #endregion        

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

        /// <summary>
        /// Remove the FTPbox context menu (delete the registry files). 
        /// Called when application is exiting.
        /// </summary>
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
            string path = Program.Account.Paths.Local;
            string applies_to = (isForMoveItem) ? string.Format("NOT System.ItemFolderPathDisplay:~< \"{0}\"", path) : string.Format("System.ItemFolderPathDisplay:~< \"{0}\"", path);
            string short_path = null;
            var Libraries = new[] { Environment.SpecialFolder.MyDocuments, Environment.SpecialFolder.MyMusic, Environment.SpecialFolder.MyPictures, Environment.SpecialFolder.MyVideos };
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
            var _tServer = new Thread(RunServerThread);
            _tServer.SetApartmentState(ApartmentState.STA);
            _tServer.Start();            
        }

        private void RunServerThread()
        {
            int i = 1;
            Log.Write(l.Client, "Started the named-pipe server, waiting for clients (if any)");

            var server = new Thread(ServerThread);
            server.SetApartmentState(ApartmentState.STA);
            server.Start();

            Thread.Sleep(250);

            while (i > 0)
                if (server != null)
                    if (server.Join(250))
                    {
                        Log.Write(l.Client, "named-pipe server thread finished");
                        server = null;
                        i--;
                    }
            Log.Write(l.Client, "named-pipe server thread exiting...");

            RunServer();
        }

        public void ServerThread()
        {
            var pipeServer = new NamedPipeServerStream("FTPbox Server", PipeDirection.InOut, 5);
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

                CheckClientArgs( ReadCombinedParameters(args).ToArray() );

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
            var list = new List<string>(args);
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
        /// <summary>
        /// Called when 'Copy HTTP link' is clicked from the context menus
        /// </summary>
        /// <param name="args"></param>
        private void CopyArgLinks(string[] args)
        {
            string c = null;
            int i = 0;
            foreach (string s in args)
            {
                if (!s.StartsWith(Program.Account.Paths.Local))
                {
                    MessageBox.Show("You cannot use this for files that are not inside the FTPbox folder.", "FTPbox - Invalid file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }

                i++;
                //if (File.Exists(s))
                c += Program.Account.GetHttpLink(s);
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
                //SetTray(null, new FTPboxLib.TrayTextNotificationArgs { MessageType = FTPboxLib.MessageType.LinkCopied });
            }
            catch (Exception e)
            {
                Common.LogError(e);
            }
            dtLastContextAction = DateTime.Now;
        }

        /// <summary>
        /// Called when 'Synchronize this file/folder' is clicked from the context menus
        /// </summary>
        /// <param name="args"></param>
        private void SyncArgItems(string[] args)
        {
            foreach (string s in args)
            {
                Log.Write(l.Info, "Syncing local item: {0}", s);
                if (!s.StartsWith(Program.Account.Paths.Local))
                {
                    MessageBox.Show("You cannot use this for files that are not inside the FTPbox folder.", "FTPbox - Invalid file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }
                var cpath = Program.Account.GetCommonPath(s, true);
                bool exists = Program.Account.Client.Exists(cpath);

                if (Common.PathIsFile(s) && File.Exists(s))
                {
                    Program.Account.SyncQueue.Add(new SyncQueueItem (Program.Account)
                    {
                        Item = new ClientItem
                        {
                            FullPath = s,
                            Name = Common._name(cpath),
                            Type = ClientItemType.File,
                            Size = exists ? Program.Account.Client.SizeOf(cpath) : new FileInfo(s).Length,
                            LastWriteTime = exists ? Program.Account.Client.GetLwtOf(cpath) : File.GetLastWriteTime(s)
                        },
                        ActionType = ChangeAction.changed,
                        SyncTo = exists ? SyncTo.Local : SyncTo.Remote
                    });
                }
                else if (!Common.PathIsFile(s) && Directory.Exists(s))
                {
                    var di = new DirectoryInfo(s);
                    Program.Account.SyncQueue.Add(new SyncQueueItem (Program.Account)
                    {
                        Item = new ClientItem
                        {
                            FullPath = di.FullName,
                            Name = di.Name,
                            Type = ClientItemType.Folder,
                            Size = 0x0,
                            LastWriteTime = DateTime.MinValue
                        },
                        ActionType = ChangeAction.changed,
                        SyncTo = exists ? SyncTo.Local : SyncTo.Remote,
                        SkipNotification = true
                    });
                }
            }
        }

        /// <summary>
        /// Called when 'Open in browser' is clicked from the context menus
        /// </summary>
        /// <param name="args"></param>
        private void OpenArgItemsInBrowser(string[] args)
        {
            foreach (string s in args)
            {
                if (!s.StartsWith(Program.Account.Paths.Local))
                {
                    MessageBox.Show("You cannot use this for files that are not inside the FTPbox folder.", "FTPbox - Invalid file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }

                string link = Program.Account.GetHttpLink(s);
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

        /// <summary>
        /// Called when 'Move to FTPbox folder' is clicked from the context menus
        /// </summary>
        /// <param name="args"></param>
        private void MoveArgItems(string[] args)
        {
            foreach (string s in args)
            {
                if (!s.StartsWith(Program.Account.Paths.Local))
                {
                    if (File.Exists(s))
                    {
                        FileInfo fi = new FileInfo(s);
                        File.Copy(s, Path.Combine(Program.Account.Paths.Local, fi.Name));
                    }
                    else if (Directory.Exists(s))
                    {
                        foreach (string dir in Directory.GetDirectories(s, "*", SearchOption.AllDirectories))
                        {
                            string name = dir.Substring(s.Length);
                            Directory.CreateDirectory(Path.Combine(Program.Account.Paths.Local, name));
                        }
                        foreach (string file in Directory.GetFiles(s, "*", SearchOption.AllDirectories))
                        {
                            string name = file.Substring(s.Length);
                            File.Copy(file, Path.Combine(Program.Account.Paths.Local, name));
                        }
                    }
                }
            }
        }

        #endregion

        #region General Tab - Event Handlers

        private void rOpenInBrowser_CheckedChanged(object sender, EventArgs e)
        {
            if (rOpenInBrowser.Checked)
            {
                Settings.General.TrayAction = TrayAction.OpenInBrowser;
                Settings.SaveGeneral();
            }
        }

        private void rCopy2Clipboard_CheckedChanged(object sender, EventArgs e)
        {
            if (rCopy2Clipboard.Checked)
            {
                Settings.General.TrayAction = TrayAction.CopyLink;
                Settings.SaveGeneral();
            }
        }

        private void rOpenLocal_CheckedChanged(object sender, EventArgs e)
        {
            if (rOpenLocal.Checked)
            {                
                Settings.General.TrayAction = TrayAction.OpenLocalFile;
                Settings.SaveGeneral();
            }
        }

        private void tParent_TextChanged(object sender, EventArgs e)
        {
            Program.Account.Paths.Parent = tParent.Text;
            Settings.SaveProfile();
        }

        private void chkShowNots_CheckedChanged(object sender, EventArgs e)
        {
            Settings.General.Notifications = chkShowNots.Checked;
            Settings.SaveGeneral();
        }

        private void chkWebInt_CheckedChanged(object sender, EventArgs e)
        {
            if (!changedfromcheck)
            {
                if (chkWebInt.Checked)
                    Program.Account.WebInterface.UpdatePending = true;
                else
                    Program.Account.WebInterface.DeletePending = true;
                
                chkWebInt.Enabled = false;

                if (!Program.Account.SyncQueue.Running)
                    Program.Account.WebInterface.Update();
            }
            changedfromcheck = false;
        }

        private void labViewInBrowser_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(Program.Account.WebInterfaceLink);
        }

        private void chkEnableLogging_CheckedChanged(object sender, EventArgs e)
        {
            Settings.General.EnableLogging = chkEnableLogging.Checked;
            Settings.SaveGeneral();

            Log.DebugEnabled = chkEnableLogging.Checked || Settings.IsDebugMode;
        }

        private void bBrowseLogs_Click(object sender, EventArgs e)
        {
            string logFile = Path.Combine(Common.AppdataFolder, "Debug.html");

            if (File.Exists(logFile))
                Process.Start("explorer.exe", logFile);

        }

        #endregion

        #region Account Tab - Event Handlers

        private void bRemoveAccount_Click(object sender, EventArgs e)
        {
            string msg = string.Format("Are you sure you want to delete profile: {0}?",
                   Settings.ProfileTitles[Settings.General.DefaultProfile]);
            if (MessageBox.Show(msg, "Confirm Account Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Settings.RemoveCurrentProfile();

                //  Restart
                Process.Start(Application.ExecutablePath);
                KillTheProcess();
            }
        }        

        private void bAddAccount_Click(object sender, EventArgs e)
        {
            Settings.General.DefaultProfile = Settings.Profiles.Count;
            Settings.SaveGeneral();

            //  Restart
            Process.Start(Application.ExecutablePath);
            KillTheProcess();
        }

        private void cProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cProfiles.SelectedIndex == Settings.General.DefaultProfile) return;

            var msg = string.Format("Switch to {0} ?", Settings.ProfileTitles[cProfiles.SelectedIndex]);
            if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Settings.General.DefaultProfile = cProfiles.SelectedIndex;
                Settings.SaveGeneral();

                //  Restart
                Process.Start(Application.ExecutablePath);
                KillTheProcess();
            }
            else
                cProfiles.SelectedIndex = Settings.General.DefaultProfile;
        }

        #endregion

        #region Filters Tab - Event Handlers

        private void bConfigureSelectiveSync_Click(object sender, EventArgs e)
        {
            fSelective.ShowDialog();
        }

        private void bConfigureExtensions_Click(object sender, EventArgs e)
        {
            var fExtensions = new fIgnoredExtensions();
            fExtensions.ShowDialog();
        }

        private void cIgnoreTempFiles_CheckedChanged(object sender, EventArgs e)
        {
            Program.Account.IgnoreList.IgnoreTempFiles = cIgnoreTempFiles.Checked;
            Program.Account.IgnoreList.Save();
        }

        private void cIgnoreDotfiles_CheckedChanged(object sender, EventArgs e)
        {
            Program.Account.IgnoreList.IgnoreDotFiles = cIgnoreDotfiles.Checked;
            Program.Account.IgnoreList.Save();
        }

        private void cIgnoreOldFiles_CheckedChanged(object sender, EventArgs e)
        {
            dtpLastModTime.Enabled = cIgnoreOldFiles.Checked;
            Program.Account.IgnoreList.IgnoreOldFiles = cIgnoreOldFiles.Checked;
            Program.Account.IgnoreList.LastModifiedMinimum = (cIgnoreOldFiles.Checked) ? dtpLastModTime.Value : DateTime.MinValue;
            Program.Account.IgnoreList.Save();
        }

        private void dtpLastModTime_ValueChanged(object sender, EventArgs e)
        {
            Program.Account.IgnoreList.IgnoreOldFiles = cIgnoreOldFiles.Checked;
            Program.Account.IgnoreList.LastModifiedMinimum = (cIgnoreOldFiles.Checked) ? dtpLastModTime.Value : DateTime.MinValue;
            Program.Account.IgnoreList.Save();
        }

        #endregion 

        #region Bandwidth Tab - Event Handlers

        private void cManually_CheckedChanged(object sender, EventArgs e)
        {
            SyncToolStripMenuItem.Enabled = cManually.Checked || !Program.Account.SyncQueue.Running;
            Program.Account.Account.SyncMethod = (cManually.Checked) ? SyncMethod.Manual : SyncMethod.Automatic;
            Settings.SaveProfile();

            if (Program.Account.Account.SyncMethod == SyncMethod.Automatic)
            {
                Program.Account.Account.SyncFrequency = Convert.ToInt32(nSyncFrequency.Value);
                nSyncFrequency.Enabled = true;
            }
            else
            {
                nSyncFrequency.Enabled = false;
                //TODO: dispose timer?
            }
        }

        private void cAuto_CheckedChanged(object sender, EventArgs e)
        {
            SyncToolStripMenuItem.Enabled = !cAuto.Checked || !Program.Account.SyncQueue.Running;
            Program.Account.Account.SyncMethod = (!cAuto.Checked) ? SyncMethod.Manual : SyncMethod.Automatic;
            Settings.SaveProfile();

            if (Program.Account.Account.SyncMethod == SyncMethod.Automatic)
            {
                Program.Account.Account.SyncFrequency = Convert.ToInt32(nSyncFrequency.Value);
                nSyncFrequency.Enabled = true;
            }
            else
            {
                nSyncFrequency.Enabled = false;
                //TODO: dispose timer?
            }
        }

        private void nSyncFrequency_ValueChanged(object sender, EventArgs e)
        {
            Program.Account.Account.SyncFrequency = Convert.ToInt32(nSyncFrequency.Value);
            Settings.SaveProfile();
        }

        private void nDownLimit_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                Settings.General.DownloadLimit = Convert.ToInt32(nDownLimit.Value);
                Settings.SaveGeneral();
            }
            catch { }
        }

        private void nUpLimit_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                Settings.General.UploadLimit = Convert.ToInt32(nUpLimit.Value);
                Settings.SaveGeneral();
            }
            catch { }
        }

        #endregion

        #region About Tab - Event Handlers

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
            Process.Start(@"http://ftpbox.org/bugs");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"http://ftpbox.org/bugs");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Process.Start(@"http://ftpbox.org/about");
        }

        #endregion

        #region Tray Menu - Event Handlers

        private void tray_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                Process.Start("explorer.exe", Program.Account.Paths.Local);
        }

        private void SyncToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartRemoteSync(".");
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
            if (Settings.General.TrayAction == TrayAction.OpenInBrowser)
            {
                try
                {
                    Process.Start(Program.Account.LinkToRecent(ind));
                }
                catch { }

            }
            else if (Settings.General.TrayAction == TrayAction.CopyLink)
            {
                try
                {
                    Clipboard.SetText(Program.Account.LinkToRecent(ind));
                    SetTray(null, new TrayTextNotificationArgs { MessageType = MessageType.LinkCopied });
                }
                catch { }
            }
            else
                Process.Start(Program.Account.PathToRecent(ind));
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            int ind = 1;
            if (Settings.General.TrayAction == TrayAction.OpenInBrowser)
            {
                try
                {
                    Process.Start(Program.Account.LinkToRecent(ind));
                }
                catch { }

            }
            else if (Settings.General.TrayAction == TrayAction.CopyLink)
            {
                try
                {
                    Clipboard.SetText(Program.Account.LinkToRecent(ind));
                    SetTray(null, new TrayTextNotificationArgs { MessageType = MessageType.LinkCopied });
                }
                catch { }
            }
            else
                Process.Start(Program.Account.PathToRecent(ind));
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            int ind = 2;
            if (Settings.General.TrayAction == TrayAction.OpenInBrowser)
            {
                try
                {
                    Process.Start(Program.Account.LinkToRecent(ind));
                }
                catch { }

            }
            else if (Settings.General.TrayAction == TrayAction.CopyLink)
            {
                try
                {
                    Clipboard.SetText(Program.Account.LinkToRecent(ind));
                    SetTray(null, new TrayTextNotificationArgs { MessageType = MessageType.LinkCopied });
                }
                catch { }
            }
            else
                Process.Start(Program.Account.PathToRecent(ind));
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            int ind = 3;
            if (Settings.General.TrayAction == TrayAction.OpenInBrowser)
            {
                try
                {
                    Process.Start(Program.Account.LinkToRecent(ind));
                }
                catch { }

            }
            else if (Settings.General.TrayAction == TrayAction.CopyLink)
            {
                try
                {
                    Clipboard.SetText(Program.Account.LinkToRecent(ind));
                    SetTray(null, new TrayTextNotificationArgs { MessageType = MessageType.LinkCopied });
                }
                catch { }
            }
            else
                Process.Start(Program.Account.PathToRecent(ind));
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            int ind = 4;
            if (Settings.General.TrayAction == TrayAction.OpenInBrowser)
            {
                try
                {
                    Process.Start(Program.Account.LinkToRecent(ind));
                }
                catch { }

            }
            else if (Settings.General.TrayAction == TrayAction.CopyLink)
            {
                try
                {
                    Clipboard.SetText(Program.Account.LinkToRecent(ind));
                    SetTray(null, new TrayTextNotificationArgs { MessageType = MessageType.LinkCopied });
                }
                catch { }
            }
            else
                Process.Start(Program.Account.PathToRecent(ind));
        }

        private void tray_BalloonTipClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(link)) return;

            if (link.EndsWith("webint"))
                Process.Start(link);
            else
            {
                if ((MouseButtons & MouseButtons.Right) != MouseButtons.Right)
                {
                    if (Settings.General.TrayAction == TrayAction.OpenInBrowser)
                    {
                        try
                        {
                            Process.Start(Program.Account.LinkToRecent());
                        }
                        catch
                        {
                            //Gotta catch 'em all 
                        }
                    }
                    else if (Settings.General.TrayAction == TrayAction.CopyLink)
                    {
                        try
                        {
                            Clipboard.SetText(Program.Account.LinkToRecent());
                        }
                        catch
                        {
                            //Gotta catch 'em all 
                        }
                        SetTray(null, new TrayTextNotificationArgs { MessageType = MessageType.LinkCopied });
                    }
                    else
                    {
                        try
                        {
                            Process.Start(Program.Account.PathToRecent());
                        }
                        catch
                        {
                            //Gotta catch 'em all
                        }
                    }
                }
            }
        }

        #endregion                

        private void bTranslate_Click(object sender, EventArgs e)
        {
            ftranslate.ShowDialog();
        }

        public void SetTray(object o, TrayTextNotificationArgs e)
        {
            try
            {
                // Save latest tray status
                _lastTrayStatus = e;

                string msg = null;
                if (!string.IsNullOrWhiteSpace(e.AssossiatedFile))
                {
                    string name = e.AssossiatedFile;
                    // Shorten long names to be all cool with the stupid 64-character limit
                    if (name.Length >= 15)
                        name = string.Format("{0}...{1}",name.Substring(0,7), name.Substring(name.Length-5));

                    msg = (e.MessageType == MessageType.Uploading) ? string.Format(Common.Languages[MessageType.Uploading], name) : string.Format(Common.Languages[MessageType.Downloading], name);
                }

                switch (e.MessageType)
                {
                    case MessageType.Uploading:
                        tray.Icon = Properties.Resources.syncing;
                        tray.Text = msg ?? Common.Languages[MessageType.Syncing];
                        break;
                    case MessageType.Downloading:
                        tray.Icon = Properties.Resources.syncing;
                        tray.Text = msg ?? Common.Languages[MessageType.Syncing];
                        break;
                    case MessageType.AllSynced:
                        tray.Icon = Properties.Resources.AS;
                        tray.Text = Common.Languages[MessageType.AllSynced];                        
                        break;
                    case MessageType.Syncing:
                        tray.Icon = Properties.Resources.syncing;
                        tray.Text = Common.Languages[MessageType.Syncing];                        
                        break;
                    case MessageType.Offline:
                        tray.Icon = Properties.Resources.offline1;
                        tray.Text = Common.Languages[MessageType.Offline];                        
                        break;
                    case MessageType.Listing:
                        tray.Icon = Properties.Resources.AS;
                        tray.Text = (Program.Account.Account.SyncMethod == SyncMethod.Automatic) ? Common.Languages[MessageType.AllSynced] : Common.Languages[MessageType.Listing];
                        break;
                    case MessageType.Connecting:
                        tray.Icon = Properties.Resources.syncing;
                        tray.Text = Common.Languages[MessageType.Connecting];                        
                        break;
                    case MessageType.Disconnected:
                        tray.Icon = Properties.Resources.offline1;
                        tray.Text = Common.Languages[MessageType.Disconnected];                        
                        break;
                    case MessageType.Reconnecting:
                        tray.Icon = Properties.Resources.syncing;
                        tray.Text = Common.Languages[MessageType.Reconnecting];                       
                        break;
                    case MessageType.Ready:
                        tray.Icon = Properties.Resources.AS;
                        tray.Text = Common.Languages[MessageType.Ready];                        
                        break;
                    case MessageType.Nothing:
                        tray.Icon = Properties.Resources.ftpboxnew;
                        tray.Text = Common.Languages[MessageType.Nothing];                        
                        break;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        /// <summary>
        /// Starts the remote-to-local syncing on the root folder.
        /// Called from the timer, when remote syncing is automatic.
        /// </summary>
        /// <param name="state"></param>
        public void StartRemoteSync(object state)
        {
            if (Program.Account.Account.SyncMethod == SyncMethod.Automatic) SyncToolStripMenuItem.Enabled = false;
            Log.Write(l.Debug, "Starting remote sync...");
            Program.Account.SyncQueue.Add(new SyncQueueItem (Program.Account)
            {
                Item = new ClientItem
                {
                    FullPath = (string)state,
                    Name = (string)state,
                    Type = ClientItemType.Folder,
                    Size = 0x0,
                    LastWriteTime = DateTime.Now
                },
                ActionType = ChangeAction.changed,
                SyncTo = SyncTo.Local,
                SkipNotification = true
            });
        }

        /// <summary>
        /// Display a messagebox with the certificate details, ask user to approve/decline it.
        /// </summary>
        public static void CheckCertificate(object sender, ValidateCertificateEventArgs n)
        {
            var msg = string.Empty;
            // Add certificate info
            if (Program.Account.Account.Protocol == FtpProtocol.SFTP)
                msg += string.Format("{0,-8}\t {1}\n{2,-8}\t {3}\n", "Key:", n.Key, "Key Size:", n.KeySize);
            else
                msg += string.Format("{0,-25}\t {1}\n{2,-25}\t {3}\n{4,-25}\t {5}\n{6,-25}\t {7}\n\n",
                    "Valid from:", n.ValidFrom, "Valid to:", n.ValidTo, "Serial number:", n.SerialNumber, "Algorithm:", n.Algorithm);

            msg += string.Format("Fingerprint: {0}\n\n", n.Fingerprint);
            msg += "Trust this certificate and continue?";

            // Do we trust the server's certificate?
            bool certificate_trusted = MessageBox.Show(msg, "Do you trust this certificate?", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
            n.IsTrusted = certificate_trusted;

            if (certificate_trusted)
            {
                Settings.TrustedCertificates.Add(n.Fingerprint);
                Settings.SaveCertificates();
            }
        }

        private void fMain_RightToLeftLayoutChanged(object sender, EventArgs e)
        {
            RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            // Inherit manually
            tabControl1.RightToLeftLayout = RightToLeftLayout;
            trayMenu.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;

            // Relocate controls where necessary
            cLanguages.Location = RightToLeftLayout ? new Point(267, 19) : new Point(9, 19);
            bTranslate.Location = RightToLeftLayout ? new Point(172, 17) : new Point(191, 17);
            bBrowseLogs.Location = RightToLeftLayout ? new Point(172, 61) : new Point(191, 61); 

            bAddAccount.Location = new Point(RightToLeftLayout ? 14 : 299, 10);
            bRemoveAccount.Location = new Point(RightToLeftLayout ? 95 : 380, 10);
            cProfiles.Location = new Point(RightToLeftLayout ? 176 : 103, 11);

            bConfigureSelectiveSync.Location = new Point(RightToLeftLayout ? 6 : 325, 19);
            bConfigureExtensions.Location = new Point(RightToLeftLayout ? 6 : 325, 48);

            //bRefresh.Location = new Point(RightToLeftLayout ? 9 : 352, 19);
            nSyncFrequency.Location = RightToLeftLayout ? new Point(344, 89) : new Point(35, 89);
            nDownLimit.Location = RightToLeftLayout ? new Point(344, 45) : new Point(35, 45);
            nUpLimit.Location = RightToLeftLayout ? new Point(344, 100) : new Point(35, 100);

            lVersion.Location = RightToLeftLayout ? new Point(100, 21) : new Point(272, 21);
            linkLabel3.Location = RightToLeftLayout ? new Point(100, 44) : new Point(272, 44);
            linkLabel4.Location = RightToLeftLayout ? new Point(100, 67) : new Point(272, 67);
            label21.Location = RightToLeftLayout ? new Point(100, 90) : new Point(272, 90);
            labSupportMail.Location = RightToLeftLayout ? new Point(100, 113) : new Point(272, 113);
            label19.Location = RightToLeftLayout ? new Point(100, 136) : new Point(272, 136);

            labCurVersion.Location = RightToLeftLayout ? new Point(272, 21) : new Point(100, 21);
            labTeam.Location = RightToLeftLayout ? new Point(272, 44) : new Point(100, 44);
            labSite.Location = RightToLeftLayout ? new Point(272, 21) : new Point(100, 71);
            labContact.Location = RightToLeftLayout ? new Point(272, 90) : new Point(100, 90);
            labLangUsed.Location = RightToLeftLayout ? new Point(272, 136) : new Point(100, 136);
        }
    }
}