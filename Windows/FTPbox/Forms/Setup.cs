using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FTPboxLib;
using System.Threading.Tasks;
using System.Security.Authentication;
using System.IO;

namespace FTPbox.Forms
{
    public partial class Setup : Form
    {
        private readonly Point _groupLocation = new Point(12, 12);

        private AccountSetupTab _prevTab = AccountSetupTab.None;
        private AccountSetupTab _currentTab;
        private readonly AccountSetupTab _initialTab;

        private readonly FolderBrowserDialog _fbd = new FolderBrowserDialog() { RootFolder = Environment.SpecialFolder.Desktop, ShowNewFolderButton = true };

        private bool _checkingNodes = false;
        public static bool JustPassword = false;

        private bool ftp => cMode.SelectedIndex == 0;
        private bool ftps => ftp && cEncryption.SelectedIndex != 0;
        private bool keyAuth => !ftp && cEncryption.SelectedIndex == 1;

        public Setup()
        {
            InitializeComponent();
            var basicSetup = string.IsNullOrEmpty(Settings.General.Language);

            _initialTab = basicSetup ? AccountSetupTab.None : AccountSetupTab.Login;

            PopulateLanguages();

            if (basicSetup)
                CheckCurrentLanguage();
            else
                SetLanguage(Settings.General.Language);

            _currentTab = _initialTab;
            
            cEncryption.SelectedIndex = 0;
            cMode.SelectedIndex = 0;
        }

        private void Setup_Load(object sender, EventArgs e)
        {
            MoveGroups();
            DisableGroups();
            HideGroups();

            SwitchTab(_currentTab);

            if (JustPassword && Program.Account.IsAccountSet)
            {
                tHost.Text = Program.Account.Account.Host;
                tUsername.Text = Program.Account.Account.Username;
                nPort.Value = Program.Account.Account.Port;
                cEncryption.SelectedIndex = (int) Program.Account.Account.FtpsMethod;
                cMode.SelectedIndex = (Program.Account.Account.Protocol != FtpProtocol.SFTP) ? 0 : 1;
                cAskForPass.Checked = true;

                if (Program.Account.IsPrivateKeyValid)
                {
                    tPrivateKey.Text = Program.Account.Account.PrivateKeyFile;
                    cEncryption.SelectedIndex = 1;

                    labEncryption.Text = Common.Languages[UiControl.Authentication];
                }

                SwitchTab(AccountSetupTab.Login);

                this.AcceptButton = bFinish;
                this.ActiveControl = tPass;
                bFinish.Enabled = true;
                bNext.Enabled = false;
                bPrevious.Enabled = false;
            }
            cEncryption.SelectedIndexChanged += cEncryption_SelectedIndexChanged;
            cMode.SelectedIndexChanged += cMode_SelectedIndexChanged;
        }

        /// <summary>
        /// Relocate all control-groups to top-left.
        /// </summary>
        private void MoveGroups()
        {
            gLanguage.Location = _groupLocation;
            gLoginDetails.Location = _groupLocation;
            gLocalFolder.Location = _groupLocation;
            gRemoteFolder.Location = _groupLocation;
            gSelectiveSync.Location = _groupLocation;
        }

        /// <summary>
        /// Hide all control-groups.
        /// </summary>
        private void HideGroups()
        {
            gLanguage.Visible = false;
            gLoginDetails.Visible = false;
            gLocalFolder.Visible = false;
            gRemoteFolder.Visible = false;
            gSelectiveSync.Visible = false;
        }

        /// <summary>
        /// Disable all control-groups.
        /// </summary>
        private void DisableGroups()
        {
            gLanguage.Enabled = false;
            gLoginDetails.Enabled = false;
            gLocalFolder.Enabled = false;
            gRemoteFolder.Enabled = false;
            gSelectiveSync.Enabled = false;
        }

