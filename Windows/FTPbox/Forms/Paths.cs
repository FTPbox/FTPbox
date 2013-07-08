/* License
 * This file is part of FTPbox - Copyright (C) 2012-2013 ftpbox.org
 * FTPbox is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published 
 * by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed 
 * in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. 
 * If not, see <http://www.gnu.org/licenses/>.
 */
/* Paths.cs
 * The form used by the user to set the local-remote paths that will be synchronized
 */

using System;
using System.Drawing;
using System.Windows.Forms;
using FTPboxLib;
using System.Diagnostics;
using System.Linq;

namespace FTPbox.Forms
{
    public partial class Paths : Form
    {
        public Paths()
        {
            InitializeComponent();
        }

        private void bDone_Click(object sender, EventArgs e)
        {
            if (!System.IO.Directory.Exists(tPath.Text))
                System.IO.Directory.CreateDirectory(tPath.Text);

            var rp = string.Format("{0}/{1}", Profile.HomePath, tFullDir.Text.RemoveSlashes());
			while (rp.StartsWith("//")) rp = rp.Substring(1);

            Profile.AddPaths(rp, tPath.Text, tParent.Text);

            Settings.Save();

            Client.WorkingDirectory = Profile.RemotePath;

            ((fMain)Tag).gotpaths = true;
            
            Hide();
        }

        private void bBrowse_Click(object sender, EventArgs e)
        {
            fbd.ShowDialog();
            tPath.Text = fbd.SelectedPath;
        }

        /// <summary>
        /// When a path is selected in the tree-view, update the full-path textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string path = "/" + e.Node.FullPath.Replace('\\', '/');
            if (path.EndsWith(".."))
            {
                path = path.Substring(0, path.Length - 2);
            }
            else if (path.EndsWith("."))
            {
                path = path.Substring(0, path.Length - 1);
            }
            else if (path.EndsWith("//"))
            {
                path = path.Substring(0, path.Length - 1);
            }
            tFullDir.Text = path;
            tParent.Text = Profile.Host + path;
        }

        /// <summary>
        /// When an item in the tree-view is expanded, get the folders inside it and update its child items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            string path = "/" + e.Node.FullPath.Replace('\\', '/');

            if (e.Node.Nodes.Count > 0)
            {
                int i = e.Node.Index;

                foreach (TreeNode tn in e.Node.Nodes)
                {
                    try
                    {
                        treeView1.Nodes[i].Nodes.Remove(tn);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(l.Debug, ex.Message);
                    }
                }
            }

            foreach (var c in Client.List(path))
            {
                if (c.Type == ClientItemType.Folder)
                {
                    TreeNode ParentNode = new TreeNode {Text = c.Name};
                    e.Node.Nodes.Add(ParentNode);

                    TreeNode ChildNode = new TreeNode {Text = c.Name};
                    ParentNode.Nodes.Add(ChildNode);
                }
            }
        }

        private void treeView1_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            int i = e.Node.Nodes.Count;
            Log.Write(l.Debug, i.ToString());

            Log.Write(l.Debug, e.Node.FullPath);

            while (i != 0)
            {
                i -= 1;
                Log.Write(l.Debug, i.ToString());
            }
        }

        private void Paths_Load(object sender, EventArgs e)
        {
            Set_Language(Profile.Language); 
            
            tPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\FTPbox";

            treeView1.Nodes.Clear();

            var first = new TreeNode {Text = "/"};
            treeView1.Nodes.Add(first);

            foreach (var c in Client.List("."))
            {
                if (c.Type == ClientItemType.Folder)
                {
                    var ParentNode = new TreeNode {Text = c.Name};
                    treeView1.Nodes.Add(ParentNode);

                    var ChildNode = new TreeNode {Text = c.Name};
                    ParentNode.Nodes.Add(ChildNode);
                }
            }

            tParent.Text = Profile.Host;            
        }

        /// <summary>
        /// update the language of the labels etc
        /// </summary>
        /// <param name="lan"></param>
        private void Set_Language(string lan)
        {
            Text = Common.Languages.Get(lan + "/paths/add_dir", "Add a new directory");
            labSelect.Text = Common.Languages.Get(lan + "/paths/select_dir", "Select directory") + ":";
            labFullPath.Text = Common.Languages.Get(lan + "/paths/full_path", "Full path") + ":";
            labLocal.Text = Common.Languages.Get(lan + "/paths/local_folder", "Local folder") + ":";
            labParent.Text = Common.Languages.Get(lan + "/main_form/account_full_path", "Account's full path") + ":";
            bBrowse.Text = Common.Languages.Get(lan + "/paths/browse", "Browse");
            bDone.Text = Common.Languages.Get(lan + "/new_account/done", "Done");

            // Is this a right-to-left language?
            RightToLeftLayout = new[] { "he" }.Contains(lan);
        }

        /// <summary>
        /// if no path was selected, "Done" button gets disabled
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tPath_TextChanged(object sender, EventArgs e)
        {
            bDone.Enabled = !string.IsNullOrWhiteSpace(tPath.Text);
        }

        private void Paths_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.TaskManagerClosing)
            {
                Log.Write(l.Info, "Killing the process.....");                
                Process.GetCurrentProcess().Kill();                
            }
        }

        private void Paths_RightToLeftLayoutChanged(object sender, EventArgs e)
        {
            // Inherit manually
            labSelect.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            treeView1.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            treeView1.RightToLeftLayout = RightToLeftLayout;
            labFullPath.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            labLocal.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            labParent.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            tFullDir.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            tParent.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            tPath.RightToLeft = RightToLeftLayout ? RightToLeft.Yes : RightToLeft.No;
            // Relocate controls where necessary
            tFullDir.Location = RightToLeftLayout ? new Point(15, 128) : new Point(122, 128);
            tPath.Location = RightToLeftLayout ? new Point(95, 179) : new Point(15, 179);
            tParent.Location = RightToLeftLayout ? new Point(95, 223) : new Point(15, 223);
            bBrowse.Location = RightToLeftLayout ? new Point(15, 177) : new Point(341, 177);
            bDone.Location = RightToLeftLayout ? new Point(15,252) : new Point(340, 252);
        }
    }
}
