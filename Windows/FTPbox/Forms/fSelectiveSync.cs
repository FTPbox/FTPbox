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

        private static List<string> previousList;

        public fSelectiveSync()
        {
            InitializeComponent();
        }

        private async void fSelectiveSync_Load(object sender, EventArgs e)
        {
            previousList = Program.Account.IgnoreList.Items;

            Set_Language(Settings.General.Language);

            await RefreshListing();
        }

        private async void bRefresh_Click(object sender, EventArgs e)
        {
            await RefreshListing();
        }

        private void bDone_Click(object sender, EventArgs e)
        {
            Program.Account.IgnoreList.Items = previousList;
            Program.Account.IgnoreList.Save();

            Hide();
        }

        /// <summary>
        /// Refresh the entire list of files/folders
        /// </summary>
        private async Task RefreshListing()
        {
            this.Text = _titleListing;
            bRefresh.Enabled = false;

            var li = (await Program.Account.Client.ListRecursive(".", false)).ToList();

            foreach (var l in li)
            {
                // convert to relative paths
                l.FullPath = Program.Account.GetCommonPath(l.FullPath, false);
            }

            // get the first level folders
            var folders = li.Where(d => d.Type == ClientItemType.Folder && !d.FullPath.Contains("/"));

            // get the first level files
            var files = li
                .Where(f => f.Type == ClientItemType.File && folders.All(d => !f.FullPath.StartsWith(d.FullPath)))
                .Select(x => new TreeNode(x.Name))
                .ToArray();

            if (!Program.Account.Client.ListingFailed)
            {
                lSelectiveSync.Nodes.Clear();

                // List directories first
                foreach (var d in folders)
                {
                    if (d.Name == "webint") continue;

                    var parent = UIHelpers.ConstructNodeFrom(li, d);

                    lSelectiveSync.Nodes.Add(parent);
                }

                lSelectiveSync.Nodes.AddRange(files);

                EditNodeCheckboxes();
            }

            lSelectiveSync.ExpandAll();

            bRefresh.Enabled = true;
            this.Text = _titleNormal;
        }

        /// <summary>
        /// Uncheck items that have been picked as ignored by the user
        /// </summary>
        private void EditNodeCheckboxes()
        {
            _checkingNodes = true;

            UIHelpers.EditNodeCheckboxesRecursive(lSelectiveSync.Nodes, previousList);

            _checkingNodes = false;
        }

        private void lSelectiveSync_AfterCheck(object sender, TreeViewEventArgs e)
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

            previousList = UIHelpers.GetUncheckedItems(lSelectiveSync.Nodes).ToList();
        }

        private void Set_Language(string lan)
        {
            var listing = Common.Languages[MessageType.Listing];
            if (listing.StartsWith("FTPbox - "))
                listing = listing.Substring("FTPbox - ".Length - 1);

            _titleListing = string.Format("{0} - {1}", Common.Languages[UiControl.SelectiveSync], listing);
            _titleNormal = Common.Languages[UiControl.SelectiveSync];

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

        private void lSelectiveSync_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Console.WriteLine($"Selected node: {e.Node.FullPath}");
        }
    }
}