        /// <summary>
        /// Show the specified group (tab), hide and disable the rest
        /// </summary>
        private void SwitchTab(AccountSetupTab tab)
        {
            HideGroups();
            DisableGroups();
            
            switch(tab)
            {
                case AccountSetupTab.Login:
                    gLoginDetails.Enabled = true;
                    gLoginDetails.Visible = true;
                    break;
                case AccountSetupTab.LocalFodler:
                    gLocalFolder.Enabled = true;
                    gLocalFolder.Visible = true;
                    break;
                case AccountSetupTab.RemoteFolder:
                case AccountSetupTab.SelectiveSync:
                    gRemoteFolder.Enabled = true;
                    gRemoteFolder.Visible = true;
                    break;
                case AccountSetupTab.FileSyncOption:
                    gSelectiveSync.Enabled = true;
                    gSelectiveSync.Visible = true;
                    break;
                default:
                    gLanguage.Enabled = true;
                    gLanguage.Visible = true;
                    break;
            }
            
            _prevTab = _currentTab;
            _currentTab = tab;

            bPrevious.Enabled = _currentTab != _initialTab;
            bNext.Enabled = _currentTab != AccountSetupTab.SelectiveSync && _currentTab != AccountSetupTab.FileSyncOption;
            bFinish.Enabled = !bNext.Enabled;
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
        }

        /// <summary>
        /// Check if the system language is available in the translations.
        /// If yes, switch to it in cLanguages
        /// </summary>
        private void CheckCurrentLanguage()
        {
            var locallangtwoletter = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            
            if (Common.LanguageList.ContainsKey(locallangtwoletter))
                cLanguages.SelectedIndex = Common.LanguageList.Keys.ToList().IndexOf(locallangtwoletter);            
        }

        /// <summary>
        /// Translate all controls to the specified language
        /// </summary>        
        private void SetLanguage(string lan)
        {
            Log.Write(l.Info, "Setting lang: {0}", lan);
            Settings.General.Language = lan;
            // Language
            // this should be left to English only.

            // Login
            gLoginDetails.Text = Common.Languages[UiControl.LoginDetails];
            labMode.Text = Common.Languages[UiControl.Protocol];
            labEncryption.Text = Common.Languages[UiControl.Encryption];
            labHost.Text = Common.Languages[UiControl.Host];
            labUN.Text = Common.Languages[UiControl.Username];
            labPass.Text = Common.Languages[UiControl.Password];
            cAskForPass.Text = Common.Languages[UiControl.AskForPassword];
        
            // Local Folder
            gLocalFolder.Text = Common.Languages[UiControl.LocalFolder];
            rDefaultLocalFolder.Text = Common.Languages[UiControl.DefaultLocalFolder];
            rCustomLocalFolder.Text = Common.Languages[UiControl.CustomLocalFolder];
            bBrowse.Text = Common.Languages[UiControl.Browse];

            // Remote Folder
            gRemoteFolder.Text = Common.Languages[UiControl.RemotePath];
            labFullPath.Text = Common.Languages[UiControl.FullRemotePath];
    
            // Selective Sync
            gSelectiveSync.Text = Common.Languages[UiControl.SelectiveSync];
            rSyncAll.Text = Common.Languages[UiControl.SyncAllFiles];
            rSyncCustom.Text = Common.Languages[UiControl.SyncSpecificFiles];

            // Buttons
            bPrevious.Text = Common.Languages[UiControl.Previous];
            bNext.Text = Common.Languages[UiControl.Next];
            bFinish.Text = Common.Languages[UiControl.Finish];

            // Is this a right-to-left language?            
            RightToLeftLayout = Common.RtlLanguages.Contains(lan);
        }

