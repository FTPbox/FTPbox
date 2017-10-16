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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using FTPbox.Properties;
using FTPboxLib;
using Microsoft.Win32;
using Newtonsoft.Json;
using Settings = FTPboxLib.Settings;

namespace FTPbox.Forms
{
    public partial class fMain : Form
    {
        private bool _changedfromcheck = true;
        private fSelectiveSync _fSelective;
        //Form instances
        private Setup _fSetup;
        private Translate _ftranslate;
        private fTrayForm _fTrayForm;

        private TrayTextNotificationArgs _lastTrayStatus = new TrayTextNotificationArgs(MessageType.AllSynced);

        public bool GotPaths; //if the paths have been set or checked
        //Links
        public string Link = string.Empty; //The web link of the last-changed file
        public string LocLink = string.Empty; //The local path to the last-changed file

        public fMain()
        {
            InitializeComponent();
            PopulateLanguages();
        }

        private async void fMain_Load(object sender, EventArgs e)
        {
            NetworkChange.NetworkAddressChanged += OnNetworkChange;

            //TODO: Should this stay?
            Program.Account.LoadLocalFolders();

            if (!Log.DebugEnabled && Settings.General.EnableLogging)
                Log.DebugEnabled = true;

            Notifications.NotificationReady += (o, n) =>
            {
                Link = Program.Account.LinkToRecent();
                tray.ShowBalloonTip(100, n.Title, n.Text, ToolTipIcon.Info);
            };

            Program.Account.WebInterface.UpdateFound += async (o, n) =>
            {
                const string msg = "A new version of the web interface is available, do you want to upgrade to it?";
                if (
                    MessageBox.Show(msg, "FTPbox - WebUI Update", MessageBoxButtons.YesNo, MessageBoxIcon.Information) ==
                    DialogResult.Yes)
                {
                    Program.Account.WebInterface.UpdatePending = true;
                    await Program.Account.WebInterface.Update();
                }
            };
            Program.Account.WebInterface.InterfaceRemoved += (o, n) =>
            {
                chkWebInt.Enabled = true;
                labViewInBrowser.Enabled = false;
                Link = string.Empty;
            };
            Program.Account.WebInterface.InterfaceUploaded += (o, n) =>
            {
                chkWebInt.Enabled = true;
                labViewInBrowser.Enabled = true;
                Link = Program.Account.WebInterfaceLink;
            };

            Notifications.TrayTextNotification += (o, n) => SetTray(o, n);

            _fSetup = new Setup {Tag = this};
            _ftranslate = new Translate {Tag = this};
            _fSelective = new fSelectiveSync();

            if (!string.IsNullOrEmpty(Settings.General.Language))
                Set_Language(Settings.General.Language);

            await StartUpWork();

            while (OfflineMode)
            {
                // wait 30 seconds before retrying to connect
                await Task.Delay(30000);
                // retry
                await StartUpWork();
            }

            _fTrayForm = new fTrayForm { Tag = this };

            CheckForUpdate();

            // Check local folder for changes
            var cpath = Program.Account.GetCommonPath(Program.Account.Paths.Local, true);
            await Program.Account.SyncQueue.Add(
                new SyncQueueItem(Program.Account)
                {
                    Item = new ClientItem(Common._name(cpath), Program.Account.Paths.Local, ClientItemType.Folder),
                    ActionType = ChangeAction.changed,
                    SyncTo = SyncTo.Remote
                });

            await ContextMenuManager.RunServer();
        }

        /// <summary>
        ///     Work done at the application startup.
        ///     Checks the saved account info, updates the form controls and starts syncing if syncing is automatic.
        ///     If there's no internet connection, puts the program to offline mode.
        /// </summary>
        private async Task StartUpWork()
        {
            Log.Write(l.Debug, "Internet connection available: {0}", Win32.ConnectedToInternet());
            OfflineMode = false;

            if (Win32.ConnectedToInternet())
            {
                if (Program.Account.IsAccountSet)
                {
                    UpdateDetails();

                    ShowInTaskbar = false;
                    Hide();
                    ShowInTaskbar = true;
                }

                await CheckAccount();

                UpdateDetails();

                if (OfflineMode) return;

                Log.Write(l.Debug, "Account: OK");

                CheckPaths();
                Log.Write(l.Debug, "Paths: OK");

                UpdateDetails();

                var e = await Program.Account.WebInterface.CheckForUpdate();
                chkWebInt.Checked = e;
                labViewInBrowser.Enabled = e;
                _changedfromcheck = false;

                Program.Account.FolderWatcher.Setup();
            }
            else
            {
                OfflineMode = true;
                SetTray(null, new TrayTextNotificationArgs(MessageType.Offline));
            }
        }

