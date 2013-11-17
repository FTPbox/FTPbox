using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using FTPboxLib;

namespace FTPbox.Forms
{
    public partial class fSelectiveSync : Form
    {

        private bool checking_nodes = false;
        private Thread tRefresh;
        private string title_listing;   // The form text when listing
        private string title_normal;    // The form text when not listing

        public fSelectiveSync()
        {
            InitializeComponent();
        }

        private void fSelectiveSync_Load(object sender, EventArgs e)
        {
            var listing = Common.Languages[MessageType.Listing];
            if (listing.StartsWith("FTPbox - ")) listing = listing.Substring("FTPbox - ".Length - 1);

            title_listing = string.Format("{0} - {1}", Common.Languages[UiControl.SelectiveSync], listing);
            title_normal = Common.Languages[UiControl.SelectiveSync];

            Set_Language(Settings.General.Language);

            if (lSelectiveSync.Nodes.Count <= 0)
                RefreshListing();
        }

        private void bRefresh_Click(object sender, EventArgs e)
        {            
            RefreshListing();
        }

        private void bDone_Click(object sender, EventArgs e)
        {
            Settings.Save(Program.Account);
            Hide();
        }

        /// <summary>
        /// Refresh the entire list of files/folders
        /// </summary>
        private void RefreshListing()
        {
            this.Text = title_listing;
            bRefresh.Enabled = false;
            if (tRefresh != null && tRefresh.IsAlive) return;

            tRefresh = new Thread(() =>
            {
                var li = new List<ClientItem>(Program.Account.Client.List(".").ToList());
                if (Program.Account.Client.ListingFailed) goto Finish;

                this.Invoke(new MethodInvoker(() => lSelectiveSync.Nodes.Clear()));
                // List directories first
                foreach (var d in li.Where(d => d.Type == ClientItemType.Folder))                    
                    {
                        if (d.Name == "webint") continue;

                        var parent = new TreeNode(d.Name);
                        this.Invoke(new MethodInvoker(delegate
                        {
                            lSelectiveSync.Nodes.Add(parent);
                            parent.Nodes.Add(new TreeNode("!tempnode!"));
                        }));

                    }
                // Then list files
                foreach (var f in li.Where(f => f.Type == ClientItemType.File))                    
                        this.Invoke(new MethodInvoker(() => lSelectiveSync.Nodes.Add(new TreeNode(f.Name))));

                this.Invoke(new MethodInvoker(EditNodeCheckboxes));
            Finish:
                this.Invoke(new MethodInvoker(delegate { 
                    bRefresh.Enabled = true;
                    this.Text = title_normal; 
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
                    if (Program.Account.IgnoreList.Items.Contains(tn.Parent.FullPath))
                        Program.Account.IgnoreList.Items.Remove(tn.Parent.FullPath);
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
                if (!Program.Account.IgnoreList.isInIgnoredFolders(t.FullPath)) t.Checked = true;
                if (t.Parent != null && !t.Parent.Checked)
                    t.Checked = false;

                foreach (TreeNode tn in t.Nodes)
                    EditNodeCheckboxesRecursive(tn);
            }
        }

        private void EditNodeCheckboxesRecursive(TreeNode t)
        {
            t.Checked = Program.Account.IgnoreList.isInIgnoredFolders(t.FullPath);
            if (t.Parent != null && !t.Parent.Checked)
                t.Checked = false;

            foreach (TreeNode tn in t.Nodes)
                EditNodeCheckboxesRecursive(tn);
        }

        /// <summary>
        /// Check/uncheck all child nodes
        /// </summary>
        /// <param name="t">The parent node</param>
        /// <param name="c"><c>True</c> to check, <c>False</c> to uncheck</param>
        private void CheckUncheckChildNodes(TreeNode t, bool c)
        {
            t.Checked = c;
            foreach (TreeNode tn in t.Nodes)
                CheckUncheckChildNodes(tn, c);
        }

        private void lSelectiveSync_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (checking_nodes || e.Node.Text == "!tempnode!") return;

            string cpath = Program.Account.GetCommonPath(e.Node.FullPath, false);

            if (e.Node.Checked && Program.Account.IgnoreList.Items.Contains(cpath))
                Program.Account.IgnoreList.Items.Remove(cpath);
            else if (!e.Node.Checked && !Program.Account.IgnoreList.Items.Contains(cpath))
                Program.Account.IgnoreList.Items.Add(cpath);
            Program.Account.IgnoreList.Save();

            checking_nodes = true;
            CheckUncheckChildNodes(e.Node, e.Node.Checked);

            if (e.Node.Checked && e.Node.Parent != null)
                if (!e.Node.Parent.Checked)
                {
                    e.Node.Parent.Checked = true;
                    if (Program.Account.IgnoreList.Items.Contains(e.Node.Parent.FullPath))
                        Program.Account.IgnoreList.Items.Remove(e.Node.Parent.FullPath);
                    CheckSingleRoute(e.Node.Parent);
                }
            Program.Account.IgnoreList.Save();
            checking_nodes = false;
        }

        private void lSelectiveSync_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            e.Node.Nodes.Clear();
            e.Node.Nodes.Add(e.Node.Name);
        }

        private void lSelectiveSync_AfterExpand(object sender, TreeViewEventArgs e)
        {
            string path = e.Node.FullPath;

            if (e.Node.Nodes.Count > 0)
            {
                foreach (TreeNode tn in e.Node.Nodes)
                {
                    try
                    {
                        lSelectiveSync.Nodes[e.Node.Index].Nodes.Remove(tn);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(l.Debug, ex.Message);
                    }
                }
            }

            var tExpandItem = new Thread(() =>
            {
                var li = new List<ClientItem>();
                try
                {
                    li = Program.Account.Client.List(path).ToList();
                }
                catch (Exception ex)
                {
                    Common.LogError(ex);
                    return;
                }
                // List directories first
                foreach (var d in li.Where(d => d.Type == ClientItemType.Folder))
                    this.Invoke(new MethodInvoker(delegate
                        {
                            var parent = new TreeNode(d.Name);
                            e.Node.Nodes.Add(parent);
                            parent.Nodes.Add(new TreeNode("!tempnode"));
                        }));
                // Then list files
                foreach (var f in li.Where(f => f.Type == ClientItemType.File))                    
                    this.Invoke(new MethodInvoker(() => e.Node.Nodes.Add(new TreeNode(f.Name))));

                foreach (TreeNode tn in e.Node.Nodes)
                    this.Invoke(new MethodInvoker(delegate
                    {
                        tn.Checked = !Program.Account.IgnoreList.isInIgnoredFolders(tn.FullPath);
                    }));
            });
            tExpandItem.Start();
        }

        private void Set_Language(string lan)
        {
            this.Text = title_normal;
            labSelectFolders.Text = Common.Languages[UiControl.UncheckFiles];
            bRefresh.Text = Common.Languages[UiControl.Refresh];
            bDone.Text = Common.Languages[UiControl.Finish];

            // Is this a right-to-left language?
            RightToLeftLayout = new[] { "he" }.Contains(lan);
        }

        private void fSelectiveSync_RightToLeftLayoutChanged(object sender, EventArgs e)
        {
            RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;

            lSelectiveSync.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            lSelectiveSync.RightToLeftLayout = RightToLeftLayout;
        }
    }
}
