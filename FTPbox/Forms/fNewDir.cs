using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FtpLib;

namespace FTPbox
{
    public partial class fNewDir : Form
    {
        //variables used
        string host;
        string UN;
        string pass;
        int port;

        FtpConnection ftp;

        public fNewDir()
        {
            InitializeComponent();
        }

        private void bBrowse_Click(object sender, EventArgs e)
        {
            fbd.ShowDialog();
            tPath.Text = fbd.SelectedPath;
        }

        private void bDone_Click(object sender, EventArgs e)
        {
            FTPbox.Properties.Settings.Default.rPath = tFullDir.Text;
            FTPbox.Properties.Settings.Default.lPath = tPath.Text;
            FTPbox.Properties.Settings.Default.delRem = !chkDelRem.Checked;
            FTPbox.Properties.Settings.Default.ftpParent = tParent.Text;
            FTPbox.Properties.Settings.Default.Save();

            ((frmMain)this.Tag).ClearLog();
            ((frmMain)this.Tag).UpdateDetails();
            ((frmMain)this.Tag).SetLocalWatcher();
            ((frmMain)this.Tag).gotpaths = true;
            try
            {
                ftp.Close();
            }
            catch { }
            this.Close();

            /*
            if (rAlreadyExists(tFullDir.Text))
            {
                MessageBox.Show(string.Format("Remote path {0} is already used in another box", tFullDir.Text), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (lAlreadyExists(tPath.Text))
            {
                MessageBox.Show(string.Format("Local path {0} is already used in another box", tPath.Text), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                ((fMain)this.Tag).Get_new_directory(tFullDir.Text, tPath.Text, chkSubdirectories.Checked, chkDelRem.Checked);

                ftp.Close();
                this.Close();
            }
            */
        }       

        private void fNewDir_Load(object sender, EventArgs e)
        {
            try
            {
                host = FTPbox.Properties.Settings.Default.ftpHost;
                UN = FTPbox.Properties.Settings.Default.ftpUsername;
                pass = FTPbox.Properties.Settings.Default.ftpPass;
                port = FTPbox.Properties.Settings.Default.ftpPort;

                ftp = new FtpConnection(host, port, UN, pass);
                ftp.Open();
                ftp.Login();

                tParent.Text = FTPbox.Properties.Settings.Default.ftpHost;

                treeView1.Nodes.Clear();

                TreeNode first = new TreeNode();
                first.Text = "/";
                treeView1.Nodes.Add(first);

                foreach (FtpDirectoryInfo dir in ftp.GetDirectories())
                {
                    if (dir.Name != "." && dir.Name != "..")
                    {
                        TreeNode ParentNode = new TreeNode();
                        ParentNode.Text = dir.Name.ToString();
                        treeView1.Nodes.Add(ParentNode);

                        TreeNode ChildNode = new TreeNode();
                        ChildNode.Text = dir.Name.ToString();
                        ParentNode.Nodes.Add(ChildNode);
                    }

                }
                treeView1.SelectedNode = first;
            }
            catch { this.Close(); }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string path = "/" + e.Node.FullPath.ToString().Replace('\\', '/');
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
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            string path = "/" + e.Node.FullPath.ToString().Replace('\\', '/');

            foreach (TreeNode tn in e.Node.Nodes)
            {
                tn.Remove();
            }

            foreach (FtpDirectoryInfo dir in ftp.GetDirectories(path))
            {
                if (dir.Name != "." && dir.Name != "..")
                {
                    TreeNode ParentNode = new TreeNode();
                    ParentNode.Text = dir.Name.ToString();
                    e.Node.Nodes.Add(ParentNode);

                    TreeNode ChildNode = new TreeNode();
                    ChildNode.Text = dir.Name.ToString();
                    ParentNode.Nodes.Add(ChildNode);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (tFullDir.Text != "" && tPath.Text != "")
            {
                bDone.Enabled = true;
            }
            else
            {
                bDone.Enabled = false;
            }
        }

        private bool rAlreadyExists(string path)
        {
            bool b = false;
            foreach (string s in FTPbox.Properties.Settings.Default.Boxes)
            {
                string[] comb = s.Split('|', '|');
                if (comb[0] == path)
                {
                    b = true;
                }
            }
            return b;
        }
        private bool lAlreadyExists(string path)
        {
            bool b = false;
            foreach (string s in FTPbox.Properties.Settings.Default.Boxes)
            {
                string[] comb = s.Split('|', '|');
                if (comb[1] == path)
                {
                    b = true;
                }
            }
            return b;
        }

        private void tParent_TextChanged(object sender, EventArgs e)
        {
            FTPbox.Properties.Settings.Default.ftpParent = tParent.Text;
            FTPbox.Properties.Settings.Default.Save();
        }
    }
}