        /// <summary>
        ///     checks if account's information used the last time has changed
        /// </summary>
        private async Task CheckAccount()
        {
            if (!Program.Account.IsAccountSet || Program.Account.IsPasswordRequired)
            {
                Log.Write(l.Info, $"Opening Setup form (just for password: {Program.Account.IsPasswordRequired})");
                Setup.JustPassword = Program.Account.IsPasswordRequired;

                _fSetup.ShowDialog();

                Log.Write(l.Info, "Done");

                Show();
            }
            else if (Program.Account.IsAccountSet)
                try
                {
                    Program.Account.Client.ConnectionClosed +=
                        (o, n) => Log.Write(l.Warning, "Connection closed: {0}", n.Text ?? string.Empty);
                    Program.Account.Client.ReconnectingFailed += (o, n) => Log.Write(l.Warning, "Reconnecting failed");
                    Program.Account.Client.ValidateCertificate += CheckCertificate;

                    await Program.Account.Client.Connect();
                }
                catch (Exception ex)
                {
                    Log.Write(l.Warning, "Connecting failed, will retry in 30 seconds...");
                    ex.LogException();

                    OfflineMode = true;
                    SetTray(null, new TrayTextNotificationArgs(MessageType.Offline));
                }
        }

        /// <summary>
        ///     checks if paths used the last time still exist
        /// </summary>
        public void CheckPaths()
        {
            if (!Program.Account.IsPathsSet)
            {
                _fSetup.ShowDialog();
                Show();

                if (!GotPaths)
                {
                    Log.Write(l.Debug, "bb cruel world");
                    KillTheProcess();
                }
            }
            else
                GotPaths = true;

            Program.Account.LoadLocalFolders();
        }

        /// <summary>
        ///     Updates the form's labels etc
        /// </summary>
        public void UpdateDetails()
        {
            Log.Write(l.Debug, "Updating the form details");

            chkStartUp.Checked = CheckStartup();

            chkShowNots.Checked = Settings.General.Notifications;
            chkEnableLogging.Checked = Settings.General.EnableLogging;
            chkShellMenus.Checked = Settings.General.AddContextMenu;

            if (Settings.General.TrayAction == TrayAction.OpenInBrowser)
                rOpenInBrowser.Checked = true;
            else if (Settings.General.TrayAction == TrayAction.CopyLink)
                rCopy2Clipboard.Checked = true;
            else
                rOpenLocal.Checked = true;

            //  Account Tab     //

            cProfiles.Items.Clear();
            cProfiles.Items.AddRange(Settings.ProfileTitles);
            cProfiles.SelectedIndex = Settings.General.DefaultProfile;

            if (Program.Account.Account.SyncDirection == SyncDirection.Both)
                rBothWaySync.Checked = true;
            else if (Program.Account.Account.SyncDirection == SyncDirection.Remote)
                rLocalToRemoteOnly.Checked = true;
            else
                rRemoteToLocalOnly.Checked = true;

            tTempPrefix.Text = Program.Account.Account.TempFilePrefix;

            //  About Tab       //

            lVersion.Text = Application.ProductVersion.Substring(0, 5) + @" Beta";

            //   Filters Tab    //

            cIgnoreDotfiles.Checked = Program.Account.IgnoreList.IgnoreDotFiles;
            cIgnoreTempFiles.Checked = Program.Account.IgnoreList.IgnoreTempFiles;

            //  Bandwidth tab   //

            nSyncFrequency.Value = Convert.ToDecimal(Program.Account.Account.SyncFrequency);
            if (nSyncFrequency.Value == 0) nSyncFrequency.Value = 10;

            if (Program.Account.Account.SyncMethod == SyncMethod.Automatic)
                cAuto.Checked = true;
            else
                cManually.Checked = true;

            nUpLimit.Value = Convert.ToDecimal(Settings.General.UploadLimit);
            nDownLimit.Value = Convert.ToDecimal(Settings.General.DownloadLimit);

            Set_Language(Settings.General.Language);

            // Disable the following in offline mode
            chkWebInt.Enabled = !OfflineMode;
            SyncToolStripMenuItem.Enabled = !OfflineMode;
        }

