using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using FTPboxLib;

namespace FTPbox.Forms
{
    public partial class fTrayForm : Form
    {
        public fTrayForm()
        {
            InitializeComponent();

            Notifications.TrayTextNotification += (o, n) =>
                {
                    if (this.IsHandleCreated)
                        this.Invoke(new MethodInvoker(() => SetStatusLabel(o, n)));
                    else
                        _lastStatus = n;
                };
        }

        /// <summary>
        /// the latest status used to set the status label
        /// </summary>
        private TrayTextNotificationArgs _lastStatus = new TrayTextNotificationArgs { AssossiatedFile = null, MessageType = MessageType.AllSynced };

        /// <summary>
        /// This item will be added to the recent list when a file transfer is in progress
        /// </summary>
        private readonly trayFormListItem _transferItem = new trayFormListItem();

        private void fTrayForm_Load(object sender, EventArgs e)
        {
            // Make sure the border doesn't appear
            this.Text = String.Empty;

            Program.Account.FileLog.FileLogChanged += (o, n) => this.Invoke(new MethodInvoker(LoadRecent));

            Program.Account.Client.TransferProgress += (o, n) =>
            {
                // Only when Downloading/Uploading.
                if (string.IsNullOrWhiteSpace(_lastStatus.AssossiatedFile)) return;
                // Update item in recent list
                this.Invoke(new MethodInvoker(() =>
                {
                    // Get status progress for the transfer
                    var progress = string.Format("{0,3}% - {1}", n.Progress, n.Rate);

                    _transferItem.FileStatusLabel = string.Format(_transferItem.SubTitleFormat, progress);
                }));
            };
            // Set the status label and load the recent files
            SetStatusLabel(null, _lastStatus);
            LoadRecent();
        }

        /// <summary>
        /// Load the recent items list
        /// </summary>
        private void LoadRecent()
        {
            var list = new List<FileLogItem>(Program.Account.RecentList);
            // Update the Recent List
            fRecentList.Controls.Clear();
            list.ForEach(f =>
                {
                    var fullPath = Path.GetFullPath(Path.Combine(Program.Account.Paths.Local, f.CommonPath));
                    var t = new trayFormListItem
                        {
                            FileNameLabel = Common._name(f.CommonPath),
                            FileStatusLabel = f.LatestChangeTime().FormatDate()
                        };
                    // Open the file in explorer.exe on click
                    t.Click += (sender, args) => Process.Start("explorer.exe", @"/select, " + fullPath);
                    fRecentList.Controls.Add(t);
                });
        }

        public void SetStatusLabel(object o, TrayTextNotificationArgs e)
        {
            try
            {
                // Save latest status
                _lastStatus = e;

                if (!string.IsNullOrWhiteSpace(e.AssossiatedFile))
                {
                    var name = Common._name(e.AssossiatedFile);

                    var format = Common.Languages[e.MessageType];

                    _transferItem.FileNameLabel = name;
                    _transferItem.SubTitleFormat = format;
                    _transferItem.FileStatusLabel = string.Format(format, string.Empty);
                    // Add to top of recent list
                    fRecentList.Controls.Add(_transferItem);
                    fRecentList.Controls.SetChildIndex(_transferItem, 0);
                }

                switch (e.MessageType)
                {
                    case MessageType.Uploading:
                    case MessageType.Downloading:
                        lCurrentStatus.Text = Common.Languages[MessageType.Syncing];
                        break;
                    case MessageType.Listing:
                        lCurrentStatus.Text = (Program.Account.Account.SyncMethod == SyncMethod.Automatic)
                                        ? Common.Languages[MessageType.AllSynced]
                                        : Common.Languages[MessageType.Listing];
                        break;
                    default:
                        lCurrentStatus.Text = Common.Languages[e.MessageType];
                        break;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex);
            }
        }

        private void fTrayForm_Leave(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void fTrayForm_Deactivate(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