        /// <summary>
        /// Attempt to login with the given details.
        /// On successful login, move to next tab or
        /// exit if this is a JustPassword dialog.
        /// </summary>
        private async Task TryLogin()
        {
            Program.Account.AddAccount(tHost.Text, tUsername.Text, tPass.Text, Convert.ToInt32(nPort.Value));
            Program.Account.Account.Protocol = ftps ? FtpProtocol.FTPS : (FtpProtocol)cMode.SelectedIndex;
            Program.Account.Account.FtpsMethod = (FtpsMethod) cEncryption.SelectedIndex;
            Program.Account.Account.PrivateKeyFile = (keyAuth) ? tPrivateKey.Text : null;

            if (keyAuth && !File.Exists(tPrivateKey.Text))
            {
                MessageBox.Show("Please select a private key to use.", "No private key selected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                Program.Account.InitClient();

                Program.Account.Client.ValidateCertificate += UIHelpers.CheckCertificate;

                await Program.Account.Client.Connect();
                Log.Write(l.Debug, "Connected: {0}", Program.Account.Client.IsConnected);

                if (JustPassword)
                {
                    Program.Account.AskForPassword = cAskForPass.Checked;
                    Settings.Save(Program.Account);
                    Hide();
                    return;
                }

                SetDefaultLocalPath();
                SwitchTab(AccountSetupTab.LocalFodler);
            }
            catch (CertificateDeclinedException)
            {
                Log.Write(l.Debug, "Certificate was declined from user");
            }
            catch (AuthenticationException ex)
            {
                MessageBox.Show("Could not authenticate. Check your account details and try again."
                    + Environment.NewLine + "Error message: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect to FTP server. Check your account details and try again."
                    + Environment.NewLine + "Error message: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Get a remote list inside the current directory
        /// and display the results in tRemoteList.        
        /// </summary>
        private async Task PopulateRemoteList()
        {
            tRemoteList.Nodes.Clear();

            Log.Write(l.Info, $"Listing from directory: {Program.Account.Client.WorkingDirectory}");

            var list = (_currentTab == AccountSetupTab.RemoteFolder)
                ? await Program.Account.Client.List(".", false)
                : await Program.Account.Client.ListRecursive(".", false);

            if (_currentTab == AccountSetupTab.RemoteFolder)
            {
                list = list.Where(x => x.Type == ClientItemType.Folder);

                var first = new TreeNode("/");
                var current = first;

                foreach (var f in Program.Account.HomePath.Split('/'))
                {
                    if (string.IsNullOrWhiteSpace(f)) continue;

                    current.Nodes.Add(f);
                    current = current.FirstNode;
                }

                tRemoteList.AfterExpand -= tRemoteList_AfterExpand;
                tRemoteList.Nodes.Add(first);
                tRemoteList.ExpandAll();

                foreach (var c in list)
                {
                    var parentNode = new TreeNode(c.Name);
                    current.Nodes.Add(parentNode);

                    var childNode = new TreeNode(c.Name);
                    parentNode.Nodes.Add(childNode);
                }

                current.Expand();
                tFullRemotePath.Text = Program.Account.HomePath;
                tRemoteList.AfterExpand += tRemoteList_AfterExpand;
            }
            else
            {
                foreach (var l in list)
                {
                    // convert to relative paths
                    l.FullPath = Program.Account.GetCommonPath(l.FullPath, false);
                }
                // get the first level folders
                var folders = list.Where(d => d.Type == ClientItemType.Folder && !d.FullPath.Contains("/"));

                // get the first level files
                var files = list
                    .Where(f => f.Type == ClientItemType.File && folders.All(d => !f.FullPath.StartsWith(d.FullPath)))
                    .Select(x => new TreeNode(x.Name))
                    .ToArray();

                // List directories first
                foreach (var d in folders)
                {
                    if (d.Name == "webint") continue;

                    var parent = UIHelpers.ConstructNodeFrom(list.ToList(), d);

                    tRemoteList.Nodes.Add(parent);
                }

                tRemoteList.Nodes.AddRange(files);

                EditNodeCheckboxes();
                tRemoteList.CollapseAll();
            }
        }

        /// <summary>
        /// Uncheck items that have been picked as ignored by the user
        /// </summary>
        private void EditNodeCheckboxes()
        {
            _checkingNodes = true;

            UIHelpers.EditNodeCheckboxesRecursive(tRemoteList.Nodes);

            _checkingNodes = false;
        }

        #region Control event handlers

        private async void bPrevious_Click(object sender, EventArgs e)
        {
            SwitchTab(_prevTab);

            switch (_prevTab)
            {
                case AccountSetupTab.None:
                    _currentTab = AccountSetupTab.None;
                    break;
                case AccountSetupTab.Login:
                    _currentTab = AccountSetupTab.None;
                    _prevTab = AccountSetupTab.None;
                    break;
                case AccountSetupTab.LocalFodler:
                    _currentTab = AccountSetupTab.Login;
                    _prevTab = _initialTab;
                    break;
                case AccountSetupTab.RemoteFolder:
                    _currentTab = AccountSetupTab.LocalFodler;
                    _prevTab = AccountSetupTab.Login;
                    break;
                case AccountSetupTab.FileSyncOption:
                    _currentTab = AccountSetupTab.RemoteFolder;
                    _prevTab = AccountSetupTab.LocalFodler;
                    break;
                case AccountSetupTab.SelectiveSync:
                    _currentTab = AccountSetupTab.FileSyncOption;
                    _prevTab = AccountSetupTab.RemoteFolder;
                    break;
            }

            if (_currentTab == AccountSetupTab.RemoteFolder)
            {
                Program.Account.Client.WorkingDirectory = Program.Account.HomePath;
                tRemoteList.CheckBoxes = false;
                tFullRemotePath.Visible = true;
                labFullPath.Text = Common.Languages[UiControl.FullRemotePath];
                await PopulateRemoteList();
            }

            bPrevious.Enabled = _currentTab != _initialTab;
            bNext.Enabled = true;
            bFinish.Enabled = false;
            this.AcceptButton = bNext;
        }

        private async void bNext_Click(object sender, EventArgs e)
        {
            switch (_currentTab)
            {
                case AccountSetupTab.None:
                    var lan = cLanguages.SelectedItem.ToString().Substring(cLanguages.SelectedItem.ToString().IndexOf("(", StringComparison.Ordinal) + 1);
                    lan = lan.Substring(0, lan.Length - 1);
                    Settings.General.Language = lan;
                    SetLanguage(lan);
                    SwitchTab(AccountSetupTab.Login);
                    break;
                case AccountSetupTab.Login:
                    bNext.Enabled = false;
                    await TryLogin();
                    bNext.Enabled = true;
                    break;
                case AccountSetupTab.LocalFodler:
                    if (!System.IO.Directory.Exists(tLocalPath.Text))
                        System.IO.Directory.CreateDirectory(tLocalPath.Text);

                    tRemoteList.CheckBoxes = false;
                    tFullRemotePath.Visible = true;
                    labFullPath.Text = Common.Languages[UiControl.FullRemotePath];
                    gRemoteFolder.Text = Common.Languages[UiControl.RemotePath];
                    SwitchTab(AccountSetupTab.RemoteFolder);
                    await PopulateRemoteList();
                    break;
                case AccountSetupTab.RemoteFolder:
                    var parentPath = Program.Account.Account.Host + tFullRemotePath.Text;
                    Program.Account.AddPaths(tFullRemotePath.Text, tLocalPath.Text, parentPath);
                    // Change directory to the specified remote folder
                    Program.Account.Client.WorkingDirectory = Program.Account.Paths.Remote;
                    // ??
                    SwitchTab(AccountSetupTab.FileSyncOption);
                    break;
                case AccountSetupTab.FileSyncOption:

                    tRemoteList.CheckBoxes = true;
                    tFullRemotePath.Visible = false;
                    labFullPath.Text = Common.Languages[UiControl.UncheckFiles];
                    gRemoteFolder.Text = Common.Languages[UiControl.SelectiveSync];
                    SwitchTab(AccountSetupTab.SelectiveSync);
                    await PopulateRemoteList();
                    break;
            }
            bFinish.Enabled = (_currentTab == AccountSetupTab.FileSyncOption && rSyncAll.Checked) || _currentTab == AccountSetupTab.SelectiveSync;
            bNext.Enabled = !bFinish.Enabled;

            this.AcceptButton = bNext.Enabled ? bNext : bFinish;
        }

        private async void bFinish_Click(object sender, EventArgs e)
        {
            if (JustPassword)
            {
                bFinish.Enabled = false;
                await TryLogin();
                bFinish.Enabled = true;
                return;
            }

            Program.Account.AskForPassword = cAskForPass.Checked;
            Settings.Save(Program.Account);
            Hide();
        }

        private void bBrowse_Click(object sender, EventArgs e)
        {
            _fbd.ShowDialog();
            if (_fbd.SelectedPath != string.Empty)
                tLocalPath.Text = _fbd.SelectedPath;
        }

        private void cMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            nPort.Value = ftp ? 21 : 22;
            
            cEncryption.Items.Clear();
            cEncryption.Items.AddRange( 
                ftp
                ? new object[] {"None", "Require implicit FTP over TLS", "Require explicit FTP over TLS" }
                : new object[] {"Normal", "Public Key Authentication"});
            cEncryption.SelectedIndex = 0;

            labEncryption.Text = ftp ? Common.Languages[UiControl.Encryption] : Common.Languages[UiControl.Authentication];

            tPrivateKey.Visible = keyAuth;
            bBrowsePrivKey.Visible = keyAuth;
            labPrivKey.Visible = keyAuth;
        }

        private void cEncryption_SelectedIndexChanged(object sender, EventArgs e)
        {
            tPrivateKey.Visible = keyAuth;
            bBrowsePrivKey.Visible = keyAuth;
            labPrivKey.Visible = keyAuth;
        }

        private void bBrowsePrivKey_Click(object sender, EventArgs e)
        {
            // start from the user's .ssh folder, the key is likely to be in here
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (Directory.Exists(Path.Combine(folder, ".ssh")))
            {
                folder = Path.Combine(folder, ".ssh");
            }

            var ofd = new OpenFileDialog()
            {
                InitialDirectory = folder,
                Multiselect = false
            };

            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            cEncryption.SelectedIndex = 1;

            tPrivateKey.Text = ofd.FileName;
        }

        private void Setup_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.TaskManagerClosing)
            {
                // If this in an abandoned new-account form, switch default profile to
                // the last profile the user set up.
                if (Settings.General.DefaultProfile > 0 && !JustPassword)
                {
                    Settings.General.DefaultProfile--;
                    Settings.SaveGeneral();
                }
                
                ((fMain)Tag).ExitedFromTray = true;
                ((fMain)Tag).KillTheProcess();
            }
        }

        private void rSyncAll_CheckedChanged(object sender, EventArgs e)
        {
            bNext.Enabled = !rSyncAll.Checked;
            bFinish.Enabled = rSyncAll.Checked;
            this.AcceptButton = bNext.Enabled ? bNext : bFinish;
        }

        private void rSyncCustom_CheckedChanged(object sender, EventArgs e)
        {
            bNext.Enabled = rSyncCustom.Checked;
            bFinish.Enabled = !rSyncCustom.Checked;
            this.AcceptButton = bNext.Enabled ? bNext : bFinish;
        }

        private void rDefaultLocalFolder_CheckedChanged(object sender, EventArgs e)
        {            
            bBrowse.Enabled = !rDefaultLocalFolder.Checked;

            SetDefaultLocalPath();
        }

        private void rCustomLocalFolder_CheckedChanged(object sender, EventArgs e)
        {            
            bBrowse.Enabled = rCustomLocalFolder.Checked;
        }

        private void SetDefaultLocalPath()
        {
            var account = string.Format("{0}@{1}", Program.Account.Account.Username, Program.Account.Account.Host);

            if (rDefaultLocalFolder.Checked)
                tLocalPath.Text = string.Format(@"{0}\FTPbox\{1}", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), account);
        }

