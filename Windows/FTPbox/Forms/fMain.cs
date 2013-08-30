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
        Account fNewFtp;
        Paths newDir;
        Translate ftranslate;

        private TrayTextNotificationArgs _lastTrayStatus = new TrayTextNotificationArgs
            {AssossiatedFile = null, MessageType = MessageType.AllSynced};
        private Thread tRefresh;

        //Links
        public string link = string.Empty;                  //The web link of the last-changed file
        public string locLink = string.Empty;               //The local path to the last-changed file

        public fMain()
        {
            InitializeComponent();
        }

        private void fMain_Load(object sender, EventArgs e)
        {
            NetworkChange.NetworkAddressChanged += OnNetworkChange;            

            Settings.Load();
            Common.Setup();
            //TODO: Should this stay?
            Common.LoadLocalFolders();
            Load_Recent();

            Notifications.NotificationReady += (o, n) =>
                {
                    link = Common.LinkToRecent();
                    tray.ShowBalloonTip(100, "FTPbox", n.Text, ToolTipIcon.Info);
                };


            Common.FileLog.FileLogChanged += (o, n) => Load_Recent();

            Client.ConnectionClosed += (o, n) => Log.Write(l.Warning, "Connection closed: {0}", n.Text);

            Client.ReconnectingFailed += (o, n) => Log.Write(l.Warning, "Reconnecting failed"); //TODO: Use this...

            Client.ValidateCertificate += CheckCertificate;

            WebInterface.UpdateFound += (o, n) =>
                {
                    const string msg = "A new version of the web interface is available, do you want to upgrade to it?";
                    if (MessageBox.Show(msg, "FTPbox - WebUI Update", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        WebInterface.UpdatePending = true;
                        WebInterface.Update();
                    }
                };
            WebInterface.InterfaceRemoved += (o, n) =>
                {
                    chkWebInt.Enabled = true;
                    labViewInBrowser.Enabled = false;
                    link = string.Empty;
                };
            WebInterface.InterfaceUploaded += (o, n) =>
                {
                    chkWebInt.Enabled = true;
                    labViewInBrowser.Enabled = true;
                    link = Common.GetHttpLink("webint");
                };

            Notifications.TrayTextNotification += (o,n) => this.Invoke(new MethodInvoker(() => SetTray(o,n)));

            Client.TransferProgress += (o,n) =>
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

            fNewFtp = new Account {Tag = this};
            newDir = new Paths {Tag = this};
            ftranslate = new Translate {Tag = this};

            Get_Language();
            
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

                if (!Profile.IsNoMenusMode)
                {
                    AddContextMenu();
                    RunServer();
                }

                RefreshListing();
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
            if (!Profile.isAccountSet || (Profile.isAccountSet && string.IsNullOrWhiteSpace(Profile.Password)))
            {
                Log.Write(l.Info, "Will open New FTP form.");
                Account.just_password = string.IsNullOrWhiteSpace(Profile.Password);
                fNewFtp.ShowDialog();

                Log.Write(l.Info, "Done");

                this.Show();
            }
            else if (Profile.isAccountSet)            
                try
                {
                    Client.Connect();

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
        
        /// <summary>
        /// checks if paths used the last time still exist
        /// </summary>
        public void CheckPaths()
        {
            if (!Profile.isPathsSet)
            {
                newDir.ShowDialog();       
                this.Show();

                if (!gotpaths)
                {
                    Log.Write(l.Debug, "bb cruel world");
                    KillTheProcess();
                }
            }
            else
                gotpaths = true;
            
            Common.LoadLocalFolders();
        }
        
        /// <summary>
        /// Updates the form's labels etc
        /// </summary>
        public void UpdateDetails()
        {
            Log.Write(l.Debug, "Updating the form details");

            bool e = WebInterface.Exists;
            chkWebInt.Checked = e;
            labViewInBrowser.Enabled = e;
            changedfromcheck = false;

            chkStartUp.Checked = CheckStartup();

            lHost.Text = Profile.Host;
            lUsername.Text = Profile.Username;
            lPort.Text = Profile.Port.ToString();
            lMode.Text = (Profile.Protocol != FtpProtocol.SFTP) ? "FTP" : "SFTP";

            lLocPath.Text = Profile.LocalPath;
            lRemPath.Text = Profile.RemotePath;
            tParent.Text = Profile.HttpPath;

            chkShowNots.Checked = Settings.settingsGeneral.Notifications;

            if (Profile.TrayAction == TrayAction.OpenInBrowser)
                rOpenInBrowser.Checked = true;
            else if (Profile.TrayAction == TrayAction.CopyLink)
                rCopy2Clipboard.Checked = true;
            else
                rOpenLocal.Checked = true;

            cProfiles.Items.AddRange(Settings.ProfileTitles);
            cProfiles.SelectedIndex = Settings.settingsGeneral.DefaultProfile;

            lVersion.Text = Application.ProductVersion.Substring(0, 5) + @" Beta";

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

            nSyncFrequency.Value = Convert.ToDecimal(Settings.DefaultProfile.Account.SyncFrequency);
            if (nSyncFrequency.Value == 0) nSyncFrequency.Value = 10;

            if (Profile.Protocol != FtpProtocol.SFTP)
            {
                if (LimitUpSpeed())
                    nUpLimit.Value = Convert.ToDecimal(Settings.settingsGeneral.UploadLimit);
                if (LimitDownSpeed())
                    nDownLimit.Value = Convert.ToDecimal(Settings.settingsGeneral.DownloadLimit);
            }
            else
                gLimits.Visible = false;

            Common.FolderWatcher.Setup();

            // in a separate thread...
            new Thread(() =>
            {
                // ...check local folder for changes
                string cpath = Common.GetCommonPath(Profile.LocalPath, true);
                Common.SyncQueue.Add(new SyncQueueItem
                    {
                        Item = new ClientItem
                            {
                                FullPath = Profile.LocalPath,
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
            var list = new List<FileLogItem>(Common.RecentList);
            int lim = list.Count > 5 ? 5 : list.Count;
            
            for (int i = 0; i < 5; i++)
            {
                if (i >= lim)
                {
                    recentFilesToolStripMenuItem.DropDownItems[i].Text = Common._(MessageType.NotAvailable);
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
        /// Checks the computer's language and offers to switch to it, if available.
        /// Finally, calls Set_Language to set the form's language
        /// </summary>
        private void Get_Language()
        {
            string curlan = Settings.settingsGeneral.Language;

            if (string.IsNullOrEmpty(curlan))
            {
                string locallangtwoletter = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;                

                var langList = new Dictionary<string, string>
                {
                    {"es", "Spanish"},
                    {"de", "German"},
                    {"fr", "French"},
                    {"nl", "Dutch"},
                    {"el", "Greek"},
                    {"it", "Italian"},
                    {"tr", "Turkish"},
                    {"pt-BR", "Brazilian Portuguese"},
                    {"fo", "Faroese"},
                    {"sv", "Swedish"},
                    {"sq", "Albanian"},
                    {"ro", "Romanian"},
                    {"ko", "Korean"},
                    {"ru", "Russian"},
                    {"ja", "Japanese"},
                    {"no", "Norwegian"},
                    {"hu", "Hungarian"},
                    {"vi", "Vietnamese"},
                    {"zh_HANS", "Simplified Chinese"},
                    {"zh_HANT", "Traditional Chinese"},
                    {"lt", "Lithuanian"},
                    {"da", "Dansk"},
                    {"pl", "Polish"},
                    {"hr", "Croatian"},
                    {"sk", "Slovak"},
                    {"pt", "Portuguese"},
                    {"gl", "Galego"},
                    {"th", "Thai"},
                    {"sl", "Slovenian"},
                    {"cs", "Czech"},
                    {"he", "Hebrew"},
                    {"sr", "Serbian"}
                };

                if (langList.ContainsKey(locallangtwoletter))
                {
                    string msg = string.Format("FTPbox detected that you use {0} as your computer language. Do you want to use {0} as the language of FTPbox as well?", langList[locallangtwoletter]);
                    DialogResult x = MessageBox.Show(msg, "FTPbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    Set_Language(x == DialogResult.Yes ? locallangtwoletter : "en");
                }
                else                
                    Set_Language("en");                
            }
            else
                Set_Language(curlan);

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
            gApp.Text = Common.Languages.Get(lan + "/main_form/application", "Application");
            gWebInt.Text = Common.Languages.Get(lan + "/web_interface/web_int", "Web Interface");
            chkWebInt.Text = Common.Languages.Get(lan + "/web_interface/use_webint", "Use the Web Interface");
            labViewInBrowser.Text = Common.Languages.Get(lan + "/web_interface/view", "(View in browser)");
            chkShowNots.Text = Common.Languages.Get(lan + "/main_form/show_nots", "Show notifications");
            chkStartUp.Text = Common.Languages.Get(lan + "/main_form/start_on_startup", "Start on system start-up");            
            //account tab
            lProfile.Text = Common.Languages.Get(lan + "/main_form/profile", "Profile") + ":";
            gDetails.Text = Common.Languages.Get(lan + "/main_form/details", "Details");
            labRemPath.Text = Common.Languages.Get(lan + "/main_form/remote_path", "Remote Path") + ":";
            labLocPath.Text = Common.Languages.Get(lan + "/main_form/local_path", "Local Path") + ":";
            bAddAccount.Text = Common.Languages.Get(lan + "/new_account/add", "Add");
            bRemoveAccount.Text = Common.Languages.Get(lan + "/main_form/remove", "Remove");
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
                                t.Text = Common._(MessageType.NotAvailable);
                    }));
                }
                else
                {
                    foreach (ToolStripItem t in recentFilesToolStripMenuItem.DropDownItems)
                        if (!t.Enabled)
                            t.Text = Common._(MessageType.NotAvailable);
                }
            }

            SetTray(null, _lastTrayStatus);

            // Is this a right-to-left language?
            RightToLeftLayout = new[] { "he" }.Contains(lan);

            // Save
            Settings.settingsGeneral.Language = lan;
            Settings.SaveGeneral();
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
                        Client.Disconnect();
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
            return Settings.settingsGeneral.UploadLimit > 0;
        }

        private bool LimitDownSpeed()
        {
            return Settings.settingsGeneral.DownloadLimit > 0;
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
            string path = Profile.LocalPath;
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
                if (!s.StartsWith(Profile.LocalPath))
                {
                    MessageBox.Show("You cannot use this for files that are not inside the FTPbox folder.", "FTPbox - Invalid file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }

                if (Common.PathIsFile(s) && File.Exists(s))
                {
                    var cpath = Common.GetCommonPath(s, true);
                    bool exists = Client.Exists(cpath);
                    Common.SyncQueue.Add(new SyncQueueItem
                    {
                        Item = new ClientItem
                        {
                            FullPath = s,
                            Name = Common._name(cpath),
                            Type = ClientItemType.File,
                            Size = exists ? Client.SizeOf(cpath) : new FileInfo(s).Length,
                            LastWriteTime = exists ? Client.GetLwtOf(cpath) : File.GetLastWriteTime(s)
                        },
                        ActionType = ChangeAction.changed,
                        SyncTo = exists ? SyncTo.Local : SyncTo.Remote
                    });
                }
                else if (!Common.PathIsFile(s) && Directory.Exists(s))
                {
                    var di = new DirectoryInfo(s);
                    Common.SyncQueue.Add(new SyncQueueItem
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
                        SyncTo = SyncTo.Local,
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

        /// <summary>
        /// Called when 'Move to FTPbox folder' is clicked from the context menus
        /// </summary>
        /// <param name="args"></param>
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

        private void RefreshListing()
        {
            if (tRefresh != null && tRefresh.IsAlive) return;   
            tRefresh = new Thread(() =>
            {
                var li = new List<ClientItem>(Client.List(".").ToList());
                if (Client.ListingFailed) goto Finish;

                this.Invoke(new MethodInvoker(() => lSelectiveSync.Nodes.Clear()));

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
                        this.Invoke(new MethodInvoker(() => lSelectiveSync.Nodes.Add(new TreeNode(f.Name))));
             
                this.Invoke(new MethodInvoker(EditNodeCheckboxes));
            Finish:
                this.Invoke(new MethodInvoker(delegate { bRefresh.Enabled = true; }));
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

        /// <summary>
        /// Uncheck items that have been picked as ignored by the user
        /// </summary>
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

            // Log.Write(l.Debug, "Node {0} is checked {1}", t.FullPath, t.Checked);

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
        #endregion

        #region General Tab - Event Handlers

        private void rOpenInBrowser_CheckedChanged(object sender, EventArgs e)
        {
            if (rOpenInBrowser.Checked)
            {
                Profile.TrayAction = TrayAction.OpenInBrowser;
                Settings.settingsGeneral.TrayAction = TrayAction.OpenInBrowser;
                Settings.SaveGeneral();
            }
        }

        private void rCopy2Clipboard_CheckedChanged(object sender, EventArgs e)
        {
            if (rCopy2Clipboard.Checked)
            {
                Profile.TrayAction = TrayAction.CopyLink;
                Settings.settingsGeneral.TrayAction = TrayAction.CopyLink;
                Settings.SaveGeneral();
            }
        }

        private void rOpenLocal_CheckedChanged(object sender, EventArgs e)
        {
            if (rOpenLocal.Checked)
            {
                Profile.TrayAction = TrayAction.OpenLocalFile;
                Settings.settingsGeneral.TrayAction = TrayAction.OpenLocalFile;
                Settings.SaveGeneral();
            }
        }

        private void tParent_TextChanged(object sender, EventArgs e)
        {
            Profile.HttpPath = tParent.Text;
            Settings.SaveProfile();
        }

        private void chkShowNots_CheckedChanged(object sender, EventArgs e)
        {
            Settings.settingsGeneral.Notifications = chkShowNots.Checked;
            Settings.SaveGeneral();
        }

        private void chkWebInt_CheckedChanged(object sender, EventArgs e)
        {
            if (!changedfromcheck)
            {
                if (chkWebInt.Checked)
                    WebInterface.UpdatePending = true;
                else
                    WebInterface.DeletePending = true;
                
                chkWebInt.Enabled = false;

                if (!Common.SyncQueue.Running)
                    WebInterface.Update();
            }
            changedfromcheck = false;
        }

        private void labViewInBrowser_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(Profile.WebInterfaceLink);
        }

        #endregion

        #region Account Tab - Event Handlers

        private void bRemoveAccount_Click(object sender, EventArgs e)
        {
            string msg = string.Format("Are you sure you want to delete profile: {0}?",
                   Settings.ProfileTitles[Settings.settingsGeneral.DefaultProfile]);
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
            Settings.settingsGeneral.DefaultProfile = Settings.Profiles.Count;
            Settings.SaveGeneral();

            //  Restart
            Process.Start(Application.ExecutablePath);
            KillTheProcess();
        }

        private void cProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cProfiles.SelectedIndex == Settings.settingsGeneral.DefaultProfile) return;

            var msg = string.Format("Switch to {0} ?", Settings.ProfileTitles[cProfiles.SelectedIndex]);
            if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Settings.settingsGeneral.DefaultProfile = cProfiles.SelectedIndex;
                Settings.SaveGeneral();

                //  Restart
                Process.Start(Application.ExecutablePath);
                KillTheProcess();
            }
            else
                cProfiles.SelectedIndex = Settings.settingsGeneral.DefaultProfile;
        }

        #endregion

        #region Filters Tab - Event Handlers

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
                    li = Client.List(path).ToList();
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
                        this.Invoke(new MethodInvoker(() => e.Node.Nodes.Add(new TreeNode(f.Name))));

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

            string cpath = Common.GetCommonPath(e.Node.FullPath, false);
            // Log.Write(l.Debug, "{0} is ignored: {1} already in list: {2}", cpath, !e.Node.Checked, Common.IgnoreList.FolderList.Contains(cpath));

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
            bRefresh.Enabled = false;
            RefreshListing();
        }

        #endregion 

        #region Bandwidth Tab - Event Handlers

        private void cManually_CheckedChanged(object sender, EventArgs e)
        {
            SyncToolStripMenuItem.Enabled = cManually.Checked || !Common.SyncQueue.Running;
            Profile.SyncingMethod = (cManually.Checked) ? SyncMethod.Manual : SyncMethod.Automatic;
            Settings.SaveProfile();

            if (Profile.SyncingMethod == SyncMethod.Automatic)
            {
                Profile.SyncFrequency = Convert.ToInt32(nSyncFrequency.Value);
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
            SyncToolStripMenuItem.Enabled = !cAuto.Checked || !Common.SyncQueue.Running;
            Profile.SyncingMethod = (!cAuto.Checked) ? SyncMethod.Manual : SyncMethod.Automatic;
            Settings.SaveProfile();

            if (Profile.SyncingMethod == SyncMethod.Automatic)
            {
                Profile.SyncFrequency = Convert.ToInt32(nSyncFrequency.Value);
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
            Profile.SyncFrequency = Convert.ToInt32(nSyncFrequency.Value);
            Settings.SaveProfile();
        }

        private void nDownLimit_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                Settings.settingsGeneral.DownloadLimit = Convert.ToInt32(nDownLimit.Value);
                Client.SetMaxDownloadSpeed(Convert.ToInt32(nDownLimit.Value));
                Settings.SaveGeneral();
            }
            catch { }
        }

        private void nUpLimit_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                Settings.settingsGeneral.UploadLimit = Convert.ToInt32(nUpLimit.Value);
                Client.SetMaxUploadSpeed(Convert.ToInt32(nUpLimit.Value));
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
                Process.Start("explorer.exe", Profile.LocalPath);
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
            if (Profile.TrayAction == TrayAction.OpenInBrowser)
            {
                try
                {
                    Process.Start(Common.LinkToRecent(ind));
                }
                catch { }

            }
            else if (Profile.TrayAction == TrayAction.CopyLink)
            {
                try
                {
                    Clipboard.SetText(Common.LinkToRecent(ind));
                    SetTray(null, new TrayTextNotificationArgs { MessageType = MessageType.LinkCopied });
                }
                catch { }
            }
            else
                Process.Start(Common.PathToRecent(ind));
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            int ind = 1;
            if (Profile.TrayAction == TrayAction.OpenInBrowser)
            {
                try
                {
                    Process.Start(Common.LinkToRecent(ind));
                }
                catch { }

            }
            else if (Profile.TrayAction == TrayAction.CopyLink)
            {
                try
                {
                    Clipboard.SetText(Common.LinkToRecent(ind));
                    SetTray(null, new TrayTextNotificationArgs { MessageType = MessageType.LinkCopied });
                }
                catch { }
            }
            else
                Process.Start(Common.PathToRecent(ind));
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            int ind = 2;
            if (Profile.TrayAction == TrayAction.OpenInBrowser)
            {
                try
                {
                    Process.Start(Common.LinkToRecent(ind));
                }
                catch { }

            }
            else if (Profile.TrayAction == TrayAction.CopyLink)
            {
                try
                {
                    Clipboard.SetText(Common.LinkToRecent(ind));
                    SetTray(null, new TrayTextNotificationArgs { MessageType = MessageType.LinkCopied });
                }
                catch { }
            }
            else
                Process.Start(Common.PathToRecent(ind));
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            int ind = 3;
            if (Profile.TrayAction == TrayAction.OpenInBrowser)
            {
                try
                {
                    Process.Start(Common.LinkToRecent(ind));
                }
                catch { }

            }
            else if (Profile.TrayAction == TrayAction.CopyLink)
            {
                try
                {
                    Clipboard.SetText(Common.LinkToRecent(ind));
                    SetTray(null, new TrayTextNotificationArgs { MessageType = MessageType.LinkCopied });
                }
                catch { }
            }
            else
                Process.Start(Common.PathToRecent(ind));
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            int ind = 4;
            if (Profile.TrayAction == TrayAction.OpenInBrowser)
            {
                try
                {
                    Process.Start(Common.LinkToRecent(ind));
                }
                catch { }

            }
            else if (Profile.TrayAction == TrayAction.CopyLink)
            {
                try
                {
                    Clipboard.SetText(Common.LinkToRecent(ind));
                    SetTray(null, new TrayTextNotificationArgs { MessageType = MessageType.LinkCopied });
                }
                catch { }
            }
            else
                Process.Start(Common.PathToRecent(ind));
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
                    if (Profile.TrayAction == TrayAction.OpenInBrowser)
                    {
                        try
                        {
                            Process.Start(Common.LinkToRecent());
                        }
                        catch
                        {
                            //Gotta catch 'em all 
                        }
                    }
                    else if (Profile.TrayAction == TrayAction.CopyLink)
                    {
                        try
                        {
                            Clipboard.SetText(Common.LinkToRecent());
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
                            Process.Start(Common.PathToRecent());
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

                    msg = (e.MessageType == MessageType.Uploading) ? string.Format(Common._(MessageType.Uploading), name) : string.Format(Common._(MessageType.Downloading), name);
                }

                switch (e.MessageType)
                {
                    case MessageType.Uploading:
                        tray.Icon = Properties.Resources.syncing;
                        tray.Text = msg ?? Common._(MessageType.Syncing);
                        break;
                    case MessageType.Downloading:
                        tray.Icon = Properties.Resources.syncing;
                        tray.Text = msg ?? Common._(MessageType.Syncing);
                        break;
                    case MessageType.AllSynced:
                        tray.Icon = Properties.Resources.AS;
                        tray.Text = Common._(MessageType.AllSynced);                        
                        break;
                    case MessageType.Syncing:
                        tray.Icon = Properties.Resources.syncing;
                        tray.Text = Common._(MessageType.Syncing);                        
                        break;
                    case MessageType.Offline:
                        tray.Icon = Properties.Resources.offline1;
                        tray.Text = Common._(MessageType.Offline);                        
                        break;
                    case MessageType.Listing:
                        tray.Icon = Properties.Resources.AS;
                        tray.Text = (Profile.SyncingMethod == SyncMethod.Automatic) ? Common._(MessageType.AllSynced) : Common._(MessageType.Listing);                        
                        break;
                    case MessageType.Connecting:
                        tray.Icon = Properties.Resources.syncing;
                        tray.Text = Common._(MessageType.Connecting);                        
                        break;
                    case MessageType.Disconnected:
                        tray.Icon = Properties.Resources.syncing;
                        tray.Text = Common._(MessageType.Disconnected);                        
                        break;
                    case MessageType.Reconnecting:
                        tray.Icon = Properties.Resources.syncing;
                        tray.Text = Common._(MessageType.Reconnecting);                       
                        break;
                    case MessageType.Ready:
                        tray.Icon = Properties.Resources.AS;
                        tray.Text = Common._(MessageType.Ready);                        
                        break;
                    case MessageType.Nothing:
                        tray.Icon = Properties.Resources.ftpboxnew;
                        tray.Text = Common._(MessageType.Nothing);                        
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
            if (Profile.SyncingMethod == SyncMethod.Automatic) SyncToolStripMenuItem.Enabled = false;
            Log.Write(l.Debug, "Starting remote sync...");
            Common.SyncQueue.Add(new SyncQueueItem
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
            if (Profile.Protocol == FtpProtocol.SFTP)
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
            lSelectiveSync.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            lSelectiveSync.RightToLeftLayout = RightToLeftLayout;
            // Relocate controls where necessary
            bRefresh.Location = new Point(RightToLeftLayout ? 9 : 352, 19);
            nSyncFrequency.Location = RightToLeftLayout ? new Point(366, 94) : new Point(35, 94);
            nDownLimit.Location = RightToLeftLayout ? new Point(373, 51) : new Point(35, 51);
            nUpLimit.Location = RightToLeftLayout ? new Point(373, 110) : new Point(35, 110);

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