        /// <summary>
        ///     Fill the combo-box of available translations.
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
        ///     Kills the current process. Called from the tray menu.
        /// </summary>
        public void KillTheProcess()
        {
            if (!Settings.IsNoMenusMode)
                ContextMenuManager.RemoveContextMenu();

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

        #region Update System

        /// <summary>
        ///     checks for an update
        ///     called on each start-up of FTPbox.
        /// </summary>
        private void CheckForUpdate()
        {
            try
            {
                var wc = new WebClient();
                wc.DownloadStringCompleted += (o, e) =>
                {
                    if (e.Cancelled || e.Error != null) return;

                    var json =
                        (Dictionary<string, string>)
                            JsonConvert.DeserializeObject(e.Result, typeof (Dictionary<string, string>));
                    var version = json["NewVersion"];

                    //  Check that the downloaded file has the correct version format, using regex.
                    if (Regex.IsMatch(version, @"[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+"))
                    {
                        Log.Write(l.Debug, "Current Version: {0} Installed Version: {1}", version,
                            Application.ProductVersion);

                        if (version == Application.ProductVersion) return;

                        // show dialog box for  download now, learn more and remind me next time
                        var nvform = new newversion {Tag = this};
                        newversion.Newvers = json["NewVersion"];
                        newversion.DownLink = json["DownloadLink"];
                        nvform.ShowDialog();
                        Show();
                    }
                };
                // Find out what the latest version is
                wc.DownloadStringAsync(new Uri(@"http://ftpbox.org/winversion.json"));
            }
            catch (Exception ex)
            {
                Log.Write(l.Debug, "Error with version checking");
                ex.LogException();
            }
        }

        #endregion

        private void bTranslate_Click(object sender, EventArgs e)
        {
            _ftranslate.ShowDialog();
        }

        public void SetTray(object o, TrayTextNotificationArgs e)
        {
            try
            {
                // Save latest tray status
                _lastTrayStatus = e;

                switch (e.MessageType)
                {
                    case MessageType.Connecting:
                    case MessageType.Reconnecting:
                    case MessageType.Syncing:
                        tray.Icon = Resources.syncing;
                        tray.Text = Common.Languages[e.MessageType];
                        break;
                    case MessageType.Uploading:
                    case MessageType.Downloading:
                        tray.Icon = Resources.syncing;
                        tray.Text = Common.Languages[MessageType.Syncing];
                        break;
                    case MessageType.AllSynced:
                    case MessageType.Ready:
                        tray.Icon = Resources.AS;
                        tray.Text = Common.Languages[e.MessageType];
                        break;
                    case MessageType.Offline:
                    case MessageType.Disconnected:
                        tray.Icon = Resources.offline1;
                        tray.Text = Common.Languages[e.MessageType];
                        break;
                    case MessageType.Listing:
                        tray.Icon = Resources.AS;
                        tray.Text = (Program.Account.Account.SyncMethod == SyncMethod.Automatic)
                            ? Common.Languages[MessageType.AllSynced]
                            : Common.Languages[MessageType.Listing];
                        break;
                    case MessageType.Nothing:
                        tray.Icon = Resources.ftpboxnew;
                        tray.Text = Common.Languages[e.MessageType];
                        break;
                }
            }
            catch (Exception ex)
            {
                ex.LogException();
            }
        }

        /// <summary>
        ///     Display a messagebox with the certificate details, ask user to approve/decline it.
        /// </summary>
        public static void CheckCertificate(object sender, ValidateCertificateEventArgs n)
        {
            // Do we trust the server's certificate?
            var certificateTrusted = 
                MessageBox.Show(n.ValidationMessage(), "Do you trust this certificate?", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information) == DialogResult.Yes;
            n.IsTrusted = certificateTrusted;

            if (certificateTrusted)
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
            cProfiles.Location = new Point(RightToLeftLayout ? 170 : 8, 11);
            bConfigureAccount.Location = new Point(RightToLeftLayout ? 6 : 325, 16);

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

        #region translations

        /// <summary>
        ///     Translate all controls and stuff to the given language.
        /// </summary>
        /// <param name="lan">The language to translate to in 2-letter format</param>
        private void Set_Language(string lan)
        {
            Settings.General.Language = lan;
            Log.Write(l.Debug, "Changing language to: {0}", lan);

            Text = "FTPbox | " + Common.Languages[UiControl.Options];
            //general tab
            tabGeneral.Text = Common.Languages[UiControl.General];
            gLinks.Text = Common.Languages[UiControl.Links];
            labLinkClicked.Text = Common.Languages[UiControl.WhenRecentFileClicked];
            rOpenInBrowser.Text = Common.Languages[UiControl.OpenUrl];
            rCopy2Clipboard.Text = Common.Languages[UiControl.CopyUrl];
            rOpenLocal.Text = Common.Languages[UiControl.OpenLocal];

            gApp.Text = Common.Languages[UiControl.Application];
            chkShowNots.Text = Common.Languages[UiControl.ShowNotifications];
            chkStartUp.Text = Common.Languages[UiControl.StartOnStartup];
            chkEnableLogging.Text = Common.Languages[UiControl.EnableLogging];
            bBrowseLogs.Text = Common.Languages[UiControl.ViewLog];
            chkShellMenus.Text = Common.Languages[UiControl.AddShellMenu];

            //account tab
            tabAccount.Text = Common.Languages[UiControl.Account];
            gAccount.Text = Common.Languages[UiControl.Profile];
            bAddAccount.Text = Common.Languages[UiControl.Add];
            bRemoveAccount.Text = Common.Languages[UiControl.Remove];
            labAccount.Text = Common.Languages[UiControl.Account];
            bConfigureAccount.Text = Common.Languages[UiControl.Details];
            chkWebInt.Text = Common.Languages[UiControl.UseWebUi];
            labViewInBrowser.Text = Common.Languages[UiControl.ViewInBrowser];
            labWayOfSync.Text = Common.Languages[UiControl.WayOfSync];
            rLocalToRemoteOnly.Text = Common.Languages[UiControl.LocalToRemoteSync];
            rRemoteToLocalOnly.Text = Common.Languages[UiControl.RemoteToLocalSync];
            rBothWaySync.Text = Common.Languages[UiControl.BothWaysSync];
            labTempPrefix.Text = Common.Languages[UiControl.TempNamePrefix];

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
            aboutToolStripMenuItem.Text = Common.Languages[UiControl.About];
            SyncToolStripMenuItem.Text = Common.Languages[UiControl.StartSync];
            exitToolStripMenuItem.Text = Common.Languages[UiControl.Exit];

            SetTray(null, _lastTrayStatus);

            _fTrayForm?.Set_Language();

            // Is this a right-to-left language?
            RightToLeftLayout = Common.RtlLanguages.Contains(lan);

            // Save
            Settings.General.Language = lan;
            Settings.SaveGeneral();
        }

        /// <summary>
        ///     When the user changes to another language, translate every label etc to that language.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cLanguages_SelectedIndexChanged(object sender, EventArgs e)
        {
            var lan =
                cLanguages.SelectedItem.ToString()
                    .Substring(cLanguages.SelectedItem.ToString().IndexOf("(", StringComparison.Ordinal) + 1);
            lan = lan.Substring(0, lan.Length - 1);
            try
            {
                Set_Language(lan);
            }
            catch
            {
            }
        }

        #endregion

        #region check internet connection

        private bool OfflineMode;

        public async void OnNetworkChange(object sender, EventArgs e)
        {
            try
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    if (OfflineMode)
                    {
                        while (!Win32.ConnectedToInternet())
                            await Task.Delay(5000);
                        await StartUpWork();
                    }
                    OfflineMode = false;
                }
                else
                {
                    if (!OfflineMode)
                    {
                        await Program.Account.Client.Disconnect();
                    }
                    OfflineMode = true;
                    SetTray(null, new TrayTextNotificationArgs(MessageType.Offline));
                }
            }
            catch
            {
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
            catch (Exception ex)
            {
                ex.LogException();
            }
        }

        /// <summary>
        ///     run FTPbox on windows startup
        ///     <param name="enable"><c>true</c> to add it to system startup, <c>false</c> to remove it</param>
        /// </summary>
        private static void SetStartup(bool enable)
        {
            const string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            var startupKey = Registry.CurrentUser.OpenSubKey(runKey);

            if (enable)
            {
                if (startupKey != null && startupKey.GetValue("FTPbox") == null)
                {
                    startupKey = Registry.CurrentUser.OpenSubKey(runKey, true);
                    if (startupKey != null)
                    {
                        startupKey.SetValue("FTPbox", Application.ExecutablePath);
                        startupKey.Close();
                    }
                }
            }
            else
            {
                // remove startup
                startupKey = Registry.CurrentUser.OpenSubKey(runKey, true);
                if (startupKey != null)
                {
                    startupKey.DeleteValue("FTPbox", false);
                    startupKey.Close();
                }
            }
        }

        /// <summary>
        ///     returns true if FTPbox is set to start on windows startup
        /// </summary>
        /// <returns></returns>
        private static bool CheckStartup()
        {
            const string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

            var startupKey = Registry.CurrentUser.OpenSubKey(runKey);

            return startupKey != null && startupKey.GetValue("FTPbox") != null;
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

        private void chkShowNots_CheckedChanged(object sender, EventArgs e)
        {
            Settings.General.Notifications = chkShowNots.Checked;
            Settings.SaveGeneral();
        }

        private async void chkWebInt_CheckedChanged(object sender, EventArgs e)
        {
            if (!Program.Account.Client.IsConnected) return;

            if (!_changedfromcheck)
            {
                if (chkWebInt.Checked)
                    Program.Account.WebInterface.UpdatePending = true;
                else
                    Program.Account.WebInterface.DeletePending = true;

                chkWebInt.Enabled = false;

                if (!Program.Account.SyncQueue.Running)
                    await Program.Account.WebInterface.Update();
            }
            _changedfromcheck = false;
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
            var logFile = Path.Combine(Common.AppdataFolder, "Debug.html");

            if (File.Exists(logFile))
                Process.Start("explorer.exe", logFile);
        }

        private void chkShellMenus_CheckedChanged(object sender, EventArgs e)
        {
            Settings.General.AddContextMenu = chkShellMenus.Checked;
            Settings.SaveGeneral();

            if (chkShellMenus.Checked)
            {
                ContextMenuManager.AddContextMenu();
            }
            else
            {
                ContextMenuManager.RemoveContextMenu();
            }
        }

        #endregion

        #region Account Tab - Event Handlers

        private void bRemoveAccount_Click(object sender, EventArgs e)
        {
            var defProfile = Settings.ProfileTitles[Settings.General.DefaultProfile];
            var msg = $"Are you sure you want to delete this profile:\n{defProfile}";
            var result = MessageBox.Show(msg, "Confirm Account Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
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

        private void bConfigureAccount_Click(object sender, EventArgs e)
        {
            new fAccountDetails().ShowDialog();
        }

        private void rWayOfSync_CheckedChanged(object sender, EventArgs e)
        {
            if (rLocalToRemoteOnly.Checked)
                Program.Account.Account.SyncDirection = SyncDirection.Remote;
            else if (rRemoteToLocalOnly.Checked)
                Program.Account.Account.SyncDirection = SyncDirection.Local;
            else if (rBothWaySync.Checked)
                Program.Account.Account.SyncDirection = SyncDirection.Both;
            // Save changes
            Settings.SaveProfile();
        }

        private void tTempPrefix_TextChanged(object sender, EventArgs e)
        {
            var val = tTempPrefix.Text;
            if (string.IsNullOrWhiteSpace(val) || !Common.IsAllowedFilename(val))
                return;
            // Save new prefix
            Program.Account.Account.TempFilePrefix = val;
            Settings.SaveProfile();
        }

        private void tTempPrefix_Leave(object sender, EventArgs e)
        {
            var val = tTempPrefix.Text;
            // Reset if the inserted value is empty or not allowed
            if (string.IsNullOrWhiteSpace(val) || !Common.IsAllowedFilename(val))
                tTempPrefix.Text = Program.Account.Account.TempFilePrefix;
        }

        #endregion

        #region Filters Tab - Event Handlers

        private void bConfigureSelectiveSync_Click(object sender, EventArgs e)
        {
            _fSelective.ShowDialog();
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
            Program.Account.IgnoreList.LastModifiedMinimum = (cIgnoreOldFiles.Checked)
                ? dtpLastModTime.Value
                : DateTime.MinValue;
            Program.Account.IgnoreList.Save();
        }

        private void dtpLastModTime_ValueChanged(object sender, EventArgs e)
        {
            Program.Account.IgnoreList.IgnoreOldFiles = cIgnoreOldFiles.Checked;
            Program.Account.IgnoreList.LastModifiedMinimum = (cIgnoreOldFiles.Checked)
                ? dtpLastModTime.Value
                : DateTime.MinValue;
            Program.Account.IgnoreList.Save();
        }

        #endregion

        #region Bandwidth Tab - Event Handlers

        private async void cManually_CheckedChanged(object sender, EventArgs e)
        {
            SyncToolStripMenuItem.Enabled = cManually.Checked || !Program.Account.SyncQueue.Running;
            Program.Account.Account.SyncMethod = (cManually.Checked) ? SyncMethod.Manual : SyncMethod.Automatic;
            Settings.SaveProfile();

            if (Program.Account.Account.SyncMethod == SyncMethod.Automatic)
            {
                Program.Account.Account.SyncFrequency = Convert.ToInt32(nSyncFrequency.Value);
                nSyncFrequency.Enabled = true;
                // Schedule auto sync
                await Program.Account.SyncQueue.ScheduleAutoSync();
            }
            else
            {
                nSyncFrequency.Enabled = false;
                // Cancel
                Program.Account.SyncQueue.CancelAutoSync();
            }
        }

        private void nSyncFrequency_ValueChanged(object sender, EventArgs e)
        {
            Program.Account.Account.SyncFrequency = Convert.ToInt32(nSyncFrequency.Value);
            Settings.SaveProfile();
        }

        private void nDownLimit_ValueChanged(object sender, EventArgs e)
        {
            Settings.General.DownloadLimit = Convert.ToInt32(nDownLimit.Value);
            Settings.SaveGeneral();
        }

        private void nUpLimit_ValueChanged(object sender, EventArgs e)
        {
            Settings.General.UploadLimit = Convert.ToInt32(nUpLimit.Value);
            Settings.SaveGeneral();
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

        private void tray_MouseClick(object sender, MouseEventArgs e)
        {
            if (_fTrayForm != null && !_fTrayForm.Visible && e.Button == MouseButtons.Left)
            {
                var mouse = MousePosition;
                // Show the tray form
                _fTrayForm.Show();
                // Make sure tray form gets focus
                _fTrayForm.Activate();
                // Move the form to the correct position
                _fTrayForm.PositionProperly(mouse);
            }
        }

        private async void SyncToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Program.Account.Client.IsConnected)
                return;

            Log.Write(l.Debug, "Remote to Local sync triggered from tray menu");

            Program.Account.SyncQueue.CancelAutoSync();

            await Program.Account.SyncQueue.CheckRemoteToLocal();
        }

        public bool ExitedFromTray;

        private void fMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ExitedFromTray && e.CloseReason != CloseReason.WindowsShutDown)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KillTheProcess();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            BringToFront();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            tabControl1.SelectedTab = tabAbout;
        }

        private void tray_BalloonTipClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Link)) return;

            if (Link.EndsWith("webint"))
            {
                Process.Start(Link);
                return;
            }
            if ((MouseButtons & MouseButtons.Right) != MouseButtons.Right)
            {
                if (Settings.General.TrayAction == TrayAction.CopyLink)
                {
                    try
                    {
                        Clipboard.SetText(Program.Account.LinkToRecent());
                    }
                    catch { }
                    SetTray(null, new TrayTextNotificationArgs(MessageType.LinkCopied));
                    return;
                }
                var link = Program.Account.PathToRecent();
                if (Settings.General.TrayAction == TrayAction.OpenInBrowser)
                {
                    link = Program.Account.LinkToRecent();
                }
                try
                {
                    Process.Start(link);
                }
                catch { }                
            }
        }

        #endregion
    }
}