        private void tRemoteList_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_checkingNodes) return;

            // If this is a folder node: uncheck all child nodes in the GUI
            // but only put this node (parent folder) in the ignored list.

            _checkingNodes = true;
            UIHelpers.CheckUncheckChildNodes(e.Node, e.Node.Checked);

            if (e.Node.Checked && e.Node.Parent != null && !e.Node.Parent.Checked)
            {
                e.Node.Parent.Checked = true;
                UIHelpers.CheckSingleRoute(e.Node.Parent);
            }

            _checkingNodes = false;

            Program.Account.IgnoreList.Items = UIHelpers.GetUncheckedItems(tRemoteList.Nodes).ToList();
        }

        private void tRemoteList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string path = "/" + e.Node.FullPath.Replace('\\', '/');
            if (path.EndsWith(".."))
                path = path.Substring(0, path.Length - 2);
            else if (path.EndsWith("."))
                path = path.Substring(0, path.Length - 1);

            while (path.Contains("//"))
                path = path.Replace("//", "/");

            tFullRemotePath.Text = path;
        }

        private async void tRemoteList_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (_currentTab == AccountSetupTab.SelectiveSync)
                return;
            
            string path = "/";
            path += e.Node.FullPath.Replace('\\', '/');
            while (path.Contains("//"))
                path = path.Replace("//", "/");

            if (e.Node.Nodes.Count > 0)
                e.Node.Nodes.Clear();

            var list = await Program.Account.Client.List(path, false);

            foreach (var c in list)
            {
                if (c.Type == ClientItemType.Folder)
                {
                    var parentNode = new TreeNode(c.Name);
                    e.Node.Nodes.Add(parentNode);

                    var childNode = new TreeNode(c.Name);
                    parentNode.Nodes.Add(childNode);
                }
            }
        }

        private void Setup_RightToLeftLayoutChanged(object sender, EventArgs e)
        {
            gLanguage.RightToLeft      = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            gLoginDetails.RightToLeft  = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            gLocalFolder.RightToLeft   = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            gRemoteFolder.RightToLeft  = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            gSelectiveSync.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;

            // Inherit manually
            tRemoteList.RightToLeft       = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            cAskForPass.RightToLeft       = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            tRemoteList.RightToLeftLayout = RightToLeftLayout;
            
            // Buttons
            bPrevious.Location = RightToLeftLayout ? new Point(174, 223) : new Point(235, 223);
            bNext.Location     = RightToLeftLayout ? new Point(93, 223) : new Point(316, 223);
            bFinish.Location   = RightToLeftLayout ? new Point(12, 223) : new Point(397, 223);
            
            // Relocate controls where necessary
            cMode.Location      = RightToLeftLayout ? new Point(261, 27) : new Point(145, 27);
            tHost.Location      = RightToLeftLayout ? new Point(94, 81) : new Point(145, 81);
            labColon.Location   = RightToLeftLayout ? new Point(78, 84) : new Point(375, 84); 
            nPort.Location      = RightToLeftLayout ? new Point(15, 81) : new Point(391, 81);
            tUsername.Location  = RightToLeftLayout ? new Point(15, 107) : new Point(145, 107);
            tPass.Location      = RightToLeftLayout ? new Point(15, 133) : new Point(145, 133);

            bBrowsePrivKey.Location = RightToLeftLayout ? new Point(15, 157) : new Point(391, 157);
            tPrivateKey.Location    = RightToLeftLayout ? new Point(78, 159) : new Point(145, 159);

            tLocalPath.Location      = RightToLeftLayout ? new Point(95, 133) : new Point(15, 133);
            bBrowse.Location         = RightToLeftLayout ? new Point(15, 131) : new Point(375, 131);
            tFullRemotePath.Location = RightToLeftLayout ? new Point(15, 29) : new Point(122, 29);
        }

        #endregion
    }

    public enum AccountSetupTab
    {
        None,
        Login,
        LocalFodler,
        RemoteFolder,
        FileSyncOption,
        SelectiveSync
    }
}
