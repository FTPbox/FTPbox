/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* fSelectiveSync.cs
 * Select what files/folders get ignored from this form.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FTPboxLib;

namespace FTPbox.Forms
{
    public partial class fSelectiveSync : Form
    {

        private bool _checkingNodes = false;
        private string _titleListing;   // The form text when listing
        private string _titleNormal;    // The form text when not listing

        public fSelectiveSync()
        {
            InitializeComponent();
        }

        private async void fSelectiveSync_Load(object sender, EventArgs e)
        {
            var listing = Common.Languages[MessageType.Listing];
            if (listing.StartsWith("FTPbox - ")) listing = listing.Substring("FTPbox - ".Length - 1);

            _titleListing = string.Format("{0} - {1}", Common.Languages[UiControl.SelectiveSync], listing);
            _titleNormal = Common.Languages[UiControl.SelectiveSync];

            Set_Language(Settings.General.Language);

            if (lSelectiveSync.Nodes.Count <= 0)
                await RefreshListing();
        }

        private async void bRefresh_Click(object sender, EventArgs e)
        {
            await RefreshListing();
        }

        private void bDone_Click(object sender, EventArgs e)
        {
            Settings.Save(Program.Account);
            Hide();
        }

        /// <summary>
        /// Refresh the entire list of files/folders
        /// </summary>
        private async Task RefreshListing()
        {
            this.Text = _titleListing;
            bRefresh.Enabled = false;
            
            var li = (await Program.Account.Client.List(".")).ToList();

            if (!Program.Account.Client.ListingFailed)
            {
                lSelectiveSync.Nodes.Clear();

                // List directories first
                foreach (var d in li.Where(d => d.Type == ClientItemType.Folder))
                {
                    if (d.Name == "webint") continue;

                    var parent = new TreeNode(d.Name);

                    lSelectiveSync.Nodes.Add(parent);
                    parent.Nodes.Add(new TreeNode("!tempnode!"));

                }
                // Then list files
                foreach (var f in li.Where(f => f.Type == ClientItemType.File))
                    lSelectiveSync.Nodes.Add(new TreeNode(f.Name));

                EditNodeCheckboxes();
            }

            bRefresh.Enabled = true;
            this.Text = _titleNormal;
        }

        private static void CheckSingleRoute(TreeNode tn)
        {
            while (true)
            {
                if (tn.Checked && tn.Parent != null)
                    if (!tn.Parent.Checked)
                    {
                        tn.Parent.Checked = true;
                        if (Program.Account.IgnoreList.Items.Contains(tn.Parent.FullPath))
                            Program.Account.IgnoreList.Items.Remove(tn.Parent.FullPath);
                        tn = tn.Parent;
                        continue;
                    }
                break;
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
        private static void CheckUncheckChildNodes(TreeNode t, bool c)
        {
            t.Checked = c;
            foreach (TreeNode tn in t.Nodes)
                CheckUncheckChildNodes(tn, c);
        }

        private void lSelectiveSync_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_checkingNodes || e.Node.Text == "!tempnode!") return;

            var cpath = Program.Account.GetCommonPath(e.Node.FullPath, false);

            if (e.Node.Checked && Program.Account.IgnoreList.Items.Contains(cpath))
                Program.Account.IgnoreList.Items.Remove(cpath);
            else if (!e.Node.Checked && !Program.Account.IgnoreList.Items.Contains(cpath))
                Program.Account.IgnoreList.Items.Add(cpath);
            Program.Account.IgnoreList.Save();

            _checkingNodes = true;
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
            _checkingNodes = false;
        }

        private void lSelectiveSync_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            e.Node.Nodes.Clear();
            e.Node.Nodes.Add(e.Node.Name);
        }

        private async void lSelectiveSync_AfterExpand(object sender, TreeViewEventArgs e)
        {
            var path = e.Node.FullPath;

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
            
            IEnumerable<ClientItem> li = new List<ClientItem>();
            try
            {
                li = await Program.Account.Client.List(path);
            }
            catch (Exception ex)
            {
                ex.LogException();
                return;
            }
            // List directories first
            foreach (var d in li.Where(d => d.Type == ClientItemType.Folder))
            {
                var parent = new TreeNode(d.Name);
                e.Node.Nodes.Add(parent);
                parent.Nodes.Add(new TreeNode("!tempnode"));
            }

            // Then list files
            foreach (var f in li.Where(f => f.Type == ClientItemType.File))
            {
                e.Node.Nodes.Add(new TreeNode(f.Name));
            }

            foreach (TreeNode tn in e.Node.Nodes)
            {
                var tn1 = tn;
                tn1.Checked = !Program.Account.IgnoreList.isInIgnoredFolders(tn1.FullPath);
            }
        }

        private void Set_Language(string lan)
        {
            this.Text = _titleNormal;
            labSelectFolders.Text = Common.Languages[UiControl.UncheckFiles];
            bRefresh.Text = Common.Languages[UiControl.Refresh];
            bDone.Text = Common.Languages[UiControl.Finish];

            // Is this a right-to-left language?
            RightToLeftLayout = Common.RtlLanguages.Contains(lan);
        }

        private void fSelectiveSync_RightToLeftLayoutChanged(object sender, EventArgs e)
        {
            RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;

            lSelectiveSync.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            lSelectiveSync.RightToLeftLayout = RightToLeftLayout;
        }
    }
